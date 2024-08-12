using EQGodot.resource_manager.wld_file.data_types;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using System;
using System.Collections.Generic;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class WldBspRegionType : WldFragment
    {
        public List<RegionType> RegionTypes { get; private set; }

        public List<int> BspRegionIndices { get; private set; }

        public string RegionString { get; set; }

        public ZonelineInfo Zoneline;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            int flags = Reader.ReadInt32();
            int regionCount = Reader.ReadInt32();

            BspRegionIndices = [];
            for (int i = 0; i < regionCount; ++i)
            {
                BspRegionIndices.Add(Reader.ReadInt32());
            }

            int regionStringSize = Reader.ReadInt32();

            string regionTypeString = regionStringSize == 0 ? Name.ToLower() :
                WldStringDecoder.Decode(Reader.ReadBytes(regionStringSize)).ToLower();

            RegionTypes = [];

            if (regionTypeString.StartsWith("wtn_") || regionTypeString.StartsWith("wt_"))
            {
                // Ex: wt_zone, wtn_XXXXXX
                RegionTypes.Add(RegionType.Water);
            }
            else if (regionTypeString.StartsWith("wtntp"))
            {
                RegionTypes.Add(RegionType.Water);
                RegionTypes.Add(RegionType.Zoneline);
                DecodeZoneline(regionTypeString);
                RegionString = regionTypeString;
            }
            else if (regionTypeString.StartsWith("lan_") || regionTypeString.StartsWith("la_"))
            {
                RegionTypes.Add(RegionType.Lava);
            }
            else if (regionTypeString.StartsWith("lantp"))
            {
                // TODO: Figure this out - soldunga
                RegionTypes.Add(RegionType.Lava);
                RegionTypes.Add(RegionType.Zoneline);
                DecodeZoneline(regionTypeString);
                RegionString = regionTypeString;
            }
            else if (regionTypeString.StartsWith("drntp"))
            {
                RegionTypes.Add(RegionType.Zoneline);
                DecodeZoneline(regionTypeString);
                RegionString = regionTypeString;
            }
            else if (regionTypeString.StartsWith("drp_"))
            {
                RegionTypes.Add(RegionType.Pvp);
            }
            else if (regionTypeString.StartsWith("drn_"))
            {
                if (regionTypeString.Contains("_s_"))
                {
                    RegionTypes.Add(RegionType.Slippery);
                }
                else
                {
                    RegionTypes.Add(RegionType.Unknown);
                }
            }
            else if (regionTypeString.StartsWith("sln_"))
            {
                // gukbottom, cazicthule (gumdrop), runnyeye, velketor
                RegionTypes.Add(RegionType.WaterBlockLOS);
            }
            else if (regionTypeString.StartsWith("vwn_"))
            {
                RegionTypes.Add(RegionType.FreezingWater);
            }
            else
            {
                // All trilogy client region types are accounted for
                // This is here in case newer clients have newer types
                // tox - "wt_zone' - Possible legacy water zonepoint for boat?
                RegionTypes.Add(RegionType.Normal);
            }
        }

        private void DecodeZoneline(string regionTypeString)
        {
            Zoneline = new ZonelineInfo();

            // TODO: Verify this
            if (regionTypeString == "drntp_zone")
            {
                Zoneline.Type = ZonelineType.Reference;
                Zoneline.Index = 0;
                return;
            }

            int zoneId = Convert.ToInt32(regionTypeString.Substring(5, 5));

            if (zoneId == 255)
            {
                int zonelineId = Convert.ToInt32(regionTypeString.Substring(10, 6));
                Zoneline.Type = ZonelineType.Reference;
                Zoneline.Index = zonelineId;

                return;
            }

            Zoneline.ZoneIndex = zoneId;

            float x = GetValueFromRegionString(regionTypeString.Substring(10, 6));
            float y = GetValueFromRegionString(regionTypeString.Substring(16, 6));
            float z = GetValueFromRegionString(regionTypeString.Substring(22, 6));
            int rot = Convert.ToInt32(regionTypeString.Substring(28, 3));

            Zoneline.Type = ZonelineType.Absolute;
            Zoneline.Position = new Vector3(x, y, z);
            Zoneline.Heading = rot;
        }

        private float GetValueFromRegionString(string substring)
        {
            if (substring.StartsWith("-"))
            {
                return -Convert.ToSingle(substring.Substring(1, 5));
            }
            else
            {
                return Convert.ToSingle(substring);
            }
        }
    }
}