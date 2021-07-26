function Get-NextVersion ([string] $tag, [System.Version] $nextVersion)
{
    $lastTag = (git describe --match "$tag[0-9]*" --abbrev=0 HEAD --tags).substring($tag.length)
    $lastVersion = [System.Version]::Parse($lastTag)

    if($nextVersion -gt $lastVersion)
    {
        $nextVersion = New-Object System.Version($nextVersion.Major, $nextVersion.Minor, 0)
    } else {
        $nextVersion = New-Object System.Version($lastVersion.Major, $lastVersion.Minor, ($lastVersion.Build + 1))
    }

    return $nextVersion
}