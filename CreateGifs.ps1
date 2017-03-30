# usage:
# ~ $log = init "D:\SteamLibrary\SteamApps\common\Worms Armageddon\User\Games\2016-02-18 17.52.14 [Online] foo, bar.WAgame"
# ~ summarizelog $log
# ~ specificturn $log 3

$wormsRoot = (Get-ItemProperty HKCU:\SOFTWARE\Team17SoftwareLTD\WormsArmageddon).PATH
$wa = join-path $wormsRoot WA.exe
$wacapture = join-path $wormsRoot User\Capture
$imageMagickRoot = (Get-ItemProperty HKLM:\SOFTWARE\ImageMagick\Current).BinPath
$convert = join-path $imageMagickRoot convert.exe
$frameskip = 3

function init($replayFile) {
    $replayFile = validatereplayfile $replayFile
    $outputDir = getoutputdir $replayFile
    copyreplay $replayFile $outputDir
    dumplog "$outputDir/replay.WAgame"
    $parsedLog = parselogfile "$outputDir\replay.log"
    return $parsedLog
}

function getoutputdir($replayFile) {
    $replayName = (getfilename $replayFile).TrimEnd('.WAgame').Replace(' ', '_')
    return (join-path (pwd) $replayName)
}

function copyreplay($replayfile, $outputDir) {
    write-output "Writing out to $outputDir ..."
    new-item -type directory $outputDir -force | out-null

    write-output "Copying $replayFile to $outputDir ..."
    copy-item -literalpath $replayFile -destination "$outputDir/replay.WAgame"
}

function dumplog($replayFile) {
    # this will write a replay log to $outputDir/replay.log
    & $wa /getlog $replayFile /quiet | write-output
}

function turngif($turn, $i, $outputDir, $startOffset = -1, $endOffset = 0) {
    $turnName = "turn_$($i.ToString('000'))_$($turn.player)"
    $turnDir = "$outputDir/$turnName"
    new-item -type directory $turnDir -force | out-null
    $startTime = $turn.weaponFired
    $startTime = ([timespan]$startTime).Add((new-timespan -seconds $startOffset)).ToString()
    $endTime = $turn.end
    $endTime = ([timespan]$endTime).Add((new-timespan -seconds $endOffset)).ToString()
    dumpframes "$outputDir/replay.WAgame" $turnDir $startTime $endTime
    write-host "Writing gif to $outputDir/$turnName.gif"
    makegif "$turnDir/replay" "$outputDir/$turnName.gif"
    remove-item $turnDir -force -recurse
}

function summarizelog($parsedlog) {
    $i = 0
    $parsedlog.turns | foreach {
      $i++
      $_.start + " " + $i.ToString('000') + " " + $_.player + " used " + $_.weapon
    }
}

function specificturn($parsedlog, $turn, $startOffset = -1, $endOffset = 0) {
    turngif $parsedlog.turns[$turn-1] $turn $parsedlog.outputdir $startOffset $endOffset
}

function initoutputdir($replayfile) {
    $outputdir = getoutputdir $replayfile
    copyreplay $replayfile $outputdir | out-null
    return $outputdir
}

function validatereplayfile($replayFile) {
    $replayFile = getfullpath $replayFile
    
    if (!$replayFile) { throw 'Pass a .WAgame file as the first argument' }
    if (!$replayFile.EndsWith('.WAgame')) { throw 'Must pass a .WAgame file as first argument ' }
    if (!(test-path -literalpath $replayFile)) { throw "Provided replay does not exist: $replayFile" }
    
    return $replayFile
}

function main($replayFile) {
    $replayFile = validatereplayfile $replayFile

    write-output "Pulling turn gifs from $replayFile ..."
    $parsedLog = init $replayFile

    summarizelog $parsedLog

    $i = 0
    $parsedLog.turns | foreach {
        $i++
        turngif $_ $i $parsedLog.outputDir
    }
}

function getfullpath($path) {
    return [System.IO.Path]::GetFullPath($path)
}

function getfilename($path) {
    return [System.IO.Path]::GetFileName($path)
}

function getdirname($path) {
    return [System.IO.Path]::GetDirectoryName($path)
}

function parselogfile($path) {

    $results = @{}
    $results.turns = @()
    $results.outputDir = getdirname $path
    $currentTurn = @{}

    $timestamp = '\[(?<timestamp>\d\d:\d\d:\d\d.\d\d)\]'

    get-content -literalpath $path | foreach {

        if ($_ -match "$timestamp ••• (?<player>.+?) starts turn") {
            write-verbose "Player turn start: $($matches['player']) at $($matches['timestamp'])"
            $currentTurn = @{ 
                'player' = $matches['player']; 
                'start' = $matches['timestamp'];
            }

        } elseif ($_ -match "$timestamp ••• (?<player>.+?) (ends|loses) turn") {
            write-verbose "Player turn end  : $($matches['player']) at $($matches['timestamp'])"
            $currentTurn.end = $matches['timestamp']
            $results.turns += $currentTurn

        } elseif ($_ -match "$timestamp ••• (?<player>.+?) fires (?<weapon>[a-z ]+)") {
            write-verbose "Player fired     : $($matches['player']) / $($matches['weapon'])"
            $currentTurn.weapon = $matches['weapon']
            $currentTurn.weaponFired = $matches['timestamp']

        } elseif ($_ -match "$timestamp ••• Damage dealt: ") {
            write-verbose "Player fired     : $($matches['player']) / $($matches['weapon'])"
            # if the turn dealt damage then make sure we see it all - this is often a while after the turn 'ends'
            $currentTurn.end = $matches['timestamp']

        } else {
            #write-verbose "Unrecognised line: $_"
        }

    } | out-null

    return $results
}

function dumpframes($replayFile, $outputDir, $start, $end) {
    write-verbose "Getting WA.exe to dump replay for $start to $end ..."
    & $wa /getvideo $replayFile $frameskip $start $end | out-null
    move-item -force -LiteralPath "$wacapture\replay" $outputDir
}

function makegif($imagesFolder, $outputFile) {
    write-verbose "Converting $imagesFolder into $outputFile ..."
    #& $convert -delay 2 -coalesce -layers optimize "$imagesFolder\*.png" $outputFile
    & $convert -delay ($frameskip * 2) -layers optimize-frame "$imagesFolder\*.png" $outputFile
}