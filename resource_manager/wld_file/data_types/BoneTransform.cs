using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file.data_types {
    // Latern Extractor class
    public class BoneTransform {
        public Vector3 Translation {
            get; set;
        }
        public Quaternion Rotation {
            get; set;
        }
        public float Scale {
            get; set;
        }
        public Quaternion ModelMatrix;
    }
}
