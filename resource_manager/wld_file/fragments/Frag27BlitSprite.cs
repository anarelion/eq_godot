
namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class Frag27BlitSprite : WldFragment
    {
        private Frag26BlitSpriteDef _reference;
        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            _reference = wld.GetFragment(Reader.ReadInt32()) as Frag26BlitSpriteDef;
            int value08 = Reader.ReadInt32(); // always 0
        }
    }
}