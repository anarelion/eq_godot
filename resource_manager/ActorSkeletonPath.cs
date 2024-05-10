using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager {
    public partial class ActorSkeletonPath : Resource {
        [Export]
        public string ModelName;
        [Export]
        public string AnimationName;
        [Export]
        public string PieceName;
        [Export]
        public int FrameMs {
            get; set;
        }
        [Export]
        public Godot.Collections.Array<Vector3> Translation {
            get; set;
        }
        [Export]
        public Godot.Collections.Array<Quaternion> Rotation {
            get; set;
        }
    }
}
