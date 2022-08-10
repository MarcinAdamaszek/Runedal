
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

        //methods for getting effective statistics (after applying all modifiers)
        public override double GetEffectiveSpeed()
        {
            //effective stat with external modifiers
            double effectiveSpeed = base.GetEffectiveSpeed();

            //attribute modifier
            double agilityModifier = Agility * 0.1;

            //weight modifier
            double weightModifier = GetCarryWeight() * -0.02;
            weightModifier += (Strength * 0.05);

            //add modifiers to effective stat
            effectiveSpeed += agilityModifier += weightModifier;

            return effectiveSpeed;
        }
        public override double GetEffectiveAttack()
        {
            //effective stat with external modifiers
            double effectiveAttack = base.GetEffectiveAttack();

            //attribute modifier
            double strengthModifier = Strength * 1.5;

            //weapon modifier
            double weaponModifier = Weapon!.Attack;

            //add modifiers to effective stat
            effectiveAttack += strengthModifier += weaponModifier;

            return effectiveAttack;
        }
        public override double GetEffectiveAtkSpeed()
        {
            double effectiveAtkSpeed = base.GetEffectiveAtkSpeed();
            double agilityModifier = Agility * 0.3;

            return effectiveAtkSpeed;
        }
        public override double GetEffectiveAccuracy()
        {
            double effectiveAccuracy = base.GetEffectiveAccuracy();
            double agilityModifier = Agility * 0.3;
            effectiveAccuracy += agilityModifier;
            return effectiveAccuracy;
        }
        public override double GetEffectiveCritical()
        {
            double effectiveCritical = base.GetEffectiveCritical();
            double agilityModifier = Agility * 0.5;
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
            double agilityModifier = Agility * 0.3;
            effectiveEvasion += agilityModifier;
            return effectiveEvasion;
        }
        public override double GetEffectiveMagicResistance()
        {
            double effectiveMagicResistance = base.GetEffectiveMagicResistance();
            double intelligenceModifier = Intelligence * 0.1;
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
