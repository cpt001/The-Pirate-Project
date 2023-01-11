using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Builder;
using Broccoli.Generator;
using Broccoli.Manager;

namespace Broccoli.Factory
{
    using Pipeline = Broccoli.Pipe.Pipeline;
    public class SproutSubfactory {
        #region Vars
        /// <summary>
        /// Internal TreeFactory instance to create branches. 
        /// It must be provided from a parent TreeFactory when initializing this subfactory.
        /// </summary>
        public TreeFactory treeFactory = null;
        /// <summary>
        /// Branch descriptor collection to handle values.
        /// </summary>
        BranchDescriptorCollection branchDescriptorCollection = null;
        /// <summary>
        /// Selected branch descriptor index.
        /// </summary>
        public int branchDescriptorIndex = 0;
        /// <summary>
        /// Saves the branch structure levels on the loaded pipeline.
        /// </summary>
        List<StructureGenerator.StructureLevel> branchLevels = new List<StructureGenerator.StructureLevel> ();
        /// <summary>
        /// Saves the sprout A structure levels on the loaded pipeline.
        /// </summary>
        List<StructureGenerator.StructureLevel> sproutALevels = new List<StructureGenerator.StructureLevel> ();
        /// <summary>
        /// Saves the sprout B structure levels on the loaded pipeline.
        /// </summary>
        List<StructureGenerator.StructureLevel> sproutBLevels = new List<StructureGenerator.StructureLevel> ();
        /// <summary>
        /// Saves the sprout mesh instances representing sprout groups.
        /// </summary>
        List<SproutMesh> sproutMeshes = new List<SproutMesh> ();
        /// <summary>
        /// Branch mapper element to set branch textures.
        /// </summary>
        BranchMapperElement branchMapperElement = null;
        /// <summary>
        /// Branch girth element to set branch girth.
        /// </summary>
        GirthTransformElement girthTransformElement = null;
        /// <summary>
        /// Sprout mapper element to set sprout textures.
        /// </summary>
        SproutMapperElement sproutMapperElement = null;
        /// <summary>
        /// Branch bender element to set branch noise.
        /// </summary>
        BranchBenderElement branchBenderElement = null;
        /// <summary>
        /// Number of branch levels available on the pipeline.
        /// </summary>
        /// <value>Count of branch levels.</value>
        public int branchLevelCount { get; private set; }
        /// <summary>
        /// Number of sprout levels available on the pipeline.
        /// </summary>
        /// <value>Count of sprout levels.</value>
        public int sproutLevelCount { get; private set; }
        /// <summary>
        /// Enum describing the possible materials to apply to a preview.
        /// </summary>
        public enum MaterialMode {
            Composite,
            Albedo,
            Normals,
            Extras,
            Subsurface
        }
        #endregion

        #region Texture Vars
        TextureManager textureManager;
        #endregion

        #region Initialization and Termination
        /// <summary>
        /// Initializes the subfactory instance.
        /// </summary>
        /// <param name="treeFactory">TreeFactory instance to use to produce branches.</param>
        public void Init (TreeFactory treeFactory) {
            this.treeFactory = treeFactory;
            if (textureManager != null) {
                textureManager.Clear ();
            }
            textureManager = new TextureManager ();
        }
        /// <summary>
        /// Check if there is a valid tree factory assigned to this sprout factory.
        /// </summary>
        /// <returns>True is there is a valid TreeFactory instance.</returns>
        public bool HasValidTreeFactory () {
            return treeFactory != null;
        }
        /// <summary>
        /// Clears data from this instance.
        /// </summary>
        public void Clear () {
            treeFactory = null;
            branchLevels.Clear ();
            sproutALevels.Clear ();
            sproutBLevels.Clear ();
            sproutMeshes.Clear ();
            branchMapperElement = null;
            girthTransformElement = null;
            sproutMapperElement = null;
            branchBenderElement = null;
            textureManager.Clear ();
        }
        #endregion

        #region Pipeline Load and Analysis
        /// <summary>
        /// Loads a Broccoli pipeline to process branches.
        /// The branch is required to have from 1 to 3 hierarchy levels of branch nodes.
        /// </summary>
        /// <param name="pipeline">Pipeline to load on this subfactory.</param>
        /// <param name="pathToAsset">Path to the asset.</param>
        public void LoadPipeline (Pipeline pipeline, BranchDescriptorCollection branchDescriptorCollection, string pathToAsset) {
            if (treeFactory != null) {
                treeFactory.UnloadAndClearPipeline ();
                treeFactory.LoadPipeline (pipeline.Clone (), pathToAsset, true , true);
                AnalyzePipeline ();
                this.branchDescriptorCollection = branchDescriptorCollection;
                ProcessTextures ();
            }
        }
        /// <summary>
        /// Analyzes the loaded pipeline to index the branch and sprout levels to modify using the
        /// BranchDescriptor instance values.
        /// </summary>
        void AnalyzePipeline () {
            branchLevelCount = 0;
            sproutLevelCount = 0;
            branchLevels.Clear ();
            sproutALevels.Clear ();
            sproutBLevels.Clear ();
            sproutMeshes.Clear ();
            // Get structures for branches and sprouts.
            StructureGeneratorElement structureGeneratorElement = 
                (StructureGeneratorElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.StructureGenerator);
            AnalyzePipelineStructure (structureGeneratorElement.rootStructureLevel);
            // Get sprout meshes.
            SproutMeshGeneratorElement sproutMeshGeneratorElement = 
                (SproutMeshGeneratorElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.SproutMeshGenerator);
            if (sproutMeshGeneratorElement != null) {
                for (int i = 0; i < sproutMeshGeneratorElement.sproutMeshes.Count; i++) {
                    sproutMeshes.Add (sproutMeshGeneratorElement.sproutMeshes [i]);
                }
            }
            // Get the branch mapper to set textures for branches.
            branchMapperElement = 
                (BranchMapperElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.BranchMapper);
            girthTransformElement = 
                (GirthTransformElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.GirthTransform);
            sproutMapperElement = 
                (SproutMapperElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.SproutMapper);
            branchBenderElement = 
                (BranchBenderElement)treeFactory.localPipeline.GetElement (PipelineElement.ClassType.BranchBender);
        }
        void AnalyzePipelineStructure (StructureGenerator.StructureLevel structureLevel) {
            if (!structureLevel.isSprout) {
                // Add branch structure level.
                branchLevels.Add (structureLevel);
                branchLevelCount++;
                // Add sprout A structure level.
                StructureGenerator.StructureLevel sproutStructureLevel = 
                    structureLevel.GetFirstSproutStructureLevel ();
                if (sproutStructureLevel != null) {
                    sproutALevels.Add (sproutStructureLevel);
                    sproutLevelCount++;
                }
                // Add sprout B structure level.
                sproutStructureLevel = structureLevel.GetSproutStructureLevel (1);
                if (sproutStructureLevel != null) {
                    sproutBLevels.Add (sproutStructureLevel);
                }
                // Send the next banch structure level to analysis if found.
                StructureGenerator.StructureLevel branchStructureLevel = 
                    structureLevel.GetFirstBranchStructureLevel ();
                if (branchStructureLevel != null) {
                    AnalyzePipelineStructure (branchStructureLevel);                    
                }
            }
        }
        public void UnloadPipeline () {
            if (treeFactory != null) {
                treeFactory.UnloadAndClearPipeline ();
            }
        }
        public void GeneratePreview () {
            treeFactory.ProcessPipelinePreview ();
            if (GlobalSettings.showSproutLabTreeFactoryInHierarchy) {
                treeFactory.previewTree.obj.SetActive (true);
            } else {
                treeFactory.previewTree.obj.SetActive (false);
            }
            BranchDescriptor branchDescriptor = branchDescriptorCollection.branchDescriptors [branchDescriptorIndex];
            branchDescriptor.seed = treeFactory.localPipeline.seed;
        }
        public void RegeneratePreview (MaterialMode materialMode = MaterialMode.Composite) {
            BranchDescriptor branchDescriptor = branchDescriptorCollection.branchDescriptors [branchDescriptorIndex];
            treeFactory.localPipeline.seed = branchDescriptor.seed;
            treeFactory.ProcessPipelinePreview (null, true, true);
            if (GlobalSettings.showSproutLabTreeFactoryInHierarchy) {
                treeFactory.previewTree.obj.SetActive (true);
            } else {
                treeFactory.previewTree.obj.SetActive (false);
            }
            MeshRenderer meshRenderer = treeFactory.previewTree.obj.GetComponent<MeshRenderer>();
            Material[] compositeMaterials = compositeMaterials = meshRenderer.sharedMaterials;
            if (materialMode == MaterialMode.Albedo) { // Albedo
                meshRenderer.sharedMaterials = GetAlbedoMaterials (compositeMaterials,
                    branchDescriptorCollection.colorTintA,
                    branchDescriptorCollection.colorTintB,
                    SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
                    SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Normals) { // Normals
                meshRenderer.sharedMaterials = GetNormalMaterials (compositeMaterials);
            } else if (materialMode == MaterialMode.Extras) { // Extras
                meshRenderer.sharedMaterials = GetExtraMaterials (compositeMaterials,
                    branchDescriptorCollection.metallicA, branchDescriptorCollection.glossinessA,
                    branchDescriptorCollection.metallicB, branchDescriptorCollection.glossinessB,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Subsurface) { // Subsurface
                meshRenderer.sharedMaterials = GetSubsurfaceMaterials (compositeMaterials,
                    branchDescriptorCollection.subsurfaceColorA, branchDescriptorCollection.subsurfaceColorB,
                    branchDescriptorCollection.colorTintA, branchDescriptorCollection.colorTintB,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            } else if (materialMode == MaterialMode.Composite) { // Composite
                meshRenderer.sharedMaterials = GetCompositeMaterials (compositeMaterials,
                    branchDescriptorCollection.colorTintA, branchDescriptorCollection.colorTintB,
                    GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
            }
        }
        #endregion

        #region Pipeline Reflection
        public void BranchDescriptorCollectionToPipeline () {
            BranchDescriptor.BranchLevelDescriptor branchLD;
            StructureGenerator.StructureLevel branchSL;
            BranchDescriptor.SproutLevelDescriptor sproutALD;
            StructureGenerator.StructureLevel sproutASL;
            BranchDescriptor.SproutLevelDescriptor sproutBLD;
            StructureGenerator.StructureLevel sproutBSL;

            BranchDescriptor branchDescriptor = branchDescriptorCollection.branchDescriptors [branchDescriptorIndex];

            // Set seed.
            treeFactory.localPipeline.seed = branchDescriptor.seed;

            // Update branch girth.
            if (girthTransformElement != null) {
                girthTransformElement.minGirthAtBase = branchDescriptor.girthAtBase;
                girthTransformElement.maxGirthAtBase = branchDescriptor.girthAtBase;
                girthTransformElement.minGirthAtTop = branchDescriptor.girthAtTop;
                girthTransformElement.maxGirthAtTop = branchDescriptor.girthAtTop;
            }
            // Update branch noise.
            if (branchBenderElement) {
                branchBenderElement.noiseAtBase = branchDescriptor.noiseAtBase;
                branchBenderElement.noiseAtTop = branchDescriptor.noiseAtTop;
                branchBenderElement.noiseScaleAtBase = branchDescriptor.noiseScaleAtBase;
                branchBenderElement.noiseScaleAtTop = branchDescriptor.noiseScaleAtTop;
            }
            // Update branch descriptor active levels.
            for (int i = 0; i < branchLevels.Count; i++) {
                if (i <= branchDescriptor.activeLevels) {
                    branchLevels [i].enabled = true;
                } else {
                    branchLevels [i].enabled = false;
                }
            }
            // Update branch level descriptors.
            for (int i = 0; i < branchDescriptor.branchLevelDescriptors.Count; i++) {
                if (i < branchLevelCount) {
                    branchLD = branchDescriptor.branchLevelDescriptors [i];
                    branchSL = branchLevels [i];
                    // Pass Values.
                    branchSL.minFrequency = branchLD.minFrequency;
                    branchSL.maxFrequency = branchLD.maxFrequency;
                    branchSL.minLengthAtBase = branchLD.minLengthAtBase;
                    branchSL.maxLengthAtBase = branchLD.maxLengthAtBase;
                    branchSL.minLengthAtTop = branchLD.minLengthAtTop;
                    branchSL.maxLengthAtTop = branchLD.maxLengthAtTop;
                    branchSL.minParallelAlignAtBase = branchLD.minParallelAlignAtBase;
                    branchSL.maxParallelAlignAtBase = branchLD.maxParallelAlignAtBase;
                    branchSL.minParallelAlignAtTop = branchLD.minParallelAlignAtTop;
                    branchSL.maxParallelAlignAtTop = branchLD.maxParallelAlignAtTop;
                    branchSL.minGravityAlignAtBase = branchLD.minGravityAlignAtBase;
                    branchSL.maxGravityAlignAtBase = branchLD.maxGravityAlignAtBase;
                    branchSL.minGravityAlignAtTop = branchLD.minGravityAlignAtTop;
                    branchSL.maxGravityAlignAtTop = branchLD.maxGravityAlignAtTop;
                }
            }
            // Update branch mapping textures.
            if (branchMapperElement != null) {
                branchMapperElement.mainTexture = branchDescriptorCollection.branchAlbedoTexture;
                branchMapperElement.normalTexture = branchDescriptorCollection.branchNormalTexture;
                branchMapperElement.mappingYDisplacement = branchDescriptorCollection.branchTextureYDisplacement;
            }
            // Update sprout A level descriptors.
            for (int i = 0; i < branchDescriptor.sproutALevelDescriptors.Count; i++) {
                if (i < branchLevelCount) {
                    sproutALD = branchDescriptor.sproutALevelDescriptors [i];
                    sproutASL = sproutALevels [i];
                    // Pass Values.
                    sproutASL.enabled = sproutALD.isEnabled;
                    sproutASL.minFrequency = sproutALD.minFrequency;
                    sproutASL.maxFrequency = sproutALD.maxFrequency;
                    sproutASL.minParallelAlignAtBase = sproutALD.minParallelAlignAtBase;
                    sproutASL.maxParallelAlignAtBase = sproutALD.maxParallelAlignAtBase;
                    sproutASL.minParallelAlignAtTop = sproutALD.minParallelAlignAtTop;
                    sproutASL.maxParallelAlignAtTop = sproutALD.maxParallelAlignAtTop;
                    sproutASL.minGravityAlignAtBase = sproutALD.minGravityAlignAtBase;
                    sproutASL.maxGravityAlignAtBase = sproutALD.maxGravityAlignAtBase;
                    sproutASL.minGravityAlignAtTop = sproutALD.minGravityAlignAtTop;
                    sproutASL.maxGravityAlignAtTop = sproutALD.maxGravityAlignAtTop;
                    sproutASL.flipSproutAlign = branchDescriptor.sproutAFlipAlign;
                    sproutASL.actionRangeEnabled = true;
                    sproutASL.minRange = sproutALD.minRange;
                    sproutASL.maxRange = sproutALD.maxRange;
                }
            }
            // Update sprout A properties.
            if (sproutMeshes.Count > 0) {
                sproutMeshes [0].width = branchDescriptor.sproutASize;
                sproutMeshes [0].scaleAtBase = branchDescriptor.sproutAScaleAtBase;
                sproutMeshes [0].scaleAtTop = branchDescriptor.sproutAScaleAtTop;
            }
            // Update sprout mapping textures.
            if (sproutMapperElement != null) {
                sproutMapperElement.sproutMaps [0].colorVarianceMode = SproutMap.ColorVarianceMode.Shades;
                sproutMapperElement.sproutMaps [0].minColorShade = branchDescriptorCollection.minColorShadeA;
                sproutMapperElement.sproutMaps [0].maxColorShade = branchDescriptorCollection.maxColorShadeA;
                sproutMapperElement.sproutMaps [0].colorTintEnabled = true;
                sproutMapperElement.sproutMaps [0].colorTint = branchDescriptorCollection.colorTintA;
                sproutMapperElement.sproutMaps [0].minColorTint = branchDescriptorCollection.minColorTintA;
                sproutMapperElement.sproutMaps [0].maxColorTint = branchDescriptorCollection.maxColorTintA;
                sproutMapperElement.sproutMaps [0].metallic = branchDescriptorCollection.metallicA;
                sproutMapperElement.sproutMaps [0].glossiness = branchDescriptorCollection.glossinessA;
                sproutMapperElement.sproutMaps [0].subsurfaceColor = branchDescriptorCollection.subsurfaceColorA;
                sproutMapperElement.sproutMaps [0].sproutAreas.Clear ();
                for (int i = 0; i < branchDescriptorCollection.sproutAMapAreas.Count; i++) {
                    SproutMap.SproutMapArea sma = branchDescriptorCollection.sproutAMapAreas [i].Clone ();
                    sma.texture = GetSproutTexture (0, i);
                    sproutMapperElement.sproutMaps [0].sproutAreas.Add (sma);
                }
            }
            // Update sprout B level descriptors.
            for (int i = 0; i < branchDescriptor.sproutBLevelDescriptors.Count; i++) {
                if (i < branchLevelCount) {
                    sproutBLD = branchDescriptor.sproutBLevelDescriptors [i];
                    sproutBSL = sproutBLevels [i];
                    // Pass Values.
                    sproutBSL.enabled = sproutBLD.isEnabled;
                    sproutBSL.minFrequency = sproutBLD.minFrequency;
                    sproutBSL.maxFrequency = sproutBLD.maxFrequency;
                    sproutBSL.minParallelAlignAtBase = sproutBLD.minParallelAlignAtBase;
                    sproutBSL.maxParallelAlignAtBase = sproutBLD.maxParallelAlignAtBase;
                    sproutBSL.minParallelAlignAtTop = sproutBLD.minParallelAlignAtTop;
                    sproutBSL.maxParallelAlignAtTop = sproutBLD.maxParallelAlignAtTop;
                    sproutBSL.minGravityAlignAtBase = sproutBLD.minGravityAlignAtBase;
                    sproutBSL.maxGravityAlignAtBase = sproutBLD.maxGravityAlignAtBase;
                    sproutBSL.minGravityAlignAtTop = sproutBLD.minGravityAlignAtTop;
                    sproutBSL.maxGravityAlignAtTop = sproutBLD.maxGravityAlignAtTop;
                    sproutBSL.flipSproutAlign = branchDescriptor.sproutBFlipAlign;
                    sproutBSL.actionRangeEnabled = true;
                    sproutBSL.minRange = sproutBLD.minRange;
                    sproutBSL.maxRange = sproutBLD.maxRange;
                }
            }
            // Update sprout A properties.
            if (sproutMeshes.Count > 1) {
                sproutMeshes [1].width = branchDescriptor.sproutBSize;
                sproutMeshes [1].scaleAtBase = branchDescriptor.sproutBScaleAtBase;
                sproutMeshes [1].scaleAtTop = branchDescriptor.sproutBScaleAtTop;
            }
            // Update sprout mapping textures.
            if (sproutMapperElement != null && sproutMapperElement.sproutMaps.Count > 1) {
                sproutMapperElement.sproutMaps [1].colorVarianceMode =  SproutMap.ColorVarianceMode.Shades;
                sproutMapperElement.sproutMaps [1].minColorShade = branchDescriptorCollection.minColorShadeB;
                sproutMapperElement.sproutMaps [1].maxColorShade = branchDescriptorCollection.maxColorShadeB;
                sproutMapperElement.sproutMaps [1].colorTintEnabled = true;
                sproutMapperElement.sproutMaps [1].colorTint = branchDescriptorCollection.colorTintB;
                sproutMapperElement.sproutMaps [1].minColorTint = branchDescriptorCollection.minColorTintB;
                sproutMapperElement.sproutMaps [1].maxColorTint = branchDescriptorCollection.maxColorTintB;
                sproutMapperElement.sproutMaps [1].metallic = branchDescriptorCollection.metallicB;
                sproutMapperElement.sproutMaps [1].glossiness = branchDescriptorCollection.glossinessB;
                sproutMapperElement.sproutMaps [1].subsurfaceColor = branchDescriptorCollection.subsurfaceColorB;
                sproutMapperElement.sproutMaps [1].sproutAreas.Clear ();
                for (int i = 0; i < branchDescriptorCollection.sproutBMapAreas.Count; i++) {
                    SproutMap.SproutMapArea sma = branchDescriptorCollection.sproutBMapAreas [i].Clone ();
                    sma.texture = GetSproutTexture (1, i);
                    sproutMapperElement.sproutMaps [1].sproutAreas.Add (sma);
                }
            }
        }
        #endregion

        #region Texture Processing
        public bool GenerateSnapshopTextures (int snapshotIndex, BranchDescriptorCollection branchDescriptorCollection,
            int width, int height,
            string albedoPath, string normalPath, string extrasPath, string subsurfacePath, string compositePath) {
            BeginSnapshotProgress (branchDescriptorCollection);
            // ALBEDO
            if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) {
                ReportProgress ("Processing albedo texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Albedo, 
                    width,
                    height,
                    albedoPath);
                ReportProgress ("Processing albedo texture.", 20f);
            }
            // NORMALS
            if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) {
                ReportProgress ("Processing normal texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex,
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Normals, 
                    width,
                    height,
                    normalPath);
                ReportProgress ("Processing normal texture.", 20f);
            }
            // EXTRAS
            if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) {
                ReportProgress ("Processing extras texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Extras, 
                    width,
                    height,
                    extrasPath);
                ReportProgress ("Processing extras texture.", 20f);
            }
            // SUBSURFACE
            if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) {
                ReportProgress ("Processing subsurface texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Subsurface, 
                    width,
                    height,
                    subsurfacePath);
                ReportProgress ("Processing subsurface texture.", 20f);
            }
            // COMPOSITE
            if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) {
                ReportProgress ("Processing composite texture.", 0f);
                GenerateSnapshopTexture (
                    branchDescriptorCollection.branchDescriptorIndex, 
                    branchDescriptorCollection,
                    SproutSubfactory.MaterialMode.Composite, 
                    width,
                    height,
                    compositePath);
                ReportProgress ("Processing composite texture.", 20f);
            }
            FinishSnapshotProgress ();
            
            // Cleanup.
            MeshFilter meshFilter = treeFactory.previewTree.obj.GetComponent<MeshFilter>();
            Object.DestroyImmediate (meshFilter.sharedMesh);

            return true;
        }
        /// <summary>
        /// Generates the texture for a giver snapshot.
        /// </summary>
        /// <param name="snapshotIndex">Index for the snapshot.</param>
        /// <param name="materialMode">Mode mode: composite, albedo, normals, extras or subsurface.</param>
        /// <param name="width">Maximum width for the texture.</param>
        /// <param name="height">Maximum height for the texture.</param>
        /// <param name="texturePath">Path to save the texture.</param>
        /// <returns>Texture generated.</returns>
        public Texture2D GenerateSnapshopTexture (
            int snapshotIndex, 
            BranchDescriptorCollection branchDescriptorCollection, 
            MaterialMode materialMode, 
            int width, 
            int height, 
            string texturePath = "") 
        {
            if (snapshotIndex >= branchDescriptorCollection.branchDescriptors.Count) {
                Debug.LogWarning ("Could not generate branch snapshot texture. Index out of range.");
            } else {
                // Regenerate branch mesh and apply material mode.
                branchDescriptorIndex = snapshotIndex;
                RegeneratePreview (materialMode);
                // Build and save texture.
                TextureBuilder tb = new TextureBuilder ();
                if (materialMode == MaterialMode.Normals) {
                    tb.backgroundColor = new Color (0.5f, 0.5f, 1f, 1f);
                    tb.textureFormat = TextureFormat.RGB24;
                } else if (materialMode == MaterialMode.Subsurface) {
                    tb.backgroundColor = new Color (0f, 0f, 0f, 1f);
                    tb.textureFormat = TextureFormat.RGB24;
                } else if (materialMode == MaterialMode.Extras) {
                    tb.backgroundColor = new Color (0f, 0f, 1f, 1f);
                    tb.textureFormat = TextureFormat.RGB24;
                }
                // Get tree mesh.
                GameObject previewTree = treeFactory.previewTree.obj;
                tb.useTextureSizeToTargetRatio = true;
                tb.BeginUsage (previewTree);
                tb.textureSize = new Vector2 (width, height);
                Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up, texturePath);
                tb.EndUsage ();
                return sproutTexture;
            }
            return null;
        }
        public bool GenerateAtlasTexture (BranchDescriptorCollection branchDescriptorCollection, int width, int height, int padding,
            string albedoPath, string normalPath, string extrasPath, string subsurfacePath, string compositePath) 
        {
            #if UNITY_EDITOR
            if (branchDescriptorCollection.branchDescriptors.Count == 0) {
                Debug.LogWarning ("Could not generate atlas texture, no branch snapshots were found.");
            } else {
                // 1. Generate each snapshot mesh.
                float largestMeshSize = 0f; 
                List<Mesh> meshes = new List<Mesh> (); // Save the mesh for each snapshot.
                List<Material[]> materials = new List<Material[]> ();
                List<Texture2D> texturesForAtlas = new List<Texture2D> ();
                Material[] modeMaterials;
                TextureBuilder tb = new TextureBuilder ();
                Texture2D atlas;
                tb.useTextureSizeToTargetRatio = true;

                double editorTime = UnityEditor.EditorApplication.timeSinceStartup;

                BeginAtlasProgress (branchDescriptorCollection);

                MeshFilter meshFilter = treeFactory.previewTree.obj.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = treeFactory.previewTree.obj.GetComponent<MeshRenderer>();
                for (int i = 0; i < branchDescriptorCollection.branchDescriptors.Count; i++) {
                    ReportProgress ("Creating mesh for snapshot " + i + ".", 0f);
                    branchDescriptorIndex = i;
                    BranchDescriptorCollectionToPipeline ();
                    RegeneratePreview ();
                    meshes.Add (Object.Instantiate (meshFilter.sharedMesh));
                    materials.Add (meshRenderer.sharedMaterials);
                    ReportProgress ("Creating mesh for snapshot " + i + ".", 10f);
                }

                // 2. Get the larger snapshot.
                for (int i = 0; i < meshes.Count; i++) {
                    if (meshes [i].bounds.max.magnitude > largestMeshSize) {
                        largestMeshSize = meshes [i].bounds.max.magnitude;
                    }
                }

                // TODO: check for destroyed meshes.

                // Generate each mode texture.
                GameObject previewTree = treeFactory.previewTree.obj;

                // ALBEDO
                if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating albedo texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.1 Albedo.
                        modeMaterials = GetAlbedoMaterials (materials [i],
                            branchDescriptorCollection.colorTintA,
                            branchDescriptorCollection.colorTintB,
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0.5f, 0.5f, 0.5f, 0f);
                        tb.textureFormat = TextureFormat.RGBA32;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating albedo texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating albedo atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, albedoPath);
                    CleanTextures (texturesForAtlas);
                    Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating albedo atlas texture.", 10f);
                }

                // NORMALS
                if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating normal texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.2 Normals.
                        modeMaterials = GetNormalMaterials (materials [i]);
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0.5f, 0.5f, 1f, 1f);
                        tb.textureFormat = TextureFormat.RGB24;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating extra texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating normal atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, normalPath);
                    CleanTextures (texturesForAtlas);
                    Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating normal atlas texture.", 10f);
                }

                // EXTRAS
                if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating extras texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.3 Extra.
                        modeMaterials = GetExtraMaterials (materials [i],
                            branchDescriptorCollection.metallicA, branchDescriptorCollection.glossinessA,
                            branchDescriptorCollection.metallicB, branchDescriptorCollection.glossinessB,
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0f, 0f, 1f, 1f);
                        tb.textureFormat = TextureFormat.RGB24;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating extras texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating extras atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, extrasPath);
                    CleanTextures (texturesForAtlas);
                    Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating extras atlas texture.", 10f);
                }

                // SUBSURFACE
                if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating subsurface texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.4 Subsurface.
                        modeMaterials = GetSubsurfaceMaterials (materials [i],
                            branchDescriptorCollection.subsurfaceColorA, branchDescriptorCollection.subsurfaceColorB,
                            branchDescriptorCollection.colorTintA, branchDescriptorCollection.colorTintB,
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0f, 0f, 0f, 1f);
                        tb.textureFormat = TextureFormat.RGB24;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating subsurface texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating subsurface atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, subsurfacePath);
                    CleanTextures (texturesForAtlas);
                    Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating subsurface atlas texture.", 10f);
                }

                // COMPOSITE
                if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) {
                    for (int i = 0; i < meshes.Count; i++) {
                        ReportProgress ("Creating composite texture for snapshot " + i + ".", 0f);
                        // 3. Get the texture scale for the texture.
                        float meshScale = meshes [i].bounds.max.magnitude / largestMeshSize;
                        // 4.5 Composite.
                        modeMaterials = materials [i];
                        /*
                        GetCompositeMaterials (materials [i],
                            GetMaterialAStartIndex (branchDescriptorCollection), GetMaterialBStartIndex (branchDescriptorCollection));
                            */
                        meshFilter.sharedMesh = meshes [i];
                        meshRenderer.sharedMaterials = modeMaterials;
                        tb.backgroundColor = new Color (0.5f, 0.5f, 0.5f, 0f);
                        tb.textureFormat = TextureFormat.RGBA32;
                        tb.BeginUsage (previewTree);
                        tb.textureSize = new Vector2 (width * meshScale, height * meshScale);
                        Texture2D sproutTexture = tb.GetTexture (new Plane (Vector3.right, Vector3.zero), Vector3.up);
                        texturesForAtlas.Add (sproutTexture);
                        tb.EndUsage ();
                        DestroyMaterials (modeMaterials);
                        ReportProgress ("Creating composite texture for snapshot " + i + ".", 20f);
                    }
                    ReportProgress ("Creating composite atlas texture.", 0f);
                    atlas = new Texture2D (2, 2);
                    atlas.PackTextures (texturesForAtlas.ToArray (), padding, width, false);
                    atlas.alphaIsTransparency = true;
                    SaveTextureToFile (atlas, compositePath);
                    CleanTextures (texturesForAtlas);
                    Object.DestroyImmediate (atlas);
                    ReportProgress ("Creating composite atlas texture.", 10f);
                }

                // Cleanup, destroy meshes, materials and textures.
                for (int i = 0; i < meshes.Count; i++) {
                    Object.DestroyImmediate (meshes [i]);
                }
                for (int i = 0; i < materials.Count; i++) {
                    for (int j = 0; j < materials [i].Length; j++) {
                        Object.DestroyImmediate (materials [i][j]);   
                    }
                }
                FinishAtlasProgress ();
                return true;
            }
            #endif
            return false;
        }
        public Texture2D GetSproutTexture (int group, int index) {
            string textureId = GetSproutTextureId (group, index);
            return textureManager.GetTexture (textureId);
        }
        Texture2D GetOriginalSproutTexture (int group, int index) {
            Texture2D texture = null;
            List<SproutMap.SproutMapArea> sproutMapAreas = null;
            if (group == 0) {
                sproutMapAreas = branchDescriptorCollection.sproutAMapAreas;
            } else if (group == 1) {
                sproutMapAreas = branchDescriptorCollection.sproutBMapAreas;
            }
            if (sproutMapAreas != null && sproutMapAreas.Count >= index) {
                texture = sproutMapAreas[index].texture;
            }
            return texture;
        }
        public void ProcessTextures () {
            textureManager.Clear ();
            string textureId;
            // Process Sprout A albedo textures.
            for (int i = 0; i < branchDescriptorCollection.sproutAMapAreas.Count; i++) {    
                Texture2D texture = ApplyTextureTransformations (
                    branchDescriptorCollection.sproutAMapAreas [i].texture, 
                    branchDescriptorCollection.sproutAMapDescriptors [i].alphaFactor);
                if (texture != null) {
                    textureId = GetSproutTextureId (0, i);
                    textureManager.AddOrReplaceTexture (textureId, texture);
                }
            }
            // Process Sprout B albedo textures.    
            for (int i = 0; i < branchDescriptorCollection.sproutBMapAreas.Count; i++) {
                Texture2D texture = ApplyTextureTransformations (
                    branchDescriptorCollection.sproutBMapAreas [i].texture, 
                    branchDescriptorCollection.sproutBMapDescriptors [i].alphaFactor);
                if (texture != null) {
                    textureId = GetSproutTextureId (1, i);
                    textureManager.AddOrReplaceTexture (textureId, texture);
                }
            }
        }
        public void ProcessTexture (int group, int index, float alpha) {
            #if UNITY_EDITOR
            string textureId = GetSproutTextureId (group, index);
            //if (textureManager.HasTexture (textureId)) {
                Texture2D originalTexture = GetOriginalSproutTexture (group, index);
                Texture2D newTexture = ApplyTextureTransformations (originalTexture, alpha);
                newTexture.alphaIsTransparency = true;
                textureManager.AddOrReplaceTexture (textureId, newTexture, true);
                BranchDescriptorCollectionToPipeline ();
            //}
            #endif
        }
        Texture2D ApplyTextureTransformations (Texture2D originTexture, float alpha) {
            if (originTexture != null) {
                return textureManager.GetCopy (originTexture, alpha);
            }
            return null;
        }
        public string GetSproutTextureId (int group, int index) {
            return  "sprout_" + group + "_" + index;
        }
        /// <summary>
		/// Saves a texture to a file.
		/// </summary>
		/// <param name="texture">Texture.</param>
		/// <param name="filename">Filename.</param>
		public void SaveTextureToFile (Texture2D texture, string filename, bool importAsset = true) {
			#if UNITY_EDITOR
			System.IO.File.WriteAllBytes (filename, texture.EncodeToPNG());
            if (importAsset)
                UnityEditor.AssetDatabase.ImportAsset (filename);
			#endif
		}
        void CleanTextures (List<Texture2D> texturesToClean) {
            for (int i = 0; i < texturesToClean.Count; i++) {
                Object.DestroyImmediate (texturesToClean [i]);
            }
            texturesToClean.Clear ();
        }
        #endregion

        #region Material Processing
        public Material[] GetCompositeMaterials (Material[] originalMaterials,
            Color tintColorA,
            Color tintColorB,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1) 
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                if (i == 0) {
                    mats[0] = originalMaterials [0];
                } else {
                    Material m = new Material (originalMaterials[i]);
                    //m.shader = Shader.Find ("Hidden/Broccoli/SproutLabComposite");
                    //m.shader = Shader.Find ("Broccoli/SproutLabComposite");
                    m.shader = GetSpeedTree8Shader ();
                    /*
                    m.EnableKeyword ("EFFECT_BUMP");
                    m.EnableKeyword ("EFFECT_SUBSURFACE");
                    m.EnableKeyword ("EFFECT_EXTRA_TEX");
                    */
                    m.EnableKeyword ("GEOM_TYPE_LEAF");
                    mats [i] = m;
                    if (i >= materialAStartIndex) {
                        if (i >= materialBStartIndex) {
                            m.SetColor ("_TintColor", tintColorB);
                        } else {
                            m.SetColor ("_TintColor", tintColorA);
                        }
                    }
                }
            }
            return mats;
        }
        public Material[] GetAlbedoMaterials (Material[] originalMaterials,
            Color tintColorA,
            Color tintColorB,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1)
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m;
                if (originalMaterials [i] == null) {
                    m = originalMaterials [i];
                } else {
                    m = new Material (originalMaterials[i]);
                    m.shader = Shader.Find ("Hidden/Broccoli/SproutLabAlbedo");
                    mats [i] = m;
                    if (i >= materialAStartIndex) {
                        if (i >= materialBStartIndex) {
                            m.SetColor ("_TintColor", tintColorB);
                        } else {
                            m.SetColor ("_TintColor", tintColorA);
                        }
                    }
                }
            }
            return mats;
        }
        public Material[] GetNormalMaterials (Material[] originalMaterials) {
            Material[] mats = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
                m.shader = Shader.Find ("Hidden/Broccoli/SproutLabNormals");
                mats [i] = m;
            }
            return mats;
        }
        public Material[] GetExtraMaterials (Material[] originalMaterials,
            float metallicA,
            float glossinessA,
            float metallicB,
            float glossinessB,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1)
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
                m.shader = Shader.Find ("Hidden/Broccoli/SproutLabExtra");
                mats [i] = m;
                if (i >= materialAStartIndex) {
                    if (i >= materialBStartIndex) {
                        m.SetFloat ("_Metallic", metallicB);
                        m.SetFloat ("_Glossiness", glossinessB);
                    } else {
                        m.SetFloat ("_Metallic", metallicA);
                        m.SetFloat ("_Glossiness", glossinessA);
                    }
                }
            }
            return mats;
        }
        public Material[] GetSubsurfaceMaterials (Material[] originalMaterials,
            Color subsurfaceColorA,
            Color subsurfaceColorB,
            Color tintColorA,
            Color tintColorB,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1) 
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
                m.shader = Shader.Find ("Hidden/Broccoli/SproutLabSubsurface");
                mats [i] = m;
                if (i >= materialAStartIndex) {
                    if (i >= materialBStartIndex) {
                        m.SetColor ("_SubsurfaceColor", subsurfaceColorB);
                        m.SetColor ("_TintColor", tintColorB);
                    } else {
                        m.SetColor ("_SubsurfaceColor", subsurfaceColorA);
                        m.SetColor ("_TintColor", tintColorA);
                    }
                }
            }
            return mats;
        }
        public Material[] GetCompositeMaterials (Material[] originalMaterials,
            int materialAStartIndex = -1,
            int materialBStartIndex = -1) 
        {
            Material[] mats = new Material[originalMaterials.Length];
            if (materialAStartIndex == -1) materialAStartIndex = 0;
            for (int i = 0; i < originalMaterials.Length; i++) {
                Material m = new Material (originalMaterials[i]);
            }
            return mats;
        }
        public static int GetMaterialAStartIndex (BranchDescriptorCollection branchDescriptorCollection) {
            return 1;
        }
        public static int GetMaterialBStartIndex (BranchDescriptorCollection branchDescriptorCollection) {
            int materialIndex = branchDescriptorCollection.sproutAMapAreas.Count + 1;
            return materialIndex;
        }
        public void DestroyMaterials (Material[] materials) {
            for (int i = 0; i < materials.Length; i++) {
                Object.DestroyImmediate (materials [i]);
            }
        }
        private Shader GetSpeedTree8Shader () {
            Shader st8Shader = null;
            var currentRenderPipeline = GraphicsSettings.renderPipelineAsset;
            if (currentRenderPipeline != null) {
                st8Shader = currentRenderPipeline.defaultSpeedTree8Shader;
            } else {
                st8Shader = Shader.Find ("Nature/SpeedTree8");
            }
            return st8Shader;
        }
        #endregion

        #region Processing Progress
        public delegate void OnReportProgress (string msg, float progress);
        public delegate void OnFinishProgress ();
        public OnReportProgress onReportProgress;
        public OnFinishProgress onFinishProgress;
        float progressGone = 0f;
        float progressToGo = 0f;
        public string progressTitle = "";
        public void BeginSnapshotProgress (BranchDescriptorCollection branchDescriptorCollection) {
            progressGone = 0f;
            progressToGo = 0f;
            if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) progressToGo += 20; // Albedo
            if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) progressToGo += 20; // Normals
            if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) progressToGo += 20; // Extras
            if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) progressToGo += 20; // Subsurface
            if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) progressToGo += 20; // Composite
            progressTitle = "Creating Snapshot Textures";
        }
        public void FinishSnapshotProgress () {
            progressGone = progressToGo;
            ReportProgress ("Finish " + progressTitle, 0f);
            onFinishProgress?.Invoke ();
        }
        public void BeginAtlasProgress (BranchDescriptorCollection branchDescriptorCollection) {
            progressGone = 0f;
            progressToGo = branchDescriptorCollection.branchDescriptors.Count * 10f;
            if ((branchDescriptorCollection.exportTexturesFlags & 1) == 1) progressToGo += 30; // Albedo
            if ((branchDescriptorCollection.exportTexturesFlags & 2) == 2) progressToGo += 30; // Normals
            if ((branchDescriptorCollection.exportTexturesFlags & 4) == 4) progressToGo += 30; // Extras
            if ((branchDescriptorCollection.exportTexturesFlags & 8) == 8) progressToGo += 30; // Subsurface
            if ((branchDescriptorCollection.exportTexturesFlags & 16) == 16) progressToGo += 30; // Composite
            progressTitle = "Creating Atlas Textures";
        }
        public void FinishAtlasProgress () {
            progressGone = progressToGo;
            ReportProgress ("Finish " + progressTitle, 0f);
            onFinishProgress?.Invoke ();
        }
        void ReportProgress (string title, float progressToAdd) {
            progressGone += progressToAdd;
            onReportProgress?.Invoke (title, progressGone/progressToGo);
        }
        #endregion
    }
}