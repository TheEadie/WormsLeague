using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WormsScheme
{
    public class Weapons
    {
        public IEnumerable<string> AllWeapons { get; } 

        public Weapons(int wscFileVersion)
        {
            AllWeapons = wscFileVersion == 1 ? v1Weapons : v2Weapons;
        }

        private static readonly IEnumerable<string> v1Weapons = new List<string>()
        {
            "Bazooka",
            "Homing Missile",
            "Mortar",
            "Grenade",
            "Cluster Bomb",
            "Skunk",
            "Petrol Bomb",
            "Banana Bomb",
            "Handgun",
            "Shotgun",
            "Uzi",
            "Minigun",
            "Longbow",
            "Airstrike",
            "Napalm Strike",
            "Mine",
            "Fire Punch",
            "Dragon Ball",
            "Kamikaze",
            "Prod",
            "Battle Axe",
            "Blowtorch",
            "Pneumatic Drill",
            "Girder",
            "Ninja Rope",
            "Parachute",
            "Bungee",
            "Teleport",
            "Dynamite",
            "Sheep",
            "Baseball Bat",
            "Flame Thrower",
            "Homing Pigeon",
            "Mad Cow",
            "Holy Hand Grenade",
            "Old Woman",
            "Sheep Launcher",
            "Super Sheep",
            "Mole Bomb",
        };

        private static readonly IEnumerable<string> v2Weapons = new List<string>()
        {
            "Bazooka",
            "Homing Missile",
            "Mortar",
            "Grenade",
            "Cluster Bomb",
            "Skunk",
            "Petrol Bomb",
            "Banana Bomb",
            "Handgun",
            "Shotgun",
            "Uzi",
            "Minigun",
            "Longbow",
            "Airstrike",
            "Napalm Strike",
            "Mine",
            "Fire Punch",
            "Dragon Ball",
            "Kamikaze",
            "Prod",
            "Battle Axe",
            "Blowtorch",
            "Pneumatic Drill",
            "Girder",
            "Ninja Rope",
            "Parachute",
            "Bungee",
            "Teleport",
            "Dynamite",
            "Sheep",
            "Baseball Bat",
            "Flame Thrower",
            "Homing Pigeon",
            "Mad Cow",
            "Holy Hand Grenade",
            "Old Woman",
            "Sheep Launcher",
            "Super Sheep",
            "Mole Bomb",
            "Jet Pack",
            "Low Gravity",
            "Laser Sight",
            "Fast Walk",
            "Invisibility",
            "Damage x2",
            "Freeze",
            "Super Banana Bomb",
            "Mine Strike",
            "Girder Starter Pack",
            "Earthquake",
            "Scales Of Justice",
            "Ming Vase",
            "Mike's Carpet Bomb",
            "Patsy's Magic Bullet",
            "Indian Nuclear Test",
            "Select Worm",
            "Salvation Army",
            "Mole Squadron",
            "MB Bomb",
            "Concrete Donkey",
            "Suicide Bomber",
            "Sheep Strike",
            "Mail Strike",
            "Armageddon"
        }; 
    }
}
