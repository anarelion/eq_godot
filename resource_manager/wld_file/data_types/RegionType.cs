using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.data_types
{

    public enum RegionType
    {
        Normal = 0,
        Water = 1,
        Lava = 2,
        Pvp = 3,
        Zoneline = 4,
        WaterBlockLOS = 5,
        FreezingWater = 6,
        Slippery = 7,
        Unknown = 8,
    }
}