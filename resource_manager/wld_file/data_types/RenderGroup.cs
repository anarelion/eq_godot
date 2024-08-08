using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.data_types
{
    // Latern Extractor class
    public class RenderGroup
    {
        public int StartPolygon
        {
            get; set;
        }
        public int PolygonCount
        {
            get; set;
        }
        public int MaterialIndex
        {
            get; set;
        }
    }
}
