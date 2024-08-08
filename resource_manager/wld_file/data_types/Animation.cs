using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.data_types
{
    // Latern Extractor class
    public class Animation
    {
        public string AnimModelBase;
        public Dictionary<string, WldTrackFragment> Tracks;
        public Dictionary<string, WldTrackFragment> TracksCleaned;
        public Dictionary<string, WldTrackFragment> TracksCleanedStripped;
        public int FrameCount;
        public int AnimationTimeMs
        {
            get; set;
        }

        public Animation()
        {
            Tracks = [];
            TracksCleaned = [];
            TracksCleanedStripped = [];
        }

        public static string CleanBoneName(string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
            {
                return boneName;
            }

            var cleanedName = CleanBoneNameDag(boneName);
            return cleanedName.Length == 0 ? "root" : cleanedName;
        }

        public static string CleanBoneAndStripBase(string boneName, string modelBase)
        {
            var cleanedName = CleanBoneNameDag(boneName);

            if (cleanedName.StartsWith(modelBase))
            {
                cleanedName = cleanedName.Substring(modelBase.Length);
            }

            return cleanedName.Length == 0 ? "root" : cleanedName;
        }

        public static string CleanBoneNameDag(string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
            {
                return boneName;
            }

            return boneName.ToLower().Replace("_dag", string.Empty);
        }

        public void AddTrack(WldTrackFragment track, string pieceName, string cleanedName, string cleanStrippedName)
        {
            // Prevent overwriting tracks
            // Drachnid edge case
            if (Tracks.ContainsKey(pieceName))
            {
                return;
            }

            Tracks[pieceName] = track;
            TracksCleaned[cleanedName] = track;
            TracksCleanedStripped[cleanStrippedName] = track;

            if (string.IsNullOrEmpty(AnimModelBase) &&
                !string.IsNullOrEmpty(track.ModelName))
            {
                AnimModelBase = track.ModelName;
            }

            if (track.TrackDefFragment.Frames.Count > FrameCount)
            {
                FrameCount = track.TrackDefFragment.Frames.Count;
            }

            int totalTime = track.TrackDefFragment.Frames.Count * track.FrameMs;

            if (totalTime > AnimationTimeMs)
            {
                AnimationTimeMs = totalTime;
            }
        }
    }
}
