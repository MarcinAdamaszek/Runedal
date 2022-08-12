using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Characters
{
    public class Trader : Character
    {
        //default constructor for json deserialization
        public Trader() : base()
        {
            Items = new Dictionary<string, int>();
        }
        public Trader(string[] descriptive, string[][] responses, int gold, string[] items) : base(descriptive, responses, gold)
        {
            Items = new Dictionary<string, int>();
        }
        public Dictionary<string, int>? Items { get; set; }
    }
}
