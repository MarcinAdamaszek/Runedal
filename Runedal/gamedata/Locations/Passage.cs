using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData.Locations
{
    public class Passage
    {
        public Passage(bool isOpen = true)
        {
            IsOpen = isOpen;
        }

        public bool IsOpen { get; set; }
    }
}
