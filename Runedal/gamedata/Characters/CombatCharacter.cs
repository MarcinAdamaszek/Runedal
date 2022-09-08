using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Runedal.GameData.Effects;

namespace Runedal.GameData.Characters
{
    public class CombatCharacter : Character
    {
        protected const double CounterMax = 1000;

        protected double _EffectiveMaxHp;
        protected double _EffectiveMaxMp;
        protected double _MaxHp;
        protected double _MaxMp;
        protected double _Hp;
        protected double _Mp;
        protected double _ActionCounter;

        //default constructor for json deserialization
        public CombatCharacter() : base()
        {
            Modifiers = new List<Modifier>();
            Opponents = new List<CombatCharacter>();
            RememberedSpells = new List<Spell>();
            InteractsWith = new Character("placeholder");

            HpCounter = CounterMax;
            MpCounter = CounterMax;
        }

        //constructor for placeholder
        public CombatCharacter(string placeholder) : base(placeholder)
        {
            Modifiers = new List<Modifier>();
            Opponents = new List<CombatCharacter>();
            RememberedSpells = new List<Spell>();

            HpCounter = CounterMax;
            MpCounter = CounterMax;
        }
        public CombatCharacter(string[] descriptive, int[] combatStats, string[][] responses, int gold)
            : base(descriptive, responses, gold)
        {
            Modifiers = new List<Modifier>();
            Opponents = new List<CombatCharacter>();
            RememberedSpells = new List<Spell>();
            InteractsWith = new Character("placeholder");

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
        }
        public CombatCharacter(CombatCharacter com) : base(com)
        {
            Modifiers = new List<Modifier>();
            Opponents = new List<CombatCharacter>();
            RememberedSpells = new List<Spell>();
            InteractsWith = new Character("placeholder");

            //create deep copy of modifiers collection
            Modifiers = com.Modifiers!.ConvertAll(mod => new Modifier(mod));

            Level = com.Level;
            MaxHp = com.MaxHp;
            MaxMp = com.MaxMp;
            HpRegen = com.HpRegen;
            MpRegen = com.MpRegen;
            Speed = com.Speed;
            Attack = com.Attack;
            AtkSpeed = com.AtkSpeed;
            Accuracy = com.Accuracy;
            Critical = com.Critical;
            Defense = com.Defense;
            Evasion = com.Evasion;
            MagicResistance = com.MagicResistance;
            Gold = com.Gold;

            Hp = MaxHp;
            Mp = MaxMp;
            HpCounter = CounterMax;
            MpCounter = CounterMax;
        }
        public enum State
        {
            Idle,
            Talk,
            Trade,
            Combat
        }

        //counter for actions
        public virtual double ActionCounter { get; set; }

        //hp/mp counters for regeneration
        public double HpCounter { get; set; }
        public double MpCounter { get; set; }

        //character's level
        public int Level { get; set; }

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

        public State CurrentState { get; set; }

        //character with whom player currently interacts
        public Character? InteractsWith { get; set; }

        //list of combat characters involved in fight with this one
        public List<CombatCharacter> Opponents { get; set; }

        //list of modifiers currently affecting character
        public List<Modifier>? Modifiers { get; set; }

        //spells remembered by character
        public List<Spell> RememberedSpells { get; set; }
        public int MaxSpellsRemembered { get; set; }


        public double Heal(double healAmount)
        {
            double hpAfterHeal = Hp + healAmount;
            double realAmountHealed;

            if (hpAfterHeal > GetEffectiveMaxHp())
            {
                realAmountHealed = GetEffectiveMaxHp() - Hp;
            }
            else
            {
                realAmountHealed = healAmount;
            }

            Hp += realAmountHealed;
            return realAmountHealed;
        }

