using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EQGodot.helpers;
using EQGodot.resource_manager.pack_file;
using EQGodot.resource_manager.wld_file.data_types;
using Godot;

namespace EQGodot.resource_manager.wld_file
{
    // Latern Extractor class
    public class WldBspTree : WldFragment
    {

        public List<BspNode> Nodes { get; private set; }

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            int nodeCount = Reader.ReadInt32();
            Nodes = [];

            for (int i = 0; i < nodeCount; ++i)
            {
                Nodes.Add(new BspNode
                {
                    NormalX = Reader.ReadSingle(),
                    NormalY = Reader.ReadSingle(),
                    NormalZ = Reader.ReadSingle(),
                    SplitDistance = Reader.ReadSingle(),
                    RegionId = Reader.ReadInt32(),
                    LeftNode = Reader.ReadInt32() - 1,
                    RightNode = Reader.ReadInt32() - 1
                });
            }
        }

        /// <summary>
        /// Links BSP nodes to their corresponding BSP Regions
        /// The RegionId is not a fragment index but instead an index in a list of BSP Regions
        /// </summary>
        /// <param name="fragments">BSP region fragments</param>
        public void LinkBspRegions(List<WldBspRegion> fragments)
        {
            foreach (var node in Nodes)
            {
                if (node.RegionId == 0)
                {
                    continue;
                }

                node.Region = fragments[node.RegionId - 1];
            }
        }
    }
}
