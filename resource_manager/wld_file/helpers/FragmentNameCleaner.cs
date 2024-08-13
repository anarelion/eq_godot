using EQGodot.resource_manager.wld_file.fragments;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot.resource_manager.wld_file.helpers
{
    // Latern Extractor class
    public static class FragmentNameCleaner
    {
        private static Dictionary<Type, string> _prefixes = new Dictionary<Type, string>
        {
            // Materials
            {typeof(Frag31MaterialPalette), "_MP"},
            {typeof(Frag30MaterialDef), "_MDF"},
            {typeof(Frag36DmSpriteDef2), "_DMSPRITEDEF"},
            {typeof(Frag2CDMSpriteDef), "_DMSPRITEDEF"},
            {typeof(Frag14ActorDef), "_ACTORDEF"},
            {typeof(Frag10HierarchicalSpriteDef), "_HS_DEF"},
            {typeof(Frag12TrackDef), "_TRACKDEF"},
            {typeof(Frag13Track), "_TRACK"},
            // {typeof(ParticleCloud), "_PCD"},
        };

        public static string CleanName(WldFragment fragment, bool toLower = true)
        {
            string cleanedName = fragment.Name;

            if (_prefixes.ContainsKey(fragment.GetType()))
            {
                cleanedName = cleanedName.Replace(_prefixes[fragment.GetType()], string.Empty);
            }

            if (toLower)
            {
                cleanedName = cleanedName.ToLower();
            }

            return cleanedName.Trim();
        }
    }
}
