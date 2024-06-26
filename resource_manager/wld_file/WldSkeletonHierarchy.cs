﻿using EQGodot2.helpers;
using EQGodot2.resource_manager.wld_file.data_types;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQGodot2.resource_manager.wld_file {
    // Latern Extractor class
    public class WldSkeletonHierarchy : WldFragment {
        public List<WldMesh> Meshes {
            get; private set;
        }

        //public List<LegacyMesh> AlternateMeshes {
        //    get; private set;
        //}
        public List<SkeletonBone> Skeleton {
            get; set;
        }

        //private PolyhedronReference _fragment18Reference;

        public string ModelBase {
            get; set;
        }
        public bool IsAssigned {
            get; set;
        }
        private Dictionary<string, SkeletonBone> SkeletonPieceDictionary {
            get; set;
        }

        public Dictionary<string, data_types.Animation> Animations = new Dictionary<string, data_types.Animation>();

        public Dictionary<int, string> BoneMappingClean = new Dictionary<int, string>();
        public Dictionary<int, string> BoneMapping = new Dictionary<int, string>();

        public float BoundingRadius;

        public List<Mesh> SecondaryMeshes = new List<Mesh>();
        //public List<LegacyMesh> SecondaryAlternateMeshes = new List<LegacyMesh>();

        private bool _hasBuiltData;

        public override void Initialize(int index, int size, byte[] data,
            List<WldFragment> fragments,
            Godot.Collections.Dictionary<int, string> stringHash, bool isNewWldFormat)
        {
            base.Initialize(index, size, data, fragments, stringHash, isNewWldFormat);

            Skeleton = new List<SkeletonBone>();
            Meshes = new List<WldMesh>();
            //AlternateMeshes = new List<LegacyMesh>();
            SkeletonPieceDictionary = new Dictionary<string, SkeletonBone>();

            Name = stringHash[-Reader.ReadInt32()];
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
            if (hasUnknownParams) {
                Reader.BaseStream.Position += 3 * sizeof(int);
            }

            // This is the sphere radius checked against the frustum to cull this object
            if (hasBoundingRadius) {
                BoundingRadius = Reader.ReadSingle();
            }

            for (int i = 0; i < boneCount; ++i) {
                // An index into the string has to get this bone's name
                int boneNameIndex = Reader.ReadInt32();
                string boneName = string.Empty;

                if (stringHash.ContainsKey(-boneNameIndex)) {
                    boneName = stringHash[-boneNameIndex];
                }

                // Always 0 for object bones
                // Confirmed
                int boneFlags = Reader.ReadInt32();

                // Reference to a bone track
                // Confirmed - is never a bad reference
                int trackReferenceIndex = Reader.ReadInt32() - 1;

                WldTrackFragment track = fragments[trackReferenceIndex] as WldTrackFragment;
                AddPoseTrack(track, boneName);

                var pieceNew = new SkeletonBone {
                    Index = i,
                    Track = track,
                    Name = boneName
                };

                pieceNew.Track.IsPoseAnimation = true;
                pieceNew.AnimationTracks = new Dictionary<string, WldTrackFragment>();

                BoneMappingClean[i] = data_types.Animation.CleanBoneAndStripBase(boneName, ModelBase);
                BoneMapping[i] = boneName;

                if (pieceNew.Track == null) {
                    GD.PrintErr("Unable to link track reference!");
                }

                int meshReferenceIndex = Reader.ReadInt32() - 1;

                if (meshReferenceIndex < 0) {
                    string name = stringHash[-meshReferenceIndex - 1];
                } else if (meshReferenceIndex != 0) {
                    pieceNew.MeshReference = fragments[meshReferenceIndex] as WldMeshReference;

                    //if (pieceNew.MeshReference == null) {
                    //    pieceNew.ParticleCloud = fragments[meshReferenceIndex] as ParticleCloud;
                    //}

                    if (pieceNew.Name == "root") {
                        pieceNew.Name = FragmentNameCleaner.CleanName(pieceNew.MeshReference.Mesh);
                    }
                }

                int childCount = Reader.ReadInt32();

                pieceNew.Children = new List<int>();

                for (int j = 0; j < childCount; ++j) {
                    int childIndex = Reader.ReadInt32();
                    pieceNew.Children.Add(childIndex);
                }

                Skeleton.Add(pieceNew);

                if (pieceNew.Name != "") {
                    if (!SkeletonPieceDictionary.ContainsKey(pieceNew.Name)) {
                        SkeletonPieceDictionary.Add(pieceNew.Name, pieceNew);
                    }
                }
            }

            // Read in mesh references
            // All meshes will have vertex bone assignments
            if (hasMeshReferences) {
                int size2 = Reader.ReadInt32();

                for (int i = 0; i < size2; ++i) {
                    int meshRefIndex = Reader.ReadInt32() - 1;

                    WldMeshReference meshRef = fragments[meshRefIndex] as WldMeshReference;

                    if (meshRef?.Mesh != null) {
                        if (Meshes.All(x => x.Name != meshRef.Mesh.Name)) {
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

                Meshes = Meshes.OrderBy(x => x.Name).ToList();

                List<int> unknown = new List<int>();

                for (int i = 0; i < size2; ++i) {
                    unknown.Add(Reader.ReadInt32());
                }
            }
        }

        public void BuildSkeletonData(bool stripModelBase)
        {
            if (_hasBuiltData) {
                return;
            }

            BuildSkeletonTreeData(0, Skeleton, null, string.Empty,
                string.Empty, string.Empty, stripModelBase);
            _hasBuiltData = true;
        }

        public override void OutputInfo()
        {
            base.OutputInfo();
            GD.Print("-----");
            GD.Print("0x10: Skeleton pieces: " + Skeleton.Count);
        }

        private void AddPoseTrack(WldTrackFragment track, string pieceName)
        {
            if (!Animations.ContainsKey("pos")) {
                Animations["pos"] = new data_types.Animation();
            }

            Animations["pos"].AddTrack(track, pieceName, data_types.Animation.CleanBoneName(pieceName),
                data_types.Animation.CleanBoneAndStripBase(pieceName, ModelBase));
            track.TrackDefFragment.IsAssigned = true;
            track.IsProcessed = true;
            track.IsPoseAnimation = true;
        }

        public void AddTrackDataEquipment(WldTrackFragment track, string boneName, bool isDefault = false)
        {
            string animationName = string.Empty;
            string modelName = string.Empty;
            string pieceName = string.Empty;

            string cleanedName = FragmentNameCleaner.CleanName(track, true);

            if (isDefault) {
                animationName = "pos";
                modelName = ModelBase;
                cleanedName = cleanedName.Replace(ModelBase, String.Empty);
                pieceName = cleanedName == string.Empty ? "root" : cleanedName;
            } else {
                if (cleanedName.Length <= 3) {
                    return;
                }

                animationName = cleanedName.Substring(0, 3);
                cleanedName = cleanedName.Remove(0, 3);

                if (cleanedName.Length < 3) {
                    return;
                }

                modelName = ModelBase;
                pieceName = boneName;

                if (pieceName == string.Empty) {
                    pieceName = "root";
                }
            }

            track.SetTrackData(modelName, animationName, pieceName);

            if (Animations.ContainsKey(track.AnimationName)) {
                if (modelName == ModelBase && ModelBase != Animations[animationName].AnimModelBase) {
                    Animations.Remove(animationName);
                }

                if (modelName != ModelBase && ModelBase == Animations[animationName].AnimModelBase) {
                    return;
                }
            }

            if (!Animations.ContainsKey(track.AnimationName)) {
                Animations[track.AnimationName] = new data_types.Animation();
            }

            Animations[track.AnimationName].AddTrack(track, track.PieceName, data_types.Animation.CleanBoneName(track.PieceName),
                data_types.Animation.CleanBoneAndStripBase(track.PieceName, ModelBase));
            track.TrackDefFragment.IsAssigned = true;
            track.IsProcessed = true;
        }

        public void AddTrackData(WldTrackFragment track, bool isDefault = false)
        {
            string animationName = string.Empty;
            string modelName = string.Empty;
            string pieceName = string.Empty;

            string cleanedName = FragmentNameCleaner.CleanName(track, true);

            if (isDefault) {
                animationName = "pos";
                modelName = ModelBase;
                cleanedName = cleanedName.Replace(ModelBase, String.Empty);
                pieceName = cleanedName == string.Empty ? "root" : cleanedName;
            } else {
                if (cleanedName.Length <= 3) {
                    return;
                }

                animationName = cleanedName.Substring(0, 3);
                cleanedName = cleanedName.Remove(0, 3);

                if (cleanedName.Length < 3) {
                    return;
                }

                modelName = cleanedName.Substring(0, 3);
                cleanedName = cleanedName.Remove(0, 3);
                pieceName = cleanedName;

                if (pieceName == string.Empty) {
                    pieceName = "root";
                }
            }

            track.SetTrackData(modelName, animationName, pieceName);

            if (Animations.ContainsKey(track.AnimationName)) {
                if (modelName == ModelBase && ModelBase != Animations[animationName].AnimModelBase) {
                    Animations.Remove(animationName);
                }

                if (modelName != ModelBase && ModelBase == Animations[animationName].AnimModelBase) {
                    return;
                }
            }

            if (!Animations.ContainsKey(track.AnimationName)) {
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

            if (bone.Name != string.Empty) {
                runningIndex += bone.Index + "/";
            }

            runningName += bone.Name;
            runningNameCleaned += bone.CleanedName;

            bone.FullPath = runningName;
            bone.CleanedFullPath = runningNameCleaned;

            if (bone.Children.Count == 0) {
                return;
            }

            runningName += "/";
            runningNameCleaned += "/";

            foreach (var childNode in bone.Children) {
                BuildSkeletonTreeData(childNode, treeNodes, bone, runningName, runningNameCleaned, runningIndex,
                    stripModelBase);
            }
        }

        private string CleanBoneName(string nodeName, bool stripModelBase)
        {
            nodeName = nodeName.Replace("_DAG", "");
            nodeName = nodeName.ToLower();
            if (stripModelBase) {
                nodeName = nodeName.Replace(ModelBase, string.Empty);
            }

            nodeName += nodeName.Length == 0 ? "root" : string.Empty;
            return nodeName;
        }

        public void AddAdditionalMesh(WldMesh mesh)
        {
            if (Meshes.Any(x => x.Name == mesh.Name)
                /* || SecondaryMeshes.Any(x => x.Name == mesh.Name) */) {
                return;
            }

            if (mesh.MobPieces.Count == 0) {
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

            if (trackName == ModelBase) {
                boneName = ModelBase;
                return true;
            }

            foreach (var bone in Skeleton) {
                string cleanBoneName = bone.Name.Replace("_DAG", string.Empty).ToLower();
                if (cleanBoneName == track) {
                    boneName = bone.Name.ToLower();
                    return true;
                }
            }

            boneName = string.Empty;
            return false;
        }

        public Transform3D GetBoneMatrix(int boneIndex, string animName, int frame)
        {
            if (!Animations.ContainsKey(animName)) {
                return Transform3D.Identity;
            }

            if (frame < 0 || frame >= Animations[animName].FrameCount) {
                return Transform3D.Identity;
            }

            var currentBone = Skeleton[boneIndex];

            Transform3D boneMatrix = Transform3D.Identity;

            while (currentBone != null) {
                if (!Animations[animName].TracksCleanedStripped.ContainsKey(currentBone.CleanedName)) {
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

                if (currentBone != null) {
                    boneIndex = currentBone.Index;
                }
            }

            return boneMatrix;
        }

        public void RenameNodeBase(string newBase)
        {
            foreach (var node in Skeleton) {
                node.Name = node.Name.Replace(ModelBase.ToUpper(), newBase.ToUpper());
            }

            var newNameMapping = new Dictionary<int, string>();
            foreach (var node in BoneMapping) {
                newNameMapping[node.Key] = node.Value.Replace(ModelBase.ToUpper(), newBase.ToUpper());
            }

            BoneMapping = newNameMapping;

            ModelBase = newBase;
        }
    }
}
