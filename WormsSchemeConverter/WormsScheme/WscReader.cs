using System.Collections.Generic;
using System.IO;

namespace WormsScheme
{
    public class WscReader
    {
        private readonly string m_FilePath;

        public WscReader(string filePath)
        {
            m_FilePath = filePath;
        }

        public Scheme GetModel()
        {
            using (BinaryReader b = new BinaryReader(File.Open(m_FilePath, FileMode.Open)))
            {
                var signature = b.ReadChars(4);
                var version = b.ReadByte();
                var hotSeatDelay = b.ReadByte();
                var retreatTime = b.ReadByte();
                var ropeRetreatTime = b.ReadByte();
                var displayTotalRoundTime = b.ReadBoolean();
                var automaticReplays = b.ReadBoolean();
                var fallDamage = b.ReadByte();
                var artilleryMode = b.ReadBoolean();
                var unusedBountyMode = b.ReadBoolean();
                var stockpilingMode = b.ReadByte();
                var wormSelect = b.ReadByte();
                var suddenDeathEvent = b.ReadByte();
                var waterRiseRate = b.ReadSByte();
                var weaponCrateProb = b.ReadSByte();
                var donorCards = b.ReadBoolean();
                var healthCrateProb = b.ReadSByte();
                var healthCrateEnergy = b.ReadByte();
                var utilityCrateProb = b.ReadSByte();
                var hazardObjects = b.ReadByte();
                var mineDelay = b.ReadByte();
                var dudMines = b.ReadBoolean();
                var wormPlacement = b.ReadBoolean();
                var initialWormEnergy = b.ReadByte();
                var turnTime = b.ReadSByte();
                var roundTime = b.ReadSByte();
                var numberOfRounds = b.ReadByte();
                var blood = b.ReadBoolean();
                var aquaSheep = b.ReadBoolean();
                var sheepHeaven = b.ReadBoolean();
                var godWorms = b.ReadBoolean();
                var indestructibleLand = b.ReadBoolean();
                var upgradedGrenade = b.ReadBoolean();
                var upgradedShotgun = b.ReadBoolean();
                var upgradedClusters = b.ReadBoolean();
                var upgradedLongbow = b.ReadBoolean();
                var teamWeapons = b.ReadBoolean();
                var superWeapons = b.ReadBoolean();

                var weapons = new List<Weapon>();

                foreach (var weaponName in Weapons.AllWeapons)
                {
                    var ammo = b.ReadByte();
                    var power = b.ReadByte();
                    var delay = b.ReadByte();
                    var crateProb = b.ReadByte();

                    weapons.Add(new Weapon(weaponName, ammo, power, delay, crateProb));
                }

                return new Scheme(new string(signature), version, hotSeatDelay, retreatTime, ropeRetreatTime,
                    displayTotalRoundTime,
                    automaticReplays, fallDamage, artilleryMode, stockpilingMode, wormSelect, suddenDeathEvent,
                    waterRiseRate, weaponCrateProb, donorCards, healthCrateProb, healthCrateEnergy, utilityCrateProb,
                    hazardObjects, mineDelay, dudMines, wormPlacement, initialWormEnergy, turnTime, roundTime,
                    numberOfRounds, blood, aquaSheep, sheepHeaven, godWorms, indestructibleLand, upgradedGrenade,
                    upgradedShotgun, upgradedClusters, upgradedLongbow, teamWeapons, superWeapons,
                    weapons);
            }
        }
    }
}
