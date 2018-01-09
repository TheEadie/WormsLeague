$installDirPath = (Get-ItemProperty HKCU:\SOFTWARE\Team17SoftwareLTD\WormsArmageddon).PATH

function PullAndCreateBranch() {
    git co random
    git pull
    git co -b Get-date
}

function BuildConverter() {

}

function MutateOptions() {
    $mutatorPath = Join-Path $PSScriptRoot 'WormsSchemeConverter\WormsScheme\bin\Debug\WormsScheme.exe'
    $randomSchemeFile = Join-Path $installDirPath '\User\Schemes\Mutated Options.wsc'

    & $mutatorPath "D:\Code\WormsLeague\Mutated Options.txt" "D:\Code\WormsLeague\Mutated Options.txt" /r
    & $mutatorPath "D:\Code\WormsLeague\Mutated Options.txt" $randomSchemeFile
}

function PushRepo() {

}

function CreatePR() {

}

PullAndCreateBranch
MutateOptions
