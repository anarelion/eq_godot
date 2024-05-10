using EQGodot2.resource_manager.wld_file;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.helpers {
    // Latern Extractor class
    public static class FragmentNameCleaner {
        private static Dictionary<Type, string> _prefixes = new Dictionary<Type, string>
        {
            // Materials
            {typeof(WldMaterialList), "_MP"},
            {typeof(WldMaterial), "_MDF"},
            {typeof(WldMesh), "_DMSPRITEDEF"},
            // {typeof(LegacyMesh), "_DMSPRITEDEF"},
            {typeof(WldActorDef), "_ACTORDEF"},
            {typeof(WldSkeletonHierarchy), "_HS_DEF"},
            {typeof(WldTrackDefFragment), "_TRACKDEF"},
            {typeof(WldTrackFragment), "_TRACK"},
            // {typeof(ParticleCloud), "_PCD"},
        };

        public static string CleanName(WldFragment fragment, bool toLower = true)
        {
            string cleanedName = fragment.Name;

            if (_prefixes.ContainsKey(fragment.GetType())) {
                cleanedName = cleanedName.Replace(_prefixes[fragment.GetType()], string.Empty);
            }

            if (toLower) {
                cleanedName = cleanedName.ToLower();
            }

            return cleanedName.Trim();
        }
    }
}
