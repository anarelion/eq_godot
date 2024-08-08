using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.data_types
{
    // Latern Extractor class
    public class Polygon
    {
        public Polygon GetCopy()
        {
            return new Polygon
            {
                IsSolid = this.IsSolid,
                Vertex1 = this.Vertex1,
                Vertex2 = this.Vertex2,
                Vertex3 = this.Vertex3,
                MaterialIndex = this.MaterialIndex
            };
        }

        public bool IsSolid
        {
            get; set;
        }
        public int Vertex1
        {
            get; set;
        }
        public int Vertex2
        {
            get; set;
        }
        public int Vertex3
        {
            get; set;
        }
        public int MaterialIndex
        {
            get; set;
        }
    }
}
