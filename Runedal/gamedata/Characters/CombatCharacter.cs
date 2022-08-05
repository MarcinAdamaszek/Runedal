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
        public CombatCharacter(string[] descriptive, int[] combatStats, string[][] responses)
            : base(descriptive, responses)
        {
            Hp = combatStats[0];
            Mp = combatStats[1];
            Speed = combatStats[2];
            Attack = combatStats[3];
            AtkSpeed = combatStats[4];
            Accuracy = combatStats[5];
            Critical = combatStats[6];
            Defense = combatStats[7];
            Evasion = combatStats[8];
            MagicResistance = combatStats[9];
            Gold = combatStats[10];
            Modifiers = new List<Modifier>();
        }
        public enum StatType
        {
            Hp,
            Mp,
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

        //health and mana points
        public double Hp { get; private set; }
        public double HpAmount { get; private set; }
        public double Mp { get; private set; }
        public double MpAmount { get; private set; }

        //base combat statistics
        public double Speed { get; private set; }
        public double Attack { get; private set; }
        public double AtkSpeed { get; private set; }
        public double Accuracy { get; private set; }
        public double Defense { get; private set; }
        public double Evasion { get; private set; }
        public double MagicResistance { get; private set; }
        public double Critical { get; private set; }


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
