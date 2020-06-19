# WormsLeague

Repo containing tooling and config for the Redgate worms league

## Installing the CLI

### Windows

```
$script = Invoke-WebRequest https://raw.githubusercontent.com/TheEadie/WormsLeague/master/InstallCli.ps1
Invoke-Expression $($script.content)
```

### Linux

```
docker run theeadie/wormscli <args>
```

## First time setup

```
worms setup
```

## Hosting a game

```
worms host
```
