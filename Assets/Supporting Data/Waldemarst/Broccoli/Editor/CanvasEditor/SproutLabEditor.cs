using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Utils;
using Broccoli.Factory;
using Broccoli.Catalog;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// SproutLab instance.
	/// </summary>
	public class SproutLabEditor {
		#region Vars
		public bool useTreeFactoryEditorWindow = false;
		/// <summary>
		/// Version of the SproutLabEditor.
		/// </summary>
		private static string version = "v0.1a";
		/// <summary>
		/// Mesh preview utility.
		/// </summary>
		MeshPreview meshPreview;
		SproutCatalog catalog;
		Color defaultPreviewBackgroundColor = new Color (0.35f, 0.35f, 0.35f, 1f);
		Color normalPreviewBackgroundColor = new Color(0.5f, 0.5f, 1.0f, 1.0f);
		Color extrasPreviewBackgroundColor = new Color(0f, 0f, 0f, 1.0f);
		Color subsurfacePreviewBackgroundColor = new Color(0f, 0f, 0f, 1.0f);
		Material[] currentPreviewMaterials = null;
		Material[] compositeMaterials = null;
		/// <summary>
		/// The area canvas.
		/// </summary>
		SproutAreaCanvasEditor areaCanvas = new SproutAreaCanvasEditor ();
		/// <summary>
		/// Editor persistence utility.
		/// </summary>
		EditorPersistence<BranchDescriptorCollectionSO> editorPersistence = null;
		public float freeViewZoomFactor = 5.5f;
		public Vector3 freeViewOffset = new Vector3 (-0.04f, 0.6f, -5.5f);
		public float topViewZoomFactor = 5.5f;
		public Vector3 topViewOffset = new Vector3 (-0.04f, 0.6f, -5.5f);
		public float minZoomFactor = 3f;
		public float maxZoomFactor = 9f;
		public SproutMap.SproutMapArea selectedSproutMap = null;
		int selectedSproutMapGroup = 0;
		int selectedSproutMapIndex = 0;
		public BranchDescriptorCollection.SproutMapDescriptor selectedSproutMapDescriptor = null;
		bool showLightControls = true;
		Rect currentRect;
		private EditorGUISplitView verticalSplitView;
		// Temp
		List<Vector3> _convexHullPoints = new List<Vector3> ();
		#endregion

		#region Debug Vars
		/*
		private bool debugEnabled = true;
		private bool debugShowConvexHull = true;
		private bool debugShowConvexHullPoints = true;
		*/
		#endregion

		#region Delegates and Events
		public delegate void BranchDescriptorCollectionChange (BranchDescriptorCollection branchDescriptorCollection);
		public delegate void ShowNotification (string notification);
		public BranchDescriptorCollectionChange onBeforeBranchDescriptorChange;
		public BranchDescriptorCollectionChange onBranchDescriptorChange;
		public ShowNotification onShowNotification;
		#endregion

		#region GUI Vars
		public enum ViewMode {
			Structure,
			Templates
		}
		public ViewMode viewMode = ViewMode.Structure;
		private const int PANEL_STRUCTURE = 0;
		private const int PANEL_TEXTURE = 1;
		private const int PANEL_MAPPING = 2;
		private const int PANEL_EXPORT = 3;
		private const int VIEW_COMPOSITE = 0;
		private const int VIEW_ALBEDO = 1;
		private const int VIEW_NORMALS = 2;
		private const int VIEW_EXTRAS = 3;
		private const int VIEW_SUBSURFACE = 4;
		private const int STRUCTURE_BRANCH = 0;
		private const int STRUCTURE_SPROUT_A = 1;
		private const int STRUCTURE_SPROUT_B = 2;
		private const int TEXTURE_VIEW_TEXTURE = 0;
		private const int TEXTURE_VIEW_STRUCTURE = 1;
		/// <summary>
		/// Tab titles for panel sections.
		/// </summary>
		private static GUIContent[] panelSectionOption = new GUIContent[4];
		/// <summary>
		/// Structure views: branch or leaves.
		/// </summary>
		private static GUIContent[] structureViewOptions = new GUIContent[3];
		/// <summary>
		/// Preview options GUIContent array.
		/// </summary>
		private static GUIContent[] mapViewOptions = new GUIContent[5];
		/// <summary>
		/// Displays the snapshots (variations of a branch) as a list of options.
		/// </summary>
		private static GUIContent[] snapshots;
		/// <summary>
		/// Reorderable list to use on assigning sprout A textures.
		/// </summary>
		ReorderableList sproutAMapList;
		/// <summary>
		/// Reorderable list to use on assigning sprout B textures.
		/// </summary>
		ReorderableList sproutBMapList;
		/// <summary>
		/// Width for the left column on secondary panels.
		/// </summary>
		private int secondaryPanelColumnWidth = 120;
		/// <summary>
		/// Panel section selected.
		/// </summary>
		int currentPanelSection = 0;
		/// <summary>
		/// Structure view selected.
		/// </summary>
		int currentStructureView = 0;
		/// <summary>
		/// Texture view selected.
		/// </summary>
		int currenTextureView = 0;
		/// <summary>
		/// Map view selected.
		/// </summary>
		int currentMapView = 0;
		/// <summary>
		/// Saves the vertical scroll position for the sprou lab view.
		/// </summary>
		private Vector2 mainScroll;
		/// <summary>
		/// Saves the vertical scroll position for the structure view.
		/// </summary>
		private Vector2 structurePanelScroll;
		/// <summary>
		/// Saves the vertical scroll position for the texture view.
		/// </summary>
		private Vector2 texturePanelScroll;
		private Vector2 mappingPanelScroll;
		private Vector2 exportPanelScroll;
		string[] levelOptions = new string[] {"Main Branch", "One Level", "Two Levels", "Three Levels"};
		bool[] branchFoldouts = new bool[4];
		bool[] sproutAFoldouts = new bool[4];
		bool[] sproutBFoldouts = new bool[4];
		BranchDescriptor selectedBranchDescriptor = null;
		BranchDescriptor.BranchLevelDescriptor selectedBranchLevelDescriptor;
		BranchDescriptor.SproutLevelDescriptor selectedSproutALevelDescriptor;
		BranchDescriptor.SproutLevelDescriptor selectedSproutBLevelDescriptor;
		BranchDescriptor.BranchLevelDescriptor proxyBranchLevelDescriptor = new BranchDescriptor.BranchLevelDescriptor ();
		BranchDescriptor.SproutLevelDescriptor proxySproutALevelDescriptor = new BranchDescriptor.SproutLevelDescriptor ();
		BranchDescriptor.SproutLevelDescriptor proxySproutBLevelDescriptor = new BranchDescriptor.SproutLevelDescriptor ();
		SproutMap.SproutMapArea proxySproutMap = new SproutMap.SproutMapArea ();
		BranchDescriptorCollection.SproutMapDescriptor proxySproutMapDescriptor = new BranchDescriptorCollection.SproutMapDescriptor ();
		bool sproutMapChanged = false;
		public static int catalogItemSize = 100;
		Texture2D tmpTexture = null;
		int lightAngleStep = 1;
		float lightAngleStepValue = 45f;
		string lightAngleDisplayStr = "front";
		float lightAngleToAddTime = 0.75f;
		float lightAngleToAddTimeTmp = -1f;
		Vector3 lightAngleEulerFrom = new Vector3 (0,-90,0);
		Vector3 lightAngleEulerTo = new Vector3 (0,-90,0);
		Vector2 moveToCameraDefaultDirection = new Vector2 (90, 0);
		Vector2 moveToCameraDefaultDirectionTmp;
		float moveToCameraDirectionTime = 0.35f;
		float moveToCameraDirectionTimeTmp = 0f;
		static string[] exportTextureOptions = new string[] {"Albedo", "Normals", "Extras", "Subsurface", "Composite"};
		bool showProgressBar = false;
		float progressBarProgress = 0f;
		string progressBarTitle = "";
		Rect meshPreviewRect = Rect.zero;
		#endregion

		#region Messages
		private static string MSG_MAPPING_COMPOSITE = "How the branch and leaves will look like applying the albedo, normals and extra textures to the shader.";
		private static string MSG_MAPPING_ALBEDO = "Unlit texture to apply color values per leaf or group of leaves. The final map receives per leaf tint variations if enabled.";
		private static string MSG_MAPPING_NORMALS = "Normal mapping is adjusted per leaf texture to the tangent space according to the leaf mesh rotation.";
		private static string MSG_MAPPING_EXTRA = "Metallic value on the red channel, smoothness (glossiness) value on the green channel and ambient occlusion on the blue channel.";
		private static string MSG_MAPPING_SUBSURFACE = "Mapping for subsurface values, basically how the light should pass trough the material.";
		private static string MSG_DELETE_SPROUT_MAP_TITLE = "Remove Sprout Map";
		private static string MSG_DELETE_SPROUT_MAP_MESSAGE = "Do you really want to remove this sprout mapping?";
		private static string MSG_DELETE_SPROUT_MAP_OK = "Yes";
		private static string MSG_DELETE_SPROUT_MAP_CANCEL = "No";
		private static string MSG_DELETE_BRANCH_DESC_TITLE = "Remove Branch Descriptor";
		private static string MSG_DELETE_BRANCH_DESC_MESSAGE = "Do you really want to remove this branch descriptor snapshot?";
		private static string MSG_DELETE_BRANCH_DESC_OK = "Yes";
		private static string MSG_DELETE_BRANCH_DESC_CANCEL = "No";
		private static string MSG_LOAD_CATALOG_ITEM_TITLE = "Load Sprout Template";
		private static string MSG_LOAD_CATALOG_ITEM_MESSAGE = "Do you really want to load this sprout template? (Unsaved settings will be lost).";
		private static string MSG_LOAD_CATALOG_ITEM_OK = "Yes";
		private static string MSG_LOAD_CATALOG_ITEM_CANCEL = "No";
		#endregion

		#region Target Vars
		public BranchDescriptorCollection branchDescriptorCollection = null;
		private SproutSubfactory sproutSubfactory = null;
		#endregion

		#region Constructor and Initialization
		/// <summary>
		/// Creates a new SproutLabEditor instance.
		/// </summary>
		public SproutLabEditor () {
			panelSectionOption [0] = 
				new GUIContent ("Structure", "Settings for tunning the structure of branches and leafs.");
			panelSectionOption [1] = 
				new GUIContent ("Textures", "Select the textures to apply to the branch and leaves.");
			panelSectionOption [2] = 
				new GUIContent ("Mapping", "Settings for textures and materials.");
			panelSectionOption [3] = 
				new GUIContent ("Export", "Settings to save texture files.");
			structureViewOptions [0] = 
				new GUIContent ("Branches", "Settings for branches.");
			structureViewOptions [1] = 
				new GUIContent ("Sprouts A", "Settings for A sprouts.");
			structureViewOptions [2] = 
				new GUIContent ("Sprouts B", "Settings for B sprouts.");
			mapViewOptions [0] = 
				new GUIContent ("Composite", "Composite branch preview.");
			mapViewOptions [1] = 
				new GUIContent ("Albedo", "Unlit albedo texture.");
			mapViewOptions [2] = 
				new GUIContent ("Normals", "Normal (bump) texture.");
			mapViewOptions [3] = 
				new GUIContent ("Extras", "Metallic (R), Glossiness (G), AO (B) texture.");
			mapViewOptions [4] = 
				new GUIContent ("Subsurface", "Subsurface texture.");
			OnEnable ();
		}
		public void OnEnable () {
			// Add update method.
			EditorApplication.update -= OnEditorUpdate;
			EditorApplication.update += OnEditorUpdate;
			// Init mesh preview
			if (meshPreview == null) {
				meshPreview = new MeshPreview ();
				meshPreview.showDebugInfo = false;
				meshPreview.showPivot = false;
				meshPreview.onDrawHandles += OnPreviewMeshDrawHandles;
				meshPreview.onDrawGUI += OnPreviewMeshDrawGUI;
				meshPreview.onRequiresRepaint += OnMeshPreviewRequiresRepaint;
				meshPreview.SetZoom (topViewZoomFactor);
				meshPreview.SetOffset (topViewOffset);
				meshPreview.minZoomFactor = minZoomFactor;
				meshPreview.maxZoomFactor = maxZoomFactor;
				Light light = meshPreview.GetLigthA ();
				light.lightShadowCasterMode = LightShadowCasterMode.Everything;
				light.spotAngle = 1f;
				light.color = Color.white;
				light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.VeryHigh;
				light.shadowStrength = 0.6f;
				light.shadowBias = 0;
				light.shadowNormalBias = 0f;
				light.shadows = LightShadows.Hard;
				meshPreview.SetLightA (1, Quaternion.Euler (0, 315, 0));
				lightAngleDisplayStr = "Left 45";

				//AddLightStep ();
				/*
				Color: white
				Shadow Type: Hard Shadows
				Shadow Strength: 0.6
				Shadow Resolution: Very High
				Shadow Bias: 0
				Shadow Normal Bias: 0.3
				}
				*/
			} else {
				meshPreview.Clear ();
			}
			// Init Editor Persistence.
			if (editorPersistence == null) {
				editorPersistence = new EditorPersistence<BranchDescriptorCollectionSO>();
				editorPersistence.elementName = "Branch Collection";
				editorPersistence.saveFileDefaultName = "SproutLabBranchCollection";
				editorPersistence.btnSaveAsNewElement = "Export to File";
				editorPersistence.btnLoadElement = "Import from File";
				editorPersistence.InitMessages ();
				editorPersistence.onCreateNew += OnCreateNewBranchDescriptorCollectionSO;
				editorPersistence.onLoad += OnLoadBranchDescriptorCollectionSO;
				editorPersistence.onGetElementToSave += OnGetBranchDescriptorCollectionSOToSave;
				editorPersistence.onGetElementToSaveFilePath += OnGetBranchDescriptorCollectionSOToSaveFilePath;
				editorPersistence.onSaveElement += OnSaveBranchDescriptorCollectionSO;
				editorPersistence.savePath = ExtensionManager.fullExtensionPath + GlobalSettings.pipelineSavePath;
				editorPersistence.showCreateNewEnabled = false;
				editorPersistence.showSaveCurrentEnabled = false;
			}

			if (verticalSplitView == null) {
				verticalSplitView = new EditorGUISplitView (EditorGUISplitView.Direction.Vertical, SproutFactoryEditorWindow.focusedWindow);
				//verticalSplitView.AddDynamicSplit (0.5f);
				verticalSplitView.AddDynamicSplit (0.6f);
				verticalSplitView.AddDynamicSplit (0.4f);
			}

			SetMapView (VIEW_COMPOSITE);
			ShowPreviewMesh (0);
		}
		public void OnDisable () {
			// Remove update method.
			EditorApplication.update -= OnEditorUpdate;
			Clear ();
		}
		void Clear () {
			if (meshPreview != null) {
				meshPreview.Clear ();
			}
			selectedBranchDescriptor = null;
			selectedSproutALevelDescriptor = null;
			selectedSproutBLevelDescriptor = null;
			selectedSproutMap = null;
			selectedSproutMapDescriptor = null;
			branchDescriptorCollection = null;
		}
		/// <summary>
		/// Event called when destroying this editor.
		/// </summary>
		private void OnDestroy() {
			meshPreview.Clear ();
			verticalSplitView.Clear ();
			if (meshPreview.onDrawHandles != null) {
				meshPreview.onDrawHandles -= OnPreviewMeshDrawHandles;
				meshPreview.onDrawGUI -= OnPreviewMeshDrawGUI;
			}
		}
		#endregion

		#region Branch Descriptor Processing
		public void LoadBranchDescriptorCollection (BranchDescriptorCollection branchDescriptorCollection, SproutSubfactory sproutSubfactory) {
			// Assign the current branch descriptor.
			this.branchDescriptorCollection = branchDescriptorCollection;
			if (this.branchDescriptorCollection.branchDescriptors.Count == 0) {
				branchDescriptorCollection.branchDescriptors.Add (new BranchDescriptor ());
			}
			// Creates the sprout factory to handle the branches processing.
			this.sproutSubfactory = sproutSubfactory;
			sproutSubfactory.onReportProgress -= OnReportProgress;
			sproutSubfactory.onReportProgress += OnReportProgress;
			sproutSubfactory.onFinishProgress -= OnFinishProgress;
			sproutSubfactory.onFinishProgress += OnFinishProgress;
			
			InitSnapshots ();
			SelectSnapshot (branchDescriptorCollection.branchDescriptorIndex);
			InitSproutMapLists ();
			// Prepare the internal tree factory to process the branch descriptor.
			LoadBranchDescriptorCollectionTreeFactory ();
		}
		public void UnloadBranchDescriptorCollection () {
			selectedBranchDescriptor = null;
			if (sproutSubfactory != null)
				sproutSubfactory.UnloadPipeline ();
		}
		private void LoadBranchDescriptorCollectionTreeFactory () {
			// Load Sprout Lab base pipeline.
			string pathToAsset = ExtensionManager.fullExtensionPath + GlobalSettings.templateSproutLabPipelinePath;
			pathToAsset = pathToAsset.Replace(Application.dataPath, "Assets");
			Broccoli.Pipe.Pipeline loadedPipeline =
				AssetDatabase.LoadAssetAtPath<Broccoli.Pipe.Pipeline> (pathToAsset);

			if (loadedPipeline == null) {
				throw new UnityException ("Cannot Load Pipeline: The file at the specified path '" + 
					pathToAsset + "' is no valid save file as it does not contain a Pipeline.");
			}
			sproutSubfactory.LoadPipeline (loadedPipeline, branchDescriptorCollection, pathToAsset);
			Resources.UnloadAsset (loadedPipeline);
			sproutSubfactory.BranchDescriptorCollectionToPipeline ();
			RegeneratePreview ();
			selectedSproutMap = null;
			selectedSproutMapDescriptor = null;
		}
		public void ReflectChangesToPipeline () {
			sproutSubfactory.BranchDescriptorCollectionToPipeline ();
		}
		/// <summary>
		/// Regenerates the mesh according to the selected snapshot.
		/// </summary>
		public void RegeneratePreview () {
			sproutSubfactory.RegeneratePreview ();
			compositeMaterials = null;
			ShowPreviewMesh (0);

			/*
			// TEMP query for Convex hull points.
			GeometryAnalyzer ga = GeometryAnalyzer.Current ();
			ga.GetBaseBranchPositions (sproutSubfactory.treeFactory.previewTree, 0f, false);
			ga.GetTerminalBranchPositions (sproutSubfactory.treeFactory.previewTree, 1f);
			ga.GetSproutPositions (sproutSubfactory.treeFactory.previewTree, -1, false);
			if (ga.branchPoints.Count > 0) {
				Debug.Log ("Found " + ga.branchPoints.Count + " branch points, " + ga.sproutPoints.Count  + " points.");
			}
			ga.branchPoints.AddRange (ga.sproutPoints);
			
			HashSet<MyVector2> points_2d = new HashSet<MyVector2>();
			foreach (Vector3 v in ga.branchPoints)
			{
				points_2d.Add(new MyVector2 (v.z, v.y));
			}

			//Normalize to range 0-1
			Normalizer2 normalizer = new Normalizer2(new List<MyVector2>(points_2d));
			HashSet<MyVector2> points_2d_normalized = normalizer.Normalize(points_2d);
			List<MyVector2> pointsOnConvexHull = _ConvexHull.JarvisMarch_2D (points_2d_normalized);
			pointsOnConvexHull = normalizer.UnNormalize (pointsOnConvexHull);

			_convexHullPoints.Clear ();
			float scale = sproutSubfactory.treeFactory.treeFactoryPreferences.factoryScale;
			for (int i = 0; i < pointsOnConvexHull.Count; i++) {
				_convexHullPoints.Add (new Vector3 (0, pointsOnConvexHull [i].y * scale, pointsOnConvexHull [i].x * scale));
			}
			Debug.Log ("Found Convex Hull points: " + _convexHullPoints.Count);
			*/
		}
		#endregion

		#region Draw Methods
        public void Draw (Rect windowRect) {
			currentRect = windowRect;
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Box ("", GUIStyle.none, 
				GUILayout.Width (150), 
				GUILayout.Height (60));
			GUI.DrawTexture (new Rect (5, 8, 140, 48), GUITextureManager.GetLogo (), ScaleMode.ScaleToFit);
			string catalogMsg = "Broccoli Tree Creator Sprout Lab beta" + version;
			EditorGUILayout.HelpBox (catalogMsg, MessageType.None);
			if (ExtensionManager.isHDRP || ExtensionManager.isURP) {
				EditorGUILayout.HelpBox ("At this time this editor does not support URP or HDRP, this might change in the future. " + 
					"Please use the standard render pipeline to create your textures later on URP or HDRP.", MessageType.None);
			}
			EditorGUILayout.EndHorizontal ();
			if (useTreeFactoryEditorWindow) {
				if (GUILayout.Button (new GUIContent ("Close Sprout Lab"))) {
					UnloadBranchDescriptorCollection ();
					TreeFactoryEditorWindow.editorWindow.SetEditorView (TreeFactoryEditorWindow.EditorView.MainOptions);
				}
			}
			EditorGUILayout.Space ();
			mainScroll = EditorGUILayout.BeginScrollView (mainScroll, GUIStyle.none, TreeCanvasGUI.verticalScrollStyle);
			if (viewMode == ViewMode.Structure) {
				if (compositeMaterials == null) {
					SetMapView (VIEW_COMPOSITE, true);
				}
				DrawStructureView (windowRect);
				DrawControlPanel ();
			} else {
				DrawTemplateView (windowRect);
			}
			EditorGUILayout.EndScrollView ();
        }
		public void SetMapView (int mapView, bool force = false) {
			if (mapView != currentMapView || force) {
				currentMapView = mapView;
				if (compositeMaterials == null) {
					compositeMaterials = sproutSubfactory.treeFactory.previewTree.obj.GetComponent<MeshRenderer>().sharedMaterials;
				}
				if (compositeMaterials.Length > 0 && compositeMaterials[0] != null) {
					if (currentMapView == VIEW_COMPOSITE) { // Composite
						currentPreviewMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = defaultPreviewBackgroundColor;
						meshPreview.freeViewEnabled = true;
						meshPreview.hasSecondPass = true;
						meshPreview.secondPassMaterials = sproutSubfactory.GetCompositeMaterials (compositeMaterials,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						showLightControls = true;
					/*
						currentPreviewMaterials = sproutSubfactory.GetCompositeMaterials (compositeMaterials,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = defaultPreviewBackgroundColor;
						meshPreview.freeViewEnabled = true;
						meshPreview.hasSecondPass = true;
						meshPreview.secondPassMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						showLightControls = true;
						*/
					} else if (currentMapView == VIEW_ALBEDO) { // Albedo
						currentPreviewMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = defaultPreviewBackgroundColor;
						ResetCameraDirection ();
						meshPreview.freeViewEnabled = false;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					} else if (currentMapView == VIEW_NORMALS) { // Normals
						currentPreviewMaterials = sproutSubfactory.GetNormalMaterials (compositeMaterials);
						meshPreview.backgroundColor = normalPreviewBackgroundColor;
						ResetCameraDirection ();
						meshPreview.freeViewEnabled = false;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					} else if (currentMapView == VIEW_EXTRAS) { // Extra
						currentPreviewMaterials = sproutSubfactory.GetExtraMaterials (compositeMaterials,
							branchDescriptorCollection.metallicA, branchDescriptorCollection.glossinessA,
							branchDescriptorCollection.metallicB, branchDescriptorCollection.glossinessB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = extrasPreviewBackgroundColor;
						ResetCameraDirection ();
						meshPreview.freeViewEnabled = false;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					} else if (currentMapView == VIEW_SUBSURFACE) { // Subsurface
						currentPreviewMaterials = sproutSubfactory.GetSubsurfaceMaterials (compositeMaterials,
							branchDescriptorCollection.subsurfaceColorA,
							branchDescriptorCollection.subsurfaceColorB,
							branchDescriptorCollection.colorTintA,
							branchDescriptorCollection.colorTintB,
							SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
							SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						meshPreview.backgroundColor = subsurfacePreviewBackgroundColor;
						ResetCameraDirection ();
						meshPreview.freeViewEnabled = false;
						meshPreview.hasSecondPass = false;
						showLightControls = false;
					}
				} else {
					currentPreviewMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
						branchDescriptorCollection.colorTintA,
						branchDescriptorCollection.colorTintB,
						SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
						SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
					meshPreview.backgroundColor = defaultPreviewBackgroundColor;
					meshPreview.hasSecondPass = true;
					meshPreview.secondPassMaterials = compositeMaterials; 
					/*
					currentPreviewMaterials = compositeMaterials;
					meshPreview.backgroundColor = defaultPreviewBackgroundColor;
					meshPreview.hasSecondPass = true;
					meshPreview.secondPassMaterials = sproutSubfactory.GetAlbedoMaterials (compositeMaterials,
						branchDescriptorCollection.colorTintA,
						branchDescriptorCollection.colorTintB,
						SproutSubfactory.GetMaterialAStartIndex (branchDescriptorCollection),
						SproutSubfactory.GetMaterialBStartIndex (branchDescriptorCollection));
						*/
				}
			}
		}
		public void DrawStructureView (Rect windowRect) {
			Rect toolboxRect = new Rect (windowRect);
			toolboxRect.height = EditorGUIUtility.singleLineHeight;
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Generate New Structure")) {
				onBeforeBranchDescriptorChange (branchDescriptorCollection);
				sproutSubfactory.GeneratePreview ();
				onBranchDescriptorChange (branchDescriptorCollection);
				ShowPreviewMesh (0);
			}
			if (GUILayout.Button ("Regenerate Current")) {
				RegeneratePreview ();
			}
			if (GUILayout.Button ("Load From Template")) {
				catalog = SproutCatalog.GetInstance ();
				viewMode = ViewMode.Templates;
			}
			GUILayout.EndHorizontal ();
			GUILayout.Box ("", GUIStyle.none, 
				GUILayout.Width (windowRect.width), 
				GUILayout.Height (windowRect.width));
			Rect viewRect = GUILayoutUtility.GetLastRect ();
			// Draw texture view or branch 3D view.
			if (currentPanelSection == PANEL_TEXTURE && (currentStructureView == STRUCTURE_SPROUT_A || currentStructureView == STRUCTURE_SPROUT_B)  
				&& selectedSproutMap != null && selectedSproutMap.texture != null)
			{
				currenTextureView = TEXTURE_VIEW_TEXTURE;
				//areaCanvas.DrawCanvas (viewRect, selectedSproutMap.texture, selectedSproutMap);
				tmpTexture = sproutSubfactory.GetSproutTexture (selectedSproutMapGroup, selectedSproutMapIndex);
				
				if (tmpTexture != null) {
					areaCanvas.DrawCanvas (viewRect, tmpTexture, selectedSproutMap);
				}

				if (areaCanvas.HasChanged ()) {
					branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
					onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					areaCanvas.ApplyChanges (selectedSproutMap);
					onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					meshPreview.backgroundColor = defaultPreviewBackgroundColor;
				}
			} else {
				if (currenTextureView == TEXTURE_VIEW_TEXTURE) {
					RegeneratePreview ();
					currenTextureView = TEXTURE_VIEW_STRUCTURE;
				}
				meshPreviewRect = viewRect;
				meshPreview.RenderViewport (viewRect, GUIStyle.none, currentPreviewMaterials);
			}
		}
		public void DrawTemplateView (Rect windowRect) {
			Rect toolboxRect = new Rect (windowRect);
			toolboxRect.height = EditorGUIUtility.singleLineHeight;
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Back to Structure View")) {
				viewMode = ViewMode.Structure;
			}
			GUILayout.EndHorizontal ();
			// Draw Templates.
			if (catalog.GetGUIContents ().Count > 0) {
				string categoryKey = "";
				var enumerator = catalog.contents.GetEnumerator ();
				while (enumerator.MoveNext ()) {
					var contentPair = enumerator.Current;
					categoryKey = contentPair.Key;
					EditorGUILayout.LabelField (categoryKey, BroccoEditorGUI.label);
					int columns = Mathf.CeilToInt ((windowRect.width - 8) / catalogItemSize);
					int height = Mathf.CeilToInt (catalog.GetGUIContents ()[categoryKey].Count / (float)columns) * catalogItemSize;
					int selectedIndex = 
						GUILayout.SelectionGrid (-1, catalog.GetGUIContents ()[categoryKey].ToArray (), 
							columns, TreeCanvasGUI.catalogItemStyle, GUILayout.Height (height), GUILayout.Width (windowRect.width - 8));
					if (selectedIndex >= 0 &&
					   EditorUtility.DisplayDialog (MSG_LOAD_CATALOG_ITEM_TITLE, 
						   MSG_LOAD_CATALOG_ITEM_MESSAGE, 
						   MSG_LOAD_CATALOG_ITEM_OK, 
						   MSG_LOAD_CATALOG_ITEM_CANCEL)) {
						// Load the Snapshot Collection SO
						string pathToCollection = ExtensionManager.fullExtensionPath + catalog.GetItemAtIndex (categoryKey, selectedIndex).path;
						BranchDescriptorCollectionSO branchDescriptorCollectionSO = editorPersistence.LoadElementFromFile (pathToCollection);
						if (branchDescriptorCollectionSO != null) {
							selectedIndex = -1;
							OnLoadBranchDescriptorCollectionSO (branchDescriptorCollectionSO, pathToCollection);
							viewMode = ViewMode.Structure;
						} else {
							Debug.LogWarning ("Could not find BranchDescriptorCollectionSO at: " + pathToCollection);
						}
					}
				}
			}
		}
		public void DrawControlPanel () {
			DrawSnapshotsPanel ();
			int _currentPanelSection = GUILayout.Toolbar (currentPanelSection, panelSectionOption, GUI.skin.button);
			if (_currentPanelSection != currentPanelSection) {
				currentPanelSection = _currentPanelSection;
				SetMapView (VIEW_COMPOSITE, true);
			}
			switch (currentPanelSection) {
				case PANEL_STRUCTURE:
					DrawStructurePanel ();
					break;
				case PANEL_TEXTURE:
					DrawTexturePanel ();
					break;
				case PANEL_MAPPING:
					DrawMappingPanel ();
					break;
				case PANEL_EXPORT:
					DrawExportPanel ();
					break;
			}
		}
		#endregion

		#region Structure Panel
		public void DrawStructurePanel () {
			bool changed = false;
			float girthAtBase = selectedBranchDescriptor.girthAtBase;
			float girthAtTop = selectedBranchDescriptor.girthAtTop;
			float noiseAtBase = selectedBranchDescriptor.noiseAtBase;
			float noiseAtTop = selectedBranchDescriptor.noiseAtTop;
			float noiseScaleAtBase = selectedBranchDescriptor.noiseScaleAtBase;
			float noiseScaleAtTop = selectedBranchDescriptor.noiseScaleAtTop;
			float sproutASize = selectedBranchDescriptor.sproutASize;
			float sproutAScaleAtBase = selectedBranchDescriptor.sproutAScaleAtBase;
			float sproutAScaleAtTop = selectedBranchDescriptor.sproutAScaleAtTop;
			float sproutAFlipAlign = selectedBranchDescriptor.sproutAFlipAlign;
			float sproutBSize = selectedBranchDescriptor.sproutBSize;
			float sproutBScaleAtBase = selectedBranchDescriptor.sproutBScaleAtBase;
			float sproutBScaleAtTop = selectedBranchDescriptor.sproutBScaleAtTop;
			float sproutBFlipAlign = selectedBranchDescriptor.sproutBFlipAlign;
			int activeLevels = 2;

			EditorGUILayout.BeginHorizontal ();
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField ("Structures", BroccoEditorGUI.labelBoldCentered);
			currentStructureView = GUILayout.SelectionGrid (currentStructureView, structureViewOptions, 1, GUILayout.Width (secondaryPanelColumnWidth));
			EditorGUILayout.EndVertical ();
			// Mapping Settings.
			structurePanelScroll = EditorGUILayout.BeginScrollView (structurePanelScroll, GUILayout.ExpandWidth (true));
			switch (currentStructureView) {
				case STRUCTURE_BRANCH: // BRANCHES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Branch Settings", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					// Active levels
					EditorGUI.BeginChangeCheck ();
					activeLevels = EditorGUILayout.Popup ("Active Levels", selectedBranchDescriptor.activeLevels, levelOptions); 
					changed = EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Branch structure settings
					girthAtBase = EditorGUILayout.Slider ("Girth at Base", selectedBranchDescriptor.girthAtBase, 0.005f, 0.4f);
					if (girthAtBase != selectedBranchDescriptor.girthAtBase) {
						changed = true;
					}
					girthAtTop = EditorGUILayout.Slider ("Girth at Top", selectedBranchDescriptor.girthAtTop, 0.005f, 0.4f);
					if (girthAtTop != selectedBranchDescriptor.girthAtTop) {
						changed = true;
					}
					EditorGUILayout.Space ();
					noiseAtBase = EditorGUILayout.Slider ("Noise at Base", selectedBranchDescriptor.noiseAtBase, 0f, 1f);
					if (noiseAtBase != selectedBranchDescriptor.noiseAtBase) {
						changed = true;
					}
					noiseAtTop = EditorGUILayout.Slider ("Noise at Top", selectedBranchDescriptor.noiseAtTop, 0f, 1f);
					if (noiseAtTop != selectedBranchDescriptor.noiseAtTop) {
						changed = true;
					}
					noiseScaleAtBase = EditorGUILayout.Slider ("Noise Scale at Base", selectedBranchDescriptor.noiseScaleAtBase, 0f, 1f);
					if (noiseScaleAtBase != selectedBranchDescriptor.noiseScaleAtBase) {
						changed = true;
					}
					noiseScaleAtTop = EditorGUILayout.Slider ("Noise Scale at Top", selectedBranchDescriptor.noiseScaleAtTop, 0f, 1f);
					if (noiseScaleAtTop != selectedBranchDescriptor.noiseScaleAtTop) {
						changed = true;
					}
					EditorGUILayout.Space ();
					// Draw Branch Structure Panel
					changed |= DrawBranchStructurePanel ();
					break;
				case STRUCTURE_SPROUT_A: // LEAVES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Sprout A Settings", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					// Active levels
					EditorGUI.BeginChangeCheck ();
					activeLevels = EditorGUILayout.Popup ("Active Levels", selectedBranchDescriptor.activeLevels, levelOptions); 
					changed = EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Sprout structure settings
					EditorGUI.BeginChangeCheck ();
					sproutASize = EditorGUILayout.Slider ("Size", selectedBranchDescriptor.sproutASize, 0.1f, 5f);
					sproutAScaleAtBase = EditorGUILayout.Slider ("Scale At Base", selectedBranchDescriptor.sproutAScaleAtBase, 0.1f, 5f);
					sproutAScaleAtTop = EditorGUILayout.Slider ("Scale At Top", selectedBranchDescriptor.sproutAScaleAtTop, 0.1f, 5f);
					sproutAFlipAlign = EditorGUILayout.Slider ("Plane Alignment", selectedBranchDescriptor.sproutAFlipAlign, 0.5f, 1f);
					changed |= EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Draw Sprout A Hierarchy Structure Panel
					changed |= DrawSproutAStructurePanel ();
					break;
				case STRUCTURE_SPROUT_B: // LEAVES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Sprout B Settings", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					// Active levels
					EditorGUI.BeginChangeCheck ();
					activeLevels = EditorGUILayout.Popup ("Active Levels", selectedBranchDescriptor.activeLevels, levelOptions); 
					changed = EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Sprout structure settings
					EditorGUI.BeginChangeCheck ();
					sproutBSize = EditorGUILayout.Slider ("Size", selectedBranchDescriptor.sproutBSize, 0.1f, 5f);
					sproutBScaleAtBase = EditorGUILayout.Slider ("Scale At Base", selectedBranchDescriptor.sproutBScaleAtBase, 0.1f, 5f);
					sproutBScaleAtTop = EditorGUILayout.Slider ("Scale At Top", selectedBranchDescriptor.sproutBScaleAtTop, 0.1f, 5f);
					sproutBFlipAlign = EditorGUILayout.Slider ("Plane Alignment", selectedBranchDescriptor.sproutBFlipAlign, 0.5f, 1f);
					changed |= EditorGUI.EndChangeCheck ();
					EditorGUILayout.Space ();
					// Draw Sprout A Hierarchy Structure Panel
					changed |= DrawSproutBStructurePanel ();
					break;
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
			if (changed) {
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				CopyFromProxyBranchLevelDescriptor ();
				CopyFromProxySproutALevelDescriptor ();
				CopyFromProxySproutBLevelDescriptor ();
				selectedBranchDescriptor.activeLevels = activeLevels;
				selectedBranchDescriptor.girthAtBase = girthAtBase;
				selectedBranchDescriptor.girthAtTop = girthAtTop;
				selectedBranchDescriptor.noiseAtBase = noiseAtBase;
				selectedBranchDescriptor.noiseAtTop = noiseAtTop;
				selectedBranchDescriptor.noiseScaleAtBase = noiseScaleAtBase;
				selectedBranchDescriptor.noiseScaleAtTop = noiseScaleAtTop;
				selectedBranchDescriptor.sproutASize = sproutASize;
				selectedBranchDescriptor.sproutAScaleAtBase = sproutAScaleAtBase;
				selectedBranchDescriptor.sproutAScaleAtTop = sproutAScaleAtTop;
				selectedBranchDescriptor.sproutAFlipAlign = sproutAFlipAlign;
				selectedBranchDescriptor.sproutBSize = sproutBSize;
				selectedBranchDescriptor.sproutBScaleAtBase = sproutBScaleAtBase;
				selectedBranchDescriptor.sproutBScaleAtTop = sproutBScaleAtTop;
				selectedBranchDescriptor.sproutBFlipAlign = sproutBFlipAlign;
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
		}
		bool DrawBranchStructurePanel () {
			bool changed = false;
			// Foldouts per hierarchy branch level.
			GUIStyle st = BroccoEditorGUI.foldoutBold;
			GUIStyle stB = BroccoEditorGUI.labelBold;
			for (int i = 0; i <= selectedBranchDescriptor.activeLevels; i++) {
				branchFoldouts [i] = EditorGUILayout.Foldout (branchFoldouts [i], "Branch Level " + i, BroccoEditorGUI.foldoutBold);
				if (branchFoldouts [i]) {
					selectedBranchLevelDescriptor = selectedBranchDescriptor.branchLevelDescriptors [i];
					CopyToProxyBranchLevelDescriptor ();
					// Properties for non-root levels.
					if (i == 0) {
						// LENGTH
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minLengthAtBase,
							ref proxyBranchLevelDescriptor.maxLengthAtBase,
							3, 15, "Length");
					} else {
						// FREQUENCY
						changed |= BroccoEditorGUI.IntRangePropertyField (
							ref proxyBranchLevelDescriptor.minFrequency,
							ref proxyBranchLevelDescriptor.maxFrequency,
							1, 12, "Frequency");
						EditorGUILayout.Space ();
						// LENGTH
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minLengthAtBase,
							ref proxyBranchLevelDescriptor.maxLengthAtBase,
							1, 12, "Length At Base");
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minLengthAtTop,
							ref proxyBranchLevelDescriptor.maxLengthAtTop,
							1, 12, "Length At Top");
						EditorGUILayout.Space ();
						// ALIGNMENT
						// Min Branch Branch Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minParallelAlignAtBase,
							ref proxyBranchLevelDescriptor.maxParallelAlignAtBase,
							-1f, 1f, "Parallel Align at Base");
						// Max Branch Branch Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minParallelAlignAtTop,
							ref proxyBranchLevelDescriptor.maxParallelAlignAtTop,
							-1f, 1f, "Parallel Align at Top");
						// Min Branch Gravity Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minGravityAlignAtBase,
							ref proxyBranchLevelDescriptor.maxGravityAlignAtBase,
							-1f, 1f, "Gravity Align at Base");
						// Max Branch Gravity Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxyBranchLevelDescriptor.minGravityAlignAtTop,
							ref proxyBranchLevelDescriptor.maxGravityAlignAtTop,
							-1f, 1f, "Gravity Align at Top");
					}
					EditorGUILayout.Space ();
				}
				if (changed) break;
			}
			return changed;
		}
		bool DrawSproutAStructurePanel () {
			bool changed = false;
			// Foldouts per hierarchy branch level.
			for (int i = 0; i <= selectedBranchDescriptor.activeLevels; i++) {
				sproutAFoldouts [i] = EditorGUILayout.Foldout (sproutAFoldouts [i], "Sprout Level " + i, BroccoEditorGUI.foldoutBold);
				if (sproutAFoldouts [i]) {
					selectedSproutALevelDescriptor = selectedBranchDescriptor.sproutALevelDescriptors [i];
					CopyToProxySproutALevelDescriptor ();
					// ENABLED
					bool isEnabled = EditorGUILayout.Toggle ("Enabled", proxySproutALevelDescriptor.isEnabled);
					if (isEnabled != proxySproutALevelDescriptor.isEnabled) {
						changed = true;
						proxySproutALevelDescriptor.isEnabled = isEnabled;
					}
					EditorGUILayout.Space ();
					if (isEnabled) {
						// FREQUENCY
						changed |= BroccoEditorGUI.IntRangePropertyField (
							ref proxySproutALevelDescriptor.minFrequency,
							ref proxySproutALevelDescriptor.maxFrequency,
							1, 25, "Frequency");
						EditorGUILayout.Space ();
						// ALIGNMENT
						// Min Branch Branch Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minParallelAlignAtBase,
							ref proxySproutALevelDescriptor.maxParallelAlignAtBase,
							-1f, 1f, "Parallel Align at Base");
						// Max Branch Branch Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minParallelAlignAtTop,
							ref proxySproutALevelDescriptor.maxParallelAlignAtTop,
							-1f, 1f, "Parallel Align at Top");
						// Min Branch Gravity Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minGravityAlignAtBase,
							ref proxySproutALevelDescriptor.maxGravityAlignAtBase,
							-1f, 1f, "Gravity Align at Base");
						// Max Branch Gravity Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minGravityAlignAtTop,
							ref proxySproutALevelDescriptor.maxGravityAlignAtTop,
							-1f, 1f, "Gravity Align at Top");
						// Min and max range for the sprouts to be spawn.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutALevelDescriptor.minRange,
							ref proxySproutALevelDescriptor.maxRange,
							-1f, 1f, "Branch Range");
						EditorGUILayout.Space ();
					}
				}
				if (changed) break;
			}
			return changed;
		}
		bool DrawSproutBStructurePanel () {
			bool changed = false;
			// Foldouts per hierarchy branch level.
			for (int i = 0; i <= selectedBranchDescriptor.activeLevels; i++) {
				sproutBFoldouts [i] = EditorGUILayout.Foldout (sproutBFoldouts [i], "Sprout Level " + i, BroccoEditorGUI.foldoutBold);
				if (sproutBFoldouts [i]) {
					selectedSproutBLevelDescriptor = selectedBranchDescriptor.sproutBLevelDescriptors [i];
					CopyToProxySproutBLevelDescriptor ();
					// ENABLED
					bool isEnabled = EditorGUILayout.Toggle ("Enabled", proxySproutBLevelDescriptor.isEnabled);
					if (isEnabled != proxySproutBLevelDescriptor.isEnabled) {
						changed = true;
						proxySproutBLevelDescriptor.isEnabled = isEnabled;
					}
					EditorGUILayout.Space ();
					if (isEnabled) {
						// FREQUENCY
						changed |= BroccoEditorGUI.IntRangePropertyField (
							ref proxySproutBLevelDescriptor.minFrequency,
							ref proxySproutBLevelDescriptor.maxFrequency,
							1, 25, "Frequency");
						EditorGUILayout.Space ();
						// ALIGNMENT
						// Min Branch Branch Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minParallelAlignAtBase,
							ref proxySproutBLevelDescriptor.maxParallelAlignAtBase,
							-1f, 1f, "Parallel Align at Base");
						// Max Branch Branch Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minParallelAlignAtTop,
							ref proxySproutBLevelDescriptor.maxParallelAlignAtTop,
							-1f, 1f, "Parallel Align at Top");
						// Min Branch Gravity Align At Base.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minGravityAlignAtBase,
							ref proxySproutBLevelDescriptor.maxGravityAlignAtBase,
							-1f, 1f, "Gravity Align at Base");
						// Max Branch Gravity Align At Top.
						changed |= BroccoEditorGUI.FloatRangePropertyField (
							ref proxySproutBLevelDescriptor.minGravityAlignAtTop,
							ref proxySproutBLevelDescriptor.maxGravityAlignAtTop,
							-1f, 1f, "Gravity Align at Top");
						EditorGUILayout.Space ();
					}
				}
				if (changed) break;
			}
			return changed;
		}
		#endregion

		#region Texture Panel
		public void DrawTexturePanel () {
			bool changed = false;
			Texture2D branchAlbedoTexture = branchDescriptorCollection.branchAlbedoTexture;
			Texture2D branchNormalTexture = branchDescriptorCollection.branchNormalTexture;
			float branchTextureYDisplacement = branchDescriptorCollection.branchTextureYDisplacement;
			EditorGUILayout.BeginHorizontal ();
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField ("Structures", BroccoEditorGUI.labelBoldCentered);
			currentStructureView = GUILayout.SelectionGrid (currentStructureView, structureViewOptions, 1, GUILayout.Width (secondaryPanelColumnWidth));
			EditorGUILayout.EndVertical ();
			// Mapping Settings.
			texturePanelScroll = EditorGUILayout.BeginScrollView (texturePanelScroll, GUILayout.ExpandWidth (true));
			switch (currentStructureView) {
				case STRUCTURE_BRANCH: // BRANCHES.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Branch Textures", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					EditorGUILayout.BeginVertical (GUILayout.Width (200));
					branchAlbedoTexture = (Texture2D) EditorGUILayout.ObjectField ("Main Texture", branchDescriptorCollection.branchAlbedoTexture, typeof (Texture2D), false);
					if (branchAlbedoTexture != branchDescriptorCollection.branchAlbedoTexture) {
						changed = true;
					}
					branchNormalTexture = (Texture2D) EditorGUILayout.ObjectField ("Normal Texture", branchDescriptorCollection.branchNormalTexture, typeof (Texture2D), false);
					if (branchNormalTexture != branchDescriptorCollection.branchNormalTexture) {
						changed = true;
					}
					EditorGUILayout.EndVertical ();
					branchTextureYDisplacement = EditorGUILayout.Slider ("Y Displacement", branchDescriptorCollection.branchTextureYDisplacement, -3f, 4f);
					if (branchTextureYDisplacement != branchDescriptorCollection.branchTextureYDisplacement) {
						changed = true;
					}
					break;
				case STRUCTURE_SPROUT_A: // SPROUT A.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Sprout A Textures", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					sproutMapChanged = false;
					sproutAMapList.DoLayoutList ();
					changed |= sproutMapChanged;
					break;
				case STRUCTURE_SPROUT_B: // SPROUT B.
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Sprout B Textures", BroccoEditorGUI.labelBold);
					GUILayout.FlexibleSpace ();
					EditorGUILayout.EndHorizontal ();
					sproutMapChanged = false;
					sproutBMapList.DoLayoutList ();
					changed |= sproutMapChanged;
					break;
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
			if (changed) {
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				CopyFromProxyBranchLevelDescriptor ();
				branchDescriptorCollection.branchAlbedoTexture = branchAlbedoTexture;
				branchDescriptorCollection.branchNormalTexture = branchNormalTexture;
				branchDescriptorCollection.branchTextureYDisplacement = branchTextureYDisplacement;
				if (sproutMapChanged && selectedSproutMap != null) {
					CopyFromProxySproutMap ();
				}
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
		}
		#endregion

		#region Mapping Panel
		public void DrawMappingPanel () {
			EditorGUILayout.BeginHorizontal ();
			// View Mode Selection.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField ("Mapping Options", BroccoEditorGUI.labelBoldCentered);
			int _currentMapView = GUILayout.SelectionGrid (currentMapView, mapViewOptions, 1, GUILayout.Width (secondaryPanelColumnWidth));
			if (_currentMapView != currentMapView) {
				SetMapView (_currentMapView);
			}
			EditorGUILayout.EndVertical ();
			// Mapping Settings.
			mappingPanelScroll = EditorGUILayout.BeginScrollView (mappingPanelScroll, GUILayout.ExpandWidth (true));
			switch (currentMapView) {
				case VIEW_COMPOSITE: // Composite.
					EditorGUILayout.LabelField ("Composite Mapping Settings", BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_COMPOSITE, MessageType.None);
					break;
				case VIEW_ALBEDO: // Albedo.
					EditorGUILayout.LabelField ("Albedo Map Settings", BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_ALBEDO, MessageType.None);
					break;
				case VIEW_NORMALS: // Normals.
					EditorGUILayout.LabelField ("Normal Map Settings", BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_NORMALS, MessageType.None);
					break;
				case VIEW_EXTRAS: // Extras.
					EditorGUILayout.LabelField ("Metallic, Smoothness and AO Map Settings", BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_EXTRA, MessageType.None);
					break;
				case VIEW_SUBSURFACE: // Subsurface
					EditorGUILayout.LabelField ("Subsurface Map Settings", BroccoEditorGUI.labelBold);
					EditorGUILayout.HelpBox (MSG_MAPPING_SUBSURFACE, MessageType.None);
					break;
			}
			DrawCompositeMappingSettings ();
			EditorGUILayout.Space ();
			/* LIGHTS SETTINGS
			EditorGUILayout.Space ();
			EditorGUI.BeginChangeCheck ();
			float lightAIntensity = meshPreview.GetLigthA ().intensity;
			lightAIntensity = EditorGUILayout.Slider ("Intensity A", lightAIntensity, 0f, 4f);
			Vector3 lightARotation = meshPreview.GetLigthA ().transform.rotation.eulerAngles;
			lightARotation = EditorGUILayout.Vector3Field ("Rotation A", lightARotation);
			float lightBIntensity = meshPreview.GetLigthB ().intensity;
			lightBIntensity = EditorGUILayout.Slider ("Intensity B", lightBIntensity, 0f, 4f);
			Vector3 lightBRotation = meshPreview.GetLigthB ().transform.rotation.eulerAngles;
			lightBRotation = EditorGUILayout.Vector3Field ("Rotation B", lightBRotation);
			if (EditorGUI.EndChangeCheck ()) {
				meshPreview.SetLightA (lightAIntensity, Quaternion.Euler (lightARotation));
				meshPreview.SetLightB (lightBIntensity, Quaternion.Euler (lightBRotation));
			}
			*/
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndHorizontal ();
		}
		public void DrawCompositeMappingSettings () {
			float minColorShadeA = branchDescriptorCollection.minColorShadeA;
			float maxColorShadeA = branchDescriptorCollection.maxColorShadeA;
			Color tintColorA = branchDescriptorCollection.colorTintA;
			float minColorTintA = branchDescriptorCollection.minColorTintA;
			float maxColorTintA = branchDescriptorCollection.maxColorTintA;
			float metallicA = branchDescriptorCollection.metallicA;
			float glossinessA = branchDescriptorCollection.glossinessA;
			Color subsurfaceColorA = branchDescriptorCollection.subsurfaceColorA;
			
			float minColorShadeB = branchDescriptorCollection.minColorShadeB;
			float maxColorShadeB = branchDescriptorCollection.maxColorShadeB;
			Color tintColorB = branchDescriptorCollection.colorTintB;
			float minColorTintB = branchDescriptorCollection.minColorTintB;
			float maxColorTintB = branchDescriptorCollection.maxColorTintB;
			float metallicB = branchDescriptorCollection.metallicB;
			float glossinessB = branchDescriptorCollection.glossinessB;
			Color subsurfaceColorB = branchDescriptorCollection.subsurfaceColorB;

			EditorGUI.BeginChangeCheck ();

			// Sprout A
			EditorGUILayout.LabelField ("Sprout A", BroccoEditorGUI.labelBold);
			// Shade A
			BroccoEditorGUI.FloatRangePropertyField (
						ref minColorShadeA,
						ref maxColorShadeA,
						0.65f, 1f, "Sprout A Shade");
			// Tint A
			tintColorA = EditorGUILayout.ColorField ("Tint Color A", tintColorA);
			BroccoEditorGUI.FloatRangePropertyField (
						ref minColorTintA,
						ref maxColorTintA,
						0f, 1f, "Sprout A Tint");
			// Metallic, glossiness, surface A
			metallicA = EditorGUILayout.Slider ("Metallic A", metallicA, 0f, 1f);
			glossinessA = EditorGUILayout.Slider ("Glossiness A", glossinessA, 0f, 1f);
			subsurfaceColorA = EditorGUILayout.ColorField ("Subsurface A", subsurfaceColorA);
			EditorGUILayout.Space ();

			// Sprout B
			EditorGUILayout.LabelField ("Sprout B", BroccoEditorGUI.labelBold);
			// Shade B
			BroccoEditorGUI.FloatRangePropertyField (
						ref minColorShadeB,
						ref maxColorShadeB,
						0.65f, 1f, "Sprout B Shade");
			// Tint B
			tintColorB = EditorGUILayout.ColorField ("Tint Color B", tintColorB);
			BroccoEditorGUI.FloatRangePropertyField (
						ref minColorTintB,
						ref maxColorTintB,
						0f, 1f, "Sprout B Tint");
			// Metallic, glossiness, surface B
			metallicB = EditorGUILayout.Slider ("Metallic B", metallicB, 0f, 1f);
			glossinessB = EditorGUILayout.Slider ("Glossiness B", glossinessB, 0f, 1f);
			subsurfaceColorB = EditorGUILayout.ColorField ("Subsurface B", subsurfaceColorB);

			if (EditorGUI.EndChangeCheck ()) {
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.minColorShadeA = minColorShadeA;
				branchDescriptorCollection.maxColorShadeA = maxColorShadeA;
				branchDescriptorCollection.colorTintA = tintColorA;
				branchDescriptorCollection.minColorTintA = minColorTintA;
				branchDescriptorCollection.maxColorTintA = maxColorTintA;
				branchDescriptorCollection.metallicA = metallicA;
				branchDescriptorCollection.glossinessA = glossinessA;
				branchDescriptorCollection.subsurfaceColorA = subsurfaceColorA;
				branchDescriptorCollection.minColorShadeB = minColorShadeB;
				branchDescriptorCollection.maxColorShadeB = maxColorShadeB;
				branchDescriptorCollection.colorTintB = tintColorB;
				branchDescriptorCollection.minColorTintB = minColorTintB;
				branchDescriptorCollection.maxColorTintB = maxColorTintB;
				branchDescriptorCollection.metallicB = metallicB;
				branchDescriptorCollection.glossinessB = glossinessB;
				branchDescriptorCollection.subsurfaceColorB = subsurfaceColorB;
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
		}
		#endregion

		#region Export Panel
		public void DrawExportPanel () {
			bool changed = false;
			EditorGUILayout.BeginHorizontal ();
			// Export Options.
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.LabelField ("Export Options", BroccoEditorGUI.labelBoldCentered, GUILayout.Width (secondaryPanelColumnWidth));
			if (GUILayout.Button ("Export Textures")) {
				ExportTextures ();
			}
			EditorGUILayout.Space ();
			// Export/import to/from file.
			EditorGUILayout.LabelField ("File Options", BroccoEditorGUI.labelBoldCentered);
			editorPersistence.DrawOptions ();
			EditorGUILayout.EndVertical ();
			// Export Settings.
			// Mapping Settings.
			exportPanelScroll = EditorGUILayout.BeginScrollView (exportPanelScroll, GUILayout.ExpandWidth (true));
			EditorGUILayout.LabelField ("Export Settings", BroccoEditorGUI.labelBold);
			// Atlas size.
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Atlas Size", BroccoEditorGUI.label);
			BranchDescriptorCollection.TextureSize exportTextureSize = 
				(BranchDescriptorCollection.TextureSize)EditorGUILayout.EnumPopup (branchDescriptorCollection.exportTextureSize, GUILayout.Width (120));
			changed |= exportTextureSize != branchDescriptorCollection.exportTextureSize;
			EditorGUILayout.EndHorizontal ();
			// Atlas padding.
			int paddingSize = EditorGUILayout.IntField ("Padding", branchDescriptorCollection.exportAtlasPadding);
			if (paddingSize < 0 || paddingSize > 25) {
				paddingSize = branchDescriptorCollection.exportAtlasPadding;			
			}
			changed |= paddingSize != branchDescriptorCollection.exportAtlasPadding;
			EditorGUILayout.Space ();
			// OUTPUT FILE
			EditorGUILayout.LabelField ("Output File", BroccoEditorGUI.labelBold);
			// Export mode.
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Export Mode", BroccoEditorGUI.label);
			BranchDescriptorCollection.ExportMode exportMode = 
				(BranchDescriptorCollection.ExportMode)EditorGUILayout.EnumPopup (branchDescriptorCollection.exportMode, GUILayout.Width (120));
			changed |= exportMode != branchDescriptorCollection.exportMode;
			EditorGUILayout.EndHorizontal ();
			// Export take.
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Take", BroccoEditorGUI.label);
			int exportTake = EditorGUILayout.IntField (branchDescriptorCollection.exportTake);
			changed |= exportTake != branchDescriptorCollection.exportTake;
			EditorGUILayout.EndHorizontal ();
			// Export path.
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Path:", BroccoEditorGUI.label);
			EditorGUILayout.LabelField ("/Assets" + branchDescriptorCollection.exportPath);
			if (GUILayout.Button (new GUIContent ("...", "Select the path to save the textures to."), GUILayout.Width (30))) {
				string currentPath = Application.dataPath + branchDescriptorCollection.exportPath;
				string selectedPath = EditorUtility.OpenFolderPanel ("Textures Folder", currentPath, "");
				if (!string.IsNullOrEmpty (selectedPath)) {
					selectedPath = selectedPath.Substring (Application.dataPath.Length);
					if (selectedPath.CompareTo (branchDescriptorCollection.exportPath) != 0) {
						branchDescriptorCollection.exportPath = selectedPath;
						changed = true;
					}
				}
				GUIUtility.ExitGUI();
			}
			EditorGUILayout.EndHorizontal ();
			// List of paths
			bool isValid = false; bool isValidTemp = false;
			bool isAtlas = branchDescriptorCollection.exportMode == BranchDescriptorCollection.ExportMode.Atlas;
			string albedoPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, isAtlas);
			string normalsPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Normals, isAtlas);
			string extrasPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Extras, isAtlas);
			string subsurfacePath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValidTemp, SproutSubfactory.MaterialMode.Subsurface, isAtlas);
			EditorGUILayout.HelpBox (albedoPath + "\n" + normalsPath + "\n" + extrasPath + "\n" + subsurfacePath, MessageType.None);
			// Export textures flags
			int exportFlags = EditorGUILayout.MaskField("Textures", branchDescriptorCollection.exportTexturesFlags, exportTextureOptions);
			changed |= exportFlags != branchDescriptorCollection.exportTexturesFlags;

			EditorGUILayout.EndScrollView ();
			if (changed) {
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.exportTextureSize = exportTextureSize;
				branchDescriptorCollection.exportAtlasPadding = paddingSize;
				branchDescriptorCollection.exportMode = exportMode;
				branchDescriptorCollection.exportTake = exportTake;
				branchDescriptorCollection.exportTexturesFlags = exportFlags;
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			}
		}
		#endregion

		#region Undo
		void CopyToProxyBranchLevelDescriptor () {
			proxyBranchLevelDescriptor.minFrequency = selectedBranchLevelDescriptor.minFrequency;
			proxyBranchLevelDescriptor.maxFrequency = selectedBranchLevelDescriptor.maxFrequency;
			proxyBranchLevelDescriptor.minLengthAtBase = selectedBranchLevelDescriptor.minLengthAtBase;
			proxyBranchLevelDescriptor.maxLengthAtBase = selectedBranchLevelDescriptor.maxLengthAtBase;
			proxyBranchLevelDescriptor.minLengthAtTop = selectedBranchLevelDescriptor.minLengthAtTop;
			proxyBranchLevelDescriptor.maxLengthAtTop = selectedBranchLevelDescriptor.maxLengthAtTop;
			proxyBranchLevelDescriptor.minParallelAlignAtTop = selectedBranchLevelDescriptor.minParallelAlignAtTop;
			proxyBranchLevelDescriptor.maxParallelAlignAtTop = selectedBranchLevelDescriptor.maxParallelAlignAtTop;
			proxyBranchLevelDescriptor.minParallelAlignAtBase = selectedBranchLevelDescriptor.minParallelAlignAtBase;
			proxyBranchLevelDescriptor.maxParallelAlignAtBase = selectedBranchLevelDescriptor.maxParallelAlignAtBase;
			proxyBranchLevelDescriptor.minGravityAlignAtTop = selectedBranchLevelDescriptor.minGravityAlignAtTop;
			proxyBranchLevelDescriptor.maxGravityAlignAtTop = selectedBranchLevelDescriptor.maxGravityAlignAtTop;
			proxyBranchLevelDescriptor.minGravityAlignAtBase = selectedBranchLevelDescriptor.minGravityAlignAtBase;
			proxyBranchLevelDescriptor.maxGravityAlignAtBase = selectedBranchLevelDescriptor.maxGravityAlignAtBase;
		}
		void CopyFromProxyBranchLevelDescriptor () {
			if (selectedBranchLevelDescriptor != null) {
				selectedBranchLevelDescriptor.minFrequency = proxyBranchLevelDescriptor.minFrequency;
				selectedBranchLevelDescriptor.maxFrequency = proxyBranchLevelDescriptor.maxFrequency;
				selectedBranchLevelDescriptor.minLengthAtBase = proxyBranchLevelDescriptor.minLengthAtBase;
				selectedBranchLevelDescriptor.maxLengthAtBase = proxyBranchLevelDescriptor.maxLengthAtBase;
				selectedBranchLevelDescriptor.minLengthAtTop = proxyBranchLevelDescriptor.minLengthAtTop;
				selectedBranchLevelDescriptor.maxLengthAtTop = proxyBranchLevelDescriptor.maxLengthAtTop;
				selectedBranchLevelDescriptor.minParallelAlignAtTop = proxyBranchLevelDescriptor.minParallelAlignAtTop;
				selectedBranchLevelDescriptor.maxParallelAlignAtTop = proxyBranchLevelDescriptor.maxParallelAlignAtTop;
				selectedBranchLevelDescriptor.minParallelAlignAtBase = proxyBranchLevelDescriptor.minParallelAlignAtBase;
				selectedBranchLevelDescriptor.maxParallelAlignAtBase = proxyBranchLevelDescriptor.maxParallelAlignAtBase;
				selectedBranchLevelDescriptor.minGravityAlignAtTop = proxyBranchLevelDescriptor.minGravityAlignAtTop;
				selectedBranchLevelDescriptor.maxGravityAlignAtTop = proxyBranchLevelDescriptor.maxGravityAlignAtTop;
				selectedBranchLevelDescriptor.minGravityAlignAtBase = proxyBranchLevelDescriptor.minGravityAlignAtBase;
				selectedBranchLevelDescriptor.maxGravityAlignAtBase = proxyBranchLevelDescriptor.maxGravityAlignAtBase;
			}
		}
		void CopyToProxySproutALevelDescriptor () {
			proxySproutALevelDescriptor.isEnabled = selectedSproutALevelDescriptor.isEnabled;
			proxySproutALevelDescriptor.minFrequency = selectedSproutALevelDescriptor.minFrequency;
			proxySproutALevelDescriptor.maxFrequency = selectedSproutALevelDescriptor.maxFrequency;
			proxySproutALevelDescriptor.minParallelAlignAtTop = selectedSproutALevelDescriptor.minParallelAlignAtTop;
			proxySproutALevelDescriptor.maxParallelAlignAtTop = selectedSproutALevelDescriptor.maxParallelAlignAtTop;
			proxySproutALevelDescriptor.minParallelAlignAtBase = selectedSproutALevelDescriptor.minParallelAlignAtBase;
			proxySproutALevelDescriptor.maxParallelAlignAtBase = selectedSproutALevelDescriptor.maxParallelAlignAtBase;
			proxySproutALevelDescriptor.minGravityAlignAtTop = selectedSproutALevelDescriptor.minGravityAlignAtTop;
			proxySproutALevelDescriptor.maxGravityAlignAtTop = selectedSproutALevelDescriptor.maxGravityAlignAtTop;
			proxySproutALevelDescriptor.minGravityAlignAtBase = selectedSproutALevelDescriptor.minGravityAlignAtBase;
			proxySproutALevelDescriptor.maxGravityAlignAtBase = selectedSproutALevelDescriptor.maxGravityAlignAtBase;
			proxySproutALevelDescriptor.minRange = selectedSproutALevelDescriptor.minRange;
			proxySproutALevelDescriptor.maxRange = selectedSproutALevelDescriptor.maxRange;
		}
		void CopyFromProxySproutALevelDescriptor () {
			if (selectedSproutALevelDescriptor != null) {
				selectedSproutALevelDescriptor.isEnabled = proxySproutALevelDescriptor.isEnabled;
				selectedSproutALevelDescriptor.minFrequency = proxySproutALevelDescriptor.minFrequency;
				selectedSproutALevelDescriptor.maxFrequency = proxySproutALevelDescriptor.maxFrequency;
				selectedSproutALevelDescriptor.minParallelAlignAtTop = proxySproutALevelDescriptor.minParallelAlignAtTop;
				selectedSproutALevelDescriptor.maxParallelAlignAtTop = proxySproutALevelDescriptor.maxParallelAlignAtTop;
				selectedSproutALevelDescriptor.minParallelAlignAtBase = proxySproutALevelDescriptor.minParallelAlignAtBase;
				selectedSproutALevelDescriptor.maxParallelAlignAtBase = proxySproutALevelDescriptor.maxParallelAlignAtBase;
				selectedSproutALevelDescriptor.minGravityAlignAtTop = proxySproutALevelDescriptor.minGravityAlignAtTop;
				selectedSproutALevelDescriptor.maxGravityAlignAtTop = proxySproutALevelDescriptor.maxGravityAlignAtTop;
				selectedSproutALevelDescriptor.minGravityAlignAtBase = proxySproutALevelDescriptor.minGravityAlignAtBase;
				selectedSproutALevelDescriptor.maxGravityAlignAtBase = proxySproutALevelDescriptor.maxGravityAlignAtBase;
				selectedSproutALevelDescriptor.minRange = proxySproutALevelDescriptor.minRange;
				selectedSproutALevelDescriptor.maxRange = proxySproutALevelDescriptor.maxRange;
			}
		}
		void CopyToProxySproutBLevelDescriptor () {
			proxySproutBLevelDescriptor.isEnabled = selectedSproutBLevelDescriptor.isEnabled;
			proxySproutBLevelDescriptor.minFrequency = selectedSproutBLevelDescriptor.minFrequency;
			proxySproutBLevelDescriptor.maxFrequency = selectedSproutBLevelDescriptor.maxFrequency;
			proxySproutBLevelDescriptor.minParallelAlignAtTop = selectedSproutBLevelDescriptor.minParallelAlignAtTop;
			proxySproutBLevelDescriptor.maxParallelAlignAtTop = selectedSproutBLevelDescriptor.maxParallelAlignAtTop;
			proxySproutBLevelDescriptor.minParallelAlignAtBase = selectedSproutBLevelDescriptor.minParallelAlignAtBase;
			proxySproutBLevelDescriptor.maxParallelAlignAtBase = selectedSproutBLevelDescriptor.maxParallelAlignAtBase;
			proxySproutBLevelDescriptor.minGravityAlignAtTop = selectedSproutBLevelDescriptor.minGravityAlignAtTop;
			proxySproutBLevelDescriptor.maxGravityAlignAtTop = selectedSproutBLevelDescriptor.maxGravityAlignAtTop;
			proxySproutBLevelDescriptor.minGravityAlignAtBase = selectedSproutBLevelDescriptor.minGravityAlignAtBase;
			proxySproutBLevelDescriptor.maxGravityAlignAtBase = selectedSproutBLevelDescriptor.maxGravityAlignAtBase;
			proxySproutBLevelDescriptor.minRange = selectedSproutBLevelDescriptor.minRange;
			proxySproutBLevelDescriptor.maxRange = selectedSproutBLevelDescriptor.maxRange;
		}
		void CopyFromProxySproutBLevelDescriptor () {
			if (selectedSproutBLevelDescriptor != null) {
				selectedSproutBLevelDescriptor.isEnabled = proxySproutBLevelDescriptor.isEnabled;
				selectedSproutBLevelDescriptor.minFrequency = proxySproutBLevelDescriptor.minFrequency;
				selectedSproutBLevelDescriptor.maxFrequency = proxySproutBLevelDescriptor.maxFrequency;
				selectedSproutBLevelDescriptor.minParallelAlignAtTop = proxySproutBLevelDescriptor.minParallelAlignAtTop;
				selectedSproutBLevelDescriptor.maxParallelAlignAtTop = proxySproutBLevelDescriptor.maxParallelAlignAtTop;
				selectedSproutBLevelDescriptor.minParallelAlignAtBase = proxySproutBLevelDescriptor.minParallelAlignAtBase;
				selectedSproutBLevelDescriptor.maxParallelAlignAtBase = proxySproutBLevelDescriptor.maxParallelAlignAtBase;
				selectedSproutBLevelDescriptor.minGravityAlignAtTop = proxySproutBLevelDescriptor.minGravityAlignAtTop;
				selectedSproutBLevelDescriptor.maxGravityAlignAtTop = proxySproutBLevelDescriptor.maxGravityAlignAtTop;
				selectedSproutBLevelDescriptor.minGravityAlignAtBase = proxySproutBLevelDescriptor.minGravityAlignAtBase;
				selectedSproutBLevelDescriptor.maxGravityAlignAtBase = proxySproutBLevelDescriptor.maxGravityAlignAtBase;
				selectedSproutBLevelDescriptor.minRange = proxySproutBLevelDescriptor.minRange;
				selectedSproutBLevelDescriptor.maxRange = proxySproutBLevelDescriptor.maxRange;
			}
		}
		void CopyToProxySproutMap () {
			if (selectedSproutMap != null) {
				proxySproutMap.texture = selectedSproutMap.texture;
				proxySproutMap.normalMap = selectedSproutMap.normalMap;
				proxySproutMap.extraMap = selectedSproutMap.extraMap;
				proxySproutMap.subsurfaceMap = selectedSproutMap.subsurfaceMap;
				if (proxySproutMapDescriptor != null && selectedSproutMapDescriptor != null) {
					proxySproutMapDescriptor.alphaFactor = selectedSproutMapDescriptor.alphaFactor;
				}
			}
		}
		void CopyFromProxySproutMap () {
			selectedSproutMap.texture = proxySproutMap.texture;
			selectedSproutMap.normalMap = proxySproutMap.normalMap;
			selectedSproutMap.extraMap = proxySproutMap.extraMap;
			selectedSproutMap.subsurfaceMap = proxySproutMap.subsurfaceMap;
			if (selectedSproutMapDescriptor != null && selectedSproutMapDescriptor != null) {
				selectedSproutMapDescriptor.alphaFactor = proxySproutMapDescriptor.alphaFactor;
			}
		}
		#endregion

		#region Snapshots
		void InitSnapshots () {
			// Build GUIContents per branch.
			snapshots = new GUIContent[branchDescriptorCollection.branchDescriptors.Count];
			for (int i = 0; i < branchDescriptorCollection.branchDescriptors.Count; i++) {
				snapshots [i] = new GUIContent ("S" + i);	
			}
		}
		void DrawSnapshotsPanel () {
			EditorGUILayout.BeginHorizontal ();
			EditorGUI.BeginChangeCheck ();
			branchDescriptorCollection.branchDescriptorIndex = GUILayout.Toolbar (branchDescriptorCollection.branchDescriptorIndex, snapshots);
			if (EditorGUI.EndChangeCheck ()) {
				SelectSnapshot (branchDescriptorCollection.branchDescriptorIndex);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("Add Snapshot")) {
				AddSnapshot ();
			}
			if (GUILayout.Button ("Remove")) {
				RemoveSnapshot ();
			}
			EditorGUILayout.EndHorizontal ();
		}
		void AddSnapshot () {
			if (branchDescriptorCollection.branchDescriptors.Count < 10) {
				BranchDescriptor newBranchDescriptor = selectedBranchDescriptor.Clone ();
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.branchDescriptors.Add (newBranchDescriptor);
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				InitSnapshots ();
				SelectSnapshot (branchDescriptorCollection.branchDescriptors.Count - 1);
				ReflectChangesToPipeline ();
				RegeneratePreview ();
			}
		}
		void RemoveSnapshot () {
			if (branchDescriptorCollection.branchDescriptors.Count <= 1) {
				Debug.LogWarning ("At least a branch snapshot is required on the branch collection.");
			}
			if (EditorUtility.DisplayDialog (MSG_DELETE_BRANCH_DESC_TITLE, 
				MSG_DELETE_BRANCH_DESC_MESSAGE, 
				MSG_DELETE_BRANCH_DESC_OK, 
				MSG_DELETE_BRANCH_DESC_CANCEL)) {
				branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
				onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				branchDescriptorCollection.branchDescriptors.RemoveAt (branchDescriptorCollection.branchDescriptorIndex);
				SelectSnapshot (0);
				onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			}
		}
		public void SelectSnapshot (int index) {
			InitSnapshots ();
			if (!(index < branchDescriptorCollection.branchDescriptors.Count)) {
				index = 0;
			}
			branchDescriptorCollection.branchDescriptorIndex = index;
			this.selectedBranchDescriptor = branchDescriptorCollection.branchDescriptors [index];
			this.sproutSubfactory.branchDescriptorIndex = index;
		}
		#endregion

		#region Mesh Preview
		/// <summary>
		/// Get a preview mesh for a SproutMesh.
		/// </summary>
		/// <returns>Mesh for previewing.</returns>
		public Mesh GetPreviewMesh (SproutMesh sproutMesh, SproutMap.SproutMapArea sproutMapArea) {
			// TODO: optimize.
			return sproutSubfactory.treeFactory.previewTree.obj.GetComponent<MeshFilter> ().sharedMesh;
		}
		/// <summary>
		/// Show a preview mesh.
		/// </summary>
		/// <param name="index">Index.</param>
		public void ShowPreviewMesh (int index) {
			if (sproutSubfactory == null) return;
			Mesh mesh = GetPreviewMesh (null, null);
			Material material = new Material(Shader.Find ("Standard"));
			meshPreview.Clear ();
			meshPreview.CreateViewport ();
			mesh.RecalculateBounds();
			if (material != null) {
				meshPreview.AddMesh (0, mesh, material, true);
			} else {
				meshPreview.AddMesh (0, mesh, true);
			}
		}
		/// <summary>
		/// Draw additional handles on the mesh preview area.
		/// </summary>
		/// <param name="r">Rect</param>
		/// <param name="camera">Camera</param>
		public void OnPreviewMeshDrawHandles (Rect r, Camera camera) {
			if (showLightControls) {
				Handles.color = Color.yellow;
				Handles.ArrowHandleCap (0,
					//Vector3.zero, 
					meshPreview.GetLigthA ().transform.rotation * Vector3.back * 1.5f,
					meshPreview.GetLigthA ().transform.rotation, 
					1f * MeshPreview.GetHandleSize (Vector3.zero, camera), 
					EventType.Repaint);
			}

			// Temp display geometry point arrays.
			/*
			List<Vector3> points = GeometryAnalyzer.Current ().branchPoints;
			float scale = sproutSubfactory.treeFactory.treeFactoryPreferences.factoryScale;
			float handleSize;
			float handleSizeScale = 0.05f;
			Handles.color = Color.white;
			for (int i = 0; i < points.Count; i++) {
				handleSize = HandleUtility.GetHandleSize (points[i] * scale) * handleSizeScale;
				Handles.DotHandleCap (-1, points [i] * scale, Quaternion.identity, handleSize, EventType.Repaint);
			}
			points = GeometryAnalyzer.Current ().sproutPoints;
			for (int i = 0; i < points.Count; i++) {
				handleSize = HandleUtility.GetHandleSize (points[i] * scale) * handleSizeScale;
				Handles.DotHandleCap (-1, points [i] * scale, Quaternion.identity, handleSize, EventType.Repaint);
			}
			*/
			//Handles.DrawPolyLine (_convexHullPoints.ToArray ());
		}
		/// <summary>
		/// Draws GUI elements on the mesh preview area.
		/// </summary>
		/// <param name="r">Rect</param>
		/// <param name="camera">Camera</param>
		public void OnPreviewMeshDrawGUI (Rect r, Camera camera) {
			if (showLightControls) {
				DrawLightControls (r);
			}
			if (showProgressBar) {
				EditorGUI.ProgressBar(new Rect(0, 0, r.width, EditorGUIUtility.singleLineHeight), 
					progressBarProgress, progressBarTitle);
			}
		}
		/// <summary>
		/// Called when the mesh preview requires repaint.
		/// </summary>
		void OnMeshPreviewRequiresRepaint () {
			if (SproutFactoryEditorWindow.editorWindow != null)
				SproutFactoryEditorWindow.editorWindow.Repaint ();
		}
		public void DrawLightControls (Rect r) {
			r.x = 4;
			r.y = r.height + 2;
			r.height = EditorGUIUtility.singleLineHeight;
			r.width = 110;
			if (GUI.Button (r, "Light: " + lightAngleDisplayStr)) {
				AddLightStep ();
			}
		}
		public void AddLightStep () {
			if (lightAngleToAddTimeTmp <= 0) {
				SetEditorDeltaTime ();
				lightAngleToAddTimeTmp = lightAngleToAddTime;
				lightAngleEulerFrom = meshPreview.GetLigthA ().transform.rotation.eulerAngles;
				lightAngleEulerTo = lightAngleEulerFrom;
				lightAngleEulerTo.y += lightAngleStepValue;
				lightAngleStep++;
				if (lightAngleStep >= 8) lightAngleStep = 0;
				switch (lightAngleStep) {
					case 0: lightAngleDisplayStr = "Front";
						break;
					case 1:  lightAngleDisplayStr = "Left 45";
						break;
					case 2:  lightAngleDisplayStr = "Left";
						break;
					case 3:  lightAngleDisplayStr = "Left -45";
						break;
					case 4:  lightAngleDisplayStr = "Back";
						break;
					case 5:  lightAngleDisplayStr = "Right -45";
						break;
					case 6:  lightAngleDisplayStr = "Right";
						break;
					case 7:  lightAngleDisplayStr = "Right 45";
						break;
				}
			}
		}
		void ResetCameraDirection () {
			if (moveToCameraDirectionTimeTmp <= 0) {
				SetEditorDeltaTime ();
				moveToCameraDirectionTimeTmp = moveToCameraDirectionTime;
				moveToCameraDefaultDirectionTmp = meshPreview.GetDirection ();
			}
		}
		#endregion

		#region Persistence
		/// <summary>
		/// Creates a new branch descriptor collection.
		/// </summary>
		private void OnCreateNewBranchDescriptorCollectionSO () {}
		/// <summary>
		/// Loads a BanchDescriptorCollection from a file.
		/// </summary>
		/// <param name="loadedBranchDescriptorCollection">Branch collection loaded.</param>
		/// <param name="pathToFile">Path to file.</param>
		private void OnLoadBranchDescriptorCollectionSO (BranchDescriptorCollectionSO loadedBranchDescriptorCollectionSO, string pathToFile) {
			onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			LoadBranchDescriptorCollection (loadedBranchDescriptorCollectionSO.branchDescriptorCollection.Clone (), sproutSubfactory);
			onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			/*
			rockFactory.localPipeline = loadedBranchDescriptorCollection;
			rockFactory.localPipelineFilepath = pathToFile;
			LoadFactory (rockFactory, true);
			RequestUpdateRockPreview ();
			_isDirty = true;
			*/
		}
		/// <summary>
		/// Gets the branch descriptor collection to save when the user requests it.
		/// </summary>
		/// <returns>Object to save.</returns>
		private BranchDescriptorCollectionSO OnGetBranchDescriptorCollectionSOToSave () {
			BranchDescriptorCollectionSO toSave = ScriptableObject.CreateInstance<BranchDescriptorCollectionSO> ();
			toSave.branchDescriptorCollection = branchDescriptorCollection;
			return toSave;
		}
		/// <summary>
		/// Gets the path to file when the user requests it.
		/// </summary>
		/// <returns>The path to file or empty string if not has been set.</returns>
		private string OnGetBranchDescriptorCollectionSOToSaveFilePath () {
			return "";
		}
		/// <summary>
		/// Receives the object just saved.
		/// </summary>
		/// <param name="branchDescriptorCollectionSO">Saved object.</param>
		/// <param name="pathToFile">Path to file.</param>
		private void OnSaveBranchDescriptorCollectionSO (BranchDescriptorCollectionSO branchDescriptorCollectionSO, string pathToFile) {
			//LoadBranchDescriptorCollection (branchDescriptorCollectionSO.branchDescriptorCollection, sproutSubfactory);
		}
		#endregion

		#region Sprout Map List
		/// <summary>
		/// Inits the sprout map list.
		/// </summary>
		private void InitSproutMapLists () {
			// Sprout A Map List.
			sproutAMapList = new ReorderableList (branchDescriptorCollection.sproutAMapAreas, 
					typeof (SproutMap.SproutMapArea), false, true, true, true);
			sproutAMapList.draggable = false;
			sproutAMapList.drawHeaderCallback += DrawSproutMapListHeader;
			sproutAMapList.drawElementCallback += DrawSproutAMapListItemElement;
			sproutAMapList.onAddCallback += AddSproutAMapListItem;
			sproutAMapList.onRemoveCallback += RemoveSproutAMapListItem;
			// Sprout B Map List.
			sproutBMapList = new ReorderableList (branchDescriptorCollection.sproutBMapAreas, 
					typeof (SproutMap.SproutMapArea), false, true, true, true);
			sproutBMapList.draggable = false;
			sproutBMapList.drawHeaderCallback += DrawSproutMapListHeader;
			sproutBMapList.drawElementCallback += DrawSproutBMapListItemElement;
			sproutBMapList.onAddCallback += AddSproutBMapListItem;
			sproutBMapList.onRemoveCallback += RemoveSproutBMapListItem;
		}
		/// <summary>
		/// Draws the sprout map list header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawSproutMapListHeader (Rect rect) {
			GUI.Label(rect, "Sprout Maps", BroccoEditorGUI.labelBoldCentered);
		}
		/// <summary>
		/// Draws each sprout map list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutAMapListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			SproutMap.SproutMapArea sproutMapArea = branchDescriptorCollection.sproutAMapAreas [index];
			if (sproutMapArea != null) {
				GUI.Label (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight + 5), 
					"Textures for Leaf Type " + (index + 1));
				if (isActive) {
					if (selectedSproutMap != branchDescriptorCollection.sproutAMapAreas [index]) {
						selectedSproutMap = branchDescriptorCollection.sproutAMapAreas [index];
						selectedSproutMapGroup = 0;
						selectedSproutMapIndex = index;
						selectedSproutMapDescriptor = branchDescriptorCollection.sproutAMapDescriptors [index];
					}
					CopyToProxySproutMap ();
					EditorGUILayout.BeginVertical (GUILayout.Width (200));
					EditorGUI.BeginChangeCheck ();
					proxySproutMap.texture = (Texture2D) EditorGUILayout.ObjectField ("Main Texture", proxySproutMap.texture, typeof (Texture2D), false);
					proxySproutMap.normalMap = (Texture2D) EditorGUILayout.ObjectField ("Normal Texture", proxySproutMap.normalMap, typeof (Texture2D), false);
					//proxySproutMap.extraMap = (Texture2D) EditorGUILayout.ObjectField ("Extra Texture", proxySproutMap.extraMap, typeof (Texture2D), false);
					EditorGUILayout.EndVertical ();
					proxySproutMapDescriptor.alphaFactor = EditorGUILayout.Slider ("Alpha Factor", proxySproutMapDescriptor.alphaFactor, 0.7f, 1f);
					if (EditorGUI.EndChangeCheck ()) {
						WaitProcessTexture (sproutSubfactory.GetSproutTextureId (0, index), proxySproutMapDescriptor.alphaFactor);
						sproutMapChanged = true;
					}
				}
			}
		}
		/// <summary>
		/// Adds the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddSproutAMapListItem (ReorderableList list) {
			branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
			onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			if (branchDescriptorCollection.sproutAMapAreas.Count < 10) {
				SproutMap.SproutMapArea sproutMapArea= new SproutMap.SproutMapArea ();
				branchDescriptorCollection.sproutAMapAreas.Add (sproutMapArea);
				BranchDescriptorCollection.SproutMapDescriptor sproutMapDescriptor = new BranchDescriptorCollection.SproutMapDescriptor ();
				branchDescriptorCollection.sproutAMapDescriptors.Add (sproutMapDescriptor);
			}
			onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
		}
		/// <summary>
		/// Removes the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveSproutAMapListItem (ReorderableList list) {
			SproutMap.SproutMapArea sproutMap = branchDescriptorCollection.sproutAMapAreas [list.index];
			if (sproutMap != null) {
				if (EditorUtility.DisplayDialog (MSG_DELETE_SPROUT_MAP_TITLE, 
					MSG_DELETE_SPROUT_MAP_MESSAGE, 
					MSG_DELETE_SPROUT_MAP_OK, 
					MSG_DELETE_SPROUT_MAP_CANCEL)) {
					branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
					onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					branchDescriptorCollection.sproutAMapAreas.RemoveAt (list.index);
					selectedSproutMap = null;
					selectedSproutMapDescriptor = null;
					onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				}
			}
		}
		/// <summary>
		/// Draws each sprout map list item element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutBMapListItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			SproutMap.SproutMapArea sproutMapArea = branchDescriptorCollection.sproutBMapAreas [index];
			if (sproutMapArea != null) {
				GUI.Label (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight + 5), 
					"Textures for Leaf Type " + (index + 1));
				if (isActive) {
					if (selectedSproutMap != branchDescriptorCollection.sproutBMapAreas [index]) {
						selectedSproutMap = branchDescriptorCollection.sproutBMapAreas [index];
						selectedSproutMapGroup = 1;
						selectedSproutMapIndex = 0;
						selectedSproutMapDescriptor = branchDescriptorCollection.sproutBMapDescriptors [index];
					}
					CopyToProxySproutMap ();
					EditorGUILayout.BeginVertical (GUILayout.Width (200));
					EditorGUI.BeginChangeCheck ();
					proxySproutMap.texture = (Texture2D) EditorGUILayout.ObjectField ("Main Texture", proxySproutMap.texture, typeof (Texture2D), false);
					proxySproutMap.normalMap = (Texture2D) EditorGUILayout.ObjectField ("Normal Texture", proxySproutMap.normalMap, typeof (Texture2D), false);
					//proxySproutMap.extraMap = (Texture2D) EditorGUILayout.ObjectField ("Extra Texture", proxySproutMap.extraMap, typeof (Texture2D), false);
					EditorGUILayout.EndVertical ();
					proxySproutMapDescriptor.alphaFactor = EditorGUILayout.Slider ("Alpha Factor", proxySproutMapDescriptor.alphaFactor, 0f, 1f);
					if (EditorGUI.EndChangeCheck ()) {
						WaitProcessTexture (sproutSubfactory.GetSproutTextureId (1, index), proxySproutMapDescriptor.alphaFactor);
						sproutMapChanged = true;
					}
				}
			}
		}
		/// <summary>
		/// Adds the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void AddSproutBMapListItem (ReorderableList list) {
			branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
			onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
			if (branchDescriptorCollection.sproutBMapAreas.Count < 10) {
				SproutMap.SproutMapArea sproutMapArea= new SproutMap.SproutMapArea ();
				branchDescriptorCollection.sproutBMapAreas.Add (sproutMapArea);
				BranchDescriptorCollection.SproutMapDescriptor sproutMapDescriptor = new BranchDescriptorCollection.SproutMapDescriptor ();
				branchDescriptorCollection.sproutBMapDescriptors.Add (sproutMapDescriptor);
			}
			onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
		}
		/// <summary>
		/// Removes the sprout map list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void RemoveSproutBMapListItem (ReorderableList list) {
			SproutMap.SproutMapArea sproutMap = branchDescriptorCollection.sproutBMapAreas [list.index];
			if (sproutMap != null) {
				if (EditorUtility.DisplayDialog (MSG_DELETE_SPROUT_MAP_TITLE, 
					MSG_DELETE_SPROUT_MAP_MESSAGE, 
					MSG_DELETE_SPROUT_MAP_OK, 
					MSG_DELETE_SPROUT_MAP_CANCEL)) {
					branchDescriptorCollection.lastBranchDescriptorIndex = branchDescriptorCollection.branchDescriptorIndex;
					onBeforeBranchDescriptorChange?.Invoke (branchDescriptorCollection);
					branchDescriptorCollection.sproutBMapAreas.RemoveAt (list.index);
					selectedSproutMap = null;
					onBranchDescriptorChange?.Invoke (branchDescriptorCollection);
				}
			}
		}
		#endregion
		
		#region Export Process
		void ExportTextures () {
			// Generate Snapshot Texture
			if (branchDescriptorCollection.exportMode == BranchDescriptorCollection.ExportMode.SelectedSnapshot) {
				ExportTexturesSingleSnapshot ();
			} else {
				// Generate atlas texture.
				ExportTexturesAtlas ();
			}
		}
		void ExportTexturesSingleSnapshot () {
			int index = branchDescriptorCollection.branchDescriptorIndex;
			string basePath = "Assets" + branchDescriptorCollection.exportPath;
			bool isValid = false;
			string albedoPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, false);
			string normalPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Normals, false);
			string extrasPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Extras, false);
			string subsurfacePath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Subsurface, false);
			string compositePath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Composite, false);
			bool done = sproutSubfactory.GenerateSnapshopTextures (
				branchDescriptorCollection.branchDescriptorIndex,
				branchDescriptorCollection,
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				albedoPath, normalPath, extrasPath, subsurfacePath, compositePath);
			if (done) {
				onShowNotification?.Invoke ("Textures for Snapshot S" + index + " saved at: \n" + basePath);
			}
		}
		void ExportTexturesAtlas () {
			bool isValid = false;
			string basePath = "Assets" + branchDescriptorCollection.exportPath;
			string albedoPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Albedo, true);
			string normalPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Normals, true);
			string extrasPath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Extras, true);
			string subsurfacePath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Subsurface, true);
			string compositePath = GetTextureFileName (branchDescriptorCollection.exportTake, out isValid, SproutSubfactory.MaterialMode.Composite, true);
			bool done = sproutSubfactory.GenerateAtlasTexture (
				branchDescriptorCollection,
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				GetTextureSize (branchDescriptorCollection.exportTextureSize),
				branchDescriptorCollection.exportAtlasPadding,
				albedoPath, normalPath, extrasPath, subsurfacePath, compositePath);
			if (done) {
				onShowNotification?.Invoke ("Atlas textures saved at: \n" + basePath);
			}
		}
		int GetTextureSize (BranchDescriptorCollection.TextureSize textureSize) {
			if (textureSize == BranchDescriptorCollection.TextureSize._2048px) {
				return 2048;
			} else if (textureSize == BranchDescriptorCollection.TextureSize._1024px) {
				return 1024;
			} else if (textureSize == BranchDescriptorCollection.TextureSize._512px) {
				return 512;
			} else if (textureSize == BranchDescriptorCollection.TextureSize._256px) {
				return 256;
			} else {
				return 128;
			}
		}
		string GetTextureFileName (int take, out bool isValid, SproutSubfactory.MaterialMode materialMode, bool isAtlas) {
			isValid = false;
			string path = "";
			string takeString;
			if (take <= 0) {
				takeString = "000";
			} else if (take < 10) {
				takeString = "00" + take;
			} else if (take < 100) {
				takeString = "0" + take;
			} else {
				takeString = take.ToString ();
			}
			string modeString;
			if (materialMode == SproutSubfactory.MaterialMode.Albedo) {
				modeString = "albedo";
			} else if (materialMode == SproutSubfactory.MaterialMode.Normals) {
				modeString = "normals";
			} else if (materialMode == SproutSubfactory.MaterialMode.Extras) {
				modeString = "extras";
			} else if (materialMode == SproutSubfactory.MaterialMode.Subsurface) {
				modeString = "subsurface";
			} else {
				modeString = "composite";
			}
			if (!string.IsNullOrEmpty (branchDescriptorCollection.exportFileName)) {
				isValid = true;
			}
			path = "Assets" + branchDescriptorCollection.exportPath + "/" + (isAtlas?"atlas_":"snapshot_") + takeString + "_" + modeString + ".png";
			return path;
		}
		void OnReportProgress (string title, float progress) {
			if (!showProgressBar) {
				showProgressBar = true;
			}
			progressBarProgress = progress;
			progressBarTitle = title;
			//UnityEditor.EditorUtility.DisplayProgressBar (sproutSubfactory.progressTitle, title, progress);
			//UnityEditor.EditorUtility.DisplayCancelableProgressBar (sproutSubfactory.progressTitle, title, progress);
			EditorGUI.ProgressBar(new Rect (0, 0, meshPreviewRect.width, 
				EditorGUIUtility.singleLineHeight), progressBarProgress, progressBarTitle);
			meshPreview.RenderViewport (meshPreviewRect, GUIStyle.none, currentPreviewMaterials);
			EditorWindow view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
			view.Repaint ();
			InternalEditorUtility.RepaintAllViews ();
		}
		void OnFinishProgress () {
			showProgressBar = false;
			//UnityEditor.EditorUtility.ClearProgressBar ();
			//GUIUtility.ExitGUI();
		}
		#endregion

		#region Editor Updates
		double editorDeltaTime = 0f;
		double lastTimeSinceStartup = 0f;
		double secondsToUpdateTexture = 0f;
		string _textureId = "";
		float _alpha = 1.0f;
		/// <summary>
		/// Raises the editor update event.
		/// </summary>
		void OnEditorUpdate () {
			if (secondsToUpdateTexture > 0) {
				SetEditorDeltaTime();
				secondsToUpdateTexture -= (float) editorDeltaTime;
				if (secondsToUpdateTexture < 0) {
					sproutSubfactory.ProcessTexture (selectedSproutMapGroup, selectedSproutMapIndex, _alpha);
					secondsToUpdateTexture = 0;
					EditorWindow view;
					if (useTreeFactoryEditorWindow) {
						view = EditorWindow.GetWindow<TreeFactoryEditorWindow>();
					} else {
						view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
					}
					view.Repaint();
				}
			}
			if (lightAngleToAddTimeTmp >= 0f) {
				SetEditorDeltaTime();
				lightAngleToAddTimeTmp -= (float)editorDeltaTime;
				UpdateLightAngle ();
				EditorWindow view;
				if (useTreeFactoryEditorWindow) {
					view = EditorWindow.GetWindow<TreeFactoryEditorWindow>();
				} else {
					view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
				}
				view.Repaint();
			}
			if (moveToCameraDirectionTimeTmp >= 0f) {
				SetEditorDeltaTime();
				moveToCameraDirectionTimeTmp -= (float)editorDeltaTime;
				meshPreview.SetDirection (Vector2.Lerp(moveToCameraDefaultDirection, moveToCameraDefaultDirectionTmp, moveToCameraDirectionTimeTmp/moveToCameraDirectionTime));
				EditorWindow view;
				if (useTreeFactoryEditorWindow) {
					view = EditorWindow.GetWindow<TreeFactoryEditorWindow>();
				} else {
					view = EditorWindow.GetWindow<SproutFactoryEditorWindow>();
				}
				view.Repaint();
			}
		}
		void SetEditorDeltaTime ()
		{
			#if UNITY_EDITOR
			if (lastTimeSinceStartup == 0f)
			{
				lastTimeSinceStartup = EditorApplication.timeSinceStartup;
			}
			editorDeltaTime = EditorApplication.timeSinceStartup - lastTimeSinceStartup;
			lastTimeSinceStartup = EditorApplication.timeSinceStartup;
			#endif
		}
		void WaitProcessTexture (string textureId, float alpha) {
			secondsToUpdateTexture = 0.5f;
			_textureId = textureId;
			_alpha = alpha;
			SetEditorDeltaTime ();
		}
		void UpdateLightAngle () {
			Vector3 angle = Vector3.Lerp (lightAngleEulerFrom, lightAngleEulerTo, Mathf.InverseLerp (lightAngleToAddTime, 0, lightAngleToAddTimeTmp));
			meshPreview.GetLigthA ().transform.rotation = Quaternion.Euler (angle);
		}
		#endregion
	}
}