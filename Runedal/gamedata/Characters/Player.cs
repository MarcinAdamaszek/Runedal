
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using Runedal.GameData.Items;
using System.Windows.Media.Effects;
using static Runedal.GameData.Characters.CombatCharacter;
using Runedal.GameData.Effects;

namespace Runedal.GameData.Characters
{
    public class Player : CombatCharacter, INotifyPropertyChanged
    {
        //values of multipliers for calculating attributes modifiers
        private const double MaxHpStrMultiplier = 10;
        private const double MaxMpIntMultiplier = 10;
        private const double HpRegenStrMultiplier = 5;
        private const double MpRegenIntMultiplier = 5;
        private const double SpeedAgiMultiplier = 0.3;
        private const double AttackStrMultiplier = 2.0;
        private const double AtkSpeedAgiMultiplier = 1.0;
        private const double AccuracyAgiMultiplier = 0.5;
        private const double CriticalAgiMultiplier = 0.5;
        private const double EvasionAgiMultiplier = 0.3;
        private const double MagicResistanceIntMultiplier = 1.0;

        //values of other multipliers
        private const double SpeedWeightMultiplier = -0.02;

        //fields required for proper hp/mp display in gui
        private double _HpPercentage;
        private double _MpPercentage;
        private int _Strength;
        private int _Intelligence;

        //default constructor for json deserialization
        public Player() : base()
        {
            //initialize items worn with placeholders
            Torso = new Armor(Armor.ArmorType.Torso, "placeholder");
            Pants = new Armor(Armor.ArmorType.Pants, "placeholder");
            Helmet = new Armor(Armor.ArmorType.Helmet, "placeholder");
            Gloves = new Armor(Armor.ArmorType.Gloves, "placeholder");
            Shoes = new Armor(Armor.ArmorType.Shoes, "placeholder");
            Weapon = new Weapon("placeholder");

            Effects = new List<EffectOnPlayer>();

            Level = 1;
            Experience = 0;
            NextLvlExpCap = 500;
        }
        public Player(string[] descriptive, int[] combatStats, int[] attributeStats, string[][] responses, int gold)
            : base(descriptive, combatStats, responses, gold)
        {
            Strength = attributeStats[0];
            Intelligence = attributeStats[1];
            Agility = attributeStats[2];
            Level = 1;
            Experience = 0;
            NextLvlExpCap = 500;

            //initialize items worn with placeholders
            Torso = new Armor(Armor.ArmorType.Torso, "placeholder");
            Pants = new Armor(Armor.ArmorType.Pants, "placeholder");
            Helmet = new Armor(Armor.ArmorType.Helmet, "placeholder");
            Gloves = new Armor(Armor.ArmorType.Gloves, "placeholder");
            Shoes = new Armor(Armor.ArmorType.Shoes, "placeholder");
            Weapon = new Weapon("placeholder");

            Effects = new List<EffectOnPlayer>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        //name of player's starting location
        public string? Start { get; set; }

        //counter for actions
        public override double ActionCounter
        {
            get { return _ActionCounter; }
            set
            {
                if (_ActionCounter != value)
                {
                    _ActionCounter = value;
                    NotifyPropertyChanged("ActionCounter");
                }
            }

        }

        //real hp/mp values
        public override double Hp
        {
            get { return _Hp; }
            set
            {
                if (_Hp != value)
                {
                    _Hp = value;
                    NotifyPropertyChanged("Hp");
                    SetHpPercentage();
                }
            }
        }
        public override double Mp
        {
            get { return _Mp; }
            set
            {
                if (_Mp != value)
                {
                    _Mp = value;
                    NotifyPropertyChanged("Mp");
                    SetMpPercentage();
                }
            }
        }

        //hp/mp percentages for proper gui hp/mp bars display via data binding
        public double HpPercentage
        {
            get { return _HpPercentage; }
            set
            {
                if (_HpPercentage != value)
                {
                    _HpPercentage = value;
                    NotifyPropertyChanged("HpPercentage");
                }
            }
        }
        public double MpPercentage
        {
            get { return _MpPercentage; }
            set
            {
                if (_MpPercentage != value)
                {
                    _MpPercentage = value;
                    NotifyPropertyChanged("MpPercentage");
                }
            }
        }

        //effective max hp/mp for data binding
        public override double EffectiveMaxHp
        {
            get { return _EffectiveMaxHp; }
            set
            {
                if (_EffectiveMaxHp != value)
                {
                    _EffectiveMaxHp = value;
                    NotifyPropertyChanged("EffectiveMaxHp");
                    SetHpPercentage();

                    //if max hp drops below real Hp value, equalize the Hp
                    if (EffectiveMaxHp < Hp)
                    {
                        Hp = EffectiveMaxHp;
                    }
                }
            }
        }
        public override double EffectiveMaxMp 
        {
            get { return _EffectiveMaxMp; }
            set
            {
                if (_EffectiveMaxMp != value)
                {
                    _EffectiveMaxMp = value;
                    NotifyPropertyChanged("EffectiveMaxMp");
                    SetMpPercentage();

                    //if max mp drops below real Mp value, equalize the Mp
                    if (EffectiveMaxHp < Hp)
                    {
                        Hp = EffectiveMaxHp;
                    }
                }
            }
        }

        //player's amount of experience
        public ulong Experience { get; set; }

        //amount of experience required to lvl-up
        public ulong NextLvlExpCap { get; set; }
        
        //player's available attribute points gained every lvl-up
        public int AttributePoints { get; set; }
        
        //player character attributes influencing other statistics
        public int Strength
        {
            get { return _Strength; }
            set
            {
                if (_Strength != value)
                {
                    _Strength = value;
                    EffectiveMaxHp = GetEffectiveMaxHp();
                }
            }
        }
        public int Intelligence
        {
            get { return _Intelligence; }
            set
            {
                if (_Intelligence != value)
                {
                    _Intelligence = value;
                    EffectiveMaxMp = GetEffectiveMaxMp();
                    MaxSpellsRemembered = Convert.ToInt32(Math.Sqrt(Intelligence) / 3);
                    if (MaxSpellsRemembered < 1)
                    {
                        MaxSpellsRemembered = 1;
                    }
                }
            }
        }
        public int Agility { get; set; }

        //items worn by player
        public Armor? Torso { get; set; }
        public Armor? Pants { get; set; }
        public Armor? Helmet { get; set; }
        public Armor? Gloves { get; set; }
        public Armor? Shoes { get; set; }
        public Weapon? Weapon { get; set; }

        //effects currently affecting player
        public List<EffectOnPlayer>? Effects { get; set; }

        /// <summary>
        /// method for increasing experience pool. If player gained lvl-up
        /// returns true, otherwise returns false.
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        public bool GainExperience(ulong exp)
        {
            ulong expAfterGain = Experience + exp;
            
            if (expAfterGain >= NextLvlExpCap)
            {
                while (expAfterGain >= NextLvlExpCap)
                {
                    expAfterGain -= NextLvlExpCap;
                    LevelUp();
                }
                Experience = expAfterGain;
                return true;
            }
            else
            {
                Experience = expAfterGain;
                return false;
            }
        }

        //method incrementing player level by one
        public void LevelUp()
        {
            Level++;
            NextLvlExpCap = Convert.ToUInt64(Level * 500);
            Strength += 1;
            Agility += 1;
            Intelligence += 1;
        }

        //method adding attribute points to player's pool
        public void AddAttributePoints(int quantity)
        {
            AttributePoints += quantity;
        }

        //method initializing Hp/Mp pools
        public override void InitializeHpMp()
        {
            Hp = GetEffectiveMaxHp();
            Mp = GetEffectiveMaxMp();
        }

        public override void PerformAttack()
        {
            double attackDelay = 6000 / GetEffectiveAtkSpeed();
            ActionCounter += attackDelay;
        }

        //method for wearing weapon-type items
        public void WearWeapon(Weapon weapon)
        {
            Weapon weaponToWear = new Weapon(weapon);

            //first, remove item from player's inventory
            RemoveItem(weapon.Name!);

            //then apply all modifiers and mark them with parent name
            weapon.Modifiers!.ForEach(mod =>
            {
                mod.Parent = weapon.Name!;
                AddModifier(mod);
            });

            //fill weapon slot
            Weapon = weaponToWear;
        }

        //method for wearing wearable armor-type items
        public void WearArmor(Armor armor)
        {
            Armor armorToWear = new Armor(armor);

            //first, remove item from player's inventory
            RemoveItem(armor.Name!);

            //then apply all modifiers and mark them with parent name
            armor.Modifiers!.ForEach(mod =>
            {
                mod.Parent = armor.Name!;
                AddModifier(mod);
            });

            //finaly fill armor slot with item depending on type
            switch (armor.Type)
            {
                case (Armor.ArmorType.Helmet):
                    Helmet = armorToWear;
                    break;
                case (Armor.ArmorType.Torso):
                    Torso = armorToWear;
                    break;
                case (Armor.ArmorType.Pants):
                    Pants = armorToWear;
                    break;
                case (Armor.ArmorType.Gloves):
                    Gloves = armorToWear;
                    break;
                case (Armor.ArmorType.Shoes):
                    Shoes = armorToWear;
                    break;
            }
        }

        //method for taking off weapons
        public string TakeOffWeapon()
        {
            Weapon weaponWorn = new Weapon();
            List<Modifier> modsToRemove = new List<Modifier>();
            int i;

            weaponWorn = Weapon!;
            Weapon = new Weapon("placeholder");

            //first save all modifiers with the same parent to temporary list,
            //then remove them from original list
            //it has to be done this way, to prevent modifiers.count dropping
            //and thus skipping last modifiers on the list
            for (i = 0; i < Modifiers!.Count; i++)
            {
                if (Modifiers[i].Parent.ToLower() == weaponWorn.Name!.ToLower())
                {
                    modsToRemove.Add(Modifiers[i]);
                }
            }
            foreach (var mod in modsToRemove)
            {
                RemoveModifier(mod);
            }

            //put item into players inventory
            if (weaponWorn.Name!.ToLower() != "placeholder")
            {
                AddItem(weaponWorn);
            }

            return weaponWorn.Name!;
        }

        //method for taking off wearable armor-type items
        public string TakeOffArmor(Armor.ArmorType armorType)
        {
            Armor armorWorn = new Armor();
            List<Modifier> modsToRemove = new List<Modifier>();
            int i;

            //depending on armor type, add armor-item to player's inventory
            //and fill armor slot with placeholder
            switch (armorType)
            {
                case (Armor.ArmorType.Helmet):
                    armorWorn = Helmet!;
                    Helmet = new Armor(Armor.ArmorType.Helmet, "placeholder");
                    break;
                case (Armor.ArmorType.Torso):
                    armorWorn = Torso!;
                    Torso = new Armor(Armor.ArmorType.Torso, "placeholder");
                    break;
                case (Armor.ArmorType.Pants):
                    armorWorn = Pants!;
                    Pants = new Armor(Armor.ArmorType.Pants, "placeholder");
                    break;
                case (Armor.ArmorType.Gloves):
                    armorWorn = Gloves!;
                    Gloves = new Armor(Armor.ArmorType.Gloves, "placeholder");
                    break;
                case (Armor.ArmorType.Shoes):
                    armorWorn = Shoes!;
                    Shoes = new Armor(Armor.ArmorType.Shoes, "placeholder");
                    break;
            }

            //first save all modifiers with the same parent to temporary list,
            //then remove them from original list
            //it has to be done this way, to prevent modifiers.count dropping
            //and thus skipping last modifiers on the list
            for (i = 0; i < Modifiers!.Count; i++)
            {
                if (Modifiers[i].Parent.ToLower() == armorWorn.Name!.ToLower())
                {
                    modsToRemove.Add(Modifiers[i]);
                }
            }
            foreach(var mod in modsToRemove)
            {
                RemoveModifier(mod);
            }

            //put item into players inventory
            if (armorWorn.Name!.ToLower() != "placeholder")
            {
                AddItem(armorWorn);
            }

            return armorWorn.Name!;
        }

        //overriden methods for adding/removing modifiers, assuring effective maxhp/mp values are updated
        public override void AddModifier(Modifier mod)
        {
            base.AddModifier(mod);
            if (mod.Type == Modifier.ModType.MaxHp || mod.Type == Modifier.ModType.Strength)
            {
                EffectiveMaxHp = GetEffectiveMaxHp();
            }
            else if (mod.Type == Modifier.ModType.MaxMp || mod.Type == Modifier.ModType.Intelligence)
            {
                EffectiveMaxMp = GetEffectiveMaxMp();
            }
        }
        public override void RemoveModifier(Modifier mod)
        {
            base.RemoveModifier(mod);
            if (mod.Type == Modifier.ModType.MaxHp || mod.Type == Modifier.ModType.Strength)
            {
                EffectiveMaxHp = GetEffectiveMaxHp();
            }
            else if (mod.Type == Modifier.ModType.MaxMp || mod.Type == Modifier.ModType.Intelligence)
            {
                EffectiveMaxMp = GetEffectiveMaxMp();
            }
        }

        //methods for setting effective max hp/mp
        public override double GetEffectiveMaxHp()
        {
            double effectiveMaxHp = MaxHp + ApplyModifiers(Modifier.ModType.MaxHp);
            double strengthModifier = MaxHpStrMultiplier * GetEffectiveStrength();
            effectiveMaxHp += strengthModifier;
            if (effectiveMaxHp < 1)
            {
                effectiveMaxHp = 1;
            }
            return effectiveMaxHp;
        }
        public override double GetEffectiveMaxMp()
        {
            double effectiveMaxMp = this.MaxMp + ApplyModifiers(Modifier.ModType.MaxMp);
            double intelligenceModifier = MaxMpIntMultiplier * GetEffectiveIntelligence();
            effectiveMaxMp += intelligenceModifier;
            if (effectiveMaxMp < 1)
            {
                effectiveMaxMp = 1;
            }
            return effectiveMaxMp;
        }

        //methods for getting effective statistics (after applying all modifiers)
        public int GetEffectiveStrength()
        {
            int effectiveStrength = Strength;
            effectiveStrength += ApplyModifiers(Modifier.ModType.Strength);
            if (effectiveStrength < 1)
            {
                effectiveStrength = 1;
            }
            return effectiveStrength;
        }
        public int GetEffectiveAgility()
        {
            int effectiveAgility = Agility;
            effectiveAgility += ApplyModifiers(Modifier.ModType.Agility);
            if (effectiveAgility < 1)
            {
                effectiveAgility = 1;
            }
            return effectiveAgility;
        }
        public int GetEffectiveIntelligence()
        {
            int effectiveIntelligence = Intelligence;
            effectiveIntelligence += ApplyModifiers(Modifier.ModType.Intelligence);
            if (effectiveIntelligence < 1)
            {
                effectiveIntelligence = 1;
            }
            return effectiveIntelligence;
        }
        public override double GetEffectiveHpRegen()
        {
            double effectiveHpRegen = this.HpRegen + ApplyModifiers(Modifier.ModType.HpRegen);
            double strengthModifier = HpRegenStrMultiplier * GetEffectiveStrength();
            effectiveHpRegen += strengthModifier;
            if (effectiveHpRegen < 1)
            {
                effectiveHpRegen = 1;
            }
            return effectiveHpRegen;
        }
        public override double GetEffectiveMpRegen()
        {
            double effectiveMpRegen = this.MpRegen + ApplyModifiers(Modifier.ModType.MpRegen);
            double intelligenceModifier = MpRegenIntMultiplier * GetEffectiveIntelligence();
            effectiveMpRegen += intelligenceModifier;
            if (effectiveMpRegen < 1)
            {
                effectiveMpRegen = 1;
            }
            return effectiveMpRegen;
        }
        public override double GetEffectiveSpeed()
        {
            //effective stat with external modifiers
            double effectiveSpeed = this.Speed + ApplyModifiers(Modifier.ModType.Speed);

            //attribute modifier
            double agilityModifier = GetEffectiveAgility() * SpeedAgiMultiplier;

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
            double effectiveAttack = this.Attack + ApplyModifiers(Modifier.ModType.Attack);

            //attribute modifier
            double strengthModifier = GetEffectiveStrength() * AttackStrMultiplier;

            //weapon modifier
            double weaponModifier = Weapon!.Attack;

            //add modifiers to effective stat
            effectiveAttack += strengthModifier += weaponModifier;
            if (effectiveAttack < 1)
            {
                effectiveAttack = 1;
            }

            return effectiveAttack;
        }
        public override double GetEffectiveAtkSpeed()
        {
            double effectiveAtkSpeed = this.AtkSpeed + ApplyModifiers(Modifier.ModType.AtkSpeed);
            double agilityModifier = GetEffectiveAgility() * AtkSpeedAgiMultiplier;
            effectiveAtkSpeed += agilityModifier;
            if (effectiveAtkSpeed < 1)
            {
                effectiveAtkSpeed = 1;
            }
            return effectiveAtkSpeed;
        }
        public override double GetEffectiveAccuracy()
        {
            double effectiveAccuracy = this.Accuracy + ApplyModifiers(Modifier.ModType.Accuracy);
            double agilityModifier = GetEffectiveAgility() * AccuracyAgiMultiplier;
            effectiveAccuracy += agilityModifier;
            if (effectiveAccuracy < 1)
            {
                effectiveAccuracy = 1;
            }
            return effectiveAccuracy;
        }
        public override double GetEffectiveCritical()
        {
            double effectiveCritical = this.Critical + ApplyModifiers(Modifier.ModType.Critical);
            double agilityModifier = GetEffectiveAgility() * CriticalAgiMultiplier;
            effectiveCritical += agilityModifier;
            if (effectiveCritical < 1)
            {
                effectiveCritical = 1;
            }
            return effectiveCritical;
        }
        public override double GetEffectiveDefense()
        {
            double effectiveDefense = this.Defense + ApplyModifiers(Modifier.ModType.Defense);
            double wornArmorModifier = Helmet!.Defense + Torso!.Defense + Pants!.Defense + Gloves!.Defense + Shoes!.Defense;
            effectiveDefense += wornArmorModifier;
            if (effectiveDefense < 1)
            {
                effectiveDefense = 1;
            }
            return effectiveDefense;
        }
        public override double GetEffectiveEvasion()
        {
            double effectiveEvasion = this.Evasion + ApplyModifiers(Modifier.ModType.Evasion);
            double agilityModifier = GetEffectiveAgility() * EvasionAgiMultiplier;
            effectiveEvasion += agilityModifier;
            if (effectiveEvasion < 1)
            {
                effectiveEvasion = 1;
            }
            return effectiveEvasion;
        }
        public override double GetEffectiveMagicResistance()
        {
            double effectiveMagicResistance = this.MagicResistance + ApplyModifiers(Modifier.ModType.MagicResistance);
            double intelligenceModifier = GetEffectiveIntelligence() * MagicResistanceIntMultiplier;
            effectiveMagicResistance += intelligenceModifier;
            if (effectiveMagicResistance < 1)
            {
                effectiveMagicResistance = 1;
            }
            return effectiveMagicResistance;
        }

        protected override int ApplyModifiers(Modifier.ModType statType)
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

                //get base stat value combined with static-modifiers values and
                //add percentage value of multiplying it by percentage modifiers sum value
                switch (statType)
                {
                    case Modifier.ModType.Strength:
                        baseStatValue = Strength + modifiersSumValue;
                        break;
                    case Modifier.ModType.Agility:
                        baseStatValue = Agility + modifiersSumValue;
                        break;
                    case Modifier.ModType.Intelligence:
                        baseStatValue = Intelligence + modifiersSumValue;
                        break;
                    case Modifier.ModType.MaxHp:
                        baseStatValue = MaxHp + modifiersSumValue + MaxHpStrMultiplier * GetEffectiveStrength();
                        break;
                    case Modifier.ModType.MaxMp:
                        baseStatValue = MaxMp + modifiersSumValue + MaxMpIntMultiplier * GetEffectiveIntelligence();
                        break;
                    case Modifier.ModType.HpRegen:
                        baseStatValue = HpRegen + modifiersSumValue + HpRegenStrMultiplier * GetEffectiveStrength();
                        break;
                    case Modifier.ModType.Speed:
                        baseStatValue = Speed + modifiersSumValue + SpeedAgiMultiplier * GetEffectiveAgility();
                        break;
                    case Modifier.ModType.Attack:
                        baseStatValue = Attack + modifiersSumValue + AttackStrMultiplier * GetEffectiveStrength();
                        break;
                    case Modifier.ModType.AtkSpeed:
                        baseStatValue = AtkSpeed + modifiersSumValue + AtkSpeedAgiMultiplier * GetEffectiveAgility();
                        break;
                    case Modifier.ModType.Accuracy:
                        baseStatValue = Accuracy + modifiersSumValue + AccuracyAgiMultiplier * GetEffectiveAgility();
                        break;
                    case Modifier.ModType.Defense:
                        baseStatValue = Defense + modifiersSumValue;
                        break;
                    case Modifier.ModType.Evasion:
                        baseStatValue = Evasion + modifiersSumValue + EvasionAgiMultiplier * GetEffectiveAgility();
                        break;
                    case Modifier.ModType.MagicResistance:
                        baseStatValue = MagicResistance + modifiersSumValue + MagicResistanceIntMultiplier * GetEffectiveIntelligence();
                        break;
                    case Modifier.ModType.Critical:
                        baseStatValue = Critical + modifiersSumValue + CriticalAgiMultiplier * GetEffectiveAgility();
                        break;
                }

                modifiersSumValue += Convert.ToInt32(baseStatValue * (0.01 * modifiersPercentValue));
            }

            return modifiersSumValue;
        }

        //methods for setting hp/mp percentages
        private void SetHpPercentage()
        {
            HpPercentage = (Hp / EffectiveMaxHp) * 100;
        }
        private void SetMpPercentage()
        {
            MpPercentage = (Mp / EffectiveMaxMp) * 100;
        }

        //method returning sum weight of all items carried by player
        public int GetCarryWeight()
        {
            //add all worn items weights to total carry weight
            int carryWeight = Torso!.Weight + Helmet!.Weight + Helmet!.Weight + Gloves!.Weight + Shoes!.Weight + Weapon!.Weight;

            //for each item in Inventory, add it's weight to total carry weight
            Inventory!.ForEach(item => carryWeight += item.Weight);

            return carryWeight;
        }

        //property changed event handler
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
}
