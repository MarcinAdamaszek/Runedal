using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class CharAction
    {
        public CharAction() { }
        public CharAction(double actionPointsCost)
        {
            ActionPointsCost = actionPointsCost;
        }
        public double ActionPointsCost { get; set; }
    }
}
