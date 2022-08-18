
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Items;

namespace Runedal.GameData.Characters
{
    public class Player : CombatCharacter
    {
        //values of multipliers for calculating attributes modifiers
        private const double MaxHpStrMultiplier = 10;
        private const double MaxMpIntMultiplier = 8;
        private const double HpRegenStrMultiplier = 0.1;
        private const double MpRegenIntMultiplier = 0.3;
        private const double SpeedAgiMultiplier = 0.1;
        private const double AttackStrMultiplier = 1.5;
        private const double AtkSpeedAgiMultiplier = 0.3;
        private const double AccuracyAgiMultiplier = 0.3;
        private const double CriticalAgiMultiplier = 0.5;
        private const double EvasionAgiMultiplier = 0.3;
        private const double MagicResistanceIntMultiplier = 0.1;

        //values of other multipliers
        private const double SpeedWeightMultiplier = -0.02;

        //default constructor for json deserialization
        public Player() : base()
        {
            //initialize items worn with placeholders to avoid null exception
            FullBody = new Armor(Armor.ArmorType.FullBody, "placeholder");
            Helmet = new Armor(Armor.ArmorType.Helmet, "placeholder");
            Gloves = new Armor(Armor.ArmorType.Gloves, "placeholder");
            Shoes = new Armor(Armor.ArmorType.Shoes, "placeholder");
            Weapon = new Weapon("placeholder");
        }
        public Player(string[] descriptive, int[] combatStats, int[] attributeStats, string[][] responses, int gold)
            : base(descriptive, combatStats, responses, gold)
        {
            Strength = attributeStats[0];
            Intelligence = attributeStats[1];
            Agility = attributeStats[2];
            Level = 1;
            Experience = 0;

            //initialize items worn with placeholders to avoid null exception
            FullBody = new Armor(Armor.ArmorType.FullBody, "placeholder");
            Helmet = new Armor(Armor.ArmorType.Helmet, "placeholder");
            Gloves = new Armor(Armor.ArmorType.Gloves, "placeholder");
            Shoes = new Armor(Armor.ArmorType.Shoes, "placeholder");
            Weapon = new Weapon("placeholder");
        }
        public enum State
        {
            Idle,
            Trade,
            Combat
        }
        public State CurrentState { get; set; }

        //player's level and amount of experience
        public int Level { get; set; }
        public int Experience { get; set; }
        
        //player character attributes influencing other statistics
        public int Strength { get; set; }
        public int Intelligence { get; set; }
        public int Agility { get; set; }

        //items worn by player
        public Armor? FullBody { get; set; }
        public Armor? Helmet { get; set; }
        public Armor? Gloves { get; set; }
        public Armor? Shoes { get; set; }
        public Weapon? Weapon { get; set; }

        //character with whom player currently interacts
        public Character? InteractsWith { get; set; }

        //methods for getting effective statistics (after applying all modifiers)
        public override double GetEffectiveMaxHp()
        {
            double effectiveMaxHp = base.GetEffectiveMaxHp();
            double strengthModifier = MaxHpStrMultiplier * Strength;
            effectiveMaxHp += strengthModifier;
            return effectiveMaxHp;
        }
        public override double GetEffectiveMaxMp()
        {
            double effectiveMaxMp = base.GetEffectiveMaxMp();
            double intelligenceModifier = MaxMpIntMultiplier * Intelligence;
            effectiveMaxMp += intelligenceModifier;
            return effectiveMaxMp;
        }
        public override double GetEffectiveHpRegen()
        {
            double effectiveHpRegen = base.GetEffectiveHpRegen();
            double strengthModifier = HpRegenStrMultiplier * Strength;
            effectiveHpRegen += strengthModifier;
            return effectiveHpRegen;
        }
        public override double GetEffectiveMpRegen()
        {
            double effectiveMpRegen = base.GetEffectiveMpRegen();
            double intelligenceModifier = MpRegenIntMultiplier * Intelligence;
            effectiveMpRegen += intelligenceModifier;
            return effectiveMpRegen;
        }
        public override double GetEffectiveSpeed()
        {
            //effective stat with external modifiers
            double effectiveSpeed = base.GetEffectiveSpeed();

            //attribute modifier
            double agilityModifier = Agility * SpeedAgiMultiplier;

            //weight modifier
            double weightModifier = GetCarryWeight() * SpeedWeightMultiplier;
            weightModifier += (Strength * 0.05);

            //add modifiers to effective stat
            effectiveSpeed += agilityModifier += weightModifier;

            //prevent effectiveSpeed from dropping below 0
            if (effectiveSpeed < 0)
            {
                effectiveSpeed = 0;
            }

            return effectiveSpeed;
        }
        public override double GetEffectiveAttack()
        {
            //effective stat with external modifiers
            double effectiveAttack = base.GetEffectiveAttack();

            //attribute modifier
            double strengthModifier = Strength * AttackStrMultiplier;

            //weapon modifier
            double weaponModifier = Weapon!.Attack;

            //add modifiers to effective stat
            effectiveAttack += strengthModifier += weaponModifier;

            return effectiveAttack;
        }
        public override double GetEffectiveAtkSpeed()
        {
            double effectiveAtkSpeed = base.GetEffectiveAtkSpeed();
            double agilityModifier = Agility * AtkSpeedAgiMultiplier;

            return effectiveAtkSpeed;
        }
        public override double GetEffectiveAccuracy()
        {
            double effectiveAccuracy = base.GetEffectiveAccuracy();
            double agilityModifier = Agility * AccuracyAgiMultiplier;
            effectiveAccuracy += agilityModifier;
            return effectiveAccuracy;
        }
        public override double GetEffectiveCritical()
        {
            double effectiveCritical = base.GetEffectiveCritical();
            double agilityModifier = Agility * CriticalAgiMultiplier;
            effectiveCritical += agilityModifier;
            return effectiveCritical;
        }
        public override double GetEffectiveDefense()
        {
            double effectiveDefense = base.GetEffectiveDefense();
            double armorModifier = FullBody!.Defense + Helmet!.Defense + Gloves!.Defense + Shoes!.Defense;
            effectiveDefense += armorModifier;
            return effectiveDefense;
        }
        public override double GetEffectiveEvasion()
        {
            double effectiveEvasion = base.GetEffectiveEvasion();
            double agilityModifier = Agility * EvasionAgiMultiplier;
            effectiveEvasion += agilityModifier;
            return effectiveEvasion;
        }
        public override double GetEffectiveMagicResistance()
        {
            double effectiveMagicResistance = base.GetEffectiveMagicResistance();
            double intelligenceModifier = Intelligence * MagicResistanceIntMultiplier;
            effectiveMagicResistance += intelligenceModifier;
            return effectiveMagicResistance;
        }

        //method returning sum weight of all items carried by player
        public int GetCarryWeight()
        {
            //add all worn items weights to total carry weight
            int carryWeight = FullBody!.Weight + Helmet!.Weight + Gloves!.Weight + Shoes!.Weight + Weapon!.Weight;

            //for each item in Inventory, add it's weight to total carry weight
            Inventory!.ForEach(item => carryWeight += item.Weight);

            return carryWeight;
        }

    }
}
