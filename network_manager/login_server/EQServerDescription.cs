
using Godot;

namespace EQGodot2.network_manager.login_server
{
    public partial class EQServerDescription : GodotObject
    {
        public string Address;
        public uint ServerType;
        public uint Id;
        public string LongName;
        public string Language;
        public string Region;
        public uint Status;
        public uint Players;

    }
}