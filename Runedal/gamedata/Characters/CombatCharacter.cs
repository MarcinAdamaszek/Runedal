using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Characters
{
    public class CombatCharacter : Character
    {
        //default constructor for json deserialization
        public CombatCharacter() : base()
        {
            Modifiers = new List<Modifier>();
        }
        public CombatCharacter(string[] descriptive, int[] combatStats, string[][] responses, int gold)
            : base(descriptive, responses, gold)
        {
            HpMax = combatStats[0];
            MpMax = combatStats[1];
            HpRegen = combatStats[2];
            MpRegen = combatStats[3];
            Speed = combatStats[4];
            Attack = combatStats[5];
            AtkSpeed = combatStats[6];
            Accuracy = combatStats[7];
            Critical = combatStats[8];
            Defense = combatStats[9];
            Evasion = combatStats[10];
            MagicResistance = combatStats[11];
            Gold = combatStats[12];

            Hp = HpMax;
            Mp = MpMax;

            Modifiers = new List<Modifier>();
        }
        public enum StatType
        {
            HpMax,
            MpMax,
            HpRegen,
            MpRegen,
            Strength,
            Intelligence,
            Agility,
            Speed,
            Attack,
            AtkSpeed,
            Accuracy,
            Critical,
            Defense,
            Evasion,
            MagicResistance
        }

        //hp/mp real pools
        public double Hp { get; set; }
        public double Mp { get; set; }

        //health and mana statistics
        public double HpMax { get; set; }
        public double MpMax { get; set; }
        public double HpRegen { get; set; }
        public double MpRegen { get; set; }

        //base combat statistics
        public double Speed { get; set; }
        public double Attack { get; set; }
        public double AtkSpeed { get; set; }
        public double Accuracy { get; set; }
        public double Defense { get; set; }
        public double Evasion { get; set; }
        public double MagicResistance { get; set; }
        public double Critical { get; set; }


        //list of modifiers currently affecting character
        public List<Modifier>? Modifiers { get; set; }


        //'getters' for effective character's statistics - calculated from base statistics and modifiers
        public virtual double GetEffectiveSpeed()
        {
            //combine base stat value and value of all its modifiers
            double effectiveSpeed = this.Speed + ApplyModifiers(StatType.Speed);

            return effectiveSpeed;
        }
        public virtual double GetEffectiveAttack()
        {
            //combine base stat value and value of all its modifiers
            double effectiveAttack = this.Attack + ApplyModifiers(StatType.Attack);

            return effectiveAttack;
        }
        public virtual double GetEffectiveAtkSpeed()
        {
            //combine base stat value and value of all its modifiers
            double effectiveAtkSpeed = this.AtkSpeed + ApplyModifiers(StatType.AtkSpeed);

            return effectiveAtkSpeed;
        }
        public virtual double GetEffectiveAccuracy()
        {
            //combine base stat value and value of all its modifiers
            double effectiveAccuracy = this.Accuracy + ApplyModifiers(StatType.Accuracy);

            return effectiveAccuracy;
        }
        public virtual double GetEffectiveCritical()
        {
            //combine base stat value and value of all its modifiers
            double effectiveCritical = this.Critical + ApplyModifiers(StatType.Critical);

            return effectiveCritical;
        }
        public virtual double GetEffectiveDefense()
        {
            //combine base stat value and value of all its modifiers
            double effectiveDefense = this.Defense + ApplyModifiers(StatType.Defense);

            return effectiveDefense;
        }
        public virtual double GetEffectiveEvasion()
        {
            //combine base stat value and value of all its modifiers
            double effectiveEvasion = this.Evasion + ApplyModifiers(StatType.Evasion);

            return effectiveEvasion;
        }
        public virtual double GetEffectiveMagicResistance()
        {
            //combine base stat value and value of all its modifiers
            double effectiveMagicResistance = this.MagicResistance + ApplyModifiers(StatType.MagicResistance);

            return effectiveMagicResistance;
        }

        /// <summary>
        /// Adds up values of all modifiers of specified type, and returns it as one combined int value
        /// </summary>
        /// <param name="statType"></param>
        /// <returns></returns>
        public int ApplyModifiers(StatType statType)
        {
            List<Modifier> modifiers;
            int modifiersSumValue = 0;

            //find all modifiers of specified type
            modifiers = this.Modifiers!.FindAll(mod => mod.Type == statType);

            //sum values of all matched modifiers
            if (modifiers.Count > 0)
            {
                modifiers.ForEach(mod => modifiersSumValue += mod.Value);
            }

            return modifiersSumValue;
        }
    }
}
