﻿using Runedal.GameData.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class GameSave
    {
        public GameSave()
        {
            Hints = new Hints();
            TakenIds = new List<ulong>();
            Locations = new List<Location>();
            Monsters = new List<Monster>();
            Traders = new List<Trader>();
            Heroes = new List<Hero>();
        }
        public GameSave(Hints hints, List<Location> locations, List<ulong> takenIds,
            List<Monster> monsters, List<Trader> traders, List<Hero> heroes, Player player)
        {
            Hints = hints;
            Locations = locations;
            Monsters = monsters;
            Traders = traders;
            Heroes = heroes;
            Player = player;
            TakenIds = takenIds;
        }
        public Hints Hints { get; set; }
        public List<ulong> TakenIds { get; set; }
        public List<Location> Locations { get; set; }
        public List<Monster> Monsters { get; set; }
        public List<Trader> Traders { get; set; }
        public List<Hero> Heroes { get; set; }
        public Player? Player { get; set; }
        public double PlayerHp { get; set; }
        public double PlayerMp { get; set; }
    }
}
