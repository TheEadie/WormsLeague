using System;
using System.Collections.Generic;

namespace WormsScheme
{
    public class RandomSchemeGenerator
    {
        public Scheme GetModel(Scheme otherModel)
        {
            var random = new Random();

            const string signature = "SCHM";
            const int version = 2;

            var hotSeatDelay = otherModel.HotSeatDelay;
            var retreatTime = otherModel.RetreatTime;
            var ropeRetreatTime = otherModel.RopeRetreatTime;
            var showRoundTime = otherModel.DisplayTotalRoundTime;
            var automaticReplays = otherModel.AutomaticReplays;
            var fallDamage = otherModel.FallDamage;
            var artilleryMode = otherModel.ArtilleryMode;
            byte stockpilingMode = otherModel.StockpilingMode;
            byte wormSelect = otherModel.WormSelect;
            byte suddenDeathEvent = otherModel.SuddenDeathEvent;
            var waterRiseRate = otherModel.WaterRiseRate;
            var weaponCrateProbability = otherModel.WeaponCrateProbability;
            var donorCards = otherModel.DonorCards;
            var healthCrateProbability = otherModel.HealthCrateProbability;
            var healthCrateEnergy = otherModel.HealthCrateEnergy;
            var utilityCrateProbability = otherModel.UtilityCrateProbability;
            var hazardObjectTypes = otherModel.HazardObjectTypes;
            var mineDelay = otherModel.MineDelay;
            var dudMines = otherModel.DudMines;
            var wormPlacement = otherModel.WormPlacement;
            var initialWormEnergy = otherModel.InitialWormEnergy;
            var turnTime = otherModel.TurnTime;
            var roundTime = otherModel.RoundTime;
            var numberOfRounds = otherModel.NumberOfRounds;
            var blood = otherModel.Blood;
            var aquaSheep = otherModel.AquaSheep;
            var sheepHeaven = otherModel.SheepHeaven;
            var godWorms = otherModel.GodWorms;
            var indestructibleLand = otherModel.IndestructibleLand;
            var upgradedGrenade = otherModel.UpgradedGrenade;
            var upgradedShotgun = otherModel.UpgradedShotgun;
            var upgradedClusterBombs = otherModel.UpgradedClusterBombs;
            var upgradedLongbow = otherModel.UpgradedLongbow;
            var teamWeapons = otherModel.TeamWeapons;
            var superWeapons = otherModel.SuperWeapons;

            var weapons = new List<Weapon>();

            foreach (var weapon in otherModel.Weapons)
            {
                var ammo = random.NextBoundedInt(weapon.Ammo, 0, 10, 1);
                var power = random.NextBoundedInt(weapon.Power, 1, 4, 0.5);

                // Only change wepaon delay if already set or 10% of the time
                var delay = (weapon.Delay != 0 || random.NextDouble() > 0.9) ? random.NextBoundedInt(weapon.Delay, 1, 10, 1) : weapon.Delay;
                var crateProb = weapon.CrateProbability;

                weapons.Add(new Weapon(weapon.Name, ammo, power, delay, crateProb));
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