﻿using EQGodot.resource_manager.wld_file.data_types;
using EQGodot.resource_manager.wld_file.helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EQGodot.resource_manager.wld_file.fragments
{
    // Latern Extractor class
    public class Frag10HierarchicalSpriteDef : WldFragment
    {
        public List<Frag36DmSpriteDef2> Meshes
        {
            get; private set;
        }

        //public List<LegacyMesh> AlternateMeshes {
        //    get; private set;
        //}
        public List<SkeletonBone> Skeleton
        {
            get; set;
        }

        //private PolyhedronReference _fragment18Reference;

        public string ModelBase
        {
            get; set;
        }
        public bool IsAssigned
        {
            get; set;
        }
        private Dictionary<string, SkeletonBone> SkeletonPieceDictionary
        {
            get; set;
        }

        public Dictionary<string, data_types.Animation> Animations = [];

        public Dictionary<int, string> BoneMappingClean = [];
        public Dictionary<int, string> BoneMapping = [];

        public float BoundingRadius;

        public List<Mesh> SecondaryMeshes = [];
        //public List<LegacyMesh> SecondaryAlternateMeshes = new List<LegacyMesh>();

        private bool _hasBuiltData;

        public override void Initialize(int index, int type, int size, byte[] data, WldFile wld)
        {
            base.Initialize(index, type, size, data, wld);

            Skeleton = [];
            Meshes = [];
            //AlternateMeshes = new List<LegacyMesh>();
            SkeletonPieceDictionary = [];

            Name = wld.GetName(Reader.ReadInt32());
            ModelBase = FragmentNameCleaner.CleanName(this, true);

            // Always 2 when used in main zone, and object files.
            // This means, it has a bounding radius
            // Some differences in character + model archives
            // Confirmed
            int flags = Reader.ReadInt32();

            var ba = new BitAnalyzer(flags);

            bool hasUnknownParams = ba.IsBitSet(0);
            bool hasBoundingRadius = ba.IsBitSet(1);
            bool hasMeshReferences = ba.IsBitSet(9);

            // Number of bones in the skeleton
            int boneCount = Reader.ReadInt32();

            // Fragment 18 reference
            int fragment18Reference = Reader.ReadInt32() - 1;

            //if (fragment18Reference > 0) {
            //    _fragment18Reference = fragments[fragment18Reference] as PolyhedronReference;
            //}

            // Three sequential DWORDs
            // This will never be hit for object animations.
            if (hasUnknownParams)
            {
                Reader.BaseStream.Position += 3 * sizeof(int);
            }

            // This is the sphere radius checked against the frustum to cull this object
            if (hasBoundingRadius)
            {
                BoundingRadius = Reader.ReadSingle();
            }

            for (int i = 0; i < boneCount; ++i)
            {

                string boneName = wld.GetName(Reader.ReadInt32());

                // Always 0 for object bones
                // Confirmed
                int boneFlags = Reader.ReadInt32();

                // Reference to a bone track
                // Confirmed - is never a bad reference
                Frag13Track track = wld.GetFragment(Reader.ReadInt32()) as Frag13Track;
                AddPoseTrack(track, boneName);

                var pieceNew = new SkeletonBone
                {
                    Index = i,
                    Track = track,
                    Name = boneName
                };

                pieceNew.Track.IsPoseAnimation = true;
                pieceNew.AnimationTracks = [];

                BoneMappingClean[i] = data_types.Animation.CleanBoneAndStripBase(boneName, ModelBase);
                BoneMapping[i] = boneName;

                if (pieceNew.Track == null)
                {
                    GD.PrintErr("Unable to link track reference!");
                }

                string meshName = wld.GetName(Reader.ReadInt32());
                pieceNew.MeshReference = wld.GetFragmentByName(meshName) as Frag2DDMSprite;

                //if (pieceNew.MeshReference == null) {
                //    pieceNew.ParticleCloud = fragments[meshReferenceIndex] as ParticleCloud;
                //}

                if (pieceNew.Name == "root")
                {
                    pieceNew.Name = FragmentNameCleaner.CleanName(pieceNew.MeshReference.Mesh);
                }

                int childCount = Reader.ReadInt32();

                pieceNew.Children = [];

                for (int j = 0; j < childCount; ++j)
                {
                    pieceNew.Children.Add(Reader.ReadInt32());
                }

                Skeleton.Add(pieceNew);

                if (pieceNew.Name != "")
                {
                    SkeletonPieceDictionary.TryAdd(pieceNew.Name, pieceNew);
                }
            }

            // Read in mesh references
            // All meshes will have vertex bone assignments
            if (hasMeshReferences)
            {
                int size2 = Reader.ReadInt32();

                for (int i = 0; i < size2; ++i)
                {
                    Frag2DDMSprite meshRef = wld.GetFragment(Reader.ReadInt32()) as Frag2DDMSprite;

                    if (meshRef?.Mesh != null)
                    {
                        if (Meshes.All(x => x.Name != meshRef.Mesh.Name))
                        {
                            Meshes.Add(meshRef.Mesh);
                            meshRef.Mesh.IsHandled = true;
                        }
                    }

                    //if (meshRef?.LegacyMesh != null) {
                    //    if (AlternateMeshes.All(x => x.Name != meshRef.LegacyMesh.Name)) {
                    //        AlternateMeshes.Add(meshRef.LegacyMesh);
                    //    }
                    //}
                }

                Meshes = [.. Meshes.OrderBy(x => x.Name)];

                List<int> unknown = [];

                for (int i = 0; i < size2; ++i)
                {
                    unknown.Add(Reader.ReadInt32());
                }
            }
        }

        public void BuildSkeletonData(bool stripModelBase)
        {
            if (_hasBuiltData)
            {
                return;
            }

            BuildSkeletonTreeData(0, Skeleton, null, string.Empty,
                string.Empty, string.Empty, stripModelBase);
            _hasBuiltData = true;
        }

        private void AddPoseTrack(Frag13Track track, string pieceName)
        {
            if (!Animations.ContainsKey("pos"))
            {
                Animations["pos"] = new data_types.Animation();
            }

            Animations["pos"].AddTrack(track, pieceName, data_types.Animation.CleanBoneName(pieceName),
                data_types.Animation.CleanBoneAndStripBase(pieceName, ModelBase));
            track.TrackDefFragment.IsAssigned = true;
            track.IsProcessed = true;
            track.IsPoseAnimation = true;
        }

        public void AddTrackDataEquipment(Frag13Track track, string boneName, bool isDefault = false)
        {
            string animationName = string.Empty;
            string modelName = string.Empty;
            string pieceName = string.Empty;

            string cleanedName = FragmentNameCleaner.CleanName(track, true);

            if (isDefault)
            {
                animationName = "pos";
                modelName = ModelBase;
                cleanedName = cleanedName.Replace(ModelBase, String.Empty);
                pieceName = cleanedName == string.Empty ? "root" : cleanedName;
            }
            else
            {
                if (cleanedName.Length <= 3)
                {
                    return;
                }

                animationName = cleanedName.Substring(0, 3);
                cleanedName = cleanedName.Remove(0, 3);

                if (cleanedName.Length < 3)
                {
                    return;
                }

                modelName = ModelBase;
                pieceName = boneName;

                if (pieceName == string.Empty)
                {
                    pieceName = "root";
                }
            }

            track.SetTrackData(modelName, animationName, pieceName);

            if (Animations.ContainsKey(track.AnimationName))
            {
                if (modelName == ModelBase && ModelBase != Animations[animationName].AnimModelBase)
                {
                    Animations.Remove(animationName);
                }

                if (modelName != ModelBase && ModelBase == Animations[animationName].AnimModelBase)
                {
                    return;
                }
            }

            if (!Animations.ContainsKey(track.AnimationName))
            {
                Animations[track.AnimationName] = new data_types.Animation();
            }

            Animations[track.AnimationName].AddTrack(track, track.PieceName, data_types.Animation.CleanBoneName(track.PieceName),
                data_types.Animation.CleanBoneAndStripBase(track.PieceName, ModelBase));
            track.TrackDefFragment.IsAssigned = true;
            track.IsProcessed = true;
        }

        public void AddTrackData(Frag13Track track, bool isDefault = false)
        {
            string animationName = string.Empty;
            string modelName = string.Empty;
            string pieceName = string.Empty;

            string cleanedName = FragmentNameCleaner.CleanName(track, true);

            if (isDefault)
            {
                animationName = "pos";
                modelName = ModelBase;
                cleanedName = cleanedName.Replace(ModelBase, String.Empty);
                pieceName = cleanedName == string.Empty ? "root" : cleanedName;
            }
            else
            {
                if (cleanedName.Length <= 3)
                {
                    return;
                }

                animationName = cleanedName.Substring(0, 3);
                cleanedName = cleanedName.Remove(0, 3);

                if (cleanedName.Length < 3)
                {
                    return;
                }

                modelName = cleanedName.Substring(0, 3);
                cleanedName = cleanedName.Remove(0, 3);
                pieceName = cleanedName;

                if (pieceName == string.Empty)
                {
                    pieceName = "root";
                }
            }

            track.SetTrackData(modelName, animationName, pieceName);

            if (Animations.ContainsKey(track.AnimationName))
            {
                if (modelName == ModelBase && ModelBase != Animations[animationName].AnimModelBase)
                {
                    Animations.Remove(animationName);
                }

                if (modelName != ModelBase && ModelBase == Animations[animationName].AnimModelBase)
                {
                    return;
                }
            }

            if (!Animations.ContainsKey(track.AnimationName))
            {
                Animations[track.AnimationName] = new data_types.Animation();
            }

            Animations[track.AnimationName]
                .AddTrack(track, track.Name, data_types.Animation.CleanBoneName(track.PieceName),
                    data_types.Animation.CleanBoneAndStripBase(track.PieceName, ModelBase));
            track.TrackDefFragment.IsAssigned = true;
            track.IsProcessed = true;
        }

        private void BuildSkeletonTreeData(int index, List<SkeletonBone> treeNodes, SkeletonBone parent,
            string runningName, string runningNameCleaned, string runningIndex, bool stripModelBase)
        {
            SkeletonBone bone = treeNodes[index];
            bone.Parent = parent;
            bone.CleanedName = CleanBoneName(bone.Name, stripModelBase);
            BoneMappingClean[index] = bone.CleanedName;

            if (bone.Name != string.Empty)
            {
                runningIndex += bone.Index + "/";
            }

            runningName += bone.Name;
            runningNameCleaned += bone.CleanedName;

            bone.FullPath = runningName;
            bone.CleanedFullPath = runningNameCleaned;

            if (bone.Children.Count == 0)
            {
                return;
            }

            runningName += "/";
            runningNameCleaned += "/";

            foreach (var childNode in bone.Children)
            {
                BuildSkeletonTreeData(childNode, treeNodes, bone, runningName, runningNameCleaned, runningIndex,
                    stripModelBase);
            }
        }

        private string CleanBoneName(string nodeName, bool stripModelBase)
        {
            nodeName = nodeName.Replace("_DAG", "");
            nodeName = nodeName.ToLower();
            if (stripModelBase)
            {
                nodeName = nodeName.Replace(ModelBase, string.Empty);
            }

            nodeName += nodeName.Length == 0 ? "root" : string.Empty;
            return nodeName;
        }

        public void AddAdditionalMesh(Frag36DmSpriteDef2 mesh)
        {
            if (Meshes.Any(x => x.Name == mesh.Name)
                /* || SecondaryMeshes.Any(x => x.Name == mesh.Name) */)
            {
                return;
            }

            if (mesh.MobPieces.Count == 0)
            {
                return;
            }

            //SecondaryMeshes.Add(mesh);
            //SecondaryMeshes = SecondaryMeshes.OrderBy(x => x.Name).ToList();
        }

        //public void AddAdditionalAlternateMesh(LegacyMesh mesh)
        //{
        //    if (AlternateMeshes.Any(x => x.Name == mesh.Name)
        //        || SecondaryAlternateMeshes.Any(x => x.Name == mesh.Name)) {
        //        return;
        //    }

        //    if (mesh.MobPieces.Count == 0) {
        //        return;
        //    }

        //    SecondaryAlternateMeshes.Add(mesh);
        //    SecondaryAlternateMeshes = SecondaryAlternateMeshes.OrderBy(x => x.Name).ToList();
        //}

        public bool IsValidSkeleton(string trackName, out string boneName)
        {
            string track = trackName.Substring(3);

            if (trackName == ModelBase)
            {
                boneName = ModelBase;
                return true;
            }

            foreach (var bone in Skeleton)
            {
                string cleanBoneName = bone.Name.Replace("_DAG", string.Empty).ToLower();
                if (cleanBoneName == track)
                {
                    boneName = bone.Name.ToLower();
                    return true;
                }
            }

            boneName = string.Empty;
            return false;
        }

        public Transform3D GetBoneMatrix(int boneIndex, string animName, int frame)
        {
            if (!Animations.ContainsKey(animName))
            {
                return Transform3D.Identity;
            }

            if (frame < 0 || frame >= Animations[animName].FrameCount)
            {
                return Transform3D.Identity;
            }

            var currentBone = Skeleton[boneIndex];

            Transform3D boneMatrix = Transform3D.Identity;

            while (currentBone != null)
            {
                if (!Animations[animName].TracksCleanedStripped.ContainsKey(currentBone.CleanedName))
                {
                    break;
                }

                var track = Animations[animName].TracksCleanedStripped[currentBone.CleanedName].TrackDefFragment;
                int realFrame = frame >= track.Frames.Count ? 0 : frame;
                currentBone = Skeleton[boneIndex].Parent;

                var modelTransform = Transform3D.Identity;

                modelTransform.Translated(track.Frames[realFrame].Translation);
                var rotationQuat = track.Frames[realFrame].Rotation;
                modelTransform.Rotated(rotationQuat.GetAxis(), rotationQuat.GetAngle());

                float scaleValue = track.Frames[realFrame].Scale;
                var scaleMat = new Vector3(scaleValue, scaleValue, scaleValue);
                modelTransform.Scaled(scaleMat);

                boneMatrix = modelTransform * boneMatrix;

                if (currentBone != null)
                {
                    boneIndex = currentBone.Index;
                }
            }

            return boneMatrix;
        }

        public void RenameNodeBase(string newBase)
        {
            foreach (var node in Skeleton)
            {
                node.Name = node.Name.Replace(ModelBase.ToUpper(), newBase.ToUpper());
            }

            var newNameMapping = new Dictionary<int, string>();
            foreach (var node in BoneMapping)
            {
                newNameMapping[node.Key] = node.Value.Replace(ModelBase.ToUpper(), newBase.ToUpper());
            }

            BoneMapping = newNameMapping;

            ModelBase = newBase;
        }
    }
}