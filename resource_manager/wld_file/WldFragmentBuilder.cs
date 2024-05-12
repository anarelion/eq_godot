using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public static class WldFragmentBuilder {
        public static Dictionary<int, Func<WldFragment>> Fragments = new Dictionary<int, Func<WldFragment>> {
            // Materials
            {0x03, () => new WldBitmapName()},
            {0x04, () => new WldBitmapInfo()},
            {0x05, () => new WldBitmapInfoReference()},
            {0x30, () => new WldMaterial()},
            {0x31, () => new WldMaterialList()},

            // BSP Tree
            //{0x21, () => new BspTree()},
            //{0x22, () => new BspRegion()},
            //{0x29, () => new BspRegionType()},

            // Meshes
            {0x36, () => new WldMesh()},
            //{0x37, () => new MeshAnimatedVertices()},
            //{0x2E, () => new LegacyMeshAnimatedVertices()},
            //{0x2F, () => new MeshAnimatedVerticesReference()},
            {0x2D, () => new WldMeshReference()},
            {0x2C, () => new WldLegacyMesh()},

            // Animation
            {0x10, () => new WldSkeletonHierarchy()},
            {0x11, () => new WldSkeletonHierarchyReference()},
            {0x12, () => new WldTrackDefFragment()},
            {0x13, () => new WldTrackFragment()},
            {0x14, () => new WldActorDef()},

            // Lights
            //{0x1B, () => new LightSource()},
            //{0x1C, () => new LightSourceReference()},
            //{0x28, () => new LightInstance()},
            //{0x2A, () => new AmbientLight()},
            //{0x35, () => new GlobalAmbientLight()},

            // Vertex colors
            //{0x32, () => new VertexColors()},
            //{0x33, () => new VertexColorsReference()},

            // Particle Cloud
            //{0x26, () => new ParticleSprite()},
            //{0x27, () => new ParticleSpriteReference()},
            //{0x34, () => new ParticleCloud()},

            // General
            //{0x15, () => new ObjectInstance()},

            // Not used/unknown
            //{0x08, () => new Camera()},
            //{0x09, () => new CameraReference()},
            //{0x16, () => new Fragment16()},
            //{0x17, () => new Polyhedron()},
            //{0x18, () => new PolyhedronReference()},
            //{0x06, () => new Fragment06()},
            //{0x07, () => new Fragment07()},
        };
    }
}
