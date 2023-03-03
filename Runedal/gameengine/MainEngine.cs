using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Runedal.GameData;
using Runedal.GameData.Characters;
using Runedal.GameData.Items;
using System.Windows.Media.Effects;
using System.Windows.Navigation;
using Microsoft.Win32.SafeHandles;
using System.Windows.Input;
using System.Numerics;
using Runedal.GameData.Actions;
using Runedal.GameData.Effects;
using System.Windows.Controls.Primitives;
using System.Diagnostics.Contracts;
using System.Windows.Automation.Provider;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Windows.Markup;
using System.IO;
using System.Xaml.Schema;
using System.Threading;

namespace Runedal.GameEngine
{
    public class MainEngine
    {
        private const int firstIntThreshold = 20;
        private const int secondIntThreshold = 30;
        private const int thirdIntThreshold = 70;
        private const int fourthIntThreshold = 130;
        private const int fifthIntThreshold = 180;
        public MainEngine(MainWindow window)
        { 
            this.Window = window;
            this.Data = new Data();
            this.Rand = new Random();
            this.AttackInstances = new List<AttackInstance>();
            this.Actions = new List<CharAction>();
            this.TeleportLocation = new Location();
            this.LastoutputBoxContent = new List<TextRange>();
            this.Hints = new Hints();
            this.TimeInterval = 100;
            this.MinTimeInterval = 1;
            this.IsInMenu = true;
            this.IsPlayerChoosingAName = false;
            this.IsInGame = false;
            this.IsLoading = false;
            this.IsSaving = false;
            this.IsDeleting = false;
            this.IsNewSave = false;
            this.IsExitConfirmation = false;
            this.IsSaveConfirmation = false;
            this.IsSaveOverwriteConfirmation = false;
            this.IsDeleteConfirmation = false;
            this.IsErrorSaving = false;

            this.GameSavePath = string.Empty;

            this.IsAutoattackOn = true;

            //set game clock for game time
            GameClock = new DispatcherTimer(DispatcherPriority.Send);
            GameClock.Interval = TimeSpan.FromMilliseconds(TimeInterval);
            GameClock.Tick += GameClockTick!;

            //PrintManual();
            LoadGameObjects();
            PrepareNewGameLoad();
            PrintWelcomeScreen();
            
            //StartNewGame("Czesiek");
            //GivePlayerExperience(100);


            //Data.Locations!.Find(loc => loc.Name == "Karczma").Characters.ForEach(ch =>
            //{
            //    if (ch.GetType() == typeof(Monster))
            //    {
            //        PrintMessage(Convert.ToString((ch as Monster).Id));
            //    }
            //});
            //double szczurAtkSpeed = (Data.Locations!.Find(loc => loc.Name == "Piwnica").Characters.
            //    Find(character => character.Name == "Szczur") as CombatCharacter).GetEffectiveAtkSpeed();
            //double playerAtkSpeed = (Data.Locations!.Find(loc => loc.Name == "Karczma").Characters.
            //    Find(character => character.Name == "Czesiek") as CombatCharacter).GetEffectiveAtkSpeed();

            //PrintMessage("atk speed szczura: " + szczurAtkSpeed);
            //PrintMessage("atk speed gracza: " + playerAtkSpeed);

            //Location karczma = Data.Locations.Find(loc => loc.Name.ToLower() == "karczma");
            //Monster skeleton = karczma.Characters.Find(ch => ch.Name.ToLower() == "dziki_pies") as Monster;
            //AttackInstances.Add(new AttackInstance(Data.Player!, skeleton));
            //AttackInstances.Add(new AttackInstance(skeleton, Data.Player!));
            //Data.Player.Hp -= 500;
            //Data.Player.Mp -= 300;
            //(Data.Characters.Find(ch => ch.Name == "Szczur") as CombatCharacter).Hp -= 10;
            //PrintMessage(Convert.ToString((Data.Characters.Find(ch => ch.Name == "Szczur") as CombatCharacter).Hp));
        }

        //enum type for type of message displayed in PrintMessage method for displaying messages in different colors
        public enum MessageType
        {
            Default,
            UserCommand,
            Action,
            SystemFeedback,
            Gain,
            Loss,
            EffectOn,
            EffectOff,
            Speech,
            DealDmg,
            ReceiveDmg,
            CriticalHit,
            Miss,
            Dodge
        }

        public MainWindow Window { get; set; }
        public Data Data { get; set; }
        public DispatcherTimer GameClock { get; set; }
        public Random Rand { get; set; }
        public List<AttackInstance> AttackInstances { get; set; }
        public List<CharAction> Actions { get; set; }
        public Location TeleportLocation { get; set; }
        public List<TextRange> LastoutputBoxContent { get; set; }
        public Hints Hints { get; set; }
        public string GameSavePath { get; set; }
        public int TimeInterval { get; set; }
        public int MinTimeInterval { get; set; }
        public bool IsAutoattackOn { get; set; }
        public bool IsPaused { get; set; }
        public bool IsInMenu { get; set; }
        public bool IsPlayerChoosingAName { get; set; }
        public bool IsInManual { get; set; }
        public bool IsSaving { get; set; }
        public bool IsNewSave { get; set; }
        public bool IsLoading { get; set; }
        public bool IsDeleting { get; set; }
        public bool IsInGame { get; set; }
        public bool IsSaveConfirmation { get; set; }
        public bool IsSaveOverwriteConfirmation { get; set; }
        public bool IsExitConfirmation { get; set; }
        public bool IsDeleteConfirmation { get; set; }
        public bool IsFromGameSaving { get; set; }
        public bool IsFromGameLoading { get; set; }
        public bool IsErrorSaving { get; set; }

        //method processing user input commands
        public void ProcessCommand()
        {
            string userCommand = string.Empty;
            string command = string.Empty;
            string argument1 = string.Empty;
            string argument2 = string.Empty;
            string[] commandParts;

            

            //get user input from inputBox
            userCommand = Window.inputBox.Text;
            Window.inputBox.Text = string.Empty;

            //clear the input from extra spaces
            userCommand = Regex.Replace(userCommand, @"^\s+", "");
            userCommand = Regex.Replace(userCommand, @"\s+", " ");
            userCommand = Regex.Replace(userCommand, @"\s+$", "");

            //format to lowercase
            userCommand = userCommand.ToLower();

            //split user input into command and it's arguments
            Regex delimeter = new Regex(" ");
            commandParts = delimeter.Split(userCommand);

            //depending on number of arguments, assign them to proper variables
            if (commandParts.Length == 1)
            {
                command = commandParts[0];
            }
            else if (commandParts.Length == 2)
            {
                command = commandParts[0];
                argument1 = commandParts[1];
                argument1 = RemoveUnderscores(argument1);
            }
            else
            {
                command = commandParts[0];
                argument1 = commandParts[1];
                argument2 = commandParts[2];
                argument1 = RemoveUnderscores(argument1);
                argument2 = RemoveUnderscores(argument2);
            }

            if (IsExitConfirmation)
            {
                if (command == "1")
                {
                    Window.Close();
                }
                else if (command == "2")
                {
                    IsExitConfirmation = false;
                    IsInMenu = true;
                    ClearOutputBox();
                    PrintMainMenu();
                }
                return;
            }

            //handle game menu before starting the game
            if (IsInMenu)
            {
                switch (command)
                {
                    case "1":
                        FirstOptionHandler();
                        IsInMenu = false;
                        break;
                    case "2":
                        SecondOptionHandler();
                        IsInMenu = false;
                        break;
                    case "3":
                        ThirdOptionHandler();
                        IsInMenu = false;
                        break;
                    case "4":
                        FourthOptionHandler();
                        IsInMenu = false;
                        break;
                    case "5":
                        FifthOptionHandler();
                        IsInMenu = false;
                        break;
                    case "6":
                        if (Data.Player!.CurrentState != CombatCharacter.State.Idle)
                        {
                            ClearOutputBox();
                            IsErrorSaving = true;
                            PrintMessage("\n              Nie możesz zapisać gry, gdy Twoja", MessageType.ReceiveDmg);
                            PrintMessage("\n              postać walczy, handluje lub rozmawia!", MessageType.ReceiveDmg);
                            return;
                        }
                        SixthOptionHandler();
                        IsInMenu = false;
                        break;
                }

                return;
            }

            //handle saving game
            if (IsSaving)
            {
                //handle new save
                if (IsNewSave)
                {

                    //handle confirmation
                    if (IsSaveConfirmation)
                    {
                        if (command == "1")
                        {
                            SaveGame("", true);
                            IsSaving = false;
                            IsNewSave = false;
                            IsInMenu = true;
                            IsSaveConfirmation = false;
                            ClearOutputBox();
                            PrintMainMenu(true);
                        }
                        else if (command == "2")
                        {
                            IsNewSave = false;
                            IsSaveConfirmation = false;
                            ClearOutputBox();
                            SaveHandler();
                        }
                        return;
                    }

                    HandleNewSaveName(command);
                    return;
                }
                else if (command == "1")
                {
                    IsNewSave = true;
                    PrintNewSaveScreen();
                }
                else
                {
                    SaveGame(command);
                }
                return;
            }

            if (IsSaveOverwriteConfirmation)
            {
                if (command == "1")
                {
                    IsSaveOverwriteConfirmation = false;
                    SaveGame("-1", true, false);
                }
                else if (command == "2")
                {
                    IsSaveOverwriteConfirmation = false;
                    SaveHandler();
                }
                return;
            }

            //handle loading game
            if (IsLoading)
            {
                if (command == "b")
                {
                    IsLoading = false;
                    IsInMenu = true;
                    ClearOutputBox();
                    PrintMainMenu();
                }
                else if (command == "d")
                {
                    IsLoading = false;
                    IsDeleting = true;
                    ClearOutputBox();
                    LoadHandler(true);
                }
                else
                {
                    LoadGame(command);
                }
                return;
            }

            //handle game saves deleting
            if (IsDeleting)
            {
                if (command == "w")
                {
                    IsDeleting = false;
                    IsLoading = true;
                    LoadHandler(false);
                }
                else
                {
                    ChooseDeletion(command);
                    IsDeleting = false;
                }
                return;
            }

            //handle game save delete confirmation
            if (IsDeleteConfirmation)
            {
                if (command == "1")
                {
                    DeleteGameSave();
                    IsDeleteConfirmation = false;
                    IsDeleting = true;
                    LoadHandler(true);
                }
                else if (command == "2")
                {
                    IsDeleteConfirmation = false;
                    IsDeleting = true;
                    LoadHandler(true);
                }
                return;
            }

            //handle manual screen
            if (IsInManual)
            {
                IsInManual = false;
                IsInMenu = true;
                PrintMainMenu();
                return;
            }

            //handle name choosing
            if (IsPlayerChoosingAName)
            {
                if (command == "b")
                {
                    IsInMenu = true;
                    IsPlayerChoosingAName = false;
                    PrintMainMenu();
                }
                else
                {
                    VerifyPlayerName(command, argument1);
                }

                return;
            }

            //print userCommand in outputBox for user to see
            if (userCommand != "help" && userCommand != "manual" &&
                userCommand != "load" && userCommand != "save")
            {
                PrintMessage(userCommand, MessageType.UserCommand);
            }

            //prevent doing anything ingame when game is paused
            //and handle unpausing
            if (IsPaused)
            {
                if (command == "pause" || command == "ps")
                {
                    PauseHandler();
                    return;
                }
                else
                {
                    PrintMessage("Nie możesz nic zrobić w trakcie pauzy", MessageType.SystemFeedback);
                    return;
                }
            }

            //match user input to proper engine action
            switch (command)
            {
                case "go":
                case "g":
                    GoHandler(argument1);
                    break;
                case "n":
                    GoHandler("n");
                    break;
                case "e":
                    GoHandler("e");
                    break;
                case "s":
                    GoHandler("s");
                    break;
                case "w":
                    GoHandler("w");
                    break;
                case "u":
                    GoHandler("u");
                    break;
                case "d":
                    GoHandler("d");
                    break;
                case "attack":
                case "a":
                    AttackHandler(argument1);
                    break;
                case "flee":
                case "f":
                    FleeHandler(argument1);
                    break;
                case "help":
                    HelpHandler();
                    break;
                case "cast":
                case "c":
                    CastHandler(argument1, argument2);
                    break;
                case "trade":
                case "tr":
                    TradeHandler(argument1);
                    break;
                case "talk":
                case "ta":
                    TalkHandler(argument1);
                    break;
                case "buy":
                    BuyHandler(argument1, argument2);
                    break;
                case "sell":
                    SellHandler(argument1, argument2);
                    break;
                case "look":
                case "l":
                    LookHandler(argument1);
                    break;
                case "use":
                    UseHandler(argument1);
                    break;
                case "wear":
                    WearHandler(argument1);
                    break;
                case "takeoff":
                case "of":
                    TakeoffHandler(argument1);
                    break;
                case "drop":
                    DropHandler(argument1, argument2);
                    break;
                case "pickup":
                case "p":
                    PickupHandler(argument1, argument2);
                    break;
                case "inventory":
                case "i":
                    InventoryHandler(Data.Player!, false);
                    break;
                case "stats":
                case "ss":
                    StatsHandler();
                    break;
                case "spells":
                case "sps":
                    SpellsHandler();
                    break;
                case "effects":
                case "ef":
                    EffectsHandler();
                    break;
                case "point":
                case "pt":
                    PointHandler(argument1);
                    break;
                case "craft":
                case "cr":
                    CraftHandler(argument1, argument2);
                    break;
                case "stop":
                case "st":
                    StopHandler();
                    break;
                case "pause":
                case "ps":
                    PauseHandler();
                    break;
                case "save":
                    SaveHandler(true);
                    break;
                case "autoattack":
                    AutoattackHandler();
                    break;
                case "clear":
                    ClearOutputBox();
                    break;
                case "speedup":
                case "up":
                    ChangeSpeedHandler(false);
                    break;
                case "slowdown":
                case "dn":
                    ChangeSpeedHandler(true);
                    break;
                case "hints":
                    HintsHandler();
                    break;
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                case "10":
                    OptionHandler(command);
                    break;

                //for testing
                case "iwannagrow":
                    GivePlayerExperience(10);
                    break;
                case "loc":
                    PrintMessage("Z = " + Data.Player!.CurrentLocation!.Z + "\n" +
                        "Y = " + Data.Player!.CurrentLocation!.Y + "\n" +
                        "X = " + Data.Player!.CurrentLocation!.X);
                    break;
                case "allchars":
                    PrintMessage("Wszystkich postaci w grze: " + Data.AllCharactersTestProp.Count);
                    break;
                case "devme":
                    Data.Player!.AddAttributePoints(1);
                    break;
                case "clockstop":
                    GameClock.Stop();
                    break;
                case "clockstart":
                    GameClock.Start();
                    break;
                default:
                    PrintMessage("Nie rozumiem. Aby zobaczyć ściągawkę komend, wpisz \"help\". Aby wyjść do menu, wciśnij esc.", MessageType.SystemFeedback);
                    break;
            }
        }

        //method launching the welcome screen of the game
        private void PrintWelcomeScreen()
        {
            const int runedalWidth = 54;
            int i, j;
            string[] runedalAscii = new string[9];

            for (i = 0; i < runedalAscii.Length; i++)
            {
                runedalAscii[i] = String.Empty;
            }

            runedalAscii[1] = "  _____  _    _ _   _ ______ _____          _      ";
            runedalAscii[2] = " |  __ \\| |  | | \\ | |  ____|  __ \\   /\\   | |     ";
            runedalAscii[3] = " | |__) | |  | |  \\| | |__  | |  | | /  \\  | |     ";
            runedalAscii[4] = " |  _  /| |  | | . ` |  __| | |  | |/ /\\ \\ | |     ";
            runedalAscii[5] = " | | \\ \\| |__| | |\\  | |____| |__| / ____ \\| |_    ";
            runedalAscii[6] = " |_|  \\_\\\\____/|_| \\_|______|_____/_/    \\_\\______|";
            runedalAscii[7] = "                                                   ";



            for (j = 0; j < runedalAscii.Length; j++)
            {
                i = runedalAscii[j].Length;
                while (i < runedalWidth)
                {
                    runedalAscii[j] += "*";
                    i++;
                }

                PrintMessage(runedalAscii[j], MessageType.SystemFeedback);
            }

            PrintMessage("\n*************** WITAJ W GRZE RUNEDAL! *****************\n\n", MessageType.Gain);
            PrintMessage("               (Naciśnij dowolny klawisz)");
        }

        //method loading all objects into list collections in Data.cs
        private void LoadGameObjects()
        {
            //initialize all data collections
            Data.LoadLocations();
            Data.LoadSpells();
            Data.LoadCharacters();
            Data.LoadItems();
            Data.LoadStackingEffects();
        }

        //method creating new json file serving as new game load
        private void PrepareNewGameLoad()
        {
            //load player into game
            Data.LoadPlayer("Czesiek");

            //method filling hp/mp pools of all combat characters
            //with their maxhp/mp values
            Data.InitializeHpMpValues();

            //fill inventories of all characters with their items
            Data.PopulateInventories();

            //load all characters into locations
            Data.PopulateLocations();

            //save newgame json file
            Data.SaveGame(Data.JsonDirectoryPath + @"NewGame.json", new Hints());
        }

        //method printing menu of the game
        public void PrintMainMenu(bool gameSaveSuccess = false)
        {
            ClearOutputBox();

            PrintMessage("*********************** MENU GŁÓWNE ***********************\n", MessageType.Gain);

            PrintMessage("                   Aby wybrać opcję menu,");
            if (IsInGame)
            {
                PrintMessage("          wpisz jedną z cyfr (1, 2, 3, 4, 5 lub 6)");
            }
            else
            {
                PrintMessage("            wpisz jedną z cyfr (1, 2, 3, 4 lub 5)");
            }
            PrintMessage("                      i naciśnij enter.\n");

            PrintMessage("                       1. NOWA GRA", MessageType.Loss);
            PrintMessage("                       2. WCZYTAJ GRĘ", MessageType.Loss);
            PrintMessage("                       3. JAK GRAĆ?", MessageType.Loss);
            PrintMessage("                       4. KOMENDY", MessageType.Loss);
            PrintMessage("                       5. WYJDŹ Z GRY", MessageType.Loss);

            if (IsInGame)
            {
                PrintMessage("                       6. ZAPISZ GRĘ", MessageType.Loss);
                PrintMessage("\n", MessageType.Default, false);
                PrintMessage("               (Wciśnij esc aby wrócić do gry)\n", MessageType.Action, false);
            }

            if (gameSaveSuccess)
            {
                PrintMessage("                       Gra zapisana!", MessageType.EffectOn);
            }

            PrintMessage("\n                TRYB PEŁNOEKRANOWY - KLAWISZ \"F11\"", MessageType.Action, false);
        }

        //method veryfing player's name
        public void VerifyPlayerName(string playerName, string secondArgument)
        {
            var regex = @"^[\w']{3,40}$";
            bool isMatch = Regex.IsMatch(playerName, regex);

            if (isMatch && secondArgument == string.Empty)
            {
                StartNewGame(playerName);
                IsPlayerChoosingAName = false;
            }
            else
            {
                ClearOutputBox();

                PrintMessage("************************ NOWA GRA *************************\n", MessageType.Gain);

                PrintMessage("             (Wciśnij esc aby wrócić do menu)\n", MessageType.Action);

                PrintMessage("                 Podałeś niepoprawne imię!\n", MessageType.ReceiveDmg);

                PrintMessage("           Imię musi zawierać od 3 do 40 znaków,");
                PrintMessage("           oraz składać się tylko z liter, cyfr,");
                PrintMessage("                 lub znaków podkreślnika(_)");
                PrintMessage("                      i  apostrofu(')\n");

                PrintMessage("                Wpisz imię dla swojej postaci");
                PrintMessage("                     i naciśnij enter\n");
            }

            //FirstOptionHandler();
            return;
        }

        //method starting a new game
        public void StartNewGame(string playerName)
        {
            IsInGame = true;

            Hints tempHints = new Hints();
            Data.LoadGame(Data.JsonDirectoryPath + @"NewGame.json", out tempHints);
            Hints = tempHints;

            //connect hp/mp/action bars to values of player object
            Window.InitializePlayerDataContext(Data.Player!);

            //name player's character with the name he has chosen
            Data.Player!.Name = playerName;

            //set number of spells remembered to 1


            GameClock.Start();

            ClearOutputBox();

            PrintMessage("> Witaj w świecie Runedal!", MessageType.EffectOn);
            PrintMessage("> Aby zrobić cokolwiek, wpisujesz komendę i naciskasz enter", MessageType.EffectOn);
            PrintMessage("> Przy wpisywaniu, wielkość liter nie ma znaczenia (SzCzUr = szczur), można pomijać polskie znaki (żmija = zmija), lub znak podkreślnika" +
                "\"_\" (mikstura_życia = miksturazycia)", MessageType.EffectOn);
            PrintMessage("> W zielonym oknie na prawo, znajduje się minimapa, która pokazuje gdzie się aktualnie znajdujesz\n", MessageType.EffectOn);
            PrintMessage("> Jeśli nie wiesz co robić - wciśnij esc aby zobaczyć menu główne i wybierz opcję \"4.KOMENDY\" lub \"3.JAK GRAĆ?\"", MessageType.EffectOn);
            PrintMessage("> Aby zobaczyć ściągawkę komend - wpisz \"help\"", MessageType.EffectOn);

            //reveal starting location and adjacent locations
            Location northAdjacentLoc = new Location();
            Location eastAdjacentLoc = new Location();
            Location southAdjacentLoc = new Location();
            Location westAdjacentLoc = new Location();

            GetNextLocation("n", out northAdjacentLoc);
            GetNextLocation("e", out eastAdjacentLoc);
            GetNextLocation("s", out southAdjacentLoc);
            GetNextLocation("w", out westAdjacentLoc);

            northAdjacentLoc.IsVisible = true;
            eastAdjacentLoc.IsVisible = true;
            southAdjacentLoc.IsVisible = true;
            westAdjacentLoc.IsVisible = true;

            Data.Player!.CurrentLocation!.IsVisible = true;


            PrintMap();
            LocationInfo(Data.Player!.CurrentLocation!);

            //trigger go hint
            if (Hints.HintsOnOff)
            {
                PrintHint(Hints.HintType.Go);
            }
        }

        //method clearing the outputBox
        public void ClearOutputBox()
        {
            Window.outputBox.SelectAll();
            Window.outputBox.Selection.Text = "";
            Window.outputBox.Document.Blocks.Add(new Paragraph());
        }

        //method saving game
        private void SaveGame(string saveNumber, bool IsPathSpecified = false, bool isQuickSave = false)
        {
            string[] saveFiles = Directory.GetFiles(Data.JsonDirectoryPath + @"SavedGames\");
            string savePath = string.Empty;
            int i;

            if (isQuickSave)
            {
                savePath = Data.JsonDirectoryPath + @"SavedGames\SZYBKI_ZAPIS";
            }

            if (IsPathSpecified)
            {
                savePath = GameSavePath;
            }

            for (i = 0; i < saveFiles.Length; i++)
            {
                if (saveNumber == Convert.ToString(i + 2))
                {
                    IsSaving = false;
                    savePath = saveFiles[i];
                    break;
                }
            }

            if (savePath == string.Empty)
            {
                return;
            }
            else if (!isQuickSave && !IsPathSpecified)
            {
                IsSaveOverwriteConfirmation = true;
                GameSavePath = savePath;
                GameSaveOverwriteConfirmation();
                return;
            }

            GameClock.Stop();

            //here save the game
            Data.SaveGame(savePath, Hints);

            if (IsInMenu)
            {
                ClearOutputBox();
                PrintMainMenu(true);
            }
            else if (isQuickSave)
            {
                PrintMessage("Gra zapisana!", MessageType.SystemFeedback);
            }
            else
            {
                ClearOutputBox();
                PrintMessage("Gra zapisana!", MessageType.SystemFeedback);
                LocationInfo(Data.Player!.CurrentLocation!);
            }

            GameClock.Start();

        }

        //method loading game
        private void LoadGame(string saveNumber)
        {
            string gameSavesPathBeginning = Data.JsonDirectoryPath + @"SavedGames\";
            string[] saveFiles = Directory.GetFiles(gameSavesPathBeginning);
            int chosenNumber = -1;
            int i;

            for (i = 0; i < saveFiles.Length; i++)
            {
                if (saveNumber == Convert.ToString(i + 1))
                {
                    chosenNumber = i;
                    break;
                }
            }

            if (chosenNumber == -1)
            {
                return;
            }

            

            IsInGame = true;
            IsLoading = false;

            GameClock.Stop();

            Hints tempHints = new Hints();
            Data.LoadGame(saveFiles[chosenNumber], out tempHints);
            Hints = tempHints;

            //reassign window's player variable
            Window.InitializePlayerDataContext(Data.Player!);

            //prevents bug causing MaxSpellsRemembered property to reach higher values than expected
            Data.Player!.RefreshMaxSpellsRemembered();
            Data.Player!.RefreshSpellsRemembered();

            PrintMap();
            GameClock.Start();
            ClearOutputBox();
            PrintMessage("Zapis \"" + Path.GetFileName(saveFiles[chosenNumber]) + "\" wczytany", MessageType.SystemFeedback);
            LocationInfo(Data.Player!.CurrentLocation!);
        }

        //method chosing game save to be deleted
        private void ChooseDeletion(string saveNumber)
        {
            string[] saveFiles = Directory.GetFiles(Data.JsonDirectoryPath + @"SavedGames\");
            int chosenNumber = -1;
            int i;

            for (i = 0; i < saveFiles.Length; i++)
            {
                if (saveNumber == Convert.ToString(i + 1))
                {
                    chosenNumber = i;
                    break;
                }
            }

            if (chosenNumber == -1)
            {
                return;
            }

            GameSavePath = saveFiles[chosenNumber];
            IsDeleteConfirmation = true;
            GameSaveDeleteConfirmation();
        }

        //method deleting game save
        private void DeleteGameSave()
        {
            File.Delete(GameSavePath);
        }

        //method handling new save game
        private void PrintNewSaveScreen()
        {
            ClearOutputBox();
            PrintMessage("************************ NOWY ZAPIS ************************\n", MessageType.Gain);
            PrintMessage("                Jak ma się nazywać nowy zapis? ", MessageType.Default, false);
            PrintMessage("                (Wpisz nazwę i  wciśnij enter)", MessageType.Default, false);
            IsNewSave = true;
        }

        //method handling saving for user-chosen save name
        private void HandleNewSaveName(string saveName)
        {
            string[] saveFiles = Directory.GetFiles(Data.JsonDirectoryPath + @"SavedGames\");
            int i;

            for (i = 0; i < saveFiles.Length; i++)
            {
                if (Path.GetFileName(saveFiles[i]) == saveName)
                {
                    ClearOutputBox();
                    PrintMessage("************************* UWAGA! *************************\n", MessageType.CriticalHit, false);
                    PrintMessage("             (Wciśnij esc aby wrócić do menu)\n", MessageType.Action, false);
                    PrintMessage("             Zapis o tej nazwie już istnieje.", MessageType.Default, false);
                    PrintMessage("          Zapisanie stanu gry spowoduje utratę", MessageType.Default, false);
                    PrintMessage("                    poprzedniego zapisu.", MessageType.Default, false);
                    PrintMessage("                     Zapisać mimo to?", MessageType.Default, false);
                    PrintMessage("             (wpisz 1 lub 2 i naciśnij enter)", MessageType.Default, false);
                    PrintMessage("                         1. TAK", MessageType.Loss, false);
                    PrintMessage("                         2. NIE", MessageType.Loss, false);

                    IsSaveConfirmation = true;
                    GameSavePath = saveFiles[i];

                    return;
                }
            }

            GameSavePath = Data.JsonDirectoryPath + @"SavedGames\" + saveName;
            IsSaving = false;
            IsNewSave = false;
            SaveGame("", true);
        }

        //method handling game save deletion confirmation
        private void GameSaveDeleteConfirmation()
        {
            string fileName = Path.GetFileName(GameSavePath);
            string leadingSpace = string.Empty;
            int numberOfSpaces = 29;
            int i;

            for (i = 0; i < fileName.Length; i += 2)
            {
                numberOfSpaces--;
            }

            for (i = 0; i < numberOfSpaces; i++)
            {
                leadingSpace += " ";
            }

            ClearOutputBox();
            PrintMessage("************************* UWAGA! *************************\n", MessageType.CriticalHit, false);
            PrintMessage("             (Wciśnij esc aby wrócić do menu)\n", MessageType.Action, false);
            PrintMessage("            Czy na pewno chcesz usunąć zapis gry.", MessageType.Default, false);
            PrintMessage("                         o nazwie:", MessageType.Default, false);
            PrintMessage(leadingSpace + "\"" + fileName + "\"?\n", MessageType.Default, false);
            PrintMessage("             (wpisz 1 lub 2 i naciśnij enter)\n", MessageType.Default, false);
            PrintMessage("                         1. TAK", MessageType.Loss, false);
            PrintMessage("                         2. NIE", MessageType.Loss, false);
        }

        //handle chosing existing game save and overwriting it
        private void GameSaveOverwriteConfirmation()
        {
            ClearOutputBox();
            PrintMessage("************************* UWAGA! *************************\n", MessageType.CriticalHit, false);
            PrintMessage("             (Wciśnij esc aby wrócić do menu)\n", MessageType.Action, false);
            PrintMessage("           Próbujesz zapisać grę na istniejącym", MessageType.Default, false);
            PrintMessage("               zapisie. Spowoduje to utratę", MessageType.Default, false);
            PrintMessage("                   poprzedniego zapisu.", MessageType.Default, false);
            PrintMessage("                     Zapisać mimo to?", MessageType.Default, false);
            PrintMessage("             (wpisz 1 lub 2 i naciśnij enter)", MessageType.Default, false);
            PrintMessage("                         1. TAK", MessageType.Loss, false);
            PrintMessage("                         2. NIE", MessageType.Loss, false);
        }







        //==============================================MENU HANDLERS=============================================

        //chosing a name for character ()
        private void FirstOptionHandler()
        {
            ClearOutputBox();
            PrintMessage("************************ NOWA GRA *************************\n", MessageType.Gain);
            PrintMessage("             (Wciśnij esc aby wrócić do menu)\n", MessageType.Action);
            PrintMessage("              Wpisz imię dla swojej postaci\n");
            PrintMessage("                          Uwaga!", MessageType.Action);
            PrintMessage("            Imię musi zawierać od 3 do 40 znaków,", MessageType.Default);
            PrintMessage("            oraz składać się tylko z liter, cyfr,", MessageType.Default);
            PrintMessage("                  lub znaku podkreślnika(_)", MessageType.Default);
            PrintMessage("                      lub  apostrofu(')", MessageType.Default);
            IsPlayerChoosingAName = true;
        }

        //loading a game
        private void SecondOptionHandler()
        {
            IsInMenu = false;
            LoadHandler(false);
        }

        //print game manual
        private void ThirdOptionHandler()
        {
            IsInManual = true;
            ClearOutputBox();
            PrintManual();
        }

        //printing commands manual
        private void FourthOptionHandler()
        {
            IsInManual = true;
            ClearOutputBox();
            PrintCommandsCS(true);
        }

        //quit the game
        private void FifthOptionHandler()
        {
            if (IsInGame)
            {
                IsExitConfirmation = true;
                ClearOutputBox();
                PrintMessage("************************** UWAGA ****************************\n", MessageType.CriticalHit, false);
                PrintMessage("              (Wcisnij esc aby wrócić do menu)\n", MessageType.Action, false);
                //PrintMessage("              Jesteś teraz w trakcie rozgrywki.", MessageType.Action, false);
                PrintMessage("              Jeśli teraz wyjdziesz, stan gry ", MessageType.Action, false);
                PrintMessage("                    NIE zostanie zapisany.", MessageType.Action, false);
                PrintMessage("                   Czy chcesz wyjść mimo to?", MessageType.Action, false);
                PrintMessage("          (aby wybrać, wpisz 1 lub 2 i naciśnij enter)\n", MessageType.Action, false);
                PrintMessage("                          1. TAK", MessageType.Loss, false);
                PrintMessage("                          2. NIE\n", MessageType.Loss, false);
            }
            else
            {
                Window.Close();
            }
        }

        //save game
        private void SixthOptionHandler()
        {
            ClearOutputBox();
            SaveHandler();
        }







        //==============================================COMMAND HANDLERS=============================================

        //method handling 'save' command
        private void SaveHandler(bool IsQuickSave = false) 
        {

            if (IsQuickSave)
            {
                //handle error when player is fighting/trading/talking
                if (Data.Player!.CurrentState != Player.State.Idle)
                {
                    PrintMessage("Nie możesz zapisać gry podczas walki, handlu lub rozmowy!", MessageType.SystemFeedback);
                    return;
                }

                SaveGame("", false, true);
                return;
            }

            IsSaving = true;

            string[] saveFiles = Directory.GetFiles(Data.JsonDirectoryPath + @"SavedGames\");
            string[] savesToChoose = new string[saveFiles.Length];
            int i;

            for (i = 0; i < savesToChoose.Length; i++)
            {
                savesToChoose[i] = Path.GetFileName(saveFiles[i]);
            }

            ClearOutputBox();

            PrintMessage("*********************** ZAPISZ GRĘ ************************\n", MessageType.Gain, false);
            PrintMessage("            (Wciśnij esc aby powrócić do menu)\n", MessageType.Action, false);

            PrintMessage("         Aby wybrać zapis gry, wpisz odpowiedni numer", MessageType.Default, false);
            PrintMessage("                     i naciśnij enter:\n", MessageType.Default, false);

            PrintMessage("                       1. NOWY ZAPIS\n", MessageType.Loss, false);

            i = 2;

            foreach (var save in savesToChoose)
            {
                //if (save != "SZYBKI_ZAPIS")
                //{
                    PrintMessage("                       " + i + ". " + save, MessageType.Loss, false);
                    i++;
                //}
            }
        }

        //method handling 'load' command
        private void LoadHandler(bool isDeleting)
        {
            IsLoading = true;

            GameClock.Stop();

            string[] saveFiles = Directory.GetFiles(Data.JsonDirectoryPath + @"SavedGames\");
            string[] savesToChoose = new string[saveFiles.Length];
            int i;

            for (i = 0; i < savesToChoose.Length; i++)
            {
                savesToChoose[i] = Path.GetFileName(saveFiles[i]);
            }

            ClearOutputBox();

            if (isDeleting)
            {
                IsLoading = false;
                PrintMessage("*********************** USUŃ ZAPIS ************************\n", MessageType.ReceiveDmg, false);
            }
            else
            {
                PrintMessage("*********************** WCZYTAJ GRĘ ***********************\n", MessageType.Gain, false);
            }
            PrintMessage("             (Wciśnij esc aby powrócić do menu)\n", MessageType.Action, false);
            if (isDeleting)
            {
                PrintMessage("          Aby USUNĄĆ zapis gry, wpisz odpowiedni numer", MessageType.Default, false);
            }
            else
            {
                PrintMessage("         Aby WCZYTAĆ zapis gry, wpisz odpowiedni numer", MessageType.Default, false);
            }
            
            PrintMessage("                     i naciśnij enter:\n", MessageType.Default, false);
            if (isDeleting)
            {
                PrintMessage("          Aby przejść do tworzenia zapisów, wpisz \"w\"", MessageType.Default, false);
                PrintMessage("                     i naciśnij enter\n", MessageType.Default, false);
            }
            else
            {
                PrintMessage("           Aby przejść do usuwania zapisów, wpisz \"d\"", MessageType.Default, false);
                PrintMessage("                     i naciśnij enter\n", MessageType.Default, false);
            }


            i = 1;
            foreach (var save in savesToChoose)
            {
                PrintMessage("                        " + i + ". " + save, MessageType.Loss, false);
                i++;
            }


        }

        //mathod handling 'autoattack' command
        private void AutoattackHandler()
        {
            if (IsAutoattackOn)
            {
                IsAutoattackOn = false;
                PrintMessage("Automatyczny atak wyłączony", MessageType.SystemFeedback);
            }
            else
            {
                IsAutoattackOn = true;
                PrintMessage("Automatyczny atak włączony", MessageType.SystemFeedback);
            }
        }

        //method handling 'help' command
        private void HelpHandler()
        {
            PrintCommandsCS(false);
        }

        //method handling game pausing

        private void PauseHandler()
        {
            if (IsPaused)
            {
                PrintMessage("Gra wznowiona", MessageType.SystemFeedback);
                IsPaused = false;
                GameClock.Start();
            }
            else
            {
                PrintMessage("Gra zatrzymana (wpisz \"pause\" lub \"ps\" aby wznowić)", MessageType.SystemFeedback);
                IsPaused = true;
                GameClock.Stop();
            }
        }

        //method moving player to next location
        private void GoHandler(string direction)
        {
            
            bool innerPassage = Data.Player!.CurrentLocation!.GetPassage(direction);
            Location nextLocation = new Location();

            //if player is in combat state
            if (Data.Player!.CurrentState! == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            if (direction == string.Empty)
            {
                PrintMessage("Musisz podać kierunek w którym chcesz się udać (n, e, s, w, u, d)", MessageType.SystemFeedback);
                return;
            }

            ResetPlayerState();

            string directionString = GetPolishDirectionName(direction);

            if (GetNextLocation(direction, out nextLocation))
            {

                //if the passage is open
                if (innerPassage)
                {
                    //calculate action points cost
                    int weightAboveLimit = Data.Player!.GetCarryWeight() - Data.Player!.GetWeightLimit();

                    if (weightAboveLimit < 0)
                    {
                        weightAboveLimit = 0;
                    }

                    int actionPoints = (int)((300 + weightAboveLimit) / Data.Player.GetEffectiveSpeed()) * 10;

                    CharAction locationChange = new LocationChange(Data.Player!, nextLocation, directionString, actionPoints);
                    AddAction(locationChange);
                }
                else
                {
                    PrintMessage("Nie potrafisz otworzyć tego przejścia", MessageType.SystemFeedback);
                }
            }
            else
            {
                PrintMessage("Nie możesz tego zrobić - niczego nie ma w tamtym kierunku", MessageType.SystemFeedback);
            }
        }

        //method handling 'look' command
        private void LookHandler(string entityName)
        {
            int index = -1;
            string description = string.Empty;
            string[] directionLetters = new string[] { "n", "e", "s", "w", "u", "d" };
            Location nextLocation;

            //if command "look" was used without argument, print location description
            if (entityName == string.Empty || entityName == "around")
            {
                LocationInfo(Data.Player!.CurrentLocation!);

                //trigger look adjacent loc hint
                if (Hints.HintsOnOff)
                {
                    if (Hints.LookAdjacentLocHint)
                    {
                        PrintHint(Hints.HintType.LookAdjacentLoc);
                    }
                }
            }
            else
            {
                //if player typed n, e, s, w, u or d - search for adjacent locations
                if (directionLetters.Contains(entityName))
                {

                    if (!GetNextLocation(entityName, out nextLocation))
                    {
                        PrintMessage("Niczego nie ma w tamtym kierunku", MessageType.SystemFeedback);
                        return;
                    }

                    PrintMessage("Spoglądasz " + GetPolishDirectionName(entityName), MessageType.Action);
                    LocationInfo(nextLocation, false);
                    return;
                }

                //search player's remembered spells
                index = Data.Player!.RememberedSpells.FindIndex(spell => FlattenPolishChars(spell.Name!.ToLower()) == FlattenPolishChars(entityName));
                if (index != -1)
                {
                    SpellInfo(Data.Player!.RememberedSpells[index]);
                    return;
                }

                //search location for items on the ground
                index = Data.Player!.CurrentLocation!.Items!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(entityName));
                if (index != -1)
                {
                    ItemInfo(entityName);
                    return;
                }


                //else search characters of current location and player's inventory for entity with name matching the argument
                index = Data.Player!.CurrentLocation!.Characters!.FindIndex(character => FlattenPolishChars(character.Name!.ToLower()) 
                == FlattenPolishChars(entityName));
                if (index != -1)
                {
                    CharacterInfo(Data.Player!.CurrentLocation!.Characters[index]);
                    return;
                }
                else
                {

                    //else search adjacent locations also
                    for (int i = 0; i < 4; i++)
                    {
                        GetNextLocation(directionLetters[i], out nextLocation);
                        index = nextLocation.Characters!.FindIndex(character => FlattenPolishChars(character.Name!.ToLower())
                        == FlattenPolishChars(entityName));
                        
                        if (index != -1)
                        {
                            CharacterInfo(nextLocation.Characters[index]);
                            return;
                        }
                    }
                }
                {

                    //else search player's inventory for item with name matching the argument
                    index = Data.Player!.Inventory!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(entityName));
                    if (index != -1)
                    {
                        ItemInfo(Data.Player!.Inventory[index].Name!);
                        return;
                    }
                    else if (Data.Player!.CurrentState == Player.State.Trade)
                    {
                        index = Data.Player!.InteractsWith!.Inventory!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower())
                        == FlattenPolishChars(entityName));
                        if (index != -1)
                        {
                            ItemInfo(Data.Player!.InteractsWith!.Inventory![index].Name!);
                            return;
                        }
                    }
                }

                //search for the item in player worn items
                string wornItemName = "placeholder";

                if (FlattenPolishChars(Data.Player!.Weapon!.Name!.ToLower()) == FlattenPolishChars(entityName))
                {
                    wornItemName = Data.Player!.Weapon!.Name;
                }
                else if (FlattenPolishChars(Data.Player!.Helmet!.Name!.ToLower()) == FlattenPolishChars(entityName))
                {
                    wornItemName = Data.Player!.Helmet!.Name;
                }
                else if (FlattenPolishChars(Data.Player!.Torso!.Name!.ToLower()) == FlattenPolishChars(entityName))
                {
                    wornItemName = Data.Player!.Torso!.Name;
                }
                else if (FlattenPolishChars(Data.Player!.Pants!.Name!.ToLower()) == FlattenPolishChars(entityName))
                {
                    wornItemName = Data.Player!.Pants!.Name;
                }
                else if (FlattenPolishChars(Data.Player!.Gloves!.Name!.ToLower()) == FlattenPolishChars(entityName))
                {
                    wornItemName = Data.Player!.Gloves!.Name;
                }
                else if (FlattenPolishChars(Data.Player!.Shoes!.Name!.ToLower()) == FlattenPolishChars(entityName))
                {
                    wornItemName = Data.Player!.Shoes!.Name;
                }

                if (wornItemName != "placeholder")
                {
                    ItemInfo(wornItemName);
                    return;
                }

                //if if nothing's matched, print appropriate message to user
                PrintMessage("Nie ma tu niczego o nazwie \"" + entityName + "\"", MessageType.SystemFeedback);
            }
        }

        //method handling 'trade' command
        private void TradeHandler(string characterName)
        {
            int index = -1;
            Character tradingCharacter = new Character("placeholder");

            //if player is in combat state
            if (Data.Player!.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            //handle lack of argument
            if (characterName == string.Empty)
            {
                PrintMessage("Musisz podać imię postaci", MessageType.SystemFeedback);
                return;
            }

            ResetPlayerState();

            //check if the character of specified name exists in player's current location
            index = Data.Player!.CurrentLocation!.Characters!.FindIndex(character => FlattenPolishChars(character.Name!.ToLower())
            == FlattenPolishChars(characterName));
            if (index != -1)
            {
                tradingCharacter = Data.Player!.CurrentLocation!.Characters![index];

                //check if chosen character is trader type or not
                if (tradingCharacter.GetType() != typeof(Trader))
                {
                    PrintMessage("Nie możesz handlować z tą postacią", MessageType.SystemFeedback);
                    return;
                }

                if ((tradingCharacter as Trader)!.IsTalker)
                {
                    PrintMessage("Nie możesz handlować z tą postacią", MessageType.SystemFeedback);
                    return;
                }

                //set player's interaction character
                Data.Player!.InteractsWith = tradingCharacter;

                //set player's state to trade
                Data.Player!.CurrentState = Player.State.Trade;

                PrintMessage("Handlujesz z: " + tradingCharacter.Name, MessageType.Action);
                InventoryInfo(tradingCharacter, true);
                InventoryInfo(Data.Player!, true);

                //trigger buy/sell hint
                if (Hints.HintsOnOff)
                {
                    if (Hints.BuySellHint)
                    {
                        Item item1 = tradingCharacter.Inventory!.First();
                        Item item2 = Data.Player!.Inventory!.First();

                        PrintHint(Hints.HintType.BuySell, item1.Name!.ToLower(), item2.Name!.ToLower());
                    }
                }
            }
            else
            {
                PrintMessage("Nie ma tu postaci o imieniu \"" + characterName + "\"", MessageType.SystemFeedback);
            }
        }

        //method handling 'talk' command
        private void TalkHandler(string characterName)
        {

            //if player is in combat state
            if (Data.Player!.CurrentState == Player.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            //handle lack of argument
            if (characterName == string.Empty)
            {
                PrintMessage("Musisz podać imię postaci", MessageType.SystemFeedback);
                return;
            }

            int index = Data.Player!.CurrentLocation!.Characters!.FindIndex(
                character => FlattenPolishChars(character.Name!.ToLower()) == FlattenPolishChars(characterName));


            ResetPlayerState();
            BreakInvisibility();

            //if character not found in current location
            if (index == -1)
            {
                PrintMessage("Nie ma tu postaci o imieniu \"" + characterName + "\"", MessageType.SystemFeedback);
                return;
            }

            Character talkingCharacter = Data.Player!.CurrentLocation!.Characters[index];

            //if there is no talking option with character
            if (talkingCharacter.Questions!.Length == 0)
            {
                if (talkingCharacter.PassiveResponses!.Length > 0)
                {
                    PrintMessage(talkingCharacter.Name + ": " +
                        talkingCharacter.PassiveResponses![Rand.Next(talkingCharacter.PassiveResponses.Length)], MessageType.Speech);
                }
                return;
            }

            //begin conversation
            Data.Player!.CurrentState = Player.State.Talk;
            Data.Player!.InteractsWith = talkingCharacter;
            PrintMessage("Rozmawiasz z " + talkingCharacter.Name, MessageType.Action);
            PrintMessage(talkingCharacter.Name + ": " + 
                talkingCharacter.PassiveResponses![Rand.Next(talkingCharacter.PassiveResponses.Length)], MessageType.Speech);

            PrintMessage("Wybierz co chcesz powiedzieć (wpisz cyfrę):");
            for (int i = 0; i < talkingCharacter.Questions.Length; i++)
            {
                PrintMessage(i + 1 + ". " + talkingCharacter.Questions[i]);
            }
            PrintMessage(Data.Player!.InteractsWith!.Questions.Length + 1 + ": Bywaj");
        }

        //method handling chosen option (1, 2, 3 etc.)
        private void OptionHandler(string option)
        {
            if (Data.Player!.CurrentState != Player.State.Talk)
            {
                PrintMessage("Nie rozmawiasz z nikim", MessageType.SystemFeedback);
                return;
            }

            int optionNumber = Int32.Parse(option);
            
            if (optionNumber > Data.Player!.InteractsWith!.Questions!.Length)
            {
                PrintSpeech(Data.Player!, "Bywaj");
                ResetPlayerState();
                return;
            }

            PrintSpeech(Data.Player!, Data.Player!.InteractsWith!.Questions![optionNumber - 1]);
            PrintSpeech(Data.Player!.InteractsWith, Data.Player!.InteractsWith!.Answers![optionNumber - 1]);

            PrintMessage("Wybierz co chcesz powiedzieć: ");
            for (int i = 0; i < Data.Player!.InteractsWith!.Questions.Length; i++)
            {
                PrintMessage(i + 1 + ". " + Data.Player!.InteractsWith!.Questions[i]);
            }
            PrintMessage(Data.Player!.InteractsWith!.Questions.Length + 1 + ": Bywaj");
        }

        //method handling 'inventory' command
        private void InventoryHandler(Player player, bool withPrice)
        {

            //if player is trading, print trader's and player's inventory (trading interface)
            if (Data.Player!.CurrentState! == Player.State.Trade)
            {
                InventoryInfo(Data.Player!.InteractsWith!, true);
                InventoryInfo(Data.Player!, true);
            }
            else
            {
                InventoryInfo(player, withPrice);
            }

            //trigger look inventory item hint
            if (Hints.HintsOnOff)
            {
                if (Hints.LookInventoryItemHint)
                {
                    Item itemToLook = Data.Player!.Inventory!.First();
                    PrintHint(Hints.HintType.LookInventoryItem, itemToLook.Name!.ToLower());
                }
            }
        }

        //method handling 'buy' command
        private void BuyHandler(string itemName, string quantity)
        {

            //if the player is trading with someone
            if (Data.Player!.CurrentState != Player.State.Trade)
            {
                PrintMessage("Obecnie z nikim nie handlujesz", MessageType.SystemFeedback);
                return;
            }

            //handle lack of argument
            if (itemName == string.Empty)
            {
                PrintMessage("Musisz podać nazwę przedmiotu", MessageType.SystemFeedback);
                return;
            }

            Trader trader = (Data.Player!.InteractsWith as Trader)!;
            int itemIndex = -1;
            int itemQuantity = 1;
            int buyingPrice;

            itemIndex = trader.Inventory!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName));

            //check if the item exists in trader's inventory
            if (itemIndex == -1)
            {
                PrintMessage(trader.Name + " nie posiada tego przedmiotu", MessageType.SystemFeedback);
                return;
            }

            //if player typed 'all' as second argument, set quantity to maximum
            if (quantity == "all" || quantity == "a")
            {
                itemQuantity = trader.Inventory[itemIndex].Quantity;
            }

            //else set quantity depending on value parsed from second argument
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }

            //prevent player from buying '0' items
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }
            
            //if player set quantity to more than trader has, set it to
            if (trader.Inventory[itemIndex].Quantity < itemQuantity)
            {
                PrintMessage(trader.Name! + " nie posiada aż tyle", MessageType.SystemFeedback);
                return;
            }

            //set buying price depending on quantity
            buyingPrice = CalculateTraderPrice(itemName) * itemQuantity;

            //if total buying price of the item is lesser than amount of gold possesed by player
            if (Data.Player!.Gold! >= buyingPrice)
            {
                
                //inform player about action
                PrintMessage("Kupujesz " + itemQuantity + " " + itemName, MessageType.Action);

                //remove item from traders inventory and gold from player's inventory
                trader.RemoveItem(itemName, itemQuantity);
                RemoveGoldFromPlayer(buyingPrice);

                //add item to player's inventory 
                AddItemToPlayer(itemName, itemQuantity);

                //add gold amount to trader's pool
                trader.Gold += buyingPrice;

                //display trader's/player's inventories to update their content
                InventoryInfo(trader, true);
                InventoryInfo(Data.Player!, true);

                //trigger hints
                if (Hints.HintsOnOff)
                {

                    //trigger wear hint
                    if (Hints.WearHint)
                    {
                        Item boughtItem = Data.Items!.Find(it => FlattenPolishChars(it.Name!.ToLower()) == 
                        FlattenPolishChars(itemName.ToLower()))!;

                        if (boughtItem.GetType() == typeof(Armor) || boughtItem.GetType() == typeof(Weapon))
                        {
                            PrintHint(Hints.HintType.Wear, boughtItem.Name!.ToLower());
                        }
                    }

                    //trigger use hint
                    if (Hints.UseHint)
                    {
                        Item boughtItem = Data.Items!.Find(it => FlattenPolishChars(it.Name!.ToLower()) == 
                        FlattenPolishChars(itemName.ToLower()))!;

                        if (boughtItem.GetType() == typeof(Consumable))
                        {
                            PrintHint(Hints.HintType.Use, boughtItem.Name!.ToLower());
                        }
                    }
                }
            }
            else
            {
                PrintMessage("Nie stać Cię", MessageType.SystemFeedback);
            }
        }

        //method handling 'sell' command
        private void SellHandler(string itemName, string quantity)
        {

            //if the player is trading with someone
            if (Data.Player!.CurrentState != Player.State.Trade)
            {
                PrintMessage("Obecnie z nikim nie handlujesz", MessageType.SystemFeedback);
                return;
            }

            //handle lack of argument
            if (itemName == string.Empty)
            {
                PrintMessage("Musisz podać nazwę przedmiotu", MessageType.SystemFeedback);
                return;
            }

            Trader trader = (Data.Player!.InteractsWith as Trader)!;
            int itemIndex = -1;
            int itemQuantity = 1;
            int sellingPrice = 0;

            itemIndex = Data.Player!.Inventory!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName));

            //check if the item exists in player's inventory
            if (itemIndex == -1)
            {
                PrintMessage("Nie posiadasz wybranego przedmiotu", MessageType.SystemFeedback);
                return;
            }

            //if player typed 'all' as second argument, set quantity to maximum
            if (quantity == "all" || quantity == "a")
            {
                itemQuantity = Data.Player!.Inventory[itemIndex].Quantity;
            }

