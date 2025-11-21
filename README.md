# WormsLeague

Repo containing tooling and config for the Redgate worms league

## Installing the CLI

### Windows

```
$script = Invoke-WebRequest https://raw.githubusercontent.com/TheEadie/WormsLeague/main/install-cli.ps1
Invoke-Expression $($script.content)
```

### Linux

```
docker run theeadie/worms-cli <args>
```

## First time setup

```
worms auth
```

then contact an admin to get your account added to the league

## Hosting a game

```
worms host
```

This will:

-   Download the latest Scheme options
-   Start a game
-   Announce the game on Slack with a link to join
-   Upload the replay at the end of the game
