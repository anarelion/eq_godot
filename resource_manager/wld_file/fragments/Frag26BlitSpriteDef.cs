
namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class Frag26BlitSpriteDef : WldFragment
    {
        private Frag05SimpleSprite _bitmapReference;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            int value04 = Reader.ReadInt32(); // flags? always 0
            _bitmapReference = wld.GetFragment(Reader.ReadInt32()) as Frag05SimpleSprite;
            int value12 = Reader.ReadInt32(); // always the same value. unlikely a float, or bytes. Not color.

        }
    }
}