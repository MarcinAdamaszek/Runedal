
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using Runedal.GameData.Items;
using System.Windows.Media.Effects;

namespace Runedal.GameData.Characters
{
    public class Player : CombatCharacter, INotifyPropertyChanged
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
        private double _HpPercentage;
        private double _MpPercentage;
        private int _Strength;
        private int _Intelligence;

        //default constructor for json deserialization
        public Player() : base()
        {
            //initialize items worn with placeholders
            Body = new Armor(Armor.ArmorType.Body, "placeholder");
            Pants = new Armor(Armor.ArmorType.Pants, "placeholder");
            Helmet = new Armor(Armor.ArmorType.Helmet, "placeholder");
            Gloves = new Armor(Armor.ArmorType.Gloves, "placeholder");
            Shoes = new Armor(Armor.ArmorType.Shoes, "placeholder");
            Weapon = new Weapon("placeholder");

            Effects = new List<EffectOnPlayer>();
        }
        public Player(string[] descriptive, int[] combatStats, int[] attributeStats, string[][] responses, int gold)
            : base(descriptive, combatStats, responses, gold)
        {
            Strength = attributeStats[0];
            Intelligence = attributeStats[1];
            Agility = attributeStats[2];
            Level = 1;
            Experience = 0;

            //initialize items worn with placeholders
            Body = new Armor(Armor.ArmorType.Body, "placeholder");
            Pants = new Armor(Armor.ArmorType.Pants, "placeholder");
            Helmet = new Armor(Armor.ArmorType.Helmet, "placeholder");
            Gloves = new Armor(Armor.ArmorType.Gloves, "placeholder");
            Shoes = new Armor(Armor.ArmorType.Shoes, "placeholder");
            Weapon = new Weapon("placeholder");

            Effects = new List<EffectOnPlayer>();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public enum State
        {
            Idle,
            Trade,
            Combat
        }
        public State CurrentState { get; set; }

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

        //player's level and amount of experience
        public int Level { get; set; }
        public int Experience { get; set; }
        
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
                }
            }
        }
        public int Agility { get; set; }

        //items worn by player
        public Armor? Body { get; set; }
        public Armor? Pants { get; set; }
        public Armor? Helmet { get; set; }
        public Armor? Gloves { get; set; }
        public Armor? Shoes { get; set; }
        public Weapon? Weapon { get; set; }

        //character with whom player currently interacts
        public Character? InteractsWith { get; set; }

        //effects currently affecting player
        public List<EffectOnPlayer>? Effects { get; set; }

        //property changed event handler
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        //method initializing Hp/Mp pools
        public override void InitializeHpMp()
        {
            Hp = GetEffectiveMaxHp();
            Mp = GetEffectiveMaxMp();
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
                case (Armor.ArmorType.Body):
                    Body = armorToWear;
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
                case (Armor.ArmorType.Body):
                    armorWorn = Body!;
                    Body = new Armor(Armor.ArmorType.Body, "placeholder");
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
            if (mod.Type == CombatCharacter.StatType.MaxHp || mod.Type == CombatCharacter.StatType.Strength)
            {
                EffectiveMaxHp = GetEffectiveMaxHp();
            }
            else if (mod.Type == CombatCharacter.StatType.MaxMp || mod.Type == CombatCharacter.StatType.Intelligence)
            {
                EffectiveMaxMp = GetEffectiveMaxMp();
            }
        }
        public override void RemoveModifier(Modifier mod)
        {
            base.RemoveModifier(mod);
            if (mod.Type == CombatCharacter.StatType.MaxHp || mod.Type == CombatCharacter.StatType.Strength)
            {
                EffectiveMaxHp = GetEffectiveMaxHp();
            }
            else if (mod.Type == CombatCharacter.StatType.MaxMp || mod.Type == CombatCharacter.StatType.Intelligence)
            {
                EffectiveMaxMp = GetEffectiveMaxMp();
            }
        }

        //methods for setting effective max hp/mp
        public override double GetEffectiveMaxHp()
        {
            double effectiveMaxHp = base.GetEffectiveMaxHp();
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
            double effectiveMaxMp = base.GetEffectiveMaxMp();
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
            effectiveStrength += base.ApplyModifiers(CombatCharacter.StatType.Strength);
            if (effectiveStrength < 1)
            {
                effectiveStrength = 1;
            }
            return effectiveStrength;
        }
        public int GetEffectiveAgility()
        {
            int effectiveAgility = Agility;
            effectiveAgility += base.ApplyModifiers(CombatCharacter.StatType.Agility);
            if (effectiveAgility < 1)
            {
                effectiveAgility = 1;
            }
            return effectiveAgility;
        }
        public int GetEffectiveIntelligence()
        {
            int effectiveIntelligence = Intelligence;
            effectiveIntelligence += base.ApplyModifiers(CombatCharacter.StatType.Intelligence);
            if (effectiveIntelligence < 1)
            {
                effectiveIntelligence = 1;
            }
            return effectiveIntelligence;
        }
        public override double GetEffectiveHpRegen()
        {
            double effectiveHpRegen = base.GetEffectiveHpRegen();
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
            double effectiveMpRegen = base.GetEffectiveMpRegen();
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
            double effectiveSpeed = base.GetEffectiveSpeed();

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
            double effectiveAttack = base.GetEffectiveAttack();

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
            double effectiveAtkSpeed = base.GetEffectiveAtkSpeed();
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
            double effectiveAccuracy = base.GetEffectiveAccuracy();
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
            double effectiveCritical = base.GetEffectiveCritical();
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
            double effectiveDefense = base.GetEffectiveDefense();
            double wornArmorModifier = Helmet!.Defense + Body!.Defense + Pants!.Defense + Gloves!.Defense + Shoes!.Defense;
            effectiveDefense += wornArmorModifier;
            if (effectiveDefense < 1)
            {
                effectiveDefense = 1;
            }
            return effectiveDefense;
        }
        public override double GetEffectiveEvasion()
        {
            double effectiveEvasion = base.GetEffectiveEvasion();
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
            double effectiveMagicResistance = base.GetEffectiveMagicResistance();
            double intelligenceModifier = GetEffectiveIntelligence() * MagicResistanceIntMultiplier;
            effectiveMagicResistance += intelligenceModifier;
            if (effectiveMagicResistance < 1)
            {
                effectiveMagicResistance = 1;
            }
            return effectiveMagicResistance;
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
            int carryWeight = Body!.Weight + Helmet!.Weight + Helmet!.Weight + Gloves!.Weight + Shoes!.Weight + Weapon!.Weight;

            //for each item in Inventory, add it's weight to total carry weight
            Inventory!.ForEach(item => carryWeight += item.Weight);

            return carryWeight;
        }

    }
}