        /// <summary>
        /// method dealing dmg to character. If the damage was lethal,
        /// return true, otherwise - false
        /// </summary>
        /// <param name="dmg"></param>
        /// <returns></returns>
        public bool DealDamage(double dmg)
        {

            double hpAfterDmg = Hp - dmg;

            if (hpAfterDmg < 0)
            {
                hpAfterDmg = 0;
            }

            Hp = hpAfterDmg;

            if (hpAfterDmg == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// method decreasing character's Mp by value of cost parameter.
        /// If mana cost exceeds actual mana pool, spend all remaining mana
        /// and return the value of mana spent
        /// </summary>
        /// <param name="cost"></param>
        /// <returns></returns>
        public double SpendMana(double cost)
        {
            double manaAfter = Mp - cost;
            double manaSpent;

            if (manaAfter < 0)
            {
                manaAfter = 0;
            }

            manaSpent = Mp - manaAfter;
            Mp = manaAfter;

            return manaSpent;
        }

        //method adding attack delay to action counter
        public virtual void PerformAttack()
        {
            double attackDelay = 6000 / GetEffectiveAtkSpeed();
            ActionCounter += attackDelay;
        }

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
            DecreaseActionCounter();
        }

        //method decreasing duration time for all modifiers
        public void DecreaseModDuration()
        {
            Modifier currentMod = new Modifier();
            List<Modifier> modsToRemove = new List<Modifier>();

            //for every mod that is temporary (Duration value != 0), decrease it's duration by one tick
            //and if duration equals 1 - remove the modificator
            for (int i = 0; i < Modifiers!.Count; i++)
            {
                currentMod = Modifiers![i];
                if (currentMod.DurationInTicks > 1)
                {
                    currentMod.DurationInTicks--;
                }
                else if (currentMod.DurationInTicks == 1)
                {
                    modsToRemove.Add(currentMod);
                }
            }
            modsToRemove.ForEach(mod =>
            {
                RemoveModifier(mod);
            });
        }

        //method descreasing attack counter
        public void DecreaseActionCounter()
        {
            bool isCharacterStunned = Modifiers!.Exists(mod => mod.Type == Modifier.ModType.Stun);
            
            if (isCharacterStunned && ActionCounter == 0)
            {
                ActionCounter++;
            }

            //if character's action is on cooldown and character isn't stunned - 
            //reduce action cooldown
            if (ActionCounter > 0 && !isCharacterStunned)
            {
                ActionCounter -= 1;
            }
        }

        /// <summary>
        /// method adding spell to character's remembered spells
        /// returns old spell which was replaced by the new one
        /// </summary>
        /// <param name="spell"></param>
        /// <returns></returns>
        public Spell AddSpell(Spell spell)
        {
            Spell removedSpell = new Spell();

            if (RememberedSpells.Count == MaxSpellsRemembered)
            {
                
                removedSpell = RememberedSpells[MaxSpellsRemembered - 1];
                RememberedSpells.Remove(removedSpell);
            }
            else
            {
                removedSpell = new Spell("placeholder");
            }

            RememberedSpells.Insert(0, new Spell(spell));
            return removedSpell;
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

        /// <summary>
        /// method adding combat-character to list of character's opponents
        /// if it's the very first opponent (meaning player wasn't in combat
        /// state) return true - otherwise return false
        /// </summary>
        /// <param name="opponent"></param>
        /// <returns></returns>
        public bool AddOpponent(CombatCharacter opponent)
        {
            Opponents.Add(opponent);

            if (Opponents.Count == 1)
            {
                CurrentState = State.Combat;
                return true;
            }

            return false;
        }

        /// <summary>
        /// method removing combat-character from character's opponents list.
        /// if removed opponent was the last one, returns true. Otherwise -
        /// returns false.
        /// </summary>
        /// <param name="opponent"></param>
        /// <returns></returns>
        public bool RemoveOpponent(CombatCharacter opponent)
        {
            Opponents.Remove(opponent);

            if (Opponents.Count == 0)
            {
                CurrentState = State.Idle;
                return true;
            }

            return false;
        }

        //'getters' for effective character's statistics - calculated from base statistics and modifiers

        public virtual double GetEffectiveMaxHp()
        {
            double effectiveMaxHp = this.MaxHp + ApplyModifiers(Modifier.ModType.MaxHp);
            return effectiveMaxHp;
        }
        public virtual double GetEffectiveMaxMp()
        {
            double effectiveMaxMp = this.MaxMp + ApplyModifiers(Modifier.ModType.MaxMp);
            return effectiveMaxMp;
        }
        public virtual double GetEffectiveHpRegen()
        {
            double effectiveHpRegen = this.HpRegen + ApplyModifiers(Modifier.ModType.HpRegen);
            return effectiveHpRegen;
        }
        public virtual double GetEffectiveMpRegen()
        {
            double effectiveMpRegen = this.MpRegen + ApplyModifiers(Modifier.ModType.MpRegen);
            return effectiveMpRegen;
        }
        public virtual double GetEffectiveSpeed()
        {
            //combine base stat value and value of all its modifiers
            double effectiveSpeed = this.Speed + ApplyModifiers(Modifier.ModType.Speed);

            return effectiveSpeed;
        }
        public virtual double GetEffectiveAttack()
        {
            //combine base stat value and value of all its modifiers
            double effectiveAttack = this.Attack + ApplyModifiers(Modifier.ModType.Attack);

            return effectiveAttack;
        }
        public virtual double GetEffectiveAtkSpeed()
        {
            //combine base stat value and value of all its modifiers
            double effectiveAtkSpeed = this.AtkSpeed + ApplyModifiers(Modifier.ModType.AtkSpeed);

            return effectiveAtkSpeed;
        }
        public virtual double GetEffectiveAccuracy()
        {
            //combine base stat value and value of all its modifiers
            double effectiveAccuracy = this.Accuracy + ApplyModifiers(Modifier.ModType.Accuracy);

            return effectiveAccuracy;
        }
        public virtual double GetEffectiveCritical()
        {
            //combine base stat value and value of all its modifiers
            double effectiveCritical = this.Critical + ApplyModifiers(Modifier.ModType.Critical);

            return effectiveCritical;
        }
        public virtual double GetEffectiveDefense()
        {
            //combine base stat value and value of all its modifiers
            double effectiveDefense = this.Defense + ApplyModifiers(Modifier.ModType.Defense);

            return effectiveDefense;
        }
        public virtual double GetEffectiveEvasion()
        {
            //combine base stat value and value of all its modifiers
            double effectiveEvasion = this.Evasion + ApplyModifiers(Modifier.ModType.Evasion);

            return effectiveEvasion;
        }
        public virtual double GetEffectiveMagicResistance()
        {
            //combine base stat value and value of all its modifiers
            double effectiveMagicResistance = this.MagicResistance + ApplyModifiers(Modifier.ModType.MagicResistance);

            return effectiveMagicResistance;
        }

        /// <summary>
        /// Adds up values of all modifiers of specified type, and returns it as one combined int value
        /// </summary>
        /// <param name="statType"></param>
        /// <returns></returns>
        protected virtual int ApplyModifiers(Modifier.ModType statType)
        {
            List<Modifier> modifiers;
            int modifiersSumValue = 0;
            int modifiersPercentValue = 0;

            //find all modifiers of specified type
            modifiers = this.Modifiers!.FindAll(mod => mod.Type == statType);

            //sum values of all matched modifiers
            if (modifiers.Count > 0)
            {
                modifiers.ForEach(mod =>
                {
                    if (mod.IsPercentage)
                    {
                        modifiersPercentValue += mod.Value;
                    }
                    else
                    {
                        modifiersSumValue += mod.Value;
                    }
                });

                double baseStatValue = 1;

                switch (statType)
                {
                    case Modifier.ModType.MaxHp:
                        baseStatValue = MaxHp;
                        break;
                    case Modifier.ModType.MaxMp:
                        baseStatValue = MaxMp;
                        break;
                    case Modifier.ModType.HpRegen:
                        baseStatValue = HpRegen;
                        break;
                    case Modifier.ModType.Speed:
                        baseStatValue = Speed;
                        break;
                    case Modifier.ModType.Attack:
                        baseStatValue = Attack;
                        break;
                    case Modifier.ModType.AtkSpeed:
                        baseStatValue = AtkSpeed;
                        break;
                    case Modifier.ModType.Accuracy:
                        baseStatValue = Accuracy;
                        break;
                    case Modifier.ModType.Defense:
                        baseStatValue = Defense;
                        break;
                    case Modifier.ModType.Evasion:
                        baseStatValue = Evasion;
                        break;
                    case Modifier.ModType.MagicResistance:
                        baseStatValue = Evasion;
                        break;
                    case Modifier.ModType.Critical:
                        baseStatValue = Evasion;
                        break;
                }

                modifiersSumValue += Convert.ToInt32((baseStatValue + modifiersSumValue) * (0.01 * modifiersPercentValue));
            }

            return modifiersSumValue;
        }
        
        //method regenerating hp
        private void RegenerateHp()
        {

            //if hp is lesser than max
            if (Hp < GetEffectiveMaxHp() && Hp > 0)
            {

                //while hpcounter is lesser or equal to zero, regenerate hp
                //and add 1000 to it until its above 0
                while (HpCounter <= 0)
                {

                    //prevent exceeding max hp with enormous hpregen values
                    if (Hp < GetEffectiveMaxHp() && Hp > 0)
                    {
                        Hp++;
                    }
                    HpCounter += CounterMax;
                }
                HpCounter -= GetEffectiveHpRegen();
            }
        }

        //method regenerating mp
        private void RegenerateMp()
        {
            //if mp is lesser than max
            if (Mp < GetEffectiveMaxMp())
            {

                //while mpcounter is lesser or equal to zero, regenerate mp
                //and add 1000 to it until its above 0
                while (MpCounter <= 0)
                {

                    //prevent exceeding max mp with enormous mpregen values
                    if (Mp < GetEffectiveMaxMp())
                    {
                        Mp++;
                    }
                    MpCounter += CounterMax;
                }
                MpCounter -= GetEffectiveMpRegen();
            }
        }
    }
}
