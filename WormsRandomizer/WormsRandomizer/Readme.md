
#Randomizer Guide

The randomizer will generate a custom worms scheme with weapons of random probability.
This scheme is intended to be played as a shopper map.

#Tips for playing the randomizer
- Starting inventory
  - 4 Random weapons
  - Unlimited ropes, bungees and parachutes
  - Some random 'Utility' items with a random delay (see below)
- All weapons have the same chance to appear in a crate (unless it was chosen to be made more common)
- There will be at least 5 weapons that are set to max strength per match
 - Some weapons are strong even with weak damage values
 - Some weapons do nothing at the weakest damage values
- The more powerful a weapon is, the more likely it is to have a longer delay (+d flag)
  - Weapons in the weakest third of the damage range will never have a delay
- Super weapons can appear, but their damage is not randomised

#Create a scheme & hosting a game
- Start the executable with the command line argument for the seed and (optionally) any extra parameters
- Copy the generated ".wsc" file over to "Worms Armageddon\User\Schemes"
- Host a worms game & select the scheme
- Pick a map that is shopper friendly
  - Disabling bridges in the map settings is recommended
  - Avoid maps that have holes worms can start in and can't get out of, the randomizer might not give you the tools to get out

Example:
```
WormsRandomizer.exe myTest h150 -Freeze -Invisibility *Teleport
```
This will create a random scheme with "myTest" as the seed where each team will start with `Teleport` and 150 health per worm.
 `Freeze` and `Invisibility` will not appear in the game.


 ####Weapon options
 ```
+[weapon]: [Weapon] will be more common
-[weapon]: [Weapon] will never appear
*[weapon]: always start with [Weapon]
[+/-]d: powerful weapons are delayed. Weapons weaker than default are never delayed. (default: +)
p[N]: minimum number of powerful weapons (default: 5)
s[N]: minimum number of starting weapons (default: 4)
au[N]: ammo quantity for utility weapons (default: 1)
aw[N]: ammo quantity for starting weapons (default: 1)
 ```

 ####Game options
 ```
h[N]: worm starting health (default: 100)
[+/-]super: super weapons are enabled (default: +)
[+/-]heaven: sheep heaven - exploding crates contain sheep (default: -)
[+/-]ug: upgraded grenade - grenades are more powerful (default: -)
[+/-]us: upgraded shotgun - shotgun shoots extra shots (default: -)
[+/-]ul: upgraded longbow - longbows are more powerful (default: -)
[+/-]uc: upgraded cluster bombs - cluster bombs contain more clusters (default: -)
[+/-]ua: upgraded sheep - super sheep are aqua sheep (default: -)
[+/-]flood: randomise the sudden death water rise rate (default: -)
[+/-]mine-dud: mines can be duds (default: -)
[+/-]mine-any: If enabled each mine will have a random delay rather than all mines having the same (random) delay (default: -)
 ```

 ####Output options
[+/-]out-starting: Output a list of starting weapons to the console (default: +)
[+/-]out-summary: Output a randomised stats of all available weapons to the console (default: -)
[+/-]out-json: Output the scheme as a json file (default: -)
[+/-]out-scheme: Output the scheme as a wsc file (default: +)


Enabling upgraded weaponry will bypass randomized power settings for that weapon.
Enabling sheep heaven will also have the side effect of making sheep more common.

####Utility items
- Prod
- Girder
- Teleport
- Jet Pack
- Low Gravity
- Laser Sight
- Fast Walk
- Invisibility
- Freeze
- Select Worm