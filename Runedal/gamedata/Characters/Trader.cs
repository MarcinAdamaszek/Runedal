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
            if (Items!.Count == 0)
            {
                IsTalker = true;
            }
        }
        public Trader(string[] descriptive, string[][] responses, int gold) : base(descriptive, responses, gold)
        {
            if (Items!.Count == 0)
            {
                IsTalker = true;
            }
        }

        //copy constructor
        public Trader(Trader tr) : base(tr)
        {
            if (Items!.Count == 0)
            {
                IsTalker = true;
            }
        }
        public bool IsTalker { get; set; }

    }
}
