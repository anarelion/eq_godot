
namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class Frag05SimpleSprite : WldFragment
    {
        /// <summary>
        /// The reference to the BitmapInfo
        /// </summary>
        public Frag04SimpleSpriteDef SimpleSpriteDef
        {
            get; private set;
        }

        public int Flags;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            SimpleSpriteDef = wld.GetFragment(Reader.ReadInt32()) as Frag04SimpleSpriteDef;

            // Either 0 or 80 - unknown
            Flags = Reader.ReadInt32();
        }
    }
}
