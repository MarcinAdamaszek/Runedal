﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runedal.GameData
{
    public class Passage
    {
        public Passage(bool isOpen)
        {
            IsOpen = isOpen;
        }

        public bool IsOpen { get; set; }
    }
}
