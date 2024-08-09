using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using System;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class WldTrackFragment : WldFragment
    {
        /// <summary>
        /// Reference to a skeleton piece
        /// </summary>
        public WldTrackDefFragment TrackDefFragment
        {
            get; set;
        }

        public bool IsPoseAnimation
        {
            get; set;
        }
        public bool IsProcessed
        {
            get; set;
        }

        public bool IsAnimated
        {
            get; set;
        }
        public bool IsReversed
        {
            get; set;
        }
        public bool InterpolateAllowed
        {
            get; set;
        }
        public int Flags
        {
            get; set;
        }
        public int FrameMs
        {
            get; set;
        }

        public string ModelName;
        public string AnimationName;
        public string PieceName;

        public bool IsNameParsed;

        public override void Initialize(int index, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, size, data, wld);
            Name = wld.GetName(Reader.ReadInt32());
            TrackDefFragment = wld.GetFragment(Reader.ReadInt32()) as WldTrackDefFragment;

            if (TrackDefFragment == null)
            {
                GD.PrintErr("Bad track def reference'");
            }

            // Either 4 or 5 - maybe something to look into
            // Bits are set 0, or 2. 0 has the extra field for delay.
            // 2 doesn't have any additional fields.
            Flags = Reader.ReadInt32();

            BitAnalyzer bitAnalyzer = new BitAnalyzer(Flags);
            IsAnimated = bitAnalyzer.IsBitSet(0);
            IsReversed = bitAnalyzer.IsBitSet(1);
            InterpolateAllowed = bitAnalyzer.IsBitSet(2);

            FrameMs = IsAnimated ? Reader.ReadInt32() : 0;

            if (Reader.BaseStream.Position != Reader.BaseStream.Length)
            {

            }
        }

        public void SetTrackData(string modelName, string animationName, string pieceName)
        {
            ModelName = modelName;
            AnimationName = animationName;
            PieceName = pieceName;
        }

        /// <summary>
        /// This is only ever called when we are finding additional animations.
        /// All animations that are not the default skeleton animations:
        /// 1. Start with a 3 letter animation abbreviation (e.g. C05)
        /// 2. Continue with a 3 letter model name
        /// 3. Continue with the skeleton piece name
        /// 4. End with _TRACK
        /// </summary>
        /// <param name="logger"></param>
        public void ParseTrackData()
        {
            string cleanedName = FragmentNameCleaner.CleanName(this, true);

            if (cleanedName.Length < 6)
            {
                if (cleanedName.Length == 3)
                {
                    ModelName = cleanedName;
                    IsNameParsed = true;
                    return;
                }

                ModelName = cleanedName;
                return;
            }

            // Equipment edge case
            if (cleanedName.Substring(0, 3) == cleanedName.Substring(3, 3))
            {
                AnimationName = cleanedName.Substring(0, 3);
                ModelName = cleanedName.Substring(Math.Min(7, cleanedName.Length));
                PieceName = "root";
                IsNameParsed = true;
                return;
            }

            AnimationName = cleanedName.Substring(0, 3);
            cleanedName = cleanedName.Remove(0, 3);
            ModelName = cleanedName.Substring(0, 3);
            cleanedName = cleanedName.Remove(0, 3);
            PieceName = cleanedName;

            IsNameParsed = true;
            //logger.LogError($"Split into, {AnimationName} {ModelName} {PieceName}");
        }

        public void ParseTrackDataEquipment(WldSkeletonHierarchy skeletonHierarchy)
        {
            string cleanedName = FragmentNameCleaner.CleanName(this, true);

            // Equipment edge case
            if (cleanedName == skeletonHierarchy.ModelBase && cleanedName.Length > 6 || cleanedName.Substring(0, 3) == cleanedName.Substring(3, 3))
            {
                AnimationName = cleanedName.Substring(0, 3);
                ModelName = cleanedName.Substring(7);
                PieceName = "root";
                IsNameParsed = true;
                return;
            }

            AnimationName = cleanedName.Substring(0, 3);
            cleanedName = cleanedName.Remove(0, 3);
            ModelName = skeletonHierarchy.ModelBase;
            cleanedName = cleanedName.Replace(skeletonHierarchy.ModelBase, string.Empty);
            PieceName = cleanedName;
            IsNameParsed = true;
        }
    }
}
