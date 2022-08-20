using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Characters
{
    public class CombatCharacter : Character
    {
        private const double CounterMax = 1000;

        protected double _EffectiveMaxHp;
        protected double _EffectiveMaxMp;
        protected double _MaxHp;
        protected double _MaxMp;
        protected double _Hp;
        protected double _Mp;

        //default constructor for json deserialization
        public CombatCharacter() : base()
        {
            Modifiers = new List<Modifier>();
            HpCounter = CounterMax;
            MpCounter = CounterMax;
        }
        public CombatCharacter(string[] descriptive, int[] combatStats, string[][] responses, int gold)
            : base(descriptive, responses, gold)
        {
            MaxHp = combatStats[0];
            MaxMp = combatStats[1];
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

            Hp = MaxHp;
            Mp = MaxMp;
            HpCounter = CounterMax;
            MpCounter = CounterMax;

            Modifiers = new List<Modifier>();
        }
        public enum StatType
        {
            MaxHp,
            MaxMp,
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

        //hp/mp counters for regeneration
        public double HpCounter { get; set; }
        public double MpCounter { get; set; }

        //health and mana statistics
        public virtual double Hp { get; set; }
        public virtual double Mp { get; set; }
        public virtual double EffectiveMaxHp { get; set; }
        public virtual double EffectiveMaxMp { get; set; }
        public virtual double MaxHp
        {
            get { return _MaxHp; }
            set
            {
                if (_MaxHp != value)
                {
                    _MaxHp = value;

                    //every time MaxHp changes, update the effective max hp value so hp bar maxhp value updates via data binding
                    EffectiveMaxHp = GetEffectiveMaxHp();
                }
            }
        }
        public virtual double MaxMp
        {
            get { return _MaxMp; }
            set
            {
                if (_MaxMp != value)
                {
                    _MaxMp = value;
                    EffectiveMaxMp = GetEffectiveMaxMp();
                }
            }
        }
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

        //method initializing hp/mp real values at the very first tick of the game clock
        public virtual void InitializeHpMp()
        {
            Hp = GetEffectiveMaxHp();
            Mp = GetEffectiveMaxMp();
        }

        //method doing actions every tick of the game clock
        public void HandleTick()
        {
            RegenerateHp();
            RegenerateMp();
            DecreaseModDuration();
        }

        //method decreasing duration time for all modifiers
        public void DecreaseModDuration()
        {
            int modAmount = Modifiers!.Count;
            Modifier currentMod = new Modifier();

            //for every mod that is temporary (Duration value != 0), decrease it's duration by one tick
            //and if duration equals 1 - remove the modificator
            for (int i = 0; i < modAmount; i++)
            {
                currentMod = Modifiers![i];
                if (currentMod.DurationInTicks > 1)
                {
                    currentMod.DurationInTicks -= 1;
                }
                else if (currentMod.DurationInTicks == 1)
                {
                    RemoveModifier(currentMod);
                }
            }
        }

        //method for adding modifier to character's list of modifiers
        public virtual void AddModifier(Modifier mod)
        {
            Modifiers!.Add(new Modifier(mod));
        }
        
        //method for removing modifiers
        public virtual void RemoveModifier(Modifier mod)
        {
            Modifiers!.Remove(mod);
        }

        //'getters' for effective character's statistics - calculated from base statistics and modifiers

        public virtual double GetEffectiveMaxHp()
        {
            double effectiveMaxHp = this.MaxHp + ApplyModifiers(StatType.MaxHp);
            return effectiveMaxHp;
        }
        public virtual double GetEffectiveMaxMp()
        {
            double effectiveMaxMp = this.MaxMp + ApplyModifiers(StatType.MaxMp);
            return effectiveMaxMp;
        }
        public virtual double GetEffectiveHpRegen()
        {
            double effectiveHpRegen = this.HpRegen + ApplyModifiers(StatType.HpRegen);
            return effectiveHpRegen;
        }
        public virtual double GetEffectiveMpRegen()
        {
            double effectiveMpRegen = this.MpRegen + ApplyModifiers(StatType.MpRegen);
            return effectiveMpRegen;
        }
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
        private int ApplyModifiers(StatType statType)
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
        
        //method regenerating hp
        private void RegenerateHp()
        {
            //if hp is lesser than max
            if (Hp < GetEffectiveMaxHp() && Hp > 0)
            {

                //if HpCounter is lesser or equal to zero, regenerate 1 hp and reset the counter,
                //otherwise decrease the counter with value of effective HpRegen
                if (HpCounter <= 0)
                {
                    Hp++;
                    HpCounter = CounterMax;
                }
                else
                {
                    HpCounter -= GetEffectiveHpRegen();
                }
            }
        }

        //method regenerating mp
        private void RegenerateMp()
        {
            //if mp is lesser than max
            if (Mp < GetEffectiveMaxMp() && Mp > 0)
            {

                //if MpCounter is lesser or equal to zero, regenerate 1 mp and reset the counter,
                //otherwise decrease the counter with value of effective HpRegen
                if (MpCounter <= 0)
                {
                    Mp++;
                    MpCounter = CounterMax;
                }
                else
                {
                    MpCounter -= GetEffectiveMpRegen();
                }
            }
        }
    }
}