            //else set quantity depending on value parsed from second argument
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }

            //prevent player from selling 0 items
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }

            //if player set quantity to more than he has, set it to
            if (Data.Player!.Inventory[itemIndex].Quantity < itemQuantity)
            {
                PrintMessage("Próbujesz sprzedać więcej niż posiadasz..", MessageType.SystemFeedback);
                return;
            }

            //set buying price depending on quantity
            sellingPrice = Data.Player!.Inventory[itemIndex].Price * itemQuantity;

            //if total buying price of the item is lesser than amount of gold possesed by player
            if (trader.Gold >= sellingPrice)
            {

                //inform player about action
                PrintMessage("Sprzedajesz " + itemQuantity + " " + itemName, MessageType.Action);

                //remove item from player's inventory and put it into trader's inventory
                RemoveItemFromPlayer(itemName, itemQuantity);
                AddItemToNpc(trader, itemName, itemQuantity);

                //swap gold from player to trader 
                AddGoldToPlayer(sellingPrice);
                trader.Gold -= sellingPrice;

                //display trader's/player's inventories to update their content
                InventoryInfo(trader, true);
                InventoryInfo(Data.Player!, true);
            }
            else
            {
                PrintMessage("Handlarza nie stać na taki zakup", MessageType.SystemFeedback);
            }
        }

        //method handling 'use' command
        private void UseHandler(string itemName)
        {
            Item itemToUse;

            ResetPlayerState();

            //if 'use' was typed without any argument
            if (itemName == string.Empty)
            {
                PrintMessage("Musisz podać nazwę przedmiotu", MessageType.SystemFeedback);
                return;
            }

            if (Data.Player!.Inventory!.Exists(item => FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName)))
            {
                itemToUse = Data.Items!.Find(item => FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName))!;

                if (itemToUse.GetType() == typeof(Consumable))
                {
                    CharAction itemUse = new ItemUse(Data.Player!, itemToUse);
                    AddAction(itemUse);
                }
                else
                {
                    PrintMessage("Nie możesz użyć tego przedmiotu", MessageType.SystemFeedback);
                }
            }
            else
            {
                PrintMessage("Nie posiadasz przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
                return;
            }

            

        }   

        //method handling 'stats' command
        private void StatsHandler()
        {
            StatsInfo();

            //trigger attributes hint

            if (Hints.HintsOnOff)
            {
                if (Hints.AttributesHint)
                {
                    PrintHint(Hints.HintType.Attributes);
                }
            }
        }

        //method handling 'drop' command
        private void DropHandler(string itemName, string quantity)
        {
            int itemIndex = Data.Player!.Inventory!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower())
            == FlattenPolishChars(itemName));
            int itemQuantity;
            Item itemToRemove = new Item();

            //handle lack of argument
            if (itemName == string.Empty)
            {
                PrintMessage("Musisz podać nazwę przedmiotu", MessageType.SystemFeedback);
                return;
            }

            ResetPlayerState();

            if (itemIndex == -1 && itemName != "złoto" && itemName != "zloto")
            {
                PrintMessage("Nie posiadasz przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
                return;
            }

            if (itemName != "złoto" && itemName != "zloto")
            {
                itemToRemove = Data.Player!.Inventory[itemIndex];
            }

            //set item quantity depedning on 2nd argument if it's not empty
            if (quantity == "all" || quantity == "a")
            {

                //depending if it's item or gold
                if (itemName != "złoto" && itemName != "zloto")
                {
                    itemQuantity = itemToRemove.Quantity;
                }
                else
                {
                    itemQuantity = Data.Player.Gold;
                }
            }
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }

            //if player typed drop without quantity argument (ConvertQuantityString method
            //assigns 0 to it's out parameter) - set quantity to 1
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }

            //if the item name is 'zloto' drop gold
            if (itemName == "złoto" || itemName == "zloto")
            {

                //if player wants to drop more quantity of gold than he has,
                //prevent doing so, and print appropriate system message
                if (Data.Player!.Gold < itemQuantity)
                {
                    PrintMessage("Próbujesz wyrzucić więcej niż posiadasz..", MessageType.SystemFeedback);
                    return;
                }

                PrintMessage("Upuszczasz " + itemQuantity + " złota", MessageType.Action);
                RemoveGoldFromPlayer(itemQuantity);
                AddGoldToLocation(Data.Player.CurrentLocation!, itemQuantity);

                return;
            }

            //if player wants to drop more quantity of items than he actually has
            //set quantity to equal actual item quantity and drop it all
            if (itemToRemove.Quantity < itemQuantity)
            {
                itemQuantity = itemToRemove.Quantity;
            }

            PrintMessage("Upuszczasz " + itemQuantity + " " + itemToRemove.Name, MessageType.Action);
            RemoveItemFromPlayer(itemName, itemQuantity);
            AddItemToLocation(Data.Player!.CurrentLocation!, itemName, itemQuantity);
        }

        //method handling 'pickup' command
        private void PickupHandler(string itemName, string quantity)
        {
            int itemIndex = Data.Player!.CurrentLocation!.Items!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower())
            == FlattenPolishChars(itemName));
            int itemQuantity = 0;
            Item itemToPickup = new Item();

            if (Data.Player.CurrentState == CombatCharacter.State.Combat)
            {
                PrintMessage("Nie możesz tego zrobić w trakcie walki!", MessageType.SystemFeedback);
                return;
            }

            ResetPlayerState();

            //handle lack of argument
            if (itemName == string.Empty)
            {
                List<Item> itemsToPickup = new List<Item>();

                Data.Player!.CurrentLocation.Items.ForEach(item =>
                {
                    itemsToPickup.Add(item);
                });

                //if there are any items in player's current location
                if (itemsToPickup.Count > 0)
                {
                    itemsToPickup.ForEach(item => 
                    {
                        PrintMessage("Podnosisz " + item.Quantity + " " + item.Name, MessageType.Action);
                        AddItemToPlayer(item.Name!, item.Quantity);
                        Data.Player!.CurrentLocation!.RemoveItem(item.Name!, item.Quantity);
                    });

                    //pick up the gold also
                    if (Data.Player!.CurrentLocation.Gold > 0)
                    {
                        PrintMessage("Podnosisz " + Data.Player!.CurrentLocation.Gold + " złota", MessageType.Action);
                        AddGoldToPlayer(Data.Player!.CurrentLocation.Gold);
                        Data.Player!.CurrentLocation.Gold = 0;
                    }
                }

                //else, if there are no items but only gold on the ground
                else if (Data.Player!.CurrentLocation.Gold > 0)
                {
                    PrintMessage("Podnosisz " + Data.Player!.CurrentLocation.Gold + " złota", MessageType.Action);
                    AddGoldToPlayer(Data.Player!.CurrentLocation.Gold);
                    Data.Player!.CurrentLocation.Gold = 0;
                }
                else
                {
                    PrintMessage("Nie ma tu niczego co można podnieść", MessageType.SystemFeedback);
                }

                return;
            }

            if (itemIndex == -1 && itemName != "złoto" && itemName != "zloto")
            {
                PrintMessage("Nie ma tu przedmiotu o nazwie \"" + itemName + "\"", MessageType.SystemFeedback);
                return;
            }

            if (itemName != "złoto" && itemName != "zloto")
            {
                itemToPickup = Data.Player!.CurrentLocation!.Items[itemIndex];
            }

                       

            //if the item name is 'zloto' pickup gold
            if (itemName == "złoto" || itemName == "zloto")
            {

                //set gold quantity
                if (quantity == String.Empty)
                {
                    itemQuantity = Data.Player!.CurrentLocation!.Gold;
                }
                else if (!ConvertQuantityString(quantity, out itemQuantity))
                {
                    PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                    return;
                }

                //if player wants to pick up more quantity of gold than there is in his current
                //location, display appropriate system message
                if (Data.Player!.CurrentLocation.Gold < itemQuantity || itemQuantity == 0)
                {
                    PrintMessage("Nie ma tu takiej ilości złota..", MessageType.SystemFeedback);
                    return;
                }

                //if there is any gold on the ground..
                if (itemQuantity != 0)
                {
                    PrintMessage("Podnosisz " + itemQuantity + " złota", MessageType.Action);
                    Data.Player!.CurrentLocation.Gold -= itemQuantity;
                    AddGoldToPlayer(itemQuantity);
                }
                else
                {
                    PrintMessage("Nie ma tu żadnego złota", MessageType.SystemFeedback);
                }

                return;
            }

            //set item quantity depedning on 2nd argument if it's not empty
            if (quantity == "all" || quantity == "a")
            {

                
            }
            else if (!ConvertQuantityString(quantity, out itemQuantity))
            {
                PrintMessage("Niepoprawna ilość", MessageType.SystemFeedback);
                return;
            }

            //if player wants to pick up more quantity of items than there are in his current
            //location
            if (itemToPickup.Quantity < itemQuantity)
            {
                PrintMessage("Za dużo..", MessageType.SystemFeedback);
                return;
            }

            //if player typed pickup without quantity argument (ConvertQuantityString method
            //assigns 0 to it's out parameter) - set quantity to 1
            if (itemQuantity == 0)
            {
                itemQuantity = 1;
            }

            PrintMessage("Podnosisz " + itemQuantity + " " + itemToPickup.Name, MessageType.Action);
            AddItemToPlayer(itemName, itemQuantity);
            Data.Player!.CurrentLocation!.RemoveItem(itemName, itemQuantity);

        }

        //method handling 'wear' command
        private void WearHandler(string itemName)
        {
            //handle lack of argument
            if (itemName == string.Empty)
            {
                PrintMessage("Musisz podać nazwę przedmiotu", MessageType.SystemFeedback);
                return;
            }

            int itemIndex = Data.Player!.Inventory!.FindIndex(item => FlattenPolishChars(item.Name!.ToLower())
            == FlattenPolishChars(itemName));

            if (itemIndex == -1)
            {
                PrintMessage("Nie posiadasz takiego przedmiotu", MessageType.SystemFeedback);
                return;
            }

            Item itemToWear = Data.Items!.Find(item => 
            FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName))!;
            
            if (itemToWear.GetType() == typeof(Armor))
            {
                WearArmorOnPlayer(itemName);

                //trigger takeoff hint
                if (Hints.HintsOnOff && Hints.TakeoffHint)
                {
                    PrintHint(Hints.HintType.Takeoff, itemName.ToLower());
                }
            }
            else if (itemToWear.GetType() == typeof(Weapon))
            {
                WearWeaponOnPlayer(itemName);

                //trigger takeoff hint
                if (Hints.HintsOnOff && Hints.TakeoffHint)
                {
                    PrintHint(Hints.HintType.Takeoff, itemName.ToLower());
                }
            }
            else
            {
                PrintMessage("Nie możesz założyć tego przedmiotu", MessageType.SystemFeedback);
                return;
            }

        }

        //method for handling 'takeoff' command
        private void TakeoffHandler(string slotName)
        {
            //handle lack of argument
            if (slotName == string.Empty)
            {
                PrintMessage("Musisz podać typ zakładanego przedmiotu (weapon, helmet, torso, pants, gloves, shoes)",
                    MessageType.SystemFeedback);
                return;
            }

            switch (slotName)
            {
                case "weapon":
                    if (!TakeOffWeaponFromPlayer())
                    {
                        PrintMessage("Nie jesteś uzbrojony", MessageType.SystemFeedback);
                    }
                    break;
                case "helmet":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Helmet))
                    {
                        PrintMessage("Nie nosisz żadnego hełmu", MessageType.SystemFeedback);
                    }
                    break;
                case "torso":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Torso))
                    {
                        PrintMessage("Nie nosisz żadnego korpusu", MessageType.SystemFeedback);
                    }
                    break;
                case "pants":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Pants))
                    {
                        PrintMessage("Nie nosisz żadnych spodni (Trochę wstyd..)", MessageType.SystemFeedback);
                    }
                    break;
                case "gloves":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Gloves))
                    {
                        PrintMessage("Nie nosisz żadnych rękawic", MessageType.SystemFeedback);
                    }
                    break;
                case "shoes":
                    if (!TakeOffArmorFromPlayer(Armor.ArmorType.Shoes))
                    {
                        PrintMessage("Nie nosisz żadnych butów (Uważaj po czym stąpasz..)", MessageType.SystemFeedback);
                    }
                    break;
                default:
                    PrintMessage("\"" + slotName + "\" Nie jest prawidłowym typem zakładanego przedmiotu. " +
                        "Prawidłowe typy to: weapon, helmet, torso, pants, gloves, shoes", MessageType.SystemFeedback);
                    break;
            }
        }

        //method for stopping actions/states
        private void StopHandler()
        {
            //stop all queued actions
            Actions.Clear();

            //stop attacking
            int instanceIndex = AttackInstances.FindIndex(ins => ins.Attacker == Data.Player!);
            if (instanceIndex != -1)
            {
                PrintMessage("Przestajesz atakować postać: " + AttackInstances[instanceIndex].Receiver.Name, MessageType.Action);
                AttackInstances.RemoveAt(instanceIndex);
                return;
            }

            //stop trade etc
            ResetPlayerState();
        }

        //method for handling 'attack' command
        private void AttackHandler(string characterName)
        {

            //break trade/talk state if needed
            ResetPlayerState();

            //if user typed 'attack' without argument
            if (characterName == string.Empty)
            {

                //if there are no opponents currently fighting with player
                if (Data.Player!.CurrentState != CombatCharacter.State.Combat)
                {
                    int combatCharIndex = Data.Player!.CurrentLocation!.Characters!.FindIndex(
                        character => character is CombatCharacter && character != Data.Player!);
                    if (combatCharIndex != -1)
                    {
                        AttackCharacter(Data.Player!, 
                            (CombatCharacter)Data.Player!.CurrentLocation!.Characters[combatCharIndex]);
                    }
                    else
                    {
                        PrintMessage("Nie ma tu nikogo do zaatakowania", MessageType.SystemFeedback);
                    }
                }

                //if player is fighting only with single opponent, attack that opponent
                else if (Data.Player.Opponents.Count == 1)
                {
                    AttackCharacter(Data.Player, Data.Player.Opponents[0]);
                }

                //if player if fighting with multiple opponents
                else
                {
                    CombatCharacter weakestOpponent = new CombatCharacter("placeholder");
                    weakestOpponent.Level = 9999999;
                    bool opponentFound = false;

                    //find the opponent with the lowest level
                    Data.Player!.Opponents!.ForEach(op =>
                    {
                        if (op.Level < weakestOpponent.Level)
                        {
                            weakestOpponent = op;
                            opponentFound = true;
                        }
                    });

                    if (opponentFound)
                    {
                        AttackCharacter(Data.Player, weakestOpponent);
                    }
                }
                return;
            }

            //else if player wanted to attack specific character (typed attack with 1 argument)
            int characterIndex = Data.Player!.CurrentLocation!.Characters!
                .FindIndex(character => FlattenPolishChars(character.Name!.ToLower())
                == FlattenPolishChars(characterName));

            if (characterIndex == -1)
            {
                PrintMessage("Nie ma tu takiej postaci", MessageType.SystemFeedback);
                return;
            }

            Character characterToAttack = Data.Player!.CurrentLocation!.Characters[characterIndex];

            if (!(characterToAttack is CombatCharacter))
            {
                PrintMessage("Nie możesz zaatakować tej postaci", MessageType.SystemFeedback);
                return;
            }

            AttackCharacter(Data.Player, (characterToAttack as CombatCharacter)!);

        }

        //method for handling 'flee' command
        private void FleeHandler(string direction)
        {
            string directionToFlee = string.Empty;

            //prevent fleeing when not fighting
            if (Data.Player!.CurrentState != CombatCharacter.State.Combat)
            {
                PrintMessage("Nie masz przed czym uciekać..", MessageType.SystemFeedback);
                return;
            }

            //if no direction is specified, pick random direction from all
            //possible directions
            if (direction == String.Empty)
            {
                Location temp = new Location();
                List<string> randomDirections = new List<string>();
                string[] directionLetters = new string[6];

                directionLetters[0] = "n";
                directionLetters[1] = "e";
                directionLetters[2] = "s";
                directionLetters[3] = "w";
                directionLetters[4] = "u";
                directionLetters[5] = "d";

                foreach (string letter in directionLetters)
                {
                    if (GetNextLocation(letter, out temp))
                    {
                        randomDirections.Add(letter);
                    }
                }

                int randomDirectionNumber = Rand.Next(0, randomDirections.Count);

                directionToFlee = randomDirections[randomDirectionNumber];
            }
            else
            {
                switch (direction)
                {
                    case "north":
                    case "n":
                        directionToFlee = "n";
                        break;
                    case "east":
                    case "e":
                        directionToFlee = "e";
                        break;
                    case "south":
                    case "s":
                        directionToFlee = "s";
                        break;
                    case "west":
                    case "w":
                        directionToFlee = "w";
                        break;
                    case "up":
                    case "u":
                        directionToFlee = "u";
                        break;
                    case "down":
                    case "d":
                        directionToFlee = "d";
                        break;
                    default:
                        PrintMessage("Nie ma takiego kierunku jak \"" + direction + "\"", MessageType.SystemFeedback);
                        return;
                }
            }

            Location escapeDestination;

            if (GetNextLocation(directionToFlee, out escapeDestination))
            {
                FleeAttempt fleeAttempt = new FleeAttempt(Data.Player!, escapeDestination, 30);
                AddAction(fleeAttempt);
            }
            else
            {
                PrintMessage("Tam nie uciekniesz, to ślepa uliczka!", MessageType.SystemFeedback);
            }
            
        }

        //method for spending attribute pointsd
        private void PointHandler(string attribute)
        {
            Player player = Data.Player!;
            string attributeWord;

            //handle lack of argument
            if (attribute == string.Empty)
            {
                PrintMessage("Musisz podać nazwę atrybutu " +
                    "(strength, agility, intelligence (lub str, agi, int))", MessageType.SystemFeedback);
                return;
            }

            if (player.AttributePoints < 1)
            {
                PrintMessage("Nie masz już więcej punktów atrybutów!", MessageType.SystemFeedback);
                return;
            }

            switch (attribute)
            {
                case "strength":
                case "str":
                    attributeWord = "siła";
                    player.Strength++;
                    break;
                case "agility":
                case "agi":
                    attributeWord = "zręczność";
                    player.Agility++;
                    break;
                case "intelligence":
                case "int":
                    attributeWord = "inteligencja";
                    player.Intelligence++;
                    break;
                default:
                    PrintMessage("\"" + attribute + "\" nie jest poprawną nazwą atrybutu. Poprawne nazwy" +
                        "to: strength, agility, intelligence (lub str, agi, int)", MessageType.SystemFeedback);
                    return;
            }

            player.AttributePoints--;
            PrintMessage("Twoja " + attributeWord + " zwiększa się o 1!");
        }

        //method for crafting spells from runes combinations
        private void CraftHandler(string firstRune, string secondRune)
        {
            bool isCombinationDouble = false;
            string spellName = string.Empty;
            string[] runeNames = new string[5] { "iskarr", "akull", "verde", "xitan", "dara" };
            Spell craftedSpell = new Spell();

            if (firstRune == String.Empty)
            {
                PrintMessage("Musisz podać nazwę runy", MessageType.SystemFeedback);
                return;
            }

            //choose spell depending on single rune choice
            if (secondRune == string.Empty)
            {
                switch (firstRune)
                {
                    case "iskarr":
                        spellName = "błogosławieństwo";
                        break;
                    case "akull":
                        spellName = "powłoka_nur'zhel";
                        break;
                    case "verde":
                        spellName = "zdrewniała_skóra";
                        break;
                    case "xitan":
                        spellName = "wampirze_ostrze";
                        break;
                    case "dara":
                        spellName = "niebiańska_łaska";
                        break;

                        //if the first rune name is incorrect
                    default:
                        PrintMessage("Nie istnieje runa o nazwie \"" + firstRune + "\"", MessageType.SystemFeedback);
                        return;
                }
            }

            //choose spell depending on double runes combination
            else
            {
                isCombinationDouble = true;

                //if the second rune name is incorrect
                if (!runeNames.Contains(secondRune))
                {
                    PrintMessage("Nie istnieje runa o nazwie \"" + secondRune + "\"", MessageType.SystemFeedback);
                    return;
                }

                //iskarr-akull combination
                if (firstRune == runeNames[0] && secondRune == runeNames[1] || firstRune == runeNames[1] && secondRune == runeNames[0])
                {
                    spellName = "eksplozja";
                }

                //iskarr-verde
                else if (firstRune == runeNames[0] && secondRune == runeNames[2] || firstRune == runeNames[2] && secondRune == runeNames[0])
                {
                    spellName = "kula_ognia";
                }

                //iskarr-xitan
                else if (firstRune == runeNames[0] && secondRune == runeNames[3] || firstRune == runeNames[3] && secondRune == runeNames[0])
                {
                    spellName = "kula_chaosu";
                }

                //iskarr-dara
                else if (firstRune == runeNames[0] && secondRune == runeNames[4] || firstRune == runeNames[4] && secondRune == runeNames[0])
                {
                    spellName = "dusza_paladyna";
                }

                //akull-verde
                else if (firstRune == runeNames[1] && secondRune == runeNames[2] || firstRune == runeNames[2] && secondRune == runeNames[1])
                {
                    spellName = "dotyk_śmierci";
                }

                //akull-xitan
                else if (firstRune == runeNames[1] && secondRune == runeNames[3] || firstRune == runeNames[3] && secondRune == runeNames[1])
                {
                    spellName = "tarcza_mentalna";
                }

                //akull-dara
                else if (firstRune == runeNames[1] && secondRune == runeNames[4] || firstRune == runeNames[4] && secondRune == runeNames[1])
                {
                    spellName = "fala_mrozu";
                }

                //verde-xitan
                else if (firstRune == runeNames[2] && secondRune == runeNames[3] || firstRune == runeNames[3] && secondRune == runeNames[2])
                {
                    spellName = "wcielenie_zabójcy";
                }

                //verde-dara
                else if (firstRune == runeNames[2] && secondRune == runeNames[4] || firstRune == runeNames[4] && secondRune == runeNames[2])
                {
                    spellName = "amok";
                }

                //xitan-dara
                else if (firstRune == runeNames[3] && secondRune == runeNames[4] || firstRune == runeNames[4] && secondRune == runeNames[3])
                {
                    spellName = "szpon_zaświatów";
                }
                else
                {
                    PrintMessage("Runy zaiskrzyły, ale nic się nie stało..");
                    return;
                }
            }

            //check if player possesses required runes
            if (!Data.Player!.Inventory!.Exists(item => FlattenPolishChars(item.Name!.ToLower())
            == FlattenPolishChars(firstRune)))
            {
                PrintMessage("Nie posiadasz runy " + firstRune, MessageType.SystemFeedback);
                return;
            }
            if (isCombinationDouble)
            {
                if (!Data.Player!.Inventory!.Exists(item => FlattenPolishChars(item.Name!.ToLower()) ==
                    FlattenPolishChars(secondRune)))
                {
                    PrintMessage("Nie posiadasz runy " + secondRune, MessageType.SystemFeedback);
                    return;
                }
            }

            //check if player has enough mana to craft a spell
            if (Data.Player!.Mp < 200)
            {
                PrintMessage("Nie masz wystarczającej ilości many aby to zrobić (potrzeba 200)", MessageType.SystemFeedback);
                return;
            }

            //choose proper spell
            craftedSpell = Data.Spells!.Find(spell => FlattenPolishChars(spell.Name!.ToLower())
            == FlattenPolishChars(spellName))!;

            //add spellcraft action to the queue
            CharAction SpellCraft = new SpellCraft(Data.Player!, craftedSpell);
            Actions.Add(SpellCraft);
        }

        //method showing player's remembered spells
        private void SpellsHandler()
        {
            SpellsInfo(Data.Player!);
        }

        //method for handling spell casting
        private void CastHandler(string spellNumberAsString, string targetName)
        {
            int i;
            int spellNumber;
            int numberOfSpells = Data.Player!.RememberedSpells!.Count;
            int[] numbers = new int[numberOfSpells];

            //break trade/talk state if needed
            ResetPlayerState();

            //if user typed 'cast' without any arguments
            if (spellNumberAsString == string.Empty)
            {
                PrintMessage("Musisz wybrać numer czaru", MessageType.SystemFeedback);
                return;
            }

            //check if string representation of spellnumber is correct integer value,
            //and convert it to int if it is
            if (!Int32.TryParse(spellNumberAsString, out spellNumber))
            {
                PrintMessage("Niepoprawny numer czaru");
                return;
            }

            for (i = 0; i < numberOfSpells; i++)
            {
                numbers[i] = i + 1;
            }

            Spell spellToCast = new Spell("placeholder");
            CombatCharacter target = new CombatCharacter("placeholder");

            if (numbers.Contains(spellNumber))
            {
                spellToCast = Data.Player.RememberedSpells[spellNumber - 1];

                //check if user's character has enough mana to cast the spell
                if (spellToCast.ManaCost > Data.Player.Mp)
                {
                    PrintMessage("Masz za mało many aby użyć tego czaru!", MessageType.SystemFeedback);
                    return;
                }

                //if user typed only first argument (spellnumber)
                if (targetName == string.Empty)
                {

                    //if the spell is a buff/heal etc. (default target is self)
                    if (spellToCast.DefaultTarget == Spell.Target.Self)
                    {

                        //if the spell is 'Demoniczny_portal'
                        if (spellToCast.Name == "Demoniczny_portal")
                        {
                            PrintMessage("Musisz wybrać miejsce, do którego chcesz się przenieść", MessageType.SystemFeedback);
                            return;
                        }

                        target = Data.Player;
                    }
                    else
                    {
                        //if player isn't fighting with anyone
                        //if (Data.Player.CurrentState != CombatCharacter.State.Combat)
                        //{
                        //    PrintMessage("Obecnie z nikim nie walczysz", MessageType.SystemFeedback);
                        //    return;
                        //}
                        if (Data.Player!.CurrentState != CombatCharacter.State.Combat)
                        {
                            int combatCharIndex = Data.Player!.CurrentLocation!.Characters!.FindIndex(
                                character => character is CombatCharacter && character != Data.Player!);
                            if (combatCharIndex != -1)
                            {
                                target = (CombatCharacter)Data.Player!.CurrentLocation!.Characters[combatCharIndex];
                            }
                            else
                            {
                                PrintMessage("Nie ma tu nikogo do zaatakowania", MessageType.SystemFeedback);
                                return;
                            }
                        }

                        //else, if player is attacking a character
                        else if (AttackInstances.Exists(ins => ins.Attacker == Data.Player!))
                        {
                            target = AttackInstances.Find(ins => ins.Attacker == Data.Player!)!.Receiver;
                        }

                        //else if player is fighting with someone but not attacking anyone,
                        //choose the weakest target from his opponents
                        else
                        {
                            CombatCharacter weakestOpponent = new CombatCharacter("placeholder");
                            weakestOpponent.Level = 9999999;

                            //find the opponent with the lowest level
                            Data.Player!.Opponents!.ForEach(op =>
                            {
                                if (op.Level < weakestOpponent.Level)
                                {
                                    weakestOpponent = op;
                                }
                            });

                            target = weakestOpponent;
                        }
                    }
                }

                //else, if player typed second argument (target name)
                else if (spellToCast.Name != "Demoniczny_portal")
                {
                    //prevent casting self-spells on enemy
                    if (spellToCast.DefaultTarget == Spell.Target.Self)
                    {
                        PrintMessage("Nie możesz użyć tego czaru na wrogu", MessageType.SystemFeedback);
                        return;
                    }

                    int characterIndex = Data.Player!.CurrentLocation!.Characters!.
                        FindIndex(character => FlattenPolishChars(character.Name!.ToLower())
                        == FlattenPolishChars(targetName));

                    //if there is no character with specified name
                    if (characterIndex == -1)
                    {
                        PrintMessage("Nie ma tu postaci o imieniu \"" + targetName + "\"", MessageType.SystemFeedback);
                        return;
                    }

                    Character specifiedTarget = Data.Player!.CurrentLocation!.Characters![characterIndex];

                    //if specified target isn't combat character
                    if (!(specifiedTarget is CombatCharacter)) 
                    {
                        PrintMessage("Nie możesz zaatakować tej postaci", MessageType.SystemFeedback);
                        return;
                    }

                    target = (specifiedTarget as CombatCharacter)!;
                }
            }
            else
            {
                PrintMessage("Nie pamiętasz czaru o tym numerze");
                return;
            }

            //change target to self for teleport spell, and set castLocation to location
            //of player's choice
            if (spellToCast.Name == "Demoniczny_portal")
            {
                
                int locationIndex = Data.Locations!.FindIndex(loc => 
                FlattenPolishChars(loc.Name!.ToLower()) == FlattenPolishChars(targetName));
                if (locationIndex != -1)
                {
                    TeleportLocation = Data.Locations![locationIndex];
                    
                    if (!TeleportLocation.IsVisible)
                    {
                        PrintMessage("Nie znasz lokacji o nazwie \"" + targetName + "\"", MessageType.SystemFeedback);
                        return;
                    }
                }
                else
                {
                    PrintMessage("Nie znasz lokacji o nazwie \"" + targetName + "\"", MessageType.SystemFeedback);
                    return;
                }

                target = Data.Player!;
            }
            
            //add action to queue
            CharAction spellcast = new SpellCast(Data.Player, target, spellToCast);
            AddAction(spellcast);
        }

        //method for handling 'effects' command
        private void EffectsHandler()
        {
            List<EffectOnPlayer> playerEffects = Data.Player!.Effects!;
            string effectLine = string.Empty;

            if (playerEffects.Count == 0)
            {
                PrintMessage("W tej chwili żaden efekt na Ciebie nie wpływa", MessageType.SystemFeedback);
                return;
            }

            playerEffects.ForEach(eff =>
            {
                effectLine = "* " + eff.Name + ": ";

                //add modifiers descriptions to effectLine
                Data.Player!.Modifiers!.ForEach(mod =>
                {
                    if (mod.Parent == eff.Name)
                    {
                        effectLine += GetModDescription(mod) + ", ";
                    }
                });

                //remove trailing comma after last modifier
                effectLine = Regex.Replace(effectLine, @",\s$", "");

                //add effect duration
                effectLine += " " + TimeValueFromSeconds(Data.Player!.Modifiers!.Find(mod => mod.Parent == eff.Name)!.DurationInTicks / 10);

                //print effect
                PrintMessage(effectLine, MessageType.EffectOn);
            });
        }

        //method for handling game speed manipulation
        private void ChangeSpeedHandler(bool isSlowDown)
        {
            if (isSlowDown)
            {
                if (TimeInterval >= 100)
                {
                    PrintMessage("Gra działa w minimalnej szybkości!", MessageType.SystemFeedback);
                }
                else
                {
                    TimeInterval *= 2;
                    GameClock.Stop();
                    GameClock.Interval = TimeSpan.FromMilliseconds(TimeInterval);
                    GameClock.Start();
                }
            }
            else
            {
                if (TimeInterval <= MinTimeInterval)
                {
                    PrintMessage("Gra działa w maksymalnej szybkości!", MessageType.SystemFeedback);
                }
                else
                {
                    TimeInterval /= 2;
                    GameClock.Stop();
                    GameClock.Interval = TimeSpan.FromMilliseconds(TimeInterval);
                    GameClock.Start();
                }
            }

            int GameSpeed = 100 / TimeInterval;

            PrintMessage("Szybkość gry: " + GameSpeed + "x", MessageType.SystemFeedback);
        }

        //method handling turning on/off hints
        private void HintsHandler()
        {
            if (Hints.HintsOnOff)
            {
                Hints.HintsOnOff = false;
                PrintMessage("Podpowiedzi wyłączone! (wpisz \"hints\" aby włączyć)", MessageType.SystemFeedback);
            }
            else
            {
                Hints.HintsOnOff = true;
                PrintMessage("Podpowiedzi włączone!", MessageType.SystemFeedback);
            }
        }








        //==============================================MANIPULATION METHODS=============================================

        //method simulating rune drop when monster dies and player achieves certain requirements
        private void TryRuneDrop()
        {
            

            if (Data.Player!.RunesAlreadyReceived == 0)
            {
                if (Data.Player!.Intelligence < firstIntThreshold)
                {
                    return;
                }
            }
            else if (Data.Player!.RunesAlreadyReceived == 1)
            {
                if (Data.Player!.Intelligence >= firstIntThreshold 
                    && Data.Player.Intelligence < secondIntThreshold)
                {
                    return;
                }
            }
            else if (Data.Player!.RunesAlreadyReceived == 2)
            {
                if (Data.Player!.Intelligence >= secondIntThreshold 
                    && Data.Player.Intelligence < thirdIntThreshold)
                {
                    return;
                }
            }
            else if (Data.Player!.RunesAlreadyReceived == 3)
            {
                if (Data.Player!.Intelligence >= thirdIntThreshold 
                    && Data.Player.Intelligence < fourthIntThreshold)
                {
                    return;
                }
            }
            else if (Data.Player!.RunesAlreadyReceived == 4)
            {
                if (Data.Player!.Intelligence >= fourthIntThreshold 
                    && Data.Player.Intelligence < fifthIntThreshold)
                {
                    return;
                }
            }


            //first gather all 5 runes in separate list
            List<Item> runes = new List<Item>();
            Data.Items!.ForEach(it =>
            {
                if (it.GetType() == typeof(RuneStone))
                {
                    //add only these, which player doesn't own yet
                    if (!Data.Player!.Inventory!.Exists(playerIt => playerIt.Name! == it.Name!))
                    {
                        runes.Add(it);
                    }
                }
            });

            //prevent doing anything if player already has all runes
            if (runes.Count == 0)
            {
                return;
            }

            //choose random rune index
            int randomRuneIndex = Rand.Next(0, runes.Count);

            //simulate monster dropping the rune
            AddItemToLocation(Data.Player!.CurrentLocation!, runes[randomRuneIndex].Name!, 1);

            //increment RunesAlreadyReceived
            Data.Player!.RunesAlreadyReceived++;
        }

        //method triggering the drop chance for every monster dying
        private void TriggerDropChance(CombatCharacter dyingChar)
        {
            //prevent dropping anything if it's rat dying
            if (dyingChar.Name!.ToLower() == "szczur")
            {
                return;
            }

            int quantityBase = Convert.ToInt32(Math.Sqrt(dyingChar.Level / 2));
            int lowTierQuantity;
            double lowTierLimit = 17;
            double mediumTierLimit = dyingChar.Level * 30;
            double highTierLimit = dyingChar.Level * 100;

            Item lowTierItem = new Item("placeholder");
            Item highConsumablesItem = new Item("placeholder");
            Item mediumTierItem = new Item("placeholder");
            //Item highTierItem = new Item("placeholder");

            List<Item> lowTierPool = new List<Item>();
            List<Item> highConsumablesPool = new List<Item>();
            List<Item> mediumTierPool = new List<Item>();
            //List<Item> highTierPool = new List<Item>();

            //if quantityBase is lower than 1 set it to 1
            if (quantityBase < 1)
            {
                quantityBase = 1;
            }
            lowTierQuantity = Rand.Next(1, quantityBase);

            //search for all low tier items and put them is separate pool
            //and then pick random out of it
            Data.Items!.ForEach(it =>
            {
                if (it.Price < lowTierLimit)
                {
                    lowTierPool.Add(it);
                }
            });
            if (lowTierPool.Count > 0)
            {
                lowTierItem = lowTierPool[Rand.Next(0, lowTierPool.Count)];
            }

            //same for high consumables
            Data.Items!.ForEach(it =>
            {
                if (it.GetType() == typeof(Consumable) && it.Price >= lowTierLimit 
                && it.Price < 100)
                {
                    if (!Regex.Match(it.Name!.ToLower(), @"kula_portalowa").Success &&
                    it.Name!.ToLower() != "derillońska" && it.Name!.ToLower() != "piwo" &&
                    it.Name!.ToLower() != "stek" && it.Name!.ToLower() != "diabelski_pył")
                    {
                        highConsumablesPool.Add(it);
                    }
                }
            });
            if (highConsumablesPool.Count > 0)
                {
                    highConsumablesItem = highConsumablesPool[Rand.Next(0, highConsumablesPool.Count)];
                }

            //same for medium tier
            Data.Items!.ForEach(it =>
            {
                if (it.Price < mediumTierLimit && it.Price >= mediumTierLimit * 0.5 
                && it.GetType() != typeof(Consumable))
                {
                    if (!Regex.Match(it.Name!.ToLower(), @"kula_portalowa").Success)
                    {
                        mediumTierPool.Add(it);
                    }
                }
            });
            if (mediumTierPool.Count > 0)
            {
                mediumTierItem = mediumTierPool[Rand.Next(0, mediumTierPool.Count)];
            }

            //high tier
            //Data.Items!.ForEach(it =>
            //{
            //    if (it.Price < highTierLimit && it.Price >= highTierLimit * 0.5)
            //    {
            //        highTierPool.Add(it);
            //    }
            //});
            //if (highTierPool.Count > 0)
            //{
            //    highTierItem = highTierPool[Rand.Next(0, highTierPool.Count)];
            //}

            double lowTierChance = 0.4;
            double highConsumableChance = 0.015 * NthRoot(dyingChar.Level, 1.2);
            double mediumTierChance = 0.02;
            //double highTierChance = 0.3;

            //modify chance drop for heroes(bosses)
            if (dyingChar.GetType() == typeof(Hero))
            {
                mediumTierChance = 1.0;
            }

            //try dropping low tier item
            if (dyingChar.Level < 31)
            {
                if (lowTierItem.Name != "placeholder" && TryOutChance(lowTierChance))
                {
                    AddItemToLocation(dyingChar.CurrentLocation!, lowTierItem.Name!, lowTierQuantity);
                }
            }

            //try dropping high consumable item
            if (highConsumablesItem.Name != "placeholder" && TryOutChance(highConsumableChance))
            {
                AddItemToLocation(dyingChar.CurrentLocation!, highConsumablesItem.Name!, 1);
            }

            //try dropping medium tier item
            if (mediumTierItem.Name != "placeholder" && TryOutChance(mediumTierChance) && 
                dyingChar.Level > 2)
            {
                AddItemToLocation(dyingChar.CurrentLocation!, mediumTierItem.Name!, 1);
            }

            //try dropping stek/mikstura_zycia when it's arimesian enemy dying
            if (dyingChar.Level > 30 && TryOutChance(0.30))
            {
                string[] itemsNames = new string[2] { "Stek", "Mikstura_życia" };

                AddItemToLocation(dyingChar.CurrentLocation!, itemsNames[Rand.Next(0, 2)], 1);
            }

            //try dropping high tier item
            //if (highTierItem.Name != "placeholder" && TryOutChance(highTierChance) &&
            //    dyingChar.GetType() == typeof(Hero))
            //{
            //    AddItemToLocation(dyingChar.CurrentLocation!, highTierItem.Name!, 1);
            //}
        }

        ///<summary>
        /// method performing action of location change and displaying 
        /// message saying that player is walking towards chosen direction
        /// if the message is dispensable - put "none" as second argument
        /// </summary>
        /// <param name="nextLocation"></param>
        /// <param name="directionString"></param>
        private void ChangePlayerLocation(Location nextLocation, string directionString)
        {
            if (directionString != "none")
            {
                PrintMessage("Idziesz " + directionString, MessageType.Action);
            }

            //remove player from previous location
            Data.Player!.CurrentLocation!.Characters!.Remove(Data.Player);

            //change player's current location
            AddCharacterToLocation(nextLocation, Data.Player!);

            //change minimap display
            PrintMap();

            //print characters welcome phrases
            nextLocation.Characters!.ForEach(ch =>
            {
                if (ch.GetType() == typeof(Trader))
                {
                    if ((ch as Trader)!.WelcomePhrase != "placeholder")
                    {
                        PrintSpeech(ch, (ch as Trader)!.WelcomePhrase);
                    }
                }
                else if (ch.GetType() == typeof(Hero) && (ch as Hero)!.Aggressiveness != Monster.AggressionType.Aggressive) 
                {
                    if ((ch as Hero)!.WelcomePhrase != "placeholder")
                    {
                        PrintSpeech(ch, (ch as Hero)!.WelcomePhrase);
                    }
                }
            });

            //display hints
            if (Hints.HintsOnOff)
            {

                //look hint
                if (Hints.LookHint)
                {
                    PrintHint(Hints.HintType.Look);
                }

                //attack hint
                if (Hints.AttackHint && nextLocation.Characters!.Exists(ch => ch.GetType() == typeof(Monster)))
                {
                    Monster monsterToAttack = (nextLocation.Characters!.Find(ch => ch.GetType() == typeof(Monster)) as Monster)!;
                    PrintHint(Hints.HintType.Attack, monsterToAttack.Name!);
                }

                //trade hint
                if (Hints.TradeHint && nextLocation.Characters!.Exists(ch => ch.GetType() == typeof(Trader)))
                {
                    Trader traderToTrade = (nextLocation.Characters!.Find(ch => ch.GetType() == typeof(Trader)) as Trader)!;

                    //check if trader isn't talker
                    if (!traderToTrade.IsTalker)
                    {
                        PrintHint(Hints.HintType.Trade, traderToTrade.Name!);
                    }
                    else
                    {
                        PrintHint(Hints.HintType.Talk, traderToTrade.Name!);
                    }
                }

                //pause hint
                if (Hints.PauseHint && nextLocation.Characters!.Exists(ch => ch.Name!.ToLower() == "żmija"))
                {
                    PrintHint(Hints.HintType.Pause);
                }

                //game speed hint
                if (Hints.GameSpeedHint && nextLocation.X == 11 && nextLocation.Y == 15 && nextLocation.Z == 0)
                {
                    PrintHint(Hints.HintType.GameSpeed);
                }

                //quick save hint
                if (Hints.QuickSaveHint && nextLocation.X == 18 && nextLocation.Y == 17 && nextLocation.Z == 0)
                {
                    PrintHint(Hints.HintType.QuickSave);
                }
            }
        }

        //method for crafting a spell
        private void CraftSpell(Spell spellToCraft)
        {

            PrintMessage("Tworzysz czar " + spellToCraft.Name, MessageType.Action);
            PrintMessage("Czujesz jak nowe zaklęcie wypełnia Twój umysł", MessageType.Action);

            Data.Player!.SpendMana(200);

            //if collection of player's spells was full and the last spell
            //was removed - print proper message to user about it

            //remove the spell from list first if it's already remembered
            if (Data.Player!.RememberedSpells.Exists(sp => sp.Name!.ToLower() == spellToCraft.Name!.ToLower()))
            {
                Spell spellToRemove = Data.Player!.RememberedSpells.Find(sp => sp.Name!.ToLower() 
                == spellToCraft.Name!.ToLower())!;
                Data.Player!.RememberedSpells.Remove(spellToRemove);
            }

            Spell returnedSpell = Data.Player!.AddSpell(spellToCraft);
            if (returnedSpell.Name != "placeholder")
            {
                PrintMessage("Zapominasz czar " + returnedSpell.Name, MessageType.Action);
            }

            SpellsInfo(Data.Player!);
        }

        //method for casting spell by specified caster onto specified target
        private void CastSpell(CombatCharacter caster, CombatCharacter target, Spell spell)
        {
            bool hasSpellLanded;
            bool isDmgLethal = false;
            bool isCasterPlayer = false;
            double spellDmg;
            double casterDmgFactor = 1;
            double casterChanceFactor;
            
            //prevent casting the spell when it's cost is higher than caster's actual mana value
            if (caster.Mp < spell.ManaCost)
            {
                return;
            }

            //if target self, then ignore magicResistance
            if (caster == target)
            {
                hasSpellLanded = true;
            }
            else
            {

                //set casterChanceFactor depending on if it's a player or a monster
                if (caster == Data.Player)
                {
                    casterChanceFactor = (caster as Player)!.GetEffectiveIntelligence() * 1.3;
                    casterDmgFactor = (caster as Player)!.GetEffectiveIntelligence() / 3;
                    isCasterPlayer = true;
                }
                else
                {
                    casterChanceFactor = caster.Level * 15;
                    casterDmgFactor = caster.Level * 1.5;
                }

                hasSpellLanded = IsSpellASuccess(casterChanceFactor, target.GetEffectiveMagicResistance());
            }

            spellDmg = (spell.Power * Math.Sqrt(casterDmgFactor)) /
                    (Math.Sqrt(target.GetEffectiveMagicResistance() * 0.02));

            //prevent weird bug of spellDmg becoming inifite number
            if (spellDmg > Int32.MaxValue || spellDmg <= 0)
            {
                spellDmg = 100;
            }

            //greatly descrease spell dmg if it failed to land
            if (!hasSpellLanded)
            {
                spellDmg *= 0.333;
            }

            //randomize spell dmg
            spellDmg = RandomizeDmg(spellDmg);

            //print message about spell being cast depending on who is caster
            if (caster == Data.Player!)
            {
                if (spell.Name == "Demoniczny_portal")
                {
                    PrintMessage("Rzucasz czar " + spell.Name, MessageType.Action);
                }
                else
                {
                    PrintMessage("Rzucasz czar " + spell.Name + " w postać: " + target.Name, MessageType.Action);
                }
            }
            else if (target == Data.Player!)
            {
                PrintMessage(caster.Name + " rzuca w Ciebie czar " + spell.Name);
            }
            else if (caster.CurrentLocation == Data.Player!.CurrentLocation)
            {
                PrintMessage(caster.Name! + " rzuca czar w postać: " + target.Name);
            }

            //spend caster's mana
            caster.SpendMana(spell.ManaCost);

            //apply effect/modifiers
            if (hasSpellLanded)
            {
                if (target == Data.Player!)
                {
                    ApplyEffect(spell.Modifiers!, spell.Name!);
                }
                else
                {
                    if (caster == Data.Player!)
                    {
                        PrintMessage("Czar się powiódł!");
                    }

                    //reset mods durations if character is already affected by it, otherwise
                    //apply the mods
                    Modifier spellMod = new Modifier();
                    if (target.Modifiers!.Exists(mod => mod.Parent!.ToLower() == spell.Name!.ToLower()))
                    {
                        target.Modifiers!.ForEach(mod =>
                        {
                            if (mod.Parent!.ToLower() == spell.Name!.ToLower())
                            {
                                mod.ResetDuration();
                            }
                        });
                    }
                    else
                    {
                        spell.Modifiers.ForEach(mod =>
                        {
                            spellMod = new Modifier(mod);
                            spellMod.Parent = spell.Name!;
                            target.AddModifier(spellMod);
                        });
                    }
                    
                }

                //apply special effects
                spell.SpecialEffects.ForEach(eff =>
                {
                    ApplySpecialEffect(target, eff, spell.Name!);
                });
            }
            else
            {
                if (target == Data.Player!)
                {
                    PrintMessage("Odpierasz czar " + spell.Name + "!");
                }
                else if (caster == Data.Player)
                {
                    PrintMessage(target.Name + " odpiera Twój czar " + spell.Name! + "!");
                }
            }

            //deal spell dmg
            if (spell.Power > 0)
            {
                isDmgLethal = DealDmgToCharacter(caster, target, Convert.ToInt32(spellDmg), true);
            }

            //break player's invis if the spell was other than invis
            if (caster == Data.Player! && spell.Name != "Powłoka_nur'zhel")
            {
                BreakInvisibility();
            }

            //remove nur'zhel effect immediately if cast during fight
            if (caster == Data.Player! && spell.Name == "Powłoka_nur'zhel" && 
                Data.Player!.CurrentState == CombatCharacter.State.Combat)
            {
                BreakInvisibility();
            }

            //trigger target's autoattack only if if the spell is offensive
            if (caster != target)
            {
                TriggerAutoAttack(caster, target, isCasterPlayer, isDmgLethal);
            }
        }

        //method for attacking character by another character
        private void AttackCharacter(CombatCharacter attacker, CombatCharacter attacked)
        {
            //make sure to remove previous attack instance, so attacker doesn't attack
            //2 (or more) characters simultaneously
            int instanceIndex = AttackInstances.FindIndex(ins => ins.Attacker == attacker);
            if (instanceIndex != -1)
            {
                AttackInstances.RemoveAt(instanceIndex);
            }
            AttackInstances.Add(new AttackInstance(attacker, attacked));

            //print npcs aggressive response when it attacks
            if (attacker != Data.Player)
            {
                if (attacker.AggressiveResponses!.Length > 0)
                {
                    int randomIndex = Rand.Next(0, attacker.AggressiveResponses!.Length);
                    PrintSpeech(attacker, attacker.AggressiveResponses[randomIndex]);
                }
            }

            //print appropriate message depending on player's position in attacker/attacked configuration
            //if (attacker == Data.Player)
            //{
            //    PrintMessage("Atakujesz postać: " + attacked.Name + "!", MessageType.Action);
            //}
            //else if (attacked == Data.Player)
            //{
            //    PrintMessage("Zostałeś zaatakowany przez: " + attacker.Name + "!");
            //}
            //else if (attacked.CurrentLocation! == Data.Player!.CurrentLocation)
            //{
            //    PrintMessage(attacker.Name! + " atakuje: " + attacked.Name);
            //}
            
            //if attacked character doesn't exist in attacker's opponents list
            if (!(attacker.Opponents.Exists(op => op == attacked)))
            {
                attacker.AddOpponent(attacked);
                attacked.AddOpponent(attacker);
            }
        }

        //method killing non-player combat character
        private void KillCharacter(CombatCharacter character)
        {
            int i;

            //erase character from it's current location
            character.CurrentLocation!.RemoveCharacter(character);

            //remove all characters opponents and interactors
            character.CurrentState = CombatCharacter.State.Idle;
            character.Opponents.Clear();
            character.InteractsWith = new Character("placeholder");

            //remove all attack instances related to dying character
            List<AttackInstance> instancesToRemove = new List<AttackInstance>();
            for (i = 0; i < AttackInstances.Count; i++)
            {
                if (AttackInstances[i].Attacker == character || AttackInstances[i].Receiver == character)
                {
                    instancesToRemove.Add(AttackInstances[i]);
                }
            }
            instancesToRemove.ForEach(ins =>
            {
                AttackInstances.Remove(ins);
            });

            //remove all modifiers from character
            List<Modifier> modsToRemove = new List<Modifier>();
            for (i = 0; i < character.Modifiers!.Count; i++)
            {
                modsToRemove.Add(character.Modifiers[i]);
            }
            modsToRemove.ForEach(mod => character.RemoveModifier(mod));

            //if it's player dying
            if (character == Data.Player!)
            {
                //fix bug causing player still fighting with the enemy he died from
                //AttackInstances.Clear();

                Character death = new Character("Śmierć");
                PrintMessage("Twój wzrok traci ostrość, a dzwięki dochodzą jakby z oddali. Przed Tobą pojawia się wysoka na 2 metry " +
                    "postać w czarnej szacie z kapturem, z której rękawów wystają jedynie białe, kościane dłonie. W prawej ręce, postać" +
                    " trzyma wielką kosę o srebrzysto-niebieskim ostrzu, tak cienkim, że wydaje się nierealne");
                PrintSpeech(death, "JA CHYBA NIE W PORĘ? NO CÓŻ, TRUDNO..");
                PrintMessage("Postać pokazuje Ci pokaźną klepsydrę z napisem \"" + Data.Player!.Name! + "\" wyrytym srebrnymi literami, w której " +
                    "ostatnie ziarenka złotego piasku przesypują się do dolnej połowy, po czym bierze zamach, a Ty widzisz tylko srebrzysto-" +
                    "niebieski błysk..");

                //remove queued action
                Actions.Clear();

                //remove all effects from player
                List<EffectOnPlayer> effectsToRemove = new List<EffectOnPlayer>();
                for (i = 0; i < (character as Player)!.Effects!.Count; i++)
                {
                    effectsToRemove.Add((character as Player)!.Effects![i]);
                }
                effectsToRemove.ForEach(eff => RemoveEffect(eff));

                //give player death penalty
                Data.Player!.Experience = 0;
                RemoveGoldFromPlayer(Data.Player!.Gold);
                PrintMessage("Tracisz całe zebrane doświadczenie i złoto!");

                //delevel player
                //if (Data.Player!.Level > 1)
                //{
                //    Data.Player!.Level -= 1;

                //    Data.Player!.Strength--;
                //    Data.Player.Agility--;
                //    Data.Player.Intelligence--;

                //    if (Data.Player!.AttributePoints >= 4)
                //    {
                //        Data.Player!.AttributePoints = 0;
                //    }
                //    else
                //    {
                //        Data.Player!.AttributePoints -= 4;
                //    }

                //    Data.Player.NextLvlExpCap = Convert.ToUInt64(Math.Pow(Data.Player.Level * 5, 1.5));

                //}

                //respawn player
                PrintMessage("Odradzasz się..", MessageType.Action);
                AddCharacterToLocation(Data.Locations!.Find(loc => loc.Name == "Karczma_Pod_Wilczym_Kłem")!, Data.Player!);
                PrintMap();

                //reapply items modifiers
                if (Data.Player!.Weapon!.Name!.ToLower() != "placeholder")
                {
                    Data.Player!.Weapon!.Modifiers!.ForEach(mod =>
                    {
                        Data.Player.AddModifier(mod);
                    });
                }
                if (Data.Player!.Torso!.Name!.ToLower() != "placeholder")
                {
                    Data.Player!.Torso!.Modifiers!.ForEach(mod =>
                    {
                        Data.Player.AddModifier(mod);
                    });
                }
                if (Data.Player!.Pants!.Name!.ToLower() != "placeholder")
                {
                    Data.Player!.Pants!.Modifiers!.ForEach(mod =>
                    {
                        Data.Player.AddModifier(mod);
                    });
                }
                if (Data.Player!.Helmet!.Name!.ToLower() != "placeholder")
                {
                    Data.Player!.Helmet!.Modifiers!.ForEach(mod =>
                    {
                        Data.Player.AddModifier(mod);
                    });
                }
                if (Data.Player!.Gloves!.Name!.ToLower() != "placeholder")
                {
                    Data.Player!.Gloves!.Modifiers!.ForEach(mod =>
                    {
                        Data.Player.AddModifier(mod);
                    });
                }
                if (Data.Player!.Shoes!.Name!.ToLower() != "placeholder")
                {
                    Data.Player!.Shoes!.Modifiers!.ForEach(mod =>
                    {
                        Data.Player.AddModifier(mod);
                    });
                }

                Data.Player!.Hp = Math.Floor(Data.Player.GetEffectiveMaxHp() * 0.4);
                Data.Player!.Mp = 0;
            }

            //else if it's npc dying
            else
            {

                //if it's dying in location occupied by the player
                if (character.CurrentLocation == Data.Player!.CurrentLocation)
                {
                    PrintMessage(character.Name + " ginie");
                }

                //try to drop a rune
                TryRuneDrop();

                //try chance to drop extra items
                TriggerDropChance(character);

                //drop character's items
                character.Inventory!.ForEach(item =>
                {
                    AddItemToLocation(character.CurrentLocation!, item.Name!, item.Quantity);
                });
                AddGoldToLocation(character.CurrentLocation!, character.Gold);

                //erase npc's id from takeIds list
                Data.TakenIds!.Remove(character.Id);

                //trigger inventory hint
                if (Hints.HintsOnOff && Hints.PickupHint)
                {
                    PrintHint(Hints.HintType.Pickup);
                }

                //open the silver gate if it's Agriluna dying
                if (character.Name!.ToLower() == "agriluna")
                {
                    PrintMessage("Księżycowa Brama została otwarta!");
                    character.CurrentLocation.NorthPassage = true;
                }
            }
        }

        /// <summary>
        /// method dealing dmg to combat-character. Returns true if the dmg is lethal,
        /// otherwise - returns false;
        /// </summary>
        /// <param name="dealer"></param>
        /// <param name="receiver"></param>
        /// <param name="dmg"></param>
        /// <returns></returns>
        private bool DealDmgToCharacter(CombatCharacter dealer, CombatCharacter receiver, int dmg, bool isSpelldmg)
        {
            bool isDealerPlayer = dealer.GetType() == typeof(Player);
            bool isReceiverPlayer = receiver.GetType() == typeof(Player);

            //handle manashield
            if (receiver.Modifiers!.Exists(mod => mod.Type == Modifier.ModType.ManaShield))
            {
                double manaShieldPercentage = 0;
                double dmgAbsorbtionValue = 0;
                double dmgAbsorbedByMana = 0;
                receiver.Modifiers!.ForEach(mod =>
                {
                    if (mod.Type == Modifier.ModType.ManaShield)
                    {
                        manaShieldPercentage += mod.Value;
                    }
                });
                dmgAbsorbtionValue = (int)(dmg * (manaShieldPercentage * 0.01));

                //spend receiver mana and get actual mana spend value
                dmgAbsorbedByMana = receiver.SpendMana(dmgAbsorbtionValue);
                
                dmg -= (int)dmgAbsorbedByMana;
            }

            bool isDmgLethal = receiver.DealDamage(dmg);

            //print appropriate messages to user about dmg dealing
            if (isDealerPlayer)
            {
                PrintMessage("Zadajesz " + dmg + " obrażeń postaci: " + receiver.Name, MessageType.DealDmg);
            }
            else if (isReceiverPlayer)
            {
                PrintMessage(dealer.Name! + " zadaje Ci " + dmg + " obrażeń", MessageType.ReceiveDmg);

                //trigger flee hint
                if (Data.Player!.Hp < Data.Player!.GetEffectiveMaxHp() * 0.6)
                {
                    if (Hints.HintsOnOff && Hints.FleeHint)
                    {
                        PrintHint(Hints.HintType.Flee);
                    }
                }
            }

            //handle lifesteal modifiers (if present on dealer and ONLY if it's not spell dmg)
            if (!isSpelldmg)
            {
                double lifestealPercentValue = 0;
                dealer.Modifiers!.ForEach(mod =>
                {
                    if (mod.Type == Modifier.ModType.Lifesteal)
                    {
                        lifestealPercentValue += mod.Value;
                    }
                });

                //heal dealer for value of dmg multiplied by
                //lifesteal percent multiplier
                if (lifestealPercentValue > 0)
                {
                    double lifestealMultiplier = lifestealPercentValue * 0.01;
                    double lifestealHeal = Math.Round(lifestealMultiplier * dmg);
                    dealer.Heal(lifestealHeal);
                    //PrintMessage("ULECZONO " + lifestealHeal, MessageType.Gain);
                }
            }

            //break invis if it's player dealing or receiving the dmg
            if (dealer == Data.Player! || receiver == Data.Player)
            {
                BreakInvisibility();
            }

            if (isDmgLethal)
            {
                KillCharacter(receiver);

                //end combat, remove opponents etc.
                if (dealer.Opponents.Exists(opponent => opponent == receiver))
                {
                    dealer.RemoveOpponent(receiver);
                }
                if (receiver.Opponents.Exists(opponent => opponent == dealer))
                {
                    receiver.RemoveOpponent(dealer);
                }

                if (isDealerPlayer)
                {
                    if (receiver.GetType() == typeof(Hero))
                    {
                        GivePlayerExperience(receiver.Level * 3);
                    }
                    else
                    {
                        GivePlayerExperience(receiver.Level);
                    }
                }
            }

            return isDmgLethal;
        }

        //method adding certain amount to player's experience pool
        private void GivePlayerExperience(int lvl)
        {
            ulong experience = Convert.ToUInt64(lvl * 15);
            int previousLevel = Data.Player!.Level;

            PrintMessage("Zdobywasz " + experience + " doświadczenia", MessageType.Action);
            if (Data.Player!.GainExperience(experience))
            {
                PrintMessage("***Zdobywasz nowy poziom!***", MessageType.Action);
                Data.Player!.AddAttributePoints((Data.Player!.Level - previousLevel) * 5);

                //trigger stats hint
                if (Hints.HintsOnOff)
                {
                    if (Hints.StatsHint)
                    {
                        PrintHint(Hints.HintType.Stats);
                    }
                }
            }
        }

        //method putting character into location
        private void AddCharacterToLocation(Location location, Character character)
        {
            location.AddCharacter(character);
            character.CurrentLocation = location;
            bool isPlayerInvisible = Data.Player!.Modifiers!.Exists(mod => mod.Type == Modifier.ModType.Invisibility);
            string[] allDirections = new string[4];
            Location adjacentLocation = new Location();

            allDirections[0] = "n";
            allDirections[1] = "e";
            allDirections[2] = "s";
            allDirections[3] = "w";


            //if added character is player
            if (character.GetType() == typeof(Player))
            {

                //reveal the location and all adjacent locations for player
                location.IsVisible = true;
                
                for (int i = 0; i < 4; i++)
                {
                    if (GetNextLocation(allDirections[i], out adjacentLocation))
                    {
                        adjacentLocation.IsVisible = true;
                    }
                }

                //display location info to user
                LocationInfo(Data.Player!.CurrentLocation!);
                PrintMap();

                //handle aggressive monsters present in location
                //to attack player (except when player is invisible)
                if (!isPlayerInvisible)
                {
                    location.Characters!.ForEach(ch =>
                    {
                        if (ch.GetType() == typeof(Monster))
                        {
                            if ((ch as Monster)!.Aggressiveness == Monster.AggressionType.Aggressive)
                            {
                                AttackCharacter((ch as CombatCharacter)!, (character as CombatCharacter)!);
                            }
                        }
                        else if (ch.GetType() == typeof(Hero))
                        {
                            if ((ch as Hero)!.Aggressiveness == Monster.AggressionType.Aggressive)
                            {
                                AttackCharacter((ch as CombatCharacter)!, (character as CombatCharacter)!);
                            }
                        }
                    });
                }
            }
            else if (Data.Player!.CurrentLocation == location)
            {
                PrintMessage("W lokacji pojawia się postać: " + character.Name);

                //if added character is aggressive monster, attack player immediately
                if (!isPlayerInvisible)
                {
                    if (character.GetType() == typeof(Monster))
                    {
                        if ((character as Monster)!.Aggressiveness == Monster.AggressionType.Aggressive)
                        {
                            AttackCharacter((character as CombatCharacter)!, Data.Player!);
                        }
                    }
                }
            }
        }

        //method handling adding items to player's inventory
        private void AddItemToPlayer(string itemName, int quantity)
        {
            Item itemToAdd = Data.Items!.Find(item => FlattenPolishChars(item.Name!.ToLower())
            == FlattenPolishChars(itemName.ToLower()))!;

            Data.Player!.AddItem(itemToAdd, quantity);
            PrintMessage("Zdobyłeś " + Convert.ToString(quantity) + " " + itemToAdd.Name, MessageType.Gain);

            //trigger hints
            if (Hints.HintsOnOff)
            {

                //trigger craft1/2 hints
                int i = 0;
                string[] runeNames = new string[5];

                runeNames[i++] = "iskarr";
                runeNames[i++] = "akull";
                runeNames[i++] = "verde";
                runeNames[i++] = "xitan";
                runeNames[i++] = "dara";

                if (Hints.CraftHint1 && runeNames.Contains(itemToAdd.Name!.ToLower()))
                {
                    PrintHint(Hints.HintType.Craft1, itemToAdd.Name!);
                }
                else if (Hints.CraftHint2 && runeNames.Contains(itemToAdd.Name!.ToLower()))
                {
                    List<Item> runesOwned = new List<Item>();

                    //check if player owns at least 2 runes
                    Data.Player!.Inventory!.ForEach(it =>
                    {
                        if (runeNames.Contains(it.Name!.ToLower()))
                        {
                            runesOwned.Add(it);
                        }
                    });

                    if (runesOwned.Count >= 2)
                    {
                        PrintHint(Hints.HintType.Craft2, runesOwned[0].Name!, runesOwned[1].Name!);
                    }
                }

                //trigger wear hint
                if (Hints.HintsOnOff && Hints.WearHint && Data.Player!.CurrentState != CombatCharacter.State.Trade)
                {
                    if (itemToAdd.GetType() == typeof(Armor) || itemToAdd.GetType() == typeof(Weapon))
                    {
                        PrintHint(Hints.HintType.Wear, itemToAdd.Name!.ToLower());
                    }
                }

                //trigger use hint
                if (Hints.HintsOnOff && Hints.UseHint && Data.Player!.CurrentState != CombatCharacter.State.Trade)
                {
                    if (itemToAdd.GetType() == typeof(Consumable))
                    {
                        PrintHint(Hints.HintType.Use, itemToAdd.Name!.ToLower());
                    }
                }
            }
        }

        //method handling removing items from player's inventory
        private void RemoveItemFromPlayer(string itemName, int quantity = 1)
        {

            //find item in data to have it's proper (first letter capitalized) name string to display in message for player
            Item itemToRemove = Data.Items!.Find(item =>
            FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName.ToLower()))!;

            if (Data.Player!.RemoveItem(itemToRemove.Name!, quantity))
            {
                PrintMessage("Straciłeś " + Convert.ToString(quantity) + " " + itemToRemove.Name, MessageType.Loss);
            }
            else
            {
                PrintMessage("Coś poszło nie tak..", MessageType.SystemFeedback);
            }
        }

        //method for wearing a weapon-type items by player
        private void WearWeaponOnPlayer(string itemName)
        {
            Weapon weaponToWear = (Data.Items!.Find(item => FlattenPolishChars(item.Name!.ToLower())
            == FlattenPolishChars(itemName.ToLower())) as Weapon)!;

            TakeOffWeaponFromPlayer();

            PrintMessage("Uzbrajasz się w " + weaponToWear.Name, MessageType.Action);
            Data.Player!.WearWeapon(weaponToWear);
        }

        //method for wearing armor type items by player
        private void WearArmorOnPlayer(string itemName)
        {
            Armor armorToWear = (Data.Items!.Find(item => 
            FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName.ToLower())) as Armor)!;
            Armor.ArmorType armorType = armorToWear.Type;
            string wornArmorName = string.Empty;

            switch (armorType)
            {
                case Armor.ArmorType.Helmet:
                    wornArmorName = Data.Player!.Helmet!.Name!;
                    break;
                case Armor.ArmorType.Torso:
                    wornArmorName = Data.Player!.Torso!.Name!;
                    break;
                case Armor.ArmorType.Pants:
                    wornArmorName = Data.Player!.Pants!.Name!;
                    break;
                case Armor.ArmorType.Gloves:
                    wornArmorName = Data.Player!.Gloves!.Name!;
                    break;
                case Armor.ArmorType.Shoes:
                    wornArmorName = Data.Player!.Shoes!.Name!;
                    break;
            }

            //if there is something in the slot already, take it off
            if (wornArmorName != "placeholder")
            {
                TakeOffArmorFromPlayer(armorType);
            }

            PrintMessage("Zakładasz " + armorToWear.Name, MessageType.Action);
            Data.Player!.WearArmor(armorToWear);
        }

        /// <summary>
        /// Takes off weapon from player's weapon slot.
        /// Returns true if weapon is taken off, false if slot is empty
        /// </summary>
        /// <returns></returns>
        private bool TakeOffWeaponFromPlayer()
        {
            string weaponName = Data.Player!.TakeOffWeapon();
            if (weaponName != "placeholder")
            {
                PrintMessage("Odkładasz " + weaponName, MessageType.Action);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Takes off armor from player's armor slot.
        /// Returns true if armor is taken off, false if slot is empty
        /// </summary>
        /// <returns></returns>
        private bool TakeOffArmorFromPlayer(Armor.ArmorType type)
        {
            string wornArmorName = Data.Player!.TakeOffArmor(type);
            if (wornArmorName != "placeholder")
            {
                PrintMessage("Zdejmujesz " + wornArmorName, MessageType.Action);
                return true;
            }
            return false;
        }

        private void TryToFlee(Location escapeDestination)
        {
            CombatCharacter fastestOpponent = new CombatCharacter("placeholder");
            fastestOpponent.Speed = 0;

            //find the fastest opponent
            Data.Player!.Opponents.ForEach(opp =>
            {
                if (opp.GetEffectiveSpeed() > fastestOpponent.GetEffectiveSpeed())
                {
                    fastestOpponent = opp;
                }
            });

            double playerChance = Math.Pow(Data.Player!.GetEffectiveSpeed(), 1.5);
            double opponentChance = Math.Pow(fastestOpponent.GetEffectiveSpeed(), 1.5);
            double fleeChance = Math.Round(playerChance / opponentChance / 2, 2);

            PrintMessage("Próbujesz ucieczki!", MessageType.Action);

            if (TryOutChance(fleeChance))
            {

                //remove all attack instances related to player
                List<AttackInstance> instancesToRemove = new List<AttackInstance>();
                AttackInstances.ForEach(ins =>
                {
                    if (ins.Attacker == Data.Player || ins.Receiver == Data.Player)
                    {
                        instancesToRemove.Add(ins);
                    }
                });
                instancesToRemove.ForEach(ins =>
                {
                    
                    //remove player from monster's opponents list
                    if (ins.Receiver == Data.Player!)
                    {
                        ins.Attacker.RemoveOpponent(ins.Receiver);
                    }

                    AttackInstances.Remove(ins);
                });

                //Remove all player opponents and remove he's combat state
                Data.Player!.Opponents.Clear();
                Data.Player!.CurrentState = CombatCharacter.State.Idle;

                PrintMessage("Udało Ci się uciec!");
                ChangePlayerLocation(escapeDestination, "none");
            }
            else
            {
                PrintMessage("Nie udaje Ci się uciec przed " + fastestOpponent.Name + "!");
                ApplySpecialEffect(Data.Player!, new SpecialEffect(SpecialEffect.EffectType.Stun, 10), "Nieudana ucieczka");
            }
        }

        //method handling adding gold to player's pool
        private void AddGoldToPlayer(int gold)
        {
            Data.Player!.Gold += gold;
            PrintMessage("Zyskałeś " + Convert.ToString(gold) + " złota", MessageType.Gain);

            //trigger inventory hint
            if (Hints.HintsOnOff && Hints.InventoryHint)
            {
                PrintHint(Hints.HintType.Inventory);
            }
        }

        //method handling removing gold from player's pool
        private void RemoveGoldFromPlayer(int gold)
        {
            if (Data.Player!.Gold >= gold)
            {
                PrintMessage("Straciłeś " + Convert.ToString(gold) + " złota", MessageType.Loss);
                Data.Player!.Gold -= gold;
            }
            else
            {
                PrintMessage("Straciłeś " + Convert.ToString(Data.Player!.Gold) + " złota", MessageType.Loss);
                Data.Player!.Gold = 0;
            }

        }

        //method for using consumable item
        public void UseConsumable(Consumable item)
        {
            //prevent using non-existent item
            if (!Data.Player!.Inventory!.Exists(it =>
            FlattenPolishChars(it.Name!.ToLower()) == FlattenPolishChars(item.Name!.ToLower())))
            {
                PrintMessage("Nie posiadasz przedmiotu " + item.Name! + "!", MessageType.SystemFeedback);
                return;
            }

            PrintMessage(item.UseActivityName! + " " + item.Name!, MessageType.Action);
            RemoveItemFromPlayer(item.Name!);

            if (item.Modifiers!.Count > 0)
            {
                ApplyEffect(item.Modifiers!, item.Name!);
            }
            if (item.AdditionalEffect!.Count > 0)
            {
                ApplyEffect(item.AdditionalEffect!, item.Name! + "(Dodatkowy)");
            }
            if (item.SpecialEffects.Count > 0)
            {
                item.SpecialEffects.ForEach(eff =>
                {
                    ApplySpecialEffect(Data.Player!, eff, item.Name!);
                });
            }
        }

        //method applying effects to player
        private void ApplyEffect(List<Modifier> modifiers, string objectName)
        {
            Player player = Data.Player!;
            EffectOnPlayer itemEffect;
            string description = objectName + ":";
            int durationInTicks = 0;

            //if there are any modifiers in the item
            if (modifiers.Count > 0)
            {

                //get duration for new effect from the longest lasting modifier in the list
                Modifier longestDurationMod = new Modifier();
                longestDurationMod.DurationInTicks = 0;
                modifiers.ForEach(mod =>
                {
                    if (mod.DurationInTicks > longestDurationMod.DurationInTicks)
                    {
                        longestDurationMod = mod;
                    }
                });

                durationInTicks = longestDurationMod.DurationInTicks;

                //set ParentEffect for each mod and add it's description to description string
                modifiers!.ForEach(mod =>
                {
                    if (mod.Duration != 0)
                    {
                        mod.Parent = objectName;

                        //add modifiers descriptors in form of 'modifier(+/-[value])' to description string 
                        description += " " + GetModDescription(mod) + ",";
                    }
                });

                //if the effect is stun/invisibility, clear the description from all extra signs
                if (objectName == "Ogłuszenie" || objectName == "Niewidzialność")
                {
                    description = Regex.Replace(description, @"[^a-zA-ZśćąęłźżŚĆĄĘŁŹŻ]", "");
                }

                //clear description from trailing comma and prepare item effect object
                description = Regex.Replace(description, @",$", "");
                itemEffect = new EffectOnPlayer(objectName, description, durationInTicks);

                //if effect is supposed to stack
                if (Data.StackingEffects!.Contains(objectName.ToLower()))
                {
                    player.Effects!.Add(itemEffect);
                    modifiers.ForEach(mod =>
                    {
                        player.AddModifier(mod);
                    });
                }
                else
                {
                    
                    //if player is already affected by the same effect, reset effect and mods durations
                    if (player.Modifiers!.Exists(mod => mod.Parent!.ToLower() == objectName.ToLower()))
                    {
                        player.Modifiers.ForEach(mod =>
                        {
                            if (mod.Parent == objectName)
                            {
                                mod.ResetDuration();
                            }
                            
                        });

                        //reset effect's durationInTicks value
                        player.Effects!.Find(effect => effect.Name!.ToLower() == objectName.ToLower())!.DurationInTicks = durationInTicks;
                    }
                    else
                    {
                        player.Effects!.Add(itemEffect);
                        modifiers.ForEach(mod =>
                        {
                            player.AddModifier(mod);
                        });
                    }
                }

                PrintMessage("Czujesz efekt działania " + itemEffect.Description, MessageType.EffectOn);

                //trigger effects hint
                if (Hints.HintsOnOff && Hints.EffectsHint)
                {
                    PrintHint(Hints.HintType.Effects);
                }
            }
        }

        //method applying special effects to combat character
        private void ApplySpecialEffect(CombatCharacter target, SpecialEffect effect, string parentName)
        {
            Modifier specialMod = new Modifier();
            parentName += "(SE)";

            if (effect.Type == SpecialEffect.EffectType.HealPercent)
            {
                double healedHp = target.Heal((effect.Value * 0.01) * target.GetEffectiveMaxHp());

                if (target == Data.Player)
                {
                    PrintMessage("Uleczono: " + healedHp + " HP");
                }
                return;
            }
            else if (effect.Type == SpecialEffect.EffectType.Heal)
            {
                double healedHp = target.Heal(effect.Value);

                if (target == Data.Player)
                {
                    PrintMessage("Uleczono: " + healedHp + " HP");
                }
                return;
            }
            else if (effect.Type == SpecialEffect.EffectType.Stun)
            {
                specialMod = new Modifier(Modifier.ModType.Stun, 0, effect.Duration, parentName);
            }
            else if (effect.Type == SpecialEffect.EffectType.Lifesteal)
            {
                specialMod = new Modifier(Modifier.ModType.Lifesteal, effect.Value, effect.Duration, parentName, true);
            }
            else if (effect.Type == SpecialEffect.EffectType.Invisibility)
            {
                specialMod = new Modifier(Modifier.ModType.Invisibility, effect.Value, effect.Duration, parentName);
            }
            else if (effect.Type == SpecialEffect.EffectType.ManaShield)
            {
                specialMod = new Modifier(Modifier.ModType.ManaShield, effect.Value, effect.Duration, parentName, true);
            }
            else if (effect.Type == SpecialEffect.EffectType.Teleport)
            {
                //remove player from it's current location
                Data.Player!.CurrentLocation!.Characters!.Remove(Data.Player);

                PrintMessage("Czujesz jak świat wokół Ciebie zaczyna wirować, rozmywać się i znikać." +
                    " Widzisz rzeczy, których nie sposób opisać słowami, a Twoje wnętrzności skręcają" +
                    " się tak mocno, że ledwo udaje Ci się powstrzymać wymioty. Nagle upadasz na ziemię, i" +
                    " powoli zaczynasz rozpoznawać otoczenie.", MessageType.Action);

                //if it's portal-orb item, set proper teleport destination
                if (Regex.Match(parentName, @"^[Kk]ula_portalowa_").Success)
                {
                    string cityName = Regex.Replace(parentName, @"^[Kk]ula_portalowa_", "");

                    switch (cityName)
                    {
                        case "Derillon(SE)":
                            TeleportLocation = Data.Locations!.Find(loc => loc.Name!.ToLower() == "ulica_derillon")!;
                            break;
                        case "Artimesia(SE)":
                            TeleportLocation = Data.Locations!.Find(loc => loc.Name!.ToLower() == "ulica_artimesii")!;
                            break;
                    }
                }

                //Add player to destined location
                AddCharacterToLocation(TeleportLocation, Data.Player!);
                return;
            }

            //apply special modifier depending on target type
            if (target == Data.Player)
            {
                List<Modifier> specialMods = new List<Modifier>();
                specialMods.Add(specialMod);
                ApplyEffect(specialMods, specialMod.Parent);
            }
            else
            {
                target.AddModifier(specialMod);
            }

        }

        //method triggering auto-attack if character becomes a target
        //of an attack attempt or offensive spell
        private void TriggerAutoAttack(CombatCharacter attacker, CombatCharacter receiver,
             bool isAttackerPlayer, bool isDmgLethal)
        {
            //if receiver is an npc character - respond with counterattack
            if (isAttackerPlayer)
            {

                //check if receiver isn't already attacking the attacker and prevent
                //counterattacking if the attacked character has been killed
                if (!AttackInstances.Exists(ins => ins.Attacker == receiver && ins.Receiver == attacker)
                    && !isDmgLethal)
                {
                    AttackCharacter(receiver, attacker);
                }

                //also, if the monster is social type, make his 'comrades'
                //attack the attacker
                if (receiver.GetType() == typeof(Monster))
                {
                    receiver.CurrentLocation!.Characters!.ForEach(character =>
                    {
                        if (character.Name! == receiver.Name! && character != receiver &&
                        (receiver as Monster)!.Aggressiveness == Monster.AggressionType.Social)
                        {

                            //make sure they aren't already attacking the attacker
                            if (!AttackInstances.Exists(ins => ins.Attacker == character && ins.Receiver == attacker))
                            {
                                AttackCharacter((character as CombatCharacter)!, attacker);
                            }
                        }
                    });
                }
            }

            //handle auto-attack
            else if (IsAutoattackOn && !AttackInstances.Exists(ins => ins.Attacker == Data.Player) && !isDmgLethal)
            {
                AttackCharacter(Data.Player!, attacker);
            }
        }

        //method removing effects from player
        private void RemoveEffect(EffectOnPlayer effect)
        {
            PrintMessage("Skończyło się działanie " + effect.Description, MessageType.EffectOff);
            Data.Player!.Effects!.Remove(effect);

            //ensure player will be attacked by aggressive monsters in his
            //current location if the effect was invis
            if (effect.Name == "Powłoka_nur'zhel(SE)")
            {
                Data.Player!.CurrentLocation!.Characters!.ForEach(character =>
                {
                    if (character.GetType() == typeof(Monster))
                    {
                        if ((character as Monster)!.Aggressiveness == Monster.AggressionType.Aggressive)
                        {
                            AttackCharacter((Monster)character, Data.Player!);
                        }
                    }
                });
            }
        }

        //method breaking trade state and printing proper message
        private void BreakTradeState()

        {
             PrintMessage("Przestajesz handlować z: " + Data.Player!.InteractsWith!.Name, MessageType.Action);
             Data.Player.InteractsWith = new Character("placeholder");
             Data.Player!.CurrentState = Player.State.Idle;
        }

        //method for breaking invis buff
        private void BreakInvisibility()
        {
            List<EffectOnPlayer> effectsToRemove = new List<EffectOnPlayer>();
            List<Modifier> modsToRemove = new List<Modifier>();

            //if there is any effect with name of invisibility spel "powłoka_nur'zhel"
            if (Data.Player!.Effects!.Exists(eff => eff.Name == "Powłoka_nur'zhel"))
            {

                //get all effects with that name
                Data.Player!.Effects.ForEach(eff =>
                {
                    if (eff.Name == "Powłoka_nur'zhel" || eff.Name == "Powłoka_nur'zhel(SE)")
                    {
                        effectsToRemove.Add(eff);
                    }
                });

                //get all modifiers with that name
                Data.Player!.Modifiers!.ForEach(mod =>
                {
                    if (mod.Parent == "Powłoka_nur'zhel" || mod.Parent == "Powłoka_nur'zhel(SE)")
                    {
                        modsToRemove.Add(mod);
                    }
                });

                //remove all gathered effects and modifiers
                effectsToRemove.ForEach(eff =>
                {
                    RemoveEffect(eff);
                });
                modsToRemove.ForEach(mod =>
                {
                    Data.Player!.RemoveModifier(mod);
                });
            }
        }

        //method breaking talk state and printing proper message
        private void BreakTalkState()
        {
            PrintMessage("Przestajesz rozmawiać z: " + Data.Player!.InteractsWith!.Name, MessageType.Action);
            Data.Player.InteractsWith = new Character("placeholder");
            Data.Player!.CurrentState = Player.State.Idle;
        }

        //method checking if player is trading/talking and breaking the state if so
        private void ResetPlayerState()
        {
            //check if player is trading with someone already
            if (Data.Player!.CurrentState == Player.State.Trade)
            {
                BreakTradeState();
            }
            else if (Data.Player!.CurrentState == Player.State.Talk)
            {
                BreakTalkState();
            }
        }

        //method adding items to non-player characters
        private void AddItemToNpc(Character character, string itemName, int quantity)
        {
            Item itemToAdd = Data.Items!.Find(item =>
            FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName.ToLower()))!;
            character.AddItem(itemToAdd, quantity);
        }

        //method adding items to location
        private void AddItemToLocation(Location location, string itemName, int quantity)
        {
            Item itemToAdd = Data.Items!.Find(item => 
            FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName.ToLower()))!;

            location.AddItem(itemToAdd, quantity);

            if (location == Data.Player!.CurrentLocation)
            {
                PrintMessage("W lokacji pojawia się przedmiot: " + itemToAdd.Name + "(" + quantity + ")");
            }
        }
        
        //method adding gold to location
        private void AddGoldToLocation(Location location, int amount)
        {
            location.Gold += amount;
            
            if (location == Data.Player!.CurrentLocation)
            {
                PrintMessage("W lokacji pojawia się złoto!");
            }
        }






        //==============================================DESCRIPTION METHODS=============================================

        //method describing location to user
        public void LocationInfo(Location currentLocation, bool isDetailed = true)
        {
            //Location currentLocation = Data.Player!.CurrentLocation!;
            Location nextLocation = new Location();
            string goldInfo = String.Empty;
            string itemsInfo = "Przedmioty: ";
            string charactersInfo = "Postacie: ";
            string exitsInfo = "Wyjścia: ";
            string[] directionsLetters = { "n", "e", "s", "w", "u", "d" };
            string[] directionsStrings = { " północ", " wschód", " południe", " zachód", " góra", " dół" };
            int currentX = currentLocation.X;
            int currentY = currentLocation.Y;

            //print location name and description
            PrintMessage("[ " + currentLocation.Name + " ]");
            PrintMessage(currentLocation.Description!);

            //describe exits for each direction
            for (int i = 0; i < directionsLetters.Length; i++)
            {

                //if the location exists
                if (GetNextLocation(directionsLetters[i], out nextLocation))
                {
                    exitsInfo += directionsStrings[i] + "(" + nextLocation.Name! + ")" + ", ";
                }
            }

            //remove the last comma 
            exitsInfo = Regex.Replace(exitsInfo, @", $", "");

            if (isDetailed)
            {
                PrintMessage(exitsInfo);
            }

            //add character names to their info strings for each character of specific type present in player's current location
            currentLocation.Characters!.ForEach((character) =>
            {
                if (character.GetType() != typeof(Player))
                {
                    charactersInfo += " " + character.Name + ",";
                }
            });

            //add items names for each item present in the location
            currentLocation.Items!.ForEach((item) =>
            {
                itemsInfo += " " + item.Name;
                if (item.Quantity > 1)
                {
                    itemsInfo += "(" + item.Quantity + ")";
                }
                itemsInfo += ",";
            });

            //set the gold description string
            if (currentLocation.Gold > 0)
            {
                goldInfo = "Złoto: " + currentLocation.Gold;
            }

            //remove the last comma
            charactersInfo = Regex.Replace(charactersInfo, @",$", "");
            itemsInfo = Regex.Replace(itemsInfo, @",$", "");


            //if any characters found, print them to outputBox
            if (charactersInfo.Length > 13)
            {
                PrintMessage(charactersInfo);
            }

            //if any items found, print them to outputBox
            if (itemsInfo.Length > 12)
            {
                PrintMessage(itemsInfo);
            }

            //if there is gold on the ground
            if (goldInfo != String.Empty)
            {
                PrintMessage(goldInfo);
            }

        }

        //method printing character's inventory
        private void InventoryInfo(Character character, bool withPrice = true)
        {
            string spaceAfterName;
            string spaceAfterQuantity;
            string spaceAfterPrice = string.Empty;
            string descriptionTable;
            string tableRow;
            string horizontalBorder = string.Empty;
            string delimeter;
            string descriptionRow;
            int nameColumnSize;
            int quantityColumnSize;
            int priceColumnSize;
            int price = 0;
            int borderSize;

            //set delimeter, borderSize and descriptionRow depending on withPrice parameter
            if (withPrice)
            {
                delimeter = "||------------------------------------------------------||";
                descriptionRow = "|| Przedmiot:                         | Ilość: | Cena:  ||\n" + delimeter;
                borderSize = 58;
            }
            else
            {
                delimeter = "||---------------------------------------------||";
                descriptionRow = "|| Przedmiot:                         | Ilość: ||\n" + delimeter;
                borderSize = 49;
            }

            //create string representing top/bottom table borders
            for (int i = 0; i < borderSize; i++)
            {
                horizontalBorder += "=";
            }

            if (!withPrice)
            {
                descriptionTable = "******************* EKWIPUNEK *******************";
            }
            else if (character.GetType() == typeof(Player))
            {
                descriptionTable = "******************** TWÓJ EKWIPUNEK **********************";
            }
            else
            {
                descriptionTable = "****************** EKWIPUNEK HANDLARZA *******************";
            }

            //print talbe description and top table border
            PrintMessage(descriptionTable);
            PrintMessage(descriptionRow);

            foreach (var item in character.Inventory!)
            {
                //calculate sizes of spaces in table rows to mantain neat table layout
                nameColumnSize = 35 - item.Name!.Length;
                quantityColumnSize = 7 - Convert.ToString(item.Quantity).Length;
                spaceAfterName = string.Empty;
                spaceAfterQuantity = string.Empty;

                //only if it's trade mode and price is needed
                if (withPrice)
                {
                    if (character == Data.Player!)
                    {
                        priceColumnSize = 7 - Convert.ToString(item.Price).Length;
                    }
                    else
                    {
                        priceColumnSize = 7 - Convert.ToString(CalculateTraderPrice(item.Name)).Length;
                    }
                    spaceAfterPrice = string.Empty;
                    for (int i = 0; i < priceColumnSize; i++)
                    {
                        spaceAfterPrice += " ";
                    }
                }

                //create strings representing spaces with calculated lenghts
                for (int i = 0; i < nameColumnSize; i++)
                {
                    spaceAfterName += " ";
                }
                for (int i = 0; i < quantityColumnSize; i++)
                {
                    spaceAfterQuantity += " ";
                }


                //set the price depending on character type (higher price for traders)
                //only if it's in trade mode
                if (withPrice)
                {
                    if (character.GetType() == typeof(Player))
                    {
                        price = item.Price;
                    }
                    else
                    {
                        price = CalculateTraderPrice(item.Name);
                    }
                }

                //create a string representing table row (with price or without depending on withPrice parameter)
                if (withPrice)
                {
                    tableRow = "|| " + item.Name + spaceAfterName + "| " + Convert.ToString(item.Quantity) + spaceAfterQuantity + "| "
                        + price + spaceAfterPrice + "||";
                }
                else
                {
                    tableRow = "|| " + item.Name + spaceAfterName + "| " + Convert.ToString(item.Quantity) + spaceAfterQuantity + "||";
                }

                PrintMessage(tableRow);
            }

            //print bottom table border
            PrintMessage(horizontalBorder);

            //print player's gold pool and weight
            if (character.GetType() == typeof(Player))
            {
                string goldAndWeight = "|| Złoto: " + Convert.ToString(Data.Player!.Gold!) + "     | Obciążenie: " +
                    Data.Player!.GetCarryWeight();

                if (Data.Player.GetWeightLimit() < Data.Player!.GetCarryWeight())
                {
                    goldAndWeight += "  (!)";
                }

                int remainingSpaces = borderSize - goldAndWeight.Length - 2;

                for (int i = 0; i < remainingSpaces; i++)
                {
                    goldAndWeight += " ";
                }
                goldAndWeight += "||";

                PrintMessage(goldAndWeight);
            }

            //separate gold display from worn items display
            PrintMessage(horizontalBorder);

            //prepare worn items description
            if (!withPrice)
            {
                string[] itemSlots = new string[6];
                int i;

                itemSlots[0] = "|| Broń: " + (character as Player)!.Weapon!.Name!;
                itemSlots[1] = "|| Hełm: " + (character as Player)!.Helmet!.Name!;
                itemSlots[2] = "|| Tors: " + (character as Player)!.Torso!.Name!;
                itemSlots[3] = "|| Spodnie: " + (character as Player)!.Pants!.Name!;
                itemSlots[4] = "|| Rękawice: " + (character as Player)!.Gloves!.Name!;
                itemSlots[5] = "|| Buty: " + (character as Player)!.Shoes!.Name!;

                //if item slot was empty (meaning was filled with placeholder)
                //swap 'placeholder' string to 'brak'
                for (i = 0; i < itemSlots.Length; i++)
                {
                    itemSlots[i] = Regex.Replace(itemSlots[i], @"\bplaceholder\b", "---");

                    //fill remaining spaces in table row
                    while (itemSlots[i].Length <= horizontalBorder.Length - 3)
                    {
                        itemSlots[i] += " ";
                    }
                    itemSlots[i] += "||";


                    PrintMessage(itemSlots[i]);
                }

                PrintMessage(horizontalBorder);
            }
        }

        //method describing character
        private void CharacterInfo(Character character)
        {
            PrintMessage("[ " + character.Name + " ]");
            if (character.GetType() == typeof(Monster) || character.GetType() == typeof(Hero))
            {
                PrintMessage("Poziom: " + (character as CombatCharacter)!.Level);
            }
            PrintMessage(character.Description!);
        }

        //method describing item
        private void ItemInfo(string itemName)
        {
            string description = string.Empty;
            string weight = string.Empty;
            string itemType = string.Empty;
            string modifiers = string.Empty;
            string effect = string.Empty;
            string additionalEffect = string.Empty;
            string specialEffects = string.Empty;
            string attack = string.Empty;
            string defense = string.Empty;
            string range = string.Empty;
            string sign = string.Empty;
            Item itemToDescribe = Data.Items!.Find(item => FlattenPolishChars(item.Name!.ToLower())
            == FlattenPolishChars(itemName.ToLower()))!;

            //set item's weight and type
            weight = "Waga: " + Convert.ToString(itemToDescribe.Weight);
            itemType = "Typ: ";

            //depending on item type, add info to description and set itemType string
            if (itemToDescribe.GetType() == typeof(Consumable))
            {
                itemType += "używalne";
                effect = "Działanie: ";

                //add modifiers descriptions to effect description for every modifier present in the item
                effect += GetEffectDescription(itemToDescribe.Modifiers!);
                if ((itemToDescribe as Consumable)!.AdditionalEffect!.Count > 0)
                {
                    additionalEffect += "Dodatkowe działanie: " + GetEffectDescription((itemToDescribe as Consumable)!.AdditionalEffect!);
                }
                if ((itemToDescribe as Consumable)!.SpecialEffects.Count > 0)
                {
                    specialEffects = "Efekty specjalne: ";
                    (itemToDescribe as Consumable)!.SpecialEffects.ForEach(eff =>
                    {
                        specialEffects += GetSpecialEffectDescription(eff) + ", ";
                    });
                }
            }
            else if (itemToDescribe.GetType() == typeof(Armor))
            {
                Armor armorToDescribe = (Armor)itemToDescribe;

                //set string for polish armor type
                itemType += GetPolishArmorType(armorToDescribe.Type);

                defense = "Obrona: " + armorToDescribe.Defense;

                //add info about armor weight type
                if (armorToDescribe.Type == Armor.ArmorType.Torso)
                {
                    if (armorToDescribe.Weight >= 500)
                    {
                        itemType += " (ciężka zbroja)";
                    }
                    else if (armorToDescribe.Weight < 500 && armorToDescribe.Weight >= 200)
                    {
                        itemType += " (lekka zbroja)";
                    }
                    else
                    {
                        itemType += " (szata)";
                    }
                }
                if (armorToDescribe.Type == Armor.ArmorType.Pants)
                {
                    if (armorToDescribe.Weight >= 400)
                    {
                        itemType += " (ciężka zbroja)";
                    }
                    else if (armorToDescribe.Weight < 400 && armorToDescribe.Weight >= 150)
                    {
                        itemType += " (lekka zbroja)";
                    }
                    else
                    {
                        itemType += " (szata)";
                    }
                }
                if (armorToDescribe.Type == Armor.ArmorType.Helmet || armorToDescribe.Type == Armor.ArmorType.Gloves ||
                    armorToDescribe.Type == Armor.ArmorType.Gloves)
                {
                    if (armorToDescribe.Weight >= 250)
                    {
                        itemType += " (ciężka zbroja)";
                    }
                    else if (armorToDescribe.Weight < 250 && armorToDescribe.Weight >= 100)
                    {
                        itemType += " (lekka zbroja)";
                    }
                    else
                    {
                        itemType += " (szata)";
                    }
                }


            }
            else if (itemToDescribe.GetType() == typeof(Weapon))
            {
                Weapon weaponToDescribe = (Weapon)itemToDescribe;

                attack = "Atak: " + weaponToDescribe!.Attack;

                //determine weapon type
                if (weaponToDescribe.Type == Weapon.WeaponType.Dagger)
                {
                    itemType += "Broń krótka";
                }
                else if (weaponToDescribe.Type == Weapon.WeaponType.Blade)
                {
                    itemType += "Broń sieczna";
                }
                else if (weaponToDescribe.Type == Weapon.WeaponType.Blunt)
                {
                    itemType += "Broń obuchowa";
                }
            }
            else if (itemToDescribe.GetType() == typeof(RuneStone))
            {
                itemType += "Runa";
            }

            //set modifiers string
            itemToDescribe.Modifiers!.ForEach(mod =>
            {
                //only if the mod is not temporary
                if (mod.Duration == 0)
                {
                    modifiers += GetModDescription(mod) + ", ";
                }
            });

            //remove trailing comma
            modifiers = Regex.Replace(modifiers, @",\s$", "");

            //print basic item info
            PrintMessage("[ " + itemToDescribe.Name + " ]");
            PrintMessage(itemToDescribe.Description!);
            PrintMessage(weight);
            PrintMessage(itemType);
            if (effect != string.Empty)
            {
                PrintMessage(effect);
            }
            if (additionalEffect != string.Empty)
            {
                PrintMessage(additionalEffect);
            }
            if (specialEffects != string.Empty)
            {
                specialEffects = Regex.Replace(specialEffects, @",\s$", "");
                PrintMessage(specialEffects);
            }
            if (defense != string.Empty)
            {
                PrintMessage(defense);
            }
            if (attack != string.Empty)
            {
                PrintMessage(attack);
            }
            if (range != string.Empty)
            {
                PrintMessage(range);
            }
            if (modifiers != string.Empty)
            {
                PrintMessage("Modyfikatory: " + modifiers);
            }
        }

        //method printing map around player's current location
        //in form of ascii art (or unicode art to be more precise)
        private void PrintMap()
        {
            Location center = Data.Player!.CurrentLocation!;
            Location examinedLocation = new Location();
            Location eastAdjacentLoc = new Location();
            Location northAdjacentLoc = new Location();
            const string enDash = "\x2500";
            const string emptyLoc = "\x25A1";
            const string itemsLoc = "\x2736";
            const string filledLoc = "\x263C";
            const string player = "\x25A0";
            const string upAndDown = "\x2195";
            const string traderLoc = "\x2302";
            const string heroLoc = "\x25D9";
            const int rangeSize = 12;
            
            int i, j, k;
            int[] horizontalRange = new int[rangeSize + 1];
            int[] verticalRange = new int[rangeSize + 1];
            string[] mapLines = new string[rangeSize * 2];
            int lowestX = center.X - rangeSize / 2;
            int lowestY = center.Y - rangeSize / 2;
            int currentZ = center.Z;

            
            //fill horizontalRange
            for (i = 0; i < rangeSize + 1; i++)
            {
                horizontalRange[i] = lowestX + i;
                verticalRange[i] = lowestY + i;
            }

            k = 0;

            //fill every second line containing hipothetical locations
            for (i = 0; i < rangeSize; i++)
            {
                for (j = 0; j < rangeSize; j++)
                {
                    if (IsThereALocation(horizontalRange[j], verticalRange[i], currentZ))
                    {
                        examinedLocation = Data.Locations!.Find(loc => loc.X == horizontalRange[j] &&
                            loc.Y == verticalRange[i] && loc.Z == currentZ)!;
                        eastAdjacentLoc = Data.Locations!.Find(loc => loc.X == horizontalRange[j] + 1 &&
                        loc.Y == verticalRange[i] && loc.Z == currentZ)!;

                        //check if location is visible to player
                        if (examinedLocation.IsVisible)
                        {

                            //if it's player's current location, sign it with 'player sign'
                            if (center.X == horizontalRange[j] && center.Y == verticalRange[i])
                            {
                                mapLines[k] += player;
                            }
                            else
                            {

                                //otherwise, if there is another location in the same Z axis
                                //directly connected to this one
                                if (IsThereALocation(horizontalRange[j], verticalRange[i], currentZ - 1) ||
                                    IsThereALocation(horizontalRange[j], verticalRange[i], currentZ + 1))
                                {
                                    mapLines[k] += upAndDown;
                                }
                                else if (examinedLocation.Characters!.Exists(character =>
                                character.GetType() == typeof(Trader)))
                                {
                                    mapLines[k] += traderLoc;
                                }
                                else if (examinedLocation.Characters!.Exists(character =>
                                character.GetType() == typeof(Monster)))
                                {
                                    mapLines[k] += filledLoc;
                                }
                                else if (examinedLocation.Characters!.Exists(character =>
                                character.GetType() == typeof(Hero)))
                                {
                                    mapLines[k] += heroLoc;
                                }
                                else if (examinedLocation.Items!.Count! > 0 || examinedLocation.Gold > 0)
                                {
                                    mapLines[k] += itemsLoc;
                                }
                                else
                                {
                                    mapLines[k] += emptyLoc;
                                }
                            }
                        }
                        else
                        {
                            mapLines[k] += " ";
                        }
                        
                    }
                    else
                    {
                        mapLines[k] += " ";
                    }

                    if (IsThereALocation(horizontalRange[j] + 1, verticalRange[i], currentZ) && 
                        IsThereALocation(horizontalRange[j], verticalRange[i], currentZ) &&
                        eastAdjacentLoc.IsVisible && examinedLocation.IsVisible) 
                    {
                        mapLines[k] += " " + enDash + " ";
                    }
                    else
                    {
                            mapLines[k] += "   ";
                    }
                }

                k += 2;
            }

            //fill the rest of in-between lines containing vertical passages
            //connecting adjacent locations
            k = 1;
            for (i = 0; i < rangeSize; i++)
            {
                for (j = 0; j < rangeSize; j++)
                {
                    examinedLocation = Data.Locations!.Find(loc => loc.X == horizontalRange[j] &&
                            loc.Y == verticalRange[i] && loc.Z == currentZ)!;
                    northAdjacentLoc = Data.Locations!.Find(loc => loc.X == horizontalRange[j] &&
                        loc.Y == verticalRange[i] + 1 && loc.Z == currentZ)!;

                    if (IsThereALocation(horizontalRange[j], verticalRange[i], currentZ) && 
                        IsThereALocation(horizontalRange[j], verticalRange[i] + 1, currentZ) && 
                        northAdjacentLoc.IsVisible && examinedLocation.IsVisible)
                    {
                        mapLines[k] += "|";
                    }
                    else
                    {
                        mapLines[k] += " ";
                    }

                    mapLines[k] += "   ";
                }


                k += 2;
            }

            //reverse the order of lines
            Array.Reverse(mapLines);

            //clear previous location centered minimap
            Window.minimapBox.SelectAll();
            Window.minimapBox.Selection.Text = "";

            //display new location centered minimap
            for (i = 1; i < mapLines.Length; i++)
            {
                TextRange tr = new(this.Window.minimapBox.Document.ContentEnd, this.Window.minimapBox.Document.ContentEnd);
                tr.Text = "\n" + mapLines[i];
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                Window.minimapBox.ScrollToEnd();
            }
        }

        //method printing player's statistics
        private void StatsInfo()
        {
            const int halfSize = 28;
            const int rowsSize = 13;
            const int numberOfAttributes = 3;
            int remainingSpace;
            int i;
            int j;
            int[] diffs = new int[numberOfAttributes];
            string[] rows = new string[rowsSize];
            string[] attributes = new string[numberOfAttributes];
            Player player = Data.Player!;

            //set string for displaying attributes base values along with modifiers bonus
            attributes[0] = Convert.ToString(player.Strength) + "(";
            attributes[1] = Convert.ToString(player.Agility) + "(";
            attributes[2] = Convert.ToString(player.Intelligence) + "(";
            diffs[0] = player.GetEffectiveStrength() - player.Strength;
            diffs[1] = player.GetEffectiveAgility() - player.Agility;
            diffs[2] = player.GetEffectiveIntelligence() - player.Intelligence;

            for (i = 0; i < numberOfAttributes; i++)
            {
                if (diffs[i] >= 0)
                {
                    attributes[i] += "+";
                }

                attributes[i] += diffs[i] + ")";
            }

            //shorten the exp numbers if they reached
            //enormous values using 'K' notation
            ulong actualExp = player.Experience;
            ulong nextLevelCap = player.NextLvlExpCap;
            string actualExpString = string.Empty;
            string nextLevelCapString = Convert.ToString(nextLevelCap);

            if (actualExp >= 100000)
            {
                actualExp /= 1000;
                actualExpString = Convert.ToString(actualExp) + "K";
            }
            else
            {
                actualExpString = Convert.ToString(player.Experience);
            }

            if (nextLevelCap >= 100000)
            {
                nextLevelCap /= 1000;
                nextLevelCapString = Convert.ToString(nextLevelCap) + "K";
            }
            else
            {
                nextLevelCapString = Convert.ToString(player.NextLvlExpCap);
            }

            //format left side of the table
            rows[0] = "********************STATYSTYKI POSTACI********************";
            rows[1] = "|| Poziom: " + player.Level;
            rows[2] = "|| Dośw.: " + actualExpString;
            rows[3] = "|| Awans za: " + nextLevelCapString;
            rows[4] = "|| Pkt. Atrybutów: " + player.AttributePoints;
            rows[5] = "||-----------------------";
            rows[6] = "|| Siła: " + attributes[0];
            rows[7] = "|| Zręczność: " + attributes[1];
            rows[8] = "|| Inteligencja: " + attributes[2];
            rows[9] = "||-----------------------";
            rows[10] = "|| Maks. HP: " + player.EffectiveMaxHp;
            rows[11] = "|| Maks. MP: " + player.EffectiveMaxMp;
            rows[12] = "==========================================================";

            //fill remaining space with space-characters
            for (i = 1; i < rowsSize - 1; i++)
            {
                remainingSpace = halfSize - rows[i].Length;

                //fill with spaces
                for (j = 0; j < remainingSpace - 3; j++)
                {
                    rows[i] += " ";
                }

                //add middle vertical border
                rows[i] += "| ";
            }

            //add right side (combat statistics)
            
            rows[1] += "Atak: " + Math.Floor(player.GetEffectiveAttack());
            rows[2] += "Szybkość Ataku: " + Math.Floor(player.GetEffectiveAtkSpeed());
            rows[3] += "Celność: " + Math.Floor(player.GetEffectiveAccuracy());
            rows[4] += "Traf. krytyczne: " + Math.Floor(player.GetEffectiveCritical()) +
                " (" + (int)Math.Sqrt(player.GetEffectiveCritical()) + "%)";
            rows[5] += "Obrona: " + Math.Floor(player.GetEffectiveDefense());
            rows[6] += "Uniki: " + Math.Floor(player.GetEffectiveEvasion());
            rows[7] += "Odporność na magię: " + Math.Floor(player.GetEffectiveMagicResistance());
            rows[8] += "Szybkość: " + Math.Floor(player.GetEffectiveSpeed());
            rows[9] += "Regeneracja HP " + Math.Floor(player.GetEffectiveHpRegen());
            rows[10] += "Regeneracja MP: " + Math.Floor(player.GetEffectiveMpRegen());
            rows[11] += "Udźwig: " + player.GetWeightLimit();

            //if the weight cap is exceeded
            if (player.GetCarryWeight() > player.GetWeightLimit())
            {
                rows[11] += " (Przeciążenie!)";
            }

            //fill remaining space
            for (i = 1; i < rowsSize - 1; i++)
            {
                remainingSpace = halfSize * 2 - rows[i].Length;

                //fill with spaces
                for (j = 0; j < remainingSpace; j++)
                {
                    rows[i] += " ";
                }

                //add middle vertical border
                rows[i] += "||";
            }

            foreach (string row in rows)
            {
                PrintMessage(row);
            }
        }

        //method printing info about player's remembered spells
        private void SpellsInfo(CombatCharacter character)
        {
            int i, j;
            int remainingSpace;
            int tableWidth = 53;
            int numberOfRows = character.MaxSpellsRemembered + 2;
            string[] tableRows = new string[numberOfRows];

            tableRows[0] = "**************** ZAPAMIĘTANE CZARY ******************";

            //bottom border of the table
            for (i = 0; i < tableWidth; i++)
            {
                tableRows[numberOfRows - 1] += "*";
            }

            //fill interior of the table with character's spells
            for (i = 1; i < numberOfRows - 1; i++)
            {
                tableRows[i] += "||   " + i + ". ";

            }


            for (i = 1; i < numberOfRows - 1; i++)
            {
                if (Data.Player!.RememberedSpells.Count > i - 1)
                {
                    tableRows[i] += Data.Player!.RememberedSpells[i - 1].Name;
                }

                //fill remaining space in every row with white space characters
                remainingSpace = tableWidth - tableRows[i].Length - 2;
                for (j = 0; j < remainingSpace; j++)
                {
                    tableRows[i] += " ";
                }

                tableRows[i] += "||";
            }

            for (i = 0; i < numberOfRows; i++)
            {
                PrintMessage(tableRows[i]);
            }

            PrintMessage("Maks. zapamiętanych czarów: (" + character.MaxSpellsRemembered + ")");

        }

        //method printing detailed info about single spell
        private void SpellInfo(Spell spell)
        {
            string name = spell.Name!;
            string description = spell.Description!;
            string defaultTarget = "Cel domyślny: ";
            string manaCost = "Koszt many: " + spell.ManaCost;
            string dmgDealt = "Moc: " + spell.Power;
            string effect = "Działanie: ";
            string specialEffects = "Efekty specjalne: ";

            //assign proper default target name
            if (spell.DefaultTarget == Spell.Target.Self)
            {
                defaultTarget += "Ty";
            }
            else
            {
                defaultTarget += "Przeciwnik";
            }

            //get special effects description
            spell.SpecialEffects.ForEach(eff =>
            {
                specialEffects += GetSpecialEffectDescription(eff) + ", ";
            });

            //remove trailing comma from special effects description
            specialEffects = Regex.Replace(specialEffects, @",\s$", "");

            effect += GetEffectDescription(spell.Modifiers!);

            //display info
            PrintMessage("[ " + name + " ]");
            PrintMessage(description);
            PrintMessage(defaultTarget);
            PrintMessage(manaCost);
            PrintMessage(dmgDealt);
            PrintMessage(effect);
            PrintMessage(specialEffects);
        }

        //method printing commands-manual
        private void PrintCommandsCS(bool isLong = true)
        {
            const int manualSize = 150;
            string[] manualLines = new string[manualSize];
            int i;
            int start = 0;
            bool shouldScroll = isLong ? false : true;

            for (i = 0; i < manualSize; i++)
            {
                manualLines[i] = string.Empty;
            }

            i = 0;

            if (isLong)
            {
                manualLines[i++] = "************************* KOMENDY *************************\n";
                manualLines[i++] = "             (Wciśnij esc aby wrócić do menu)\n";
                manualLines[i++] = "         Grą sterujesz wpisując komendy. Do niektórych";
                manualLines[i++] = "         komend można dodać jakąś nazwę lub liczbę.";
                manualLines[i++] = "         Na przykład komenda 'look' to patrzenie.";
                manualLines[i++] = "         Jeśli wpiszesz samo 'look' otrzymasz opis";
                manualLines[i++] = "         miejsca w jakim przebywasz, ale jeśli dodasz";
                manualLines[i++] = "         nazwę obiektu, np 'look szczur' wyświetli Ci";
                manualLines[i++] = "         się opis postaci o nazwie szczur.\n";
                manualLines[i++] = "         Przy wpisywaniu, wielkość liter";
                manualLines[i++] = "         nie ma znaczenia (np. SzCzUr = szczur)";
                manualLines[i++] = "         można pomijać polskie znaki ";
                manualLines[i++] = "         (np. \"żmija\" = \"zmija\").";
                manualLines[i++] = "         oraz pomijać znak \"_\"";
                manualLines[i++] = "         (np. mikstura_życia = miksturazycia)\n";
                manualLines[i++] = "  >>> OBEJŻYJ";
                manualLines[i++] = "     * Komenda: 'look [nazwa obiektu]'";
                manualLines[i++] = "     * Skrót: 'l'";
                manualLines[i++] = "           Ogląda obiekt o wybranej nazwie {np. 'look zardzewiały_miecz'}. Aby obejżeć lokację (rozejżeć się), " +
                    "wpisz samą komendę 'look'. Możesz też spojrzeć do sąsiedniej lokacji wpisując jeden z kierunków (n, e, s, w, u, d)" +
                    " zamiast nazwy obiektu {np. 'look n'}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> IDŹ";
                manualLines[i++] = "     * Komenda: '[litera kierunku]'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Wpisujesz jedną z liter kierunku ('n' - północ, 'e' - wschód, 's' - południe, 'w' - zachód, 'u' - góra, 'd' - dół),";
                manualLines[i++] = "aby twoja postać poszła w wybranym kierunku {np. 'n' - idzie na północ} ";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> ATAKUJ";
                manualLines[i++] = "     * Komenda: 'attack [nazwa przeciwnika]'";
                manualLines[i++] = "     * Skrót: 'a'";
                manualLines[i++] = "           Atakuje wybranego przeciwnika {np. 'attack szczur'}. Jeśli wpiszesz samo 'attack', Twoja postać zaatakuję pierwszego lepszego" +
                    " przeciwnika. Jeśli już z kimś walczysz, wystarczy samo 'attack' aby zaatakować" +
                    " Twojego przeciwnika (lub zaatakować najsłabszego z wielu przeciwników z którymi walczysz jednocześnie)";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> UCIEKNIJ";
                manualLines[i++] = "     * Komenda: 'flee [nazwa kierunku]'";
                manualLines[i++] = "     * Skrót: 'f'";
                manualLines[i++] = "           Twoja postać próbuje ucieczki we losowym kierunku. Możesz wskazać kierunek ucieczki {np. 'flee n'}.";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> UŻYJ PRZEDMIOTU";
                manualLines[i++] = "     * Komenda: 'use [nazwa obiektu]'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Używa wybranego obiektu {np. 'use mikstura_many'}.";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> ROZMAWIAJ";
                manualLines[i++] = "     * Komenda: 'talk [nazwa postaci]'";
                manualLines[i++] = "     * Skrót: 'ta'";
                manualLines[i++] = "           Rozpoczyna rozmowę z wybraną postacią {np. 'talk karczmarz'}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> HANDLUJ";
                manualLines[i++] = "     * Komenda: 'trade [nazwa postaci]'";
                manualLines[i++] = "     * Skrót: 'tr'";
                manualLines[i++] = "           Rozpoczyna handel z wybraną postacią {np. 'trade karczmarz'}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> KUP";
                manualLines[i++] = "     * Komenda: 'buy [nazwa przedmiotu] [ilość]'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Kupuje jedną sztukę wybranego przedmiotu {np. 'buy piwo'}. Możesz dodać ilość sztuk jaką " +
                    "chcesz kupić {np. 'buy piwo 3'}. Jeśli chcesz kupić wszystko co ma handlarz - wpisz " +
                    "'all' lub 'a' {np. 'buy piwo all' lub 'buy piwo a'}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> SPRZEDAJ";
                manualLines[i++] = "     * Komenda: 'sell [nazwa przedmiotu] [ilość]'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Sprzedaje jedną sztukę wybranego przedmiotu {np. sell piwo}. Możesz dodać ilość sztuk jaką " +
                    "chcesz sprzedać {np. 'sell piwo 3'}. Jeśli chcesz sprzedać wszystko co posiadasz, wpisz " +
                    "'all' lub 'a' {np. 'sell piwo all' lub 'sell piwo a'} (tylko co potem będziesz pił?)}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> PODNIEŚ";
                manualLines[i++] = "     * Komenda: 'pickup [nazwa przedmiotu] [ilość]'";
                manualLines[i++] = "     * Skrót: 'p'";
                manualLines[i++] = "           Podnosi wszystkie przedmioty oraz złoto leżące w lokacji. Możesz dodać nazwę przedmiotu aby podnieść tylko " +
                    "wybrany przedmiot {np. 'pickup drewniana_pałka'}. Możesz też dodać ilość sztuk {np. 'pickup złoto 10' - podniesie 10 złota}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> WYRZUĆ";
                manualLines[i++] = "     * Komenda: 'drop [nazwa przedmiotu] [ilość]'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Wyrzuca jeden wybrany przedmiot {np. 'drop drewniana_pałka'}. Możesz dodać ilość sztuk" +
                    " {np. 'drop piwo 3' (kto to widział piwo wylewać..)}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> ZAŁÓŻ PRZEDMIOT";
                manualLines[i++] = "     * Komenda: 'wear [nazwa przedmiotu]'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Zakłada wybrany przedmiot {np. 'wear skórzana_kurtka'}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> ŚCIĄGNIJ PRZEDMIOT";
                manualLines[i++] = "     * Komenda: 'takeoff [typ przedmiotu]'";
                manualLines[i++] = "     * Skrót: 'of'";
                manualLines[i++] = "           Ściąga przedmiot wybranego typu ('weapon' - broń, 'helmet' - hełm, 'torso' - tors, " +
                    "'pants' - spodnie, 'gloves' - rękawice, 'shoes' - buty) {np. 'takeoff weapon'}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> RZUĆ CZAR";
                manualLines[i++] = "     * Komenda: 'cast [numer czaru] [cel]'";
                manualLines[i++] = "     * Skrót: 'c'";
                manualLines[i++] = "           Rzuca czar z listy zapamiętanych czarów {np. 'cast 1' - rzuci pierwszy czar na liście}. " +
                    "Możesz wybrać cel w jaki chcesz rzucić czar {np. 'cast 1 szkielet_wojownik' - rzuci pierwszy czar na liście w szkielet_wojownik}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> UTWÓRZ CZAR";
                manualLines[i++] = "     * Komenda: 'craft [nazwa runy] [nazwa runy]'";
                manualLines[i++] = "     * Skrót: 'cr'";
                manualLines[i++] = "           Tworzy czar z wybranej runy lub kombinacji run {np. 'craft akull verde' - tworzy czar z run " +
                    "'akull' i 'verde'}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> UŻYJ PUNKTU ATRYBUTU";
                manualLines[i++] = "     * Komenda: 'point [nazwa atrybutu]'";
                manualLines[i++] = "     * Skrót: 'pt'";
                manualLines[i++] = "           Zużywa jeden punkt atrybutu i dodaje jeden do wybranego atrybutu ('strength' - siła, " +
                    "'agility' - zręczność, 'intelligence' - inteligencja) (lub skróty: 'str', 'agi', 'int') {np. 'point strength' - dodaje jeden do siły}";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> EKWIPUNEK"; 
                manualLines[i++] = "     * Komenda: 'inventory'";
                manualLines[i++] = "     * Skrót: 'i'";
                manualLines[i++] = "           Pokazuje ekwipunek Twojej postaci";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> STATYSTYKI";
                manualLines[i++] = "     * Komenda: 'stats'";
                manualLines[i++] = "     * Skrót: 'ss'";
                manualLines[i++] = "           Pokazuje statystyki Twojej postaci";
                manualLines[i++] = "  >>> EFEKTY";
                manualLines[i++] = "     * Komenda: 'effects'";
                manualLines[i++] = "     * Skrót: 'ef'";
                manualLines[i++] = "           Pokazuje wszystkie efekty wpływające w tej chwili na Twoją postać";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> ZAKLĘCIA";
                manualLines[i++] = "     * Komenda: 'spells'";
                manualLines[i++] = "     * Skrót: 'sps'";
                manualLines[i++] = "           Pokazuje zapamiętane czary";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> PRZERWIJ AKCJĘ";
                manualLines[i++] = "     * Komenda: 'stop'";
                manualLines[i++] = "     * Skrót: 'st'";
                manualLines[i++] = "           Przerywa ostatnio zakolejkowaną akcję, handel, rozmowę lub atak";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> ZATRZYMAJ/WZNÓW GRĘ (PAUZA)";
                manualLines[i++] = "     * Komenda: 'pause'";
                manualLines[i++] = "     * Skrót: 'ps'";
                manualLines[i++] = "           Całkowicie wstrzymuje/wznawia grę."; 
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> SZYBKI ZAPIS";
                manualLines[i++] = "     * Komenda: 'save'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Zapisuje bieżący stan gry";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> WYŁĄCZ/WŁĄCZ AUTOMATYCZNY ATAK";
                manualLines[i++] = "     * Komenda: 'autoattack'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Wyłącza/włącza automatyczny atak Twojej postaci.";
                manualLines[i++] = " ";
                manualLines[i++] = "  >>> CZYSZCZENIE EKRANU";
                manualLines[i++] = "     * Komenda: 'clear'";
                manualLines[i++] = "     * Skrót: brak";
                manualLines[i++] = "           Wymazuje całą, dotychczasową treść ekranu gry";
                manualLines[i++] = " >>> PRZYSPIESZANIE/SPOWALNIANIE GRY";
                manualLines[i++] = "     * Komenda 'speedup'/'slowdown'";
                manualLines[i++] = "     * Skrót: 'up'/'dn'";
                manualLines[i++] = "           Przyspiesza/spowalnia działanie zegara gry.";
            }
            else
            {
                manualLines[i++] = " * 'komenda' ('skrót komendy') - opis komendy";
                manualLines[i++] = " * 'look' ('l') - oglądanie";
                manualLines[i++] = " * 'n'/'e'/'s'/'w'/'u'/'d' - poruszanie się";
                manualLines[i++] = " * 'attack' ('a') - atakowanie";
                manualLines[i++] = " * 'flee' ('f') - ucieczka";
                manualLines[i++] = " * 'use' - używanie przedmiotów";
                manualLines[i++] = " * 'talk' ('ta') - rozmawianie";
                manualLines[i++] = " * 'trade' ('tr') - handel";
                manualLines[i++] = " * 'buy' - kupno";
                manualLines[i++] = " * 'sell' - sprzedaż";
                manualLines[i++] = " * 'pickup' ('p') - podnoszenie";
                manualLines[i++] = " * 'wear' - zakładanie przedmiotów";
                manualLines[i++] = " * 'drop' - wyrzucanie przedmiotów";
                manualLines[i++] = " * 'takeoff' ('of') - zdejmowanie przedmiotów";
                manualLines[i++] = " * 'cast' ('c') - rzucanie czarów";
                manualLines[i++] = " * 'craft' ('cr') - tworzenie czarów";
                manualLines[i++] = " * 'point' ('pt') - używanie pkt. atrybutów";
                manualLines[i++] = " * 'inventory' ('i') - oglądanie ekwipunku";
                manualLines[i++] = " * 'stats' ('ss') - oglądanie statystyk";
                manualLines[i++] = " * 'effects' ('ef') - oglądanie efektów";
                manualLines[i++] = " * 'spells' ('sps') - oglądanie zapamiętanych czarów";
                manualLines[i++] = " * 'stop' ('st') - przerywanie akcji";
                manualLines[i++] = " * 'pause' ('ps') - zatrzymanie/wznowienie gry";
                manualLines[i++] = " * 'save' - szybki zapis";
                manualLines[i++] = " * 'autoattack' - włączenie/wyłączenie auto. ataku";
                manualLines[i++] = " * 'clear' - czyszczenie ekranu";
                manualLines[i++] = " * 'speedup' ('up') - przyspiesza grę";
                manualLines[i++] = " * 'slowdown' ('dn') - spowalnia grę";
            }

            //make space before short commands cheatsheet
            if (!isLong) PrintMessage(" ");

            for (i = start; i < manualLines.Length; i++)
            {
                if (manualLines[i] == string.Empty)
                {
                    break;
                }

                //print first (description) line of short commands CS in different color
                if(i == 0 && !isLong)
                {
                    PrintMessage(manualLines[i], MessageType.EffectOn, shouldScroll);
                    continue;
                }

                if (i == 1 && isLong)
                {
                    PrintMessage(manualLines[i], MessageType.Action, shouldScroll);
                    continue;
                }
                if (Regex.IsMatch(manualLines[i], @"^\s+>>>"))
                {
                    PrintMessage(manualLines[i], MessageType.EffectOn, shouldScroll);
                }
                else if (Regex.IsMatch(manualLines[i], @"^\s+\*\s"))
                {
                    PrintMessage(manualLines[i], MessageType.UserCommand, shouldScroll);
                }
                else if (Regex.IsMatch(manualLines[i], @"^\*{3,20}"))
                {
                    PrintMessage(manualLines[i], MessageType.Gain, shouldScroll);
                }
                else 
                { 
                PrintMessage(manualLines[i], MessageType.Default, shouldScroll);
                }
            }
        }

        //method printing game manual
        private void PrintManual(bool isExitInfoPrinted = true)
        {
            const int manualSize = 220;
            string[] manualLines = new string[manualSize];
            int i;

            for (i = 0; i < manualSize; i++)
            {
                manualLines[i] = string.Empty;
            }

            i = 0;

            string title = "********************** INSTRUKCJA GRY *********************\n";
            string exitInfo = "           (Wciśnij esc aby wrócić do menu)\n";

            manualLines[i++] = "      Runedal jest grą typu RPG, w której wszystko co ";
            manualLines[i++] = "      musisz robić, to czytać i pisać.";
            manualLines[i++] = "      Grasz postacią, która może się poruszać z";
            manualLines[i++] = "      lokacji do lokacji, rozmawiać, atakować i ";
            manualLines[i++] = "      handlować z innymi postaciami, używać przedmiotów";
            manualLines[i++] = "      czy tworzyć i rzucać zaklęcia. Zdobywasz ";
            manualLines[i++] = "      kolejne poziomy, przedmioty i rozwijasz swoją";
            manualLines[i++] = "      postać w dowolny sposób.";
            manualLines[i++] = "      Grą sterujesz za pomocą wpisywania komend.";
            manualLines[i++] = "      Aby zobaczyć listę komend, wyjdź do menu";
            manualLines[i++] = "      i wybierz opcję 4.KOMENDY.\n";
            manualLines[i++] = "  >>> INTERFEJS UŻYTKOWNIKA\n";
            manualLines[i++] = "      Praktycznie cała akcja gry, odbywa się w tym";
            manualLines[i++] = "      największym, czarnym oknie, na którym";
            manualLines[i++] = "      czytasz teraz ten tekst. To tutaj wyświetlane";
            manualLines[i++] = "      będą opisy lokacji, dialogi czy przebiegi walk.";
            manualLines[i++] = "      Nieco niżej znajdują się paski HP (zdrowie),";
            manualLines[i++] = "      MP (mana) oraz pasek akcji (ten po prawej)";
            manualLines[i++] = "      Poniżej pasków jest miejsce do wpisywania komend";
            manualLines[i++] = "      za pomocą, których sterujesz grą.";
            manualLines[i++] = "      Ten zielony prostokąt w prawym górnym rogu,";
            manualLines[i++] = "      to mapka, która pokazuje gdzie się znajdujesz.\n";
            manualLines[i++] = "  >>> ZATRZYMANIE/WZNOWIENIE GRY\n";
            manualLines[i++] = "      W każdym momencie rozgrywki możesz zatrzymać grę";
            manualLines[i++] = "      wpisując komendę 'pause'. Jeśli chcesz wznowić grę";
            manualLines[i++] = "      znowu wpisujesz 'pause'.\n";
            manualLines[i++] = "  >>> PORUSZANIE SIĘ\n    ";
            manualLines[i++] = "      Świat gry jest podzielony na tzw. lokacje.";
            manualLines[i++] = "      Każda lokacja może mieć wyjścia w sześciu";
            manualLines[i++] = "      możliwych kierunkach: północ, południe, wschód";
            manualLines[i++] = "      zachód, dół i góra. Możesz przemieszczać się";
            manualLines[i++] = "      pomiędzy lokacjami, zwyczajnie wpisując";
            manualLines[i++] = "      jedną z liter kierunku:";
            manualLines[i++] = "      > 'n' - północ";
            manualLines[i++] = "      > 's' - południe";
            manualLines[i++] = "      > 'e' - wschód";
            manualLines[i++] = "      > 'w' - zachód";
            manualLines[i++] = "      > 'u' - góra";
            manualLines[i++] = "      > 'd' - dół";
            manualLines[i++] = "       Jeśli chcesc iść np. na północ";
            manualLines[i++] = "       - wpisujesz 'n' i naciskasz enter.\n";
            manualLines[i++] = "  >>> PATRZENIE\n";
            manualLines[i++] = "      W grze nie ma żadnej grafiki, więc wszystko";
            manualLines[i++] = "      co \"widzisz\" będzie opisywane słowami.";
            manualLines[i++] = "      Do patrzenia służy komenda 'look'. Jeżeli";
            manualLines[i++] = "      wpiszesz 'look', wyświetli Ci się podstawowy";
            manualLines[i++] = "      opis lokacji, w której przebywa Twoja postać:";
            manualLines[i++] = "      nazwa lokacji, opis, możliwe kierunki wyjścia, ";
            manualLines[i++] = "      postacie jakie się w niej znajdują oraz";
            manualLines[i++] = "      przedmioty leżące na podłodze.";
            manualLines[i++] = "      Można dowiedzieć się więcej o jakimś obiekcie";
            manualLines[i++] = "      (np przedmiocie), dodając jego nazwę do komendy";
            manualLines[i++] = "      'look'. Np. 'look piwo' wyświetli dokładny opis";
            manualLines[i++] = "      przedmiotu o nazwie \"piwo\". Można oglądać:";
            manualLines[i++] = "      przedmioty, postacie, czary oraz sąsiednie ";
            manualLines[i++] = "      lokacje. Aby zajżeć do sąsiedniej lokacji,";
            manualLines[i++] = "      wpisujesz 'look' i literę kierunku (np. 'look n')\n";
            manualLines[i++] = "  >>> POSTACIE\n   ";
            manualLines[i++] = "      W każdej lokacji mogą znajdować się jakieś";
            manualLines[i++] = "      postacie. Ze wszystkimi postaciami możesz rozmawiać";
            manualLines[i++] = "      używając komendy 'talk'. Np. 'talk karczmarz'";
            manualLines[i++] = "      rozpocznie rozmowę z karczmarzem. Na tej samej";
            manualLines[i++] = "      zasadzie możesz handlować (komenda 'trade') lub";
            manualLines[i++] = "      atakować (komenda 'attack') inne postacie.\n";
            manualLines[i++] = "  >>> PRZEDMIOTY\n";
            manualLines[i++] = "      Jeśli wpiszesz komendę 'inventory' wyświetli Ci";
            manualLines[i++] = "      się Twój ekwipunek, czyli wszystko co posiadasz";
            manualLines[i++] = "      Na dole ekwipunku widzisz przedmioty które nosi";
            manualLines[i++] = "      Twoja postać na sobie (np broń lub spodnie).";
            manualLines[i++] = "      Możesz zakładać przedmioty, które masz w plecaku";
            manualLines[i++] = "      za pomocą komendy 'wear' i nazwy przedmiotu. Np.";
            manualLines[i++] = "      'wear stalowy_topór'. Aby ściągnąć przedmiot";
            manualLines[i++] = "      używasz komendy 'takeoff' i typu przedmiotu.";
            manualLines[i++] = "      Typy przedmiotów to:";
            manualLines[i++] = "      > 'weapon' - broń";
            manualLines[i++] = "      > 'helmet' - hełm";
            manualLines[i++] = "      > 'torso' - korpus";
            manualLines[i++] = "      > 'pants' - spodnie";
            manualLines[i++] = "      > 'gloves' - rękawice";
            manualLines[i++] = "      > 'shoes' - buty";
            manualLines[i++] = "      Jeśli np. masz założoną jakąś broń i chcesz ją";
            manualLines[i++] = "      ściągnąc, wpisujesz 'takeoff weapon'. Jeśli ";
            manualLines[i++] = "      chcesz ściągnąć rękawice - 'takeoff gloves'. itd.\n";
            manualLines[i++] = "  >>> HANDEL\n   ";
            manualLines[i++] = "      Aby rozpocząć handel z postacią, używasz komendy";
            manualLines[i++] = "      'trade' i nazwy postaci (np. 'trade karczmarz').";
            manualLines[i++] = "      Podczas handlu, możesz używać dwóch dodatkowych";
            manualLines[i++] = "      komend: 'buy' i 'sell'. 'buy' służy do kupowania,";
            manualLines[i++] = "      a 'sell' do sprzedawania. Jeżeli chcesz";
            manualLines[i++] = "      kupić/sprzedać jedną sztukę jakiegoś przedmiotu,";
            manualLines[i++] = "      wpisujesz 'buy'/'sell' i nazwę tego przedmiotu";
            manualLines[i++] = "      (np. 'buy piwo'). Jeśli chcesz kupić więcej niż";
            manualLines[i++] = "      jeden, dodajesz do tego liczbę. Np. 'buy piwo 5'";
            manualLines[i++] = "      kupi pięć sztuk piwa. Tak samo działa komenda 'sell'\n";
            manualLines[i++] = "  >>> AKCJE\n    ";
            manualLines[i++] = "      Niektóre czynności w grze to tzw. \"akcje\".";
            manualLines[i++] = "      Każde wykonanie akcji powoduje że pasek akcji";
            manualLines[i++] = "      rośnie, a Twoja postać nie może nić zrobić, dopóki";
            manualLines[i++] = "      pasek nie spadnie do zera. Jeśli w tym czasie";
            manualLines[i++] = "      wydasz jakąś komendę akcji, Twoja postać poczeka";
            manualLines[i++] = "      aż pasek będzie pusty i dopiero wtedy wykona";
            manualLines[i++] = "      polecenie. Możesz ją przed tym powstrzymać, ";
            manualLines[i++] = "      wpisując komendę 'stop', albo inną";
            manualLines[i++] = "      komendę akcji (wtedy postać wykona ostatnie";
            manualLines[i++] = "      wpisane polecenie). Czynności, które są akcjami to:";
            manualLines[i++] = "      > przejście do innej lokacji";
            manualLines[i++] = "      > każdy pojedynczy atak";
            manualLines[i++] = "      > rzucenie czaru";
            manualLines[i++] = "      > utworzenie czaru";
            manualLines[i++] = "      > użycie przedmiotu";
            manualLines[i++] = "      Pozostałe czynności, które nie są akcjami, Twoja";
            manualLines[i++] = "      postać wykona w każdej chwili, gdy tylko wpiszesz";
            manualLines[i++] = "      komendę (np obejżenie czy podniesienie przedmiotu)\n";
            manualLines[i++] = "  >>> WALKA\n    ";
            manualLines[i++] = "      Podczas walki nie możesz przechodzić do innych";
            manualLines[i++] = "      lokacji, rozmawiać ani handlować.";
            manualLines[i++] = "      Możesz natomiast atakować, używać przedmiotów,";
            manualLines[i++] = "      rzucać (lub tworzyć) czary, i próbować ucieczki.";
            manualLines[i++] = "      Aby zaatakować jakąś postać, wpisujesz komendę";
            manualLines[i++] = "      'attack' i nazwę postaci (np. 'attack dziki_pies').'";
            manualLines[i++] = "      Gdy to zrobisz, Twoja postać będzie";
            manualLines[i++] = "      atakowała tak długo, aż przeciwnik nie padnie";
            manualLines[i++] = "      trupem, albo nie karzesz jej przestać komendą";
            manualLines[i++] = "      'stop'. Gdy ktoś najpierw zaatakuje Ciebie, Twoja";
            manualLines[i++] = "      postać automatycznie odpowie atakiem. Możesz ";
            manualLines[i++] = "      wyłączyć to zachowanie, wpisując komendę ";
            manualLines[i++] = "      'autoattack'.";
            manualLines[i++] = "      Każda postać, gdy ją zaatakujesz, odpowie atakiem.";
            manualLines[i++] = "      Niektóre postacie są \"społeczne\", i zaatakują Cię";
            manualLines[i++] = "      gdy Ty zaatakujesz ich pobratymca. Ostatni typ,";
            manualLines[i++] = "      to postacie agresywne, które same zaatakują Cię";
            manualLines[i++] = "      gdy tylko pojawisz się w tej samej lokacji co one.\n";
            manualLines[i++] = "  >>> UCIECZKA \n";
            manualLines[i++] = "      Aby spróbować ucieczki, wpisujesz komendę 'flee'";
            manualLines[i++] = "      i literę kierunku w którym chcesz uciec. Np.";
            manualLines[i++] = "      jeśli wpiszesz 'flee n', Twoja postać ucieknie";
            manualLines[i++] = "      na północ, a walka się zakończy. Możesz też wpisać";
            manualLines[i++] = "      samo 'flee', a Twoja postać ucieknie w losowo";
            manualLines[i++] = "      wybranym kierunku. Ucieczka może się nie udać";
            manualLines[i++] = "       - wtedy walka trwa dalej, a Twoja postać";
            manualLines[i++] = "      zostaje ogłuszona na kilka sekund. Szanse na";
            manualLines[i++] = "      powodzenie ucieczki zależą od szybkości postaci.";
            manualLines[i++] = "      Jeśli jesteś szybszy od przeciwnika - Twoje szanse";
            manualLines[i++] = "      są spore. Jeśli przeciwnik jest szybszy - Twoje";
            manualLines[i++] = "      szanse są małe. Dlatego często nie opłaca się";
            manualLines[i++] = "      próbować ucieczki przed szybszym przeciwnikiem.\n";
            manualLines[i++] = "  >>> RUNY\n";
            manualLines[i++] = "      Runy to kamienie z których tworzy się czary.";
            manualLines[i++] = "      Istnieje 5 różnych run:";
            manualLines[i++] = "      > \"iskarr\"";
            manualLines[i++] = "      > \"akull\"";
            manualLines[i++] = "      > \"verde\"";
            manualLines[i++] = "      > \"xitan\"";
            manualLines[i++] = "      > \"dara\"";
            manualLines[i++] = "      Możesz utworzyć czar jednej runy, albo z kombinacji";
            manualLines[i++] = "      dwóch run. ";
            manualLines[i++] = "      Aby utworzyć czar, musisz posiadać runy w plecaku,";
            manualLines[i++] = "      i wpisać komendę 'craft' i nazwę runy (lub dwóch).";
            manualLines[i++] = "      Np. 'craft iskarr' - utworzy czar z runy ";
            manualLines[i++] = "      \"iskarr\". Natomiast 'craft verde xitan' utworzy";
            manualLines[i++] = "      czar z kombinacji run \"verde\" i \"xitan\" itd.\n";
            manualLines[i++] = "  >>> CZARY\n";
            manualLines[i++] = "      Gdy tworzysz nowy czar, dodaje się on do listy";
            manualLines[i++] = "      zapamiętanych czarów. Twoja postać może zapamiętać";
            manualLines[i++] = "      tylko ograniczoną ilość czarów. Jeśli stworzysz";
            manualLines[i++] = "      nowy czar gdy lista będzie pełna, ostatni czar";
            manualLines[i++] = "      z listy zostanie zapomniany.";
            manualLines[i++] = "      Zapamiętane na liście czary możesz rzucać używając";
            manualLines[i++] = "      komendy 'cast' i numeru czaru na liście. Np. aby";
            manualLines[i++] = "      rzucić pierwszy czar, wpisz 'cast 1'.";
            manualLines[i++] = "      Jeśli jesteś w trakcie walki i rzucasz czar";
            manualLines[i++] = "      ofensywny, Twoja postać automatycznie wyceluje";
            manualLines[i++] = "      w tego kogo atakujesz. Jeśli chcesz wybrać cel,";
            manualLines[i++] = "      musisz dopisać nazwę celu np. 'cast 1 dziki_pies'.";
            manualLines[i++] = "      Czary defensywne, Twoja postać automatycznie";
            manualLines[i++] = "      rzuca na samą siebie.";
            manualLines[i++] = "      Każde rzucenie czaru kosztuje Cię manę, a każdy";
            manualLines[i++] = "      czar ma swój koszt many.";
            manualLines[i++] = "      Czary mogą zadawać obrażenia, wrzucać modyfikatory";
            manualLines[i++] = "      (np. -30% obrony), lub specjalne efekty ";
            manualLines[i++] = "      (np. ogłuszenie). ";
            manualLines[i++] = "      ";
            manualLines[i++] = "  >>> ATRYBUTY I STATYSTYKI\n";
            manualLines[i++] = "      Wszystkie postacie w grze, posiadają szereg";
            manualLines[i++] = "      statystyk:";
            manualLines[i++] = "      > Szybkość - wpływa na to jak szybko Twoja postać";
            manualLines[i++] = "      porusza się między lokacjami. Od szybkości Twojej i ";
            manualLines[i++] = "      przeciwnika zależą też szanse na udaną ucieczkę.";
            manualLines[i++] = "      > Atak - im większy atak, tym większe obrażenia ";
            manualLines[i++] = "      zadaje postać podczas ataku.";
            manualLines[i++] = "      > Szybkość ataku - im większa szybkość ataku, tym ";
            manualLines[i++] = "      szybsze ataki (mniejsze zapełnienie paska akcji)";
            manualLines[i++] = "      > Celność - zwiększa szansę na trafienie przeciwnika";
            manualLines[i++] = "      > Trafienia krytyczne - im więcej, tym większa ";
            manualLines[i++] = "      szansa na trafienie krytyczne";
            manualLines[i++] = "      > Obrona - zmniejsza obrażenia zadawane przez ataki";
            manualLines[i++] = "      przeciwnika.";
            manualLines[i++] = "      > Uniki - im więcej, tym większa szansa na ";
            manualLines[i++] = "      uniknięcie ataku przeciwnika.";
            manualLines[i++] = "      > Odporność na magię - zmniejsza obrażenia zadawane";
            manualLines[i++] = "      przez czary przeciwnika. Oprócz tego, zwiększa ";
            manualLines[i++] = "      szansę na odparcie czaru.";
            manualLines[i++] = "      > Maksymalne HP - maksymalna ilość zdrowia postaci";
            manualLines[i++] = "      > Maksymalne MP - maksymalna ilość many postaci";
            manualLines[i++] = "      > Regeneracja HP - im więcej, tym szybciej ";
            manualLines[i++] = "      regeneruje się zdrowie postaci";
            manualLines[i++] = "      > Regeneracja MP - im więcej, tym szybciej ";
            manualLines[i++] = "      regeneruje się mana postaci";
            manualLines[i++] = "      Twoja postać, oprócz statystyk, posiada jeszcze trzy";
            manualLines[i++] = "      atrybuty, które zwiększają jej statystyki:";
            manualLines[i++] = "      > Siła - zwiększa maksymalne HP, regenerację HP";
            manualLines[i++] = "      i atak";
            manualLines[i++] = "      > Zręczność - zwiększa szybkość, szybkość ataku, ";
            manualLines[i++] = "      celność, trafienia krytyczne i uniki.";
            manualLines[i++] = "      > Inteligencja - zwiększa maksymalne MP, ";
            manualLines[i++] = "      regenerację MP, odporność na magię, obrażenia ";
            manualLines[i++] = "      zadawane z czarów, szansę na powodzenie zaklęcia ";
            manualLines[i++] = "      oraz maksymalną ilość zapamiętanych czarów.";

            PrintMessage(title, MessageType.Gain, false);

            //if user came from main menu, display info about coming back
            if (isExitInfoPrinted)
            {
                PrintMessage(exitInfo, MessageType.Action, false);
            }

            for (i = 0; i < manualLines.Length; i++)
            {
                if (Regex.IsMatch(manualLines[i], @"^\s+>>>"))
                {
                    PrintMessage(manualLines[i], MessageType.EffectOn, false);
                }
                else if (Regex.IsMatch(manualLines[i], @"^\s+\*\s"))
                {
                    PrintMessage(manualLines[i], MessageType.UserCommand, false);
                }
                else if (Regex.IsMatch(manualLines[i], @"^\s\*{3,20}"))
                {
                    PrintMessage(manualLines[i], MessageType.Gain, false);
                }
                else
                {
                    PrintMessage(manualLines[i], MessageType.Default, false);
                }
            }
        }

        //method printing hints for player doing something for the very first time
        private void PrintHint(Hints.HintType type, string objectName1 = "placeholder", string objectName2 = "placeholder")
        {
            int i;
            int hintLinesLength = 10;
            string[] hintLines = new string[hintLinesLength];

            //fill every hint line with a proper beginning
            hintLines[0] = "> PODPOWIEDŹ!";
            for (i = 1; i < hintLinesLength; i++)
            {
                hintLines[i] = "> ";
            }

            switch (type)
            {
                case Hints.HintType.Go:
                    hintLines[1] += "Aby udać się w którymś kierunku, wpisz literę kierunku i naciśnij enter (np. \"w\" aby pójść na zachód)";
                    hintLines[2] += "Litery kierunków: \"n\" = północ; \"e\" = wschód; \"s\" = południe; \"w\" = zachód; \"u\" = góra; \"d\" = dół."; 
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.GoHint = false;
                    break;
                case Hints.HintType.Attack:
                    hintLines[1] += "Jest tutaj postać (" + objectName1 + "), którą możesz zaatakować!";
                    hintLines[2] += "Aby zaatakować pierwszą postać z brzegu, wpisz \"attack\" (skrót \"a\")";
                    hintLines[3] += "Możesz też wybrać postać, którą chcesz zaatakować, dodając jej nazwę (np. \"attack " + objectName1 + "\")";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.AttackHint = false;
                    break;
                case Hints.HintType.Trade:
                    hintLines[1] += "Jest tutaj postać (" + objectName1 + "), z którą możesz handlować!";
                    hintLines[2] += "Aby rozpocząć handel z postacią, wpisz komendę \"trade\" (skrót \"tr\") i nazwę postaci (np. \"trade " + objectName1 +
                        "\")";
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.TradeHint = false;
                    break;
                case Hints.HintType.Talk:
                    hintLines[1] += "Jest tutaj postać (" + objectName1 + "), z którą możesz porozmawiać!";
                    hintLines[2] += "Aby rozpocząć dialog z postacią, wpisz komendę \"talk\" (skrót \"ta\") i nazwę postaci (np. \"talk " + objectName1 +
                        "\")";
                    hintLines[3] += "Rozmawiać możesz ze wszystkimi postaciami w grze!";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.TalkHint = false;
                    break;
                case Hints.HintType.Pickup:
                    hintLines[1] += "Aby podnieść wszystko co leży na ziemi, wpisz \"pickup\" (skrót \"p\")";
                    hintLines[2] += "Możesz też wybrać przedmiot, który chcesz podnieść (np. \"pickup złoto\")";
                    hintLines[3] += "Aby podnieść konkretną ilość, możesz dodąc liczbę (np. \"pickup złoto 2\" - podniesie 2 złota)";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.PickupHint = false;
                    break;
                case Hints.HintType.Look:
                    hintLines[1] += "Aby się rozejrzeć, wpisz \"look\" (lub skrót \"l\")";
                    hintLines[2] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.LookHint = false;
                    break;
                case Hints.HintType.Inventory:
                    hintLines[1] += "Aby zobaczyć swój ekwipunek wpisz \"inventory\" lub skrót \"i\".";
                    hintLines[2] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.InventoryHint = false;
                    break;
                case Hints.HintType.LookInventoryItem:
                    hintLines[1] += "Aby obejżeć przedmiot, użyj komendy \"look\" (skrót \"l\") i nazwy przedmiotu (np. \"look " +
                        objectName1 + "\" aby obejżeć " + objectName1 + ")";
                    hintLines[2] += "Aby wyrzucić przedmiot, użyj komendy \"drop\" i nazwy przedmiotu (np. \"drop " + objectName1 +"\")";
                    hintLines[3] += "Aby wyrzucić większą ilość przedmiotu, możesz dodać liczbę (np. \"drop " + objectName1 + " 2\")";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.LookInventoryItemHint = false;
                    break;
                case Hints.HintType.BuySell:
                    hintLines[1] += "Aby obejżeć przedmiot w ekwipunku handlarza użyj komendy \"look\" (skrót \"l\") (np. \"look " + objectName1 + "\")";
                    hintLines[2] += "Aby kupić przedmiot, użyj komendy \"buy\" (np. \"buy " + objectName1 + "\")";
                    hintLines[3] += "Aby sprzedać przedmiot, użyj komendy \"sell\" (np. \"sell " + objectName2 + "\")";
                    hintLines[4] += "UWAGA!!! Sprzedajesz taniej niż kupujesz!";
                    hintLines[5] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.BuySellHint = false;
                    break;
                case Hints.HintType.LookAdjacentLoc:
                    hintLines[1] += "Aby popatrzeć w którymś kierunku, użyj komendy \"look\" (skrót \"l\") i litery kierunku (np \"look w\")";
                    hintLines[2] += "Litery kierunków: \"n\" = północ; \"e\" = wschód; \"s\" = południe; \"w\" = zachód; \"u\" = góra; \"d\" = dół.";
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.LookAdjacentLocHint = false;
                    break;
                case Hints.HintType.Stats:
                    hintLines[1] += "Z każdym nowym poziomem dostajesz 5 pkt. atrybutów do wydania!";
                    hintLines[2] += "Posiadane pkt. możesz wydać na jeden z trzech atrybutów: siłę, inteligencję lub zręczność.";
                    hintLines[3] += "Aby obejżeć statystyki postaci, wpisz \"stats\" (lub skrót \"ss\")";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.StatsHint = false;
                    break;
                case Hints.HintType.Attributes:
                    hintLines[1] += "Aby ulepszyć atrybut, użyj komendy \"point\" (skrót \"pt\") i skrótu atrybutu (np. \"point str\" aby dodać 1 do siły)";
                    hintLines[2] += "Skróty atrybutów: \"str\" = siła; \"agi\" = zręczność; \"int\" = inteligencja";
                    hintLines[3] += "UWAGA!!! Raz zużyty pkt. atrybutu, nie może zostać cofnięty!";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.AttributesHint = false;
                    break;
                case Hints.HintType.Craft1:
                    hintLines[1] += "Zdobyłeś runę " + objectName1 + "!";
                    hintLines[2] += "Aby utworzyć nowy czar, uzyj komendy \"craft\" i nazwy runy (np. \"craft " + objectName1 + "\")";
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.CraftHint1 = false;
                    break;
                case Hints.HintType.Craft2:
                    hintLines[1] += "Zdobyłeś kolejną runę: " + objectName1 + "!";
                    hintLines[2] += "Aby utworzyć czar z kombinacji dwóch run, użyj komendy \"craft\" (np. \"craft " + objectName1 +
                        " " + objectName2 + "\")";
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.CraftHint2 = false;
                    break;
                case Hints.HintType.Spells:
                    hintLines[1] += "Aby zobaczyć listę zapamiętanych czarów, wpisz \"spells\" lub skrót \"sps\"";
                    hintLines[2] += "Aby zobaczyć opis czaru, użyj komendy \"look\" (skrót \"l\") (np. \"look " + objectName1 + "\")";
                    hintLines[3] += "Aby rzucić czar, użyj komendy \"cast\" (skrót \"c\") i numeru czaru na liście (np. \"cast 1\")";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.SpellsHint = false;
                    break;
                case Hints.HintType.Wear:
                    hintLines[1] += "Zdobyłeś przedmiot, który możesz założyć: " + objectName1 + "";
                    hintLines[2] += "Aby założyć przedmiot, uzyj komendy \"wear\" i nazwy przedmiotu (np. \"wear " + objectName1 + "\")";
                    hintLines[3] += "Założone przedmioty, możesz zobaczyć w dolnej części swojego ekwipunku (\"inventory\" lub \"i\")";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.WearHint = false;
                    break;
                case Hints.HintType.Takeoff:
                    hintLines[1] += "Założyłeś przedmiot: " + objectName1 + "";
                    hintLines[2] += "Aby ściągnąć przedmiot, użyj komendy \"takeoff\" (skrót \"of\") i typu zakładanego przedmiotu (np. \"takeoff helmet\" - " +
                        "aby ściągnąć nakrycie głowy)";
                    hintLines[3] += "Typy zakładanych przedmiotów: \"weapon\" = broń; \"helmet\" = hełm/czapka; \"torso\" = napierśnik/koszula; \"pants\" = spodnie; " +
                        "\"gloves\" = rękawice; \"shoes\" = buty";
                    hintLines[4] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.TakeoffHint = false;
                    break;
                case Hints.HintType.Use:
                    hintLines[1] += "Zdobyłeś używalny przedmiot: " + objectName1;
                    hintLines[2] += "Aby użyć przedmiotu, użyj komendy \"use\" i nazwy przedmiotu (np. \"use " + objectName1 + "\")";
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.UseHint = false;
                    break;
                case Hints.HintType.Pause:
                    hintLines[1] += "Aby zatrzymać grę, użyj komendy \"pause\" (skrót \"ps\")";
                    hintLines[2] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.PauseHint = false;
                    break;
                case Hints.HintType.Flee:
                    hintLines[1] += "Aby spróbować ucieczki, użyj komendy \"flee\" (skrót \"f\")";
                    hintLines[2] += "Możesz wybrać kierunek ucieczki, dodając literę kierunku (np. \"flee n\" - ucieczka na północ)";
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.FleeHint = false;
                    break;
                case Hints.HintType.GameSpeed:
                    hintLines[1] += "Aby zwiększyć szybkość gry, użyj komendy \"speedup\" (skrót \"up\")";
                    hintLines[2] += "Aby zmniejszyć szybkość gry, użyj komendy \"slowdown\" (skrót \"dn\")";
                    hintLines[3] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.GameSpeedHint = false;
                    break;
                case Hints.HintType.Effects:
                    hintLines[1] += "Aby zobaczyć efekty nałożone na Twoją postać, wpisz \"effects\" (skrót \"ef\")";
                    hintLines[2] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.EffectsHint = false;
                    break;
                case Hints.HintType.QuickSave:
                    hintLines[1] += "Aby zrobić szybki zapis gry, wpisz \"save\"";
                    hintLines[2] += "(Aby wyłączyć/włączyć podpowiedzi, wpisz \"hints\")";
                    Hints.QuickSaveHint = false;
                    break;

            }

            for (i = 0; i < hintLinesLength; i++)
            {
                if (hintLines[i].Length > 2)
                {
                    PrintMessage(hintLines[i], MessageType.EffectOn);
                }
            }

        }









        //==============================================HELPER METHODS=============================================

        //method calculating nth root of a double value
        public static double NthRoot(double A, double N)
        {
            return Math.Pow(A, 1.0 / N);
        }

        //method determining if location with specified coordinates exists
        private bool IsThereALocation(int X, int Y, int Z)
        {
            if (Data.Locations!.Exists(loc => loc.X == X && loc.Y == Y && loc.Z == Z))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //method returning formatted string representing special effect description
        private string GetSpecialEffectDescription(SpecialEffect effect)
        {
            string effectDescription = string.Empty;

            if (effect.Type == SpecialEffect.EffectType.HealPercent)
            {
                effectDescription = "Leczenie(" + effect.Value + "_%_MaxHP)";
            }
            else if (effect.Type == SpecialEffect.EffectType.Heal)
            {
                effectDescription = "Leczenie(" + effect.Value + "_HP)";
            }
            else if (effect.Type == SpecialEffect.EffectType.Stun)
            {
                effectDescription = "Ogłuszenie " + TimeValueFromSeconds(effect.Duration);
            }
            else if (effect.Type == SpecialEffect.EffectType.Lifesteal)
            {
                effectDescription = "Kradzież życia(+" + effect.Value + "_%) " + TimeValueFromSeconds(effect.Duration);
            }
            else if (effect.Type == SpecialEffect.EffectType.Invisibility)
            {
                effectDescription = "Niewidzialność " + TimeValueFromSeconds(effect.Duration);
            }
            else if (effect.Type == SpecialEffect.EffectType.ManaShield)
            {
                effectDescription = "Tarcza MP(+" + effect.Value + "_%) " + TimeValueFromSeconds(effect.Duration);
            }
            else if (effect.Type == SpecialEffect.EffectType.Teleport)
            {
                effectDescription = "Teleport";
            }

            return effectDescription;
        }

        //method returning formatted string representing effect description (it's modifiers and duration)
        private string GetEffectDescription(List<Modifier> modifiers, bool withRealDuration = false)
        {
            string effect = string.Empty;

            if (modifiers.Count > 0)
            {
                modifiers.ForEach(mod =>
                {
                    effect += GetModDescription(mod) + ", ";
                });

                //remove trailing comma and add duration
                effect = Regex.Replace(effect, @",\s$", "");

                //print starting or actual duration depending on second argument
                if (withRealDuration)
                {
                    effect += " " + TimeValueFromSeconds((modifiers[0].DurationInTicks / 10));
                }
                else
                {
                    effect += " " + TimeValueFromSeconds(modifiers[0].Duration);
                }
            }
            else
            {
                effect += "brak";
            }

            return effect;
        }

        //method returning formatted string representing modifier and it's value with sign
        private string GetModDescription(Modifier modifier)
        {
            string description = string.Empty;
            string modType = GetPolishModType(modifier.Type);
            string valueSign = string.Empty;
            string percentSign = string.Empty;

            //add percent sign if the modifier is percentage
            if (modifier.IsPercentage)
            {
                percentSign = "_%";
            }

            //set sign of modifier to + if its positive number (for negative, minus sign is displayed automatically)
            if (modifier.Value > 0)
            {
                valueSign = "+";
            }

            description = modType + "(" + valueSign + modifier.Value + percentSign + ")";

            //special description for special modifiers
            if (modifier.Type == Modifier.ModType.Invisibility ||
                modifier.Type == Modifier.ModType.Stun)
            {
                description = modType;
            }
            return description;
        }

        //method returning polish string representing direction in which player does something
        //(ie goes or looks)
        private string GetPolishDirectionName(string directionLetter)
        {
            string directionString = string.Empty;

            switch (directionLetter)
            {
                case "n":
                    directionString = "na północ";
                    break;
                case "e":
                    directionString = "na wschód";
                    break;
                case "s":
                    directionString = "na południe";
                    break;
                case "w":
                    directionString = "na zachód";
                    break;
                case "u":
                    directionString = "do góry";
                    break;
                case "d":
                    directionString = "w dół";
                    break;
            }

            return directionString;
        }

        private string FlattenPolishChars(string polishName)
        {
            polishName = Regex.Replace(polishName, @"[ą]", "a");
            polishName = Regex.Replace(polishName, @"[ć]", "c");
            polishName = Regex.Replace(polishName, @"[ę]", "e");
            polishName = Regex.Replace(polishName, @"[ł]", "l");
            polishName = Regex.Replace(polishName, @"[ń]", "n");
            polishName = Regex.Replace(polishName, @"[ó]", "o");
            polishName = Regex.Replace(polishName, @"[ś]", "s");
            polishName = Regex.Replace(polishName, @"[źż]", "z");

            //remove underscores
            polishName = RemoveUnderscores(polishName);

            return polishName;
        }

        private string RemoveUnderscores(string objectName)
        {
            return Regex.Replace(objectName, @"_", "");
        }

        //method returning polish string representing specified type of ArmorType type
        private string GetPolishArmorType(Armor.ArmorType type)
        {
            string armorType = string.Empty;
            switch (type)
            {
                case Armor.ArmorType.Torso:
                    armorType = "Korpus";
                    break;
                case Armor.ArmorType.Pants:
                    armorType = "Spodnie";
                    break;
                case Armor.ArmorType.Helmet:
                    armorType = "Hełm";
                    break;
                case Armor.ArmorType.Shoes:
                    armorType = "Buty";
                    break;
                case Armor.ArmorType.Gloves:
                    armorType = "Rękawice";
                    break;
            }
            return armorType;
        }

        //method returning polish string representing specified type of CombatCharacter statistic 
        private string GetPolishModType(Modifier.ModType type)
        {
            string modType = string.Empty;

            switch (type)
            {
                case (Modifier.ModType.HpRegen):
                    modType = "Regeneracja Hp";
                    break;
                case (Modifier.ModType.MpRegen):
                    modType = "Regeneracja Mp";
                    break;
                case (Modifier.ModType.MaxHp):
                    modType = "Maks. Hp";
                    break;
                case (Modifier.ModType.MaxMp):
                    modType = "Maks. Mp";
                    break;
                case (Modifier.ModType.Strength):
                    modType = "Siła";
                    break;
                case (Modifier.ModType.Intelligence):
                    modType = "Inteligencja";
                    break;
                case (Modifier.ModType.Agility):
                    modType = "Zręczność";
                    break;
                case (Modifier.ModType.Speed):
                    modType = "Szybkość";
                    break;
                case (Modifier.ModType.Attack):
                    modType = "Atak";
                    break;
                case (Modifier.ModType.AtkSpeed):
                    modType = "Szybkość ataku";
                    break;
                case (Modifier.ModType.Accuracy):
                    modType = "Celność";
                    break;
                case (Modifier.ModType.Critical):
                    modType = "Trafienia krytyczne";
                    break;
                case (Modifier.ModType.Defense):
                    modType = "Obrona";
                    break;
                case (Modifier.ModType.Evasion):
                    modType = "Uniki";
                    break;
                case (Modifier.ModType.MagicResistance):
                    modType = "Odporność na magię";
                    break;
                case (Modifier.ModType.Lifesteal):
                    modType = "Kradzież życia";
                    break;
                case (Modifier.ModType.Invisibility):
                    modType = "Niewidzialność";
                    break;
                case (Modifier.ModType.Stun):
                    modType = "Obezwładnienie";
                    break;
                case (Modifier.ModType.ManaShield):
                    modType = "Tarcza MP";
                    break;
            }
            return modType;
        }

        /// <summary>
        /// finds location in the direction specified by 'direction' argument and returns true if found, false otherwise
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        /// 
        private bool GetNextLocation(string direction, out Location nextLocation)
        {
            nextLocation = Data.Player!.CurrentLocation!;
            int currentX = nextLocation.X;
            int currentY = nextLocation.Y;
            int currentZ = nextLocation.Z;
            int locationIndex = -1;
            bool isFound = false;

            switch (direction)
            {
                case "n":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX && loc.Y == currentY + 1 && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "e":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX + 1 && loc.Y == currentY && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "s":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX && loc.Y == currentY - 1 && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "w":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.X == currentX - 1 && loc.Y == currentY && loc.Z == currentZ);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "u":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Z == currentZ + 1 && loc.X == currentX && loc.Y == currentY);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
                case "d":
                    locationIndex = Data.Locations!.FindIndex(loc => loc.Z == currentZ - 1 && loc.X == currentX && loc.Y == currentY);
                    if (locationIndex != -1)
                    {
                        nextLocation = Data.Locations![locationIndex];
                        isFound = true;
                    }
                    break;
            }

            return isFound;
        }

        //helper method for calculating selling price (trader price) of the item
        private int CalculateTraderPrice(string itemName)
        {
            Item itemToEvaluate = Data.Items!.Find(item =>
            FlattenPolishChars(item.Name!.ToLower()) == FlattenPolishChars(itemName.ToLower()))!;

            double doublePrice = Convert.ToDouble(itemToEvaluate.Price);
            int roundedPrice = Convert.ToInt32(Math.Round(doublePrice * Data!.PriceMultiplier));
            return roundedPrice;
        }

        //method converting quantity string to number and returning true
        //if conversion succeded and value is > 0  (returns false otherwise)
        private bool ConvertQuantityString(string quantityString, out int quantityValue)
        {
            int parsedQuantity = 0;

            if (quantityString != string.Empty)
            {
                if (!int.TryParse(quantityString, out parsedQuantity))
                {
                    quantityValue = parsedQuantity;
                    return false;
                }
                if (parsedQuantity <= 0)
                {
                    quantityValue = parsedQuantity;
                    return false;
                }
            }

            quantityValue = parsedQuantity;
            return true;
        }

        //method converting seconds number to time value in hours, minutes and seconds
        private string TimeValueFromSeconds(int seconds)
        {
            int hours = 0;
            int minutes = 0;
            string time = string.Empty;

            while (seconds >= 3600)
            {
                seconds -= 3600;
                hours++;
            }

            while (seconds >= 60)
            {
                seconds -= 60;
                minutes++;
            }

            if (hours == 0 && minutes == 0)
            {
                time = "{" + seconds + " sek.}";    
            }
            else if (hours == 0)
            {
                time = "{" + minutes + " min. " + seconds + " sek.}"; 
            }
            else
            {
                time = "{" + hours + " h " + minutes + " min. " + seconds + " sek.}";
            }

            return time;
        }

        //method printing characters line in form of speech
        private void PrintSpeech(Character character, string line)
        {
            string characterLine = character.Name + ": " + line;
            PrintMessage(characterLine, MessageType.Speech);
        }

        //method displaying communicates in outputBox of the gui
        public void PrintMessage(string msg, MessageType type = MessageType.Default, bool shouldScroll = true)
        {
             
            // Maximum of blocks in the document
            int MaxBlocks = 100;

            // Maximum of lines in one block (paragraph)
            int InlinesPerBlock = 20;

            SolidColorBrush brush = Brushes.LightGray;
            switch (type)
            {
                case (MessageType.Default):
                    brush = Brushes.LightGray;
                    break;
                case (MessageType.UserCommand):
                    brush = Brushes.Aqua;
                    break;
                case (MessageType.SystemFeedback):
                    brush = Brushes.DarkSalmon;
                    break;
                case (MessageType.Action):
                    brush = Brushes.LightSkyBlue;
                    break;
                case (MessageType.Gain):
                    brush = Brushes.Yellow;
                    break;
                case (MessageType.Loss):
                    brush = Brushes.Goldenrod;;
                    break;
                case (MessageType.EffectOn):
                    brush = Brushes.MediumSpringGreen;
                    break;
                case (MessageType.EffectOff):
                    brush = Brushes.SeaGreen;
                    break;
                case (MessageType.Speech):
                    brush = Brushes.DarkKhaki;
                    break;
                case (MessageType.DealDmg):
                    brush = Brushes.Chartreuse;
                    break;
                case (MessageType.ReceiveDmg):
                    brush = Brushes.Crimson;
                    break;
                case (MessageType.CriticalHit):
                    brush = Brushes.Magenta;
                    break;
                case (MessageType.Miss):
                    brush = Brushes.BlueViolet;
                    break;
                case (MessageType.Dodge):
                    brush = Brushes.LightSeaGreen;
                    break;
            }

            // Get the latest block in the document and try to append a new message to it
            if (Window.outputBox.Document.Blocks.LastBlock is Paragraph paragraph)
            {
                var nl = Environment.NewLine;

                // If the current block already contains the maximum count of lines create a new paragraph
                if (paragraph.Inlines.Count >= InlinesPerBlock)
                {
                    nl = string.Empty;
                    paragraph = new Paragraph();
                    Window.outputBox.Document.Blocks.Add(paragraph);
                }
                paragraph.Inlines.Add(new Run(nl + msg) { Foreground = brush });
            }

            if (Window.outputBox.Document.Blocks.Count >= MaxBlocks)
            {
                // When the number of lines more that (MaxBlocks-1)*InlinesPerBlock  remove the first block in the document
                Window.outputBox.Document.Blocks.Remove(Window.outputBox.Document.Blocks.FirstBlock);
            }

            if (shouldScroll)
            {
                Window.outputBox.ScrollToEnd();
            }
        }

        /// <summary>
        /// /// method taking chance parameter (as double 0.01-1.00 value, indicating percent
        /// number) returns true if succeded and false if not
        /// </summary>
        /// <param name="chance"></param>
        /// <returns></returns>
        private bool TryOutChance(double chance)
        {
            double randShot = Rand.NextDouble();
            if (chance < randShot)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// /// method determining if an attack reached the target on basis of two 
        /// parameters: accuracy and evasion. If attack is a success - returns true
        /// if missed - returns false
        /// </summary>
        /// <param name="accuracy"></param>
        /// <param name="evasion"></param>
        /// <returns></returns>
        private bool IsAttackHit(double accuracy, double evasion)
        {
            double hitChance = Rand.NextDouble() * accuracy;
            double missChance = Rand.NextDouble() * evasion;

            if (hitChance > missChance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// method determining if a spell succeded or failed on basis of two parameters:
        /// casterChanceFactor(player int value or monster's lvl) and targetMagicResistance
        /// </summary>
        /// <param name="casterChanceFactor"></param>
        /// <param name="targetMagicResistance"></param>
        /// <returns></returns>
        private bool IsSpellASuccess(double casterChanceFactor, double targetMagicResistance)
        {
            double successChance = Rand.NextDouble() * casterChanceFactor * 3;
            double failChance = Rand.NextDouble() * targetMagicResistance;

            if (successChance > failChance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsHitCritical(double critical)
        {
            double chance = Math.Sqrt(critical) / 100;
            bool isCritical = TryOutChance(chance);
            return isCritical;
        }

        //method calculating dmg from attack and defense values
        private double CalculateDmg(double attack, double defense)
        {
            double reductionMultiplier = 1 / (Math.Sqrt(defense) / 10);
            double dmg = attack / 5 * reductionMultiplier;
            return dmg;
        }

        //method randomizing dmg
        private double RandomizeDmg(double staticDmg)
        {
            double randomDmgMultiplier = Rand.Next(80, 121) * 0.01;
            double randomizedDmg = staticDmg * randomDmgMultiplier;
            return randomizedDmg;
        }

        //method adding action to actions list
        private void AddAction(CharAction action)
        {
            //prevention fix for player's character sometimes losing items modifiers without
            //taking them off
            RefreshItemsModifiers();

            int actionIndex = Actions.FindIndex(act => act.Performer == action.Performer);

            if (actionIndex != -1)
            {
                Actions.RemoveAt(actionIndex);
            }

            Actions.Add(action);
        }

        //method refreshing all worn-items modifiers on player's character
        private void RefreshItemsModifiers()
        {
            Item[] wornItems = new Item[6];
            string parentName = string.Empty;

            wornItems[0] = Data.Player!.Weapon!;
            wornItems[1] = Data.Player!.Torso!;
            wornItems[2] = Data.Player!.Pants!;
            wornItems[3] = Data.Player!.Gloves!;
            wornItems[4] = Data.Player!.Shoes!;
            wornItems[5] = Data.Player!.Helmet!;

            for (int i = 0; i < 6; i++)
            {
                if (wornItems[i].Name != "placeholder")
                {
                    wornItems[i].Modifiers!.ForEach(itemMod =>
                    {
                        if (!Data.Player!.Modifiers!.Exists(playerMod => playerMod.Parent == itemMod.Parent))
                        {
                            Data.Player!.AddModifier(itemMod);
                        }
                    });
                }
            }
        }




        //==============================================EVENT HANDLERS=============================================

        //handler for tick event of GameClock
        private void GameClockTick(object sender, EventArgs e)
        {
            //handle all characters regeneration/duration-decrease of modifiers etc
            Data.Locations!.ForEach(loc =>
            {
                loc.Characters!.ForEach(character =>
                {
                    if (character is CombatCharacter)
                    {
                        (character as CombatCharacter)!.HandleTick();
                    }
                });
            });

            PlayerEffectsTick();
            AttacksTick();
            ActionsTick();
        }

        //method handling player effects tick
        private void PlayerEffectsTick()
        {
            List<EffectOnPlayer> playerEffects;
            List<EffectOnPlayer> effectsToRemove = new List<EffectOnPlayer>();
            playerEffects = Data.Player!.Effects!;

            //handle duration-decrease and wearing off of effects affecting player
            for (int i = 0; i < playerEffects.Count; i++)
            {

                //if duration is greater than 1 - decrement it.
                //otherwise, if it equals 1 - effect has ended so remove it
                if (playerEffects[i].DurationInTicks > 1)
                {
                    playerEffects[i].DurationInTicks--;
                }
                else if (playerEffects[i].DurationInTicks == 1)
                {
                    effectsToRemove.Add(playerEffects[i]);
                }
            }
            effectsToRemove.ForEach(eff =>
            {
                RemoveEffect(eff);
            });
        }

        //method handling attacks for every attack instance
        private void AttacksTick()
        {
            bool isSpellCasted = false;
            bool isAttackerPlayer;
            bool isReceiverPlayer;
            bool isDmgLethal = false;
            int i;
            double staticDmg;
            double rawDmg;
            double dealtDmg;
            double chanceToCast;
            int dmgAsInt;
            int randomSpellNumber;
            
            CombatCharacter attacker = new CombatCharacter("placeholder");
            CombatCharacter receiver = new CombatCharacter("placeholder");

            for (i = 0; i < AttackInstances.Count; i++)
            {
                attacker = AttackInstances[i].Attacker;
                receiver = AttackInstances[i].Receiver;

                //skip attack if attacker's attack is on cooldown or there is
                //any action to be performed by the attacker waiting in the queue
                if (attacker.ActionCounter > 0 || Actions.Exists(action => action.Performer == attacker))
                {
                    continue;
                }

                isAttackerPlayer = attacker.GetType() == typeof(Player);
                isReceiverPlayer = receiver.GetType() == typeof(Player);

                //cast a spell once a while if it's attacking the player
                if (isReceiverPlayer && attacker.RememberedSpells.Count > 0)
                {
                    chanceToCast = attacker.RememberedSpells.Count * 0.2;
                    isSpellCasted = TryOutChance(chanceToCast);

                    if (isSpellCasted)
                    {
                        randomSpellNumber = Rand.Next(0, attacker.RememberedSpells.Count);

                        //if char has enough mana for the spell
                        if (attacker.Mp >= attacker.RememberedSpells[randomSpellNumber].ManaCost)
                        {
                            CastSpell(attacker, receiver, attacker.RememberedSpells[randomSpellNumber]);
                            attacker.ActionCounter += 40;

                            return;
                        }
                    }
                }

                attacker.PerformAttack();

                //print info about performing an attack
                if (isAttackerPlayer)
                {
                    PrintMessage("Atakujesz postać: " + receiver.Name);
                }
                else
                {
                    PrintMessage(attacker.Name + " atakuje Cię..");
                }

                //try if attack actually hits or misses
                if (!IsAttackHit(attacker.GetEffectiveAccuracy(), receiver.GetEffectiveEvasion()))
                {
                    //display appropriate message if missed
                    if (isAttackerPlayer)
                    {
                        PrintMessage("Nie trafiasz", MessageType.Miss);
                    }
                    if (isReceiverPlayer)
                    {
                        PrintMessage("Unikasz ataku " + attacker.Name + "!", MessageType.Dodge);
                    }
                }
                else
                {

                    staticDmg = CalculateDmg(attacker.GetEffectiveAttack(), receiver.GetEffectiveDefense());
                    rawDmg = RandomizeDmg(staticDmg);

                    if (IsHitCritical(attacker.GetEffectiveCritical()))
                    {
                        if (isAttackerPlayer)
                        {
                            PrintMessage("Trafienie krytyczne!", MessageType.CriticalHit);
                        }
                        dealtDmg = rawDmg * 4;
                    }
                    else
                    {
                        dealtDmg = rawDmg;
                    }

                    dmgAsInt = Convert.ToInt32(dealtDmg);
                    isDmgLethal = DealDmgToCharacter(attacker, receiver, dmgAsInt, false);
                }

                //trigger auto-attack on attacked character (receiver)
                if (!isDmgLethal)
                {
                    TriggerAutoAttack(attacker, receiver, isAttackerPlayer, isDmgLethal);
                }
            }
        }

        //method handling actions done by combat characters
        private void ActionsTick()
        {
            List<CharAction> actionsToRemove = new List<CharAction>();

            Actions.ForEach(action =>
            {

                //skip action if it's performers action counter is on cooldown
                if (action.Performer!.ActionCounter > 0)
                {
                    return;
                }

                //perform the action:
                //if it's location change
                if (action.GetType() == typeof(LocationChange))
                {
                    LocationChange locationChange = (LocationChange)action;
                    ChangePlayerLocation(locationChange.nextLocation, locationChange.DirectionString);
                }

                //if it's flee attempt
                else if (action.GetType() == typeof(FleeAttempt))
                {
                    FleeAttempt fleeAttempt = (FleeAttempt)action;
                    TryToFlee(fleeAttempt.EscapeDestination);
                }

                //if it's item use
                else if (action.GetType() == typeof(ItemUse))
                {
                    ItemUse itemUse = (ItemUse)action;
                    
                    if (itemUse.ItemToUse.GetType() == typeof(Consumable))
                    {
                        UseConsumable((itemUse.ItemToUse as Consumable)!);
                    }
                }

                //else if it's spell casting
                else if (action.GetType() == typeof(SpellCast))
                {
                    SpellCast spellCast = (SpellCast)action;
                    CastSpell(spellCast.Performer!, spellCast.Target!, spellCast.SpellToCast!);
                }

                //else if it's spell CRAFTING
                else if (action.GetType() == typeof(SpellCraft))
                {
                    SpellCraft spellCraft = (SpellCraft)action;
                    CraftSpell(spellCraft.SpellToCraft);

                    //trigger spells hint
                    if (Hints.HintsOnOff)
                    {
                        if (Hints.SpellsHint)
                        {
                            PrintHint(Hints.HintType.Spells, spellCraft.SpellToCraft.Name!.ToLower());
                        }
                    }
                }

                //add cooldown to performer's action counter
                action.Performer!.ActionCounter += action.ActionPointsCost;

                //make sure action will be removed from the list
                actionsToRemove.Add(action);
            });

            //remove actions that were performed successfully
            actionsToRemove.ForEach(action =>
            {
                Actions.Remove(action);
            });

        }
    }
}
