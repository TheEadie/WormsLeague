using System;
using System.Collections.Generic;

namespace WormsScheme
{
    public class RandomSchemeGenerator
    {
        public Scheme GetModel()
        {
            var random = new Random();

            const string signature = "SCHM";
            const int version = 2;

            var hotSeatDelay = 5;
            var retreatTime = 3;
            var ropeRetreatTime = 5;
            var showRoundTime = true;
            var automaticReplays = false;
            var fallDamage = 1;
            var artilleryMode = false;
            byte stockpilingMode = 0;
            byte wormSelect = 0;
            byte suddenDeathEvent = 3;
            var waterRiseRate = 2;
            var weaponCrateProbability = 80;
            var donorCards = true;
            var healthCrateProbability = 20;
            var healthCrateEnergy = 50;
            var utilityCrateProbability = 20;
            var hazardObjectTypes = 43;
            var mineDelay = 0;
            var dudMines = false;
            var wormPlacement = false;
            var initialWormEnergy = 100;
            var turnTime = 45;
            var roundTime = 10;
            var numberOfRounds = 1;
            var blood = false;
            var aquaSheep = true;
            var sheepHeaven = false;
            var godWorms = false;
            var indestructibleLand = false;
            var upgradedGrenade = false;
            var upgradedShotgun = false;
            var upgradedClusterBombs = false;
            var upgradedLongbow = false;
            var teamWeapons = false;
            var superWeapons = false;

            var weapons = new List<Weapon>();

            foreach (var weaponName in Weapons.AllWeapons)
            {
                var ammo = random.NextBoundedInt(5, 0, 10, 0.5);
                var power = 0;
                var delay = 0;
                var crateProb = 0;

                weapons.Add(new Weapon(weaponName, ammo, power, delay, crateProb));
            }

            return new Scheme(
                signature,
                version,
                hotSeatDelay,
                retreatTime,
                ropeRetreatTime,
                showRoundTime,
                automaticReplays,
                fallDamage,
                artilleryMode,
                stockpilingMode,
                wormSelect,
                suddenDeathEvent,
                waterRiseRate,
                weaponCrateProbability,
                donorCards,
                healthCrateProbability,
                healthCrateEnergy,
                utilityCrateProbability,
                hazardObjectTypes,
                mineDelay,
                dudMines,
                wormPlacement,
                initialWormEnergy,
                turnTime,
                roundTime,
                numberOfRounds,
                blood,
                aquaSheep,
                sheepHeaven,
                godWorms,
                indestructibleLand,
                upgradedGrenade,
                upgradedShotgun,
                upgradedClusterBombs,
                upgradedLongbow,
                teamWeapons,
                superWeapons,
                weapons);
        }
    }
}