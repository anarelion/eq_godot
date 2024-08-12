
namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class WldBspRegion : WldFragment
    {
        public bool ContainsPolygons { get; private set; }

        public WldMesh Mesh { get; private set; }

        public WldBspRegionType RegionType { get; private set; }

        public int Flags;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());

            // Flags
            // 0x181 - Regions with polygons
            // 0x81 - Regions without
            // Bit 5 - PVS is WORDS
            // Bit 7 - PVS is bytes
            Flags = Reader.ReadInt32();

            if (Flags == 0x181)
            {
                ContainsPolygons = true;
            }

            // Always 0
            int unknown1 = Reader.ReadInt32();
            int data1Size = Reader.ReadInt32();
            int data2Size = Reader.ReadInt32();

            // Always 0
            int unknown2 = Reader.ReadInt32();
            int data3Size = Reader.ReadInt32();
            int data4Size = Reader.ReadInt32();

            // Always 0
            int unknown3 = Reader.ReadInt32();
            int data5Size = Reader.ReadInt32();
            int data6Size = Reader.ReadInt32();

            // Move past data1 and 2
            Reader.BaseStream.Position += 12 * data1Size + 12 * data2Size;

            // Move past data3
            for (int i = 0; i < data3Size; ++i)
            {
                int data3Flags = Reader.ReadInt32();
                int data3Size2 = Reader.ReadInt32();
                Reader.BaseStream.Position += data3Size2 * 4;
            }

            // Move past the data 4
            for (int i = 0; i < data4Size; ++i)
            {
                // Unhandled for now
            }

            // Move past the data5
            for (int i = 0; i < data5Size; i++)
            {
                Reader.BaseStream.Position += 7 * 4;
            }

            // Get the size of the PVS and allocate memory
            short pvsSize = Reader.ReadInt16();
            Reader.BaseStream.Position += pvsSize;

            // Move past the unknowns 
            uint bytes = Reader.ReadUInt32();
            Reader.BaseStream.Position += 16;

            // Get the mesh reference index and link to it
            if (ContainsPolygons)
            {
                Mesh = wld.GetFragment(Reader.ReadInt32()) as WldMesh;
            }
        }

        public void SetRegionFlag(WldBspRegionType bspRegionType)
        {
            RegionType = bspRegionType;
        }
    }
}