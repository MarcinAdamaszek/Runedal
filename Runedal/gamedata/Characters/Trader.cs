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

        }
        public Trader(string[] descriptive, string[][] responses) : base(descriptive, responses)
        {

        }

       
    }
}
