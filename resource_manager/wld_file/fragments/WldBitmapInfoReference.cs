
namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class WldBitmapInfoReference : WldFragment
    {
        /// <summary>
        /// The reference to the BitmapInfo
        /// </summary>
        public WldBitmapInfo BitmapInfo
        {
            get; private set;
        }

        public int Flags;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            BitmapInfo = wld.GetFragment(Reader.ReadInt32()) as WldBitmapInfo;

            // Either 0 or 80 - unknown
            Flags = Reader.ReadInt32();
        }
    }
}
