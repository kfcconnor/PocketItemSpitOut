using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled;

namespace PocketItemSpitOut
{
    using Exiled.API.Interfaces;
    using log = Exiled.API.Features.Log;
    public class config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool debug { get; set; } = false;
        public bool allowTesla { get; set; } = false;
        public bool allowDropOnLarryDeath { get; set; } = false;
        public int minTimeForDrop { get; set; } = 10;
        public int maxTimeForDrop { get; set; } = 25;
        public int chanceForDrop { get; set; } = 75;
        public int itemsDroppedOnLarryDeath { get; set; } = 10;
    }
}


