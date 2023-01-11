using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Builder;
using Broccoli.Manager;
using Broccoli.Factory;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sprout mesh generator node editor.
	/// </summary>
	[CustomEditor(typeof(SproutMeshGeneratorNode))]
	public class SproutMeshGeneratorNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The sprout mesh generator node.
		/// </summary>
		public SproutMeshGeneratorNode sproutMeshGeneratorNode;
		/// <summary>
		/// The meshes list.
		/// </summary>
		ReorderableList meshesList;
		/// <summary>
		/// Normal mode for the sprout mesh.
		/// </summary>
		SerializedProperty propNormalMode;
		/// <summary>
		/// Normal mode lerp  from original sprout mesh normals.
		/// </summary>
		SerializedProperty propNormalModeStrength;
		/// <summary>
		/// Mesh preview utility.
		/// </summary>
		MeshPreview meshPreview;
		bool meshPreviewEnabled = true;
		Dictionary<int, Mesh> previewMeshes = new Dictionary<int, Mesh> ();
		Dictionary<int, Material> previewMaterials = new Dictionary<int, Material> ();
		private GUIContent previewTitleGUIContent;
		private static Rect scaleCurveRange = new Rect (0f, 0f, 1f, 1f);
		//private static Rect billboardRotationCurveRange = new Rect (0f, 0f, 1f, 1f);
		private static Rect gravityBendingCurveRange = new Rect (0f, 0f, 1f, 1f);
		SerializedProperty propSproutMeshes;
		GUIStyle pivotLabelStyle = new GUIStyle ();
		GUIStyle gravityVectorLabelStyle = new GUIStyle ();
		/// <summary>
		/// Flag to use autozoom only on the first mesh.
		/// </summary>
		bool autoZoomUsed = false;
		#endregion

		#region Messages
		private static string MSG_SPROUT_GROUP = "Sprout group this mesh group belongs to.";
		private static string MSG_MODE = "Mode used to generate the sprouts.";
		private static string MSG_DEPTH = "Depth of the sprout mesh on its center.";
		private static string MSG_WIDTH = "Width for the mesh plane.";
		private static string MSG_HEIGHT = "Height for the mesh plane.";
		private static string MSG_PIVOT_X = "The x coordinate for the sprout point of origin on the mesh.";
		private static string MSG_PIVOT_Y = "The y coordinate for the sprout point of origin on the mesh.";
		private static string MSG_OVERRIDE_HEIGHT_WITH_TEXTURE = "Check this to let the height be set " +
			"keeping the aspect radio of the texture assigned to the sprout group.";
		private static string MSG_INCLUDE_SCALE_FROM_ATLAS = "Check this to apply an automatic scaling from the area the mapping " +
			"for this sprout takes on an texture atlas. This helps you get uniformed scaled sprout meshes when they come from a single texture atlas.";
		private static string MSG_SCALE_AT_BASE = "Scale of the sprouts at the base of the branch.";
		private static string MSG_SCALE_AT_TOP = "Scale of the sprouts at the top of the branch.";
		private static string MSG_SCALE_CURVE = "Curve used to transition between scale at base and at top values.";
		private static string MSG_HORIZONTAL_ALIGN_AT_BASE = "Horizontal plane alignment of the plane sprouts at the base of the branch.";
		private static string MSG_HORIZONTAL_ALIGN_AT_TOP = "Horizontal plane alignment of the plane sprouts at the top of the branch.";
		private static string MSG_HORIZONTAL_ALIGN_CURVE = "Curve used to transition between horizontal align at base and at top values.";
		/*
		private static string MSG_BILLBOARD_AT_ORIGIN = "Check this to get the billboards placed at sprout origin.";
		private static string MSG_BILLBOARD_ROTATION_AT_TOP = "Rotation for the billboard plane at top of the branch.";
		private static string MSG_BILLBOARD_ROTATION_AT_BASE = "Rotation for the billboard plane at base of the branch.";
		private static string MSG_BILLBOARD_ROTATION_CURVE = "Curve used to transition between rotation at base and at top values.";
		*/
		private static string MSG_MESH = "Custom mesh to use as model to create the sprout meshes.";
		private static string MSG_MESH_SCALE = "Scale value for the custom mesh.";
		private static string MSG_MESH_ROTATION = "Rotation used on the custom mesh center.";
		private static string MSG_MESH_OFFSET = "Offset used on the custom mesh center.";
		private static string MSG_RESOLUTION_WIDTH = "Number of divisions for the plane on the width side of it.";
		private static string MSG_RESOLUTION_HEIGHT = "Number of divisions for the plane on the height side of it.";
		private static string MSG_GRAVITY_BENDING_AT_BASE = "How much the sprouts bend against gravity at the base of the branches.";
		private static string MSG_GRAVITY_BENDING_AT_TOP = "How much the sprouts bend agains gravity at the top of the branches.";
		private static string MSG_GRAVITY_BENDING_CURVE = "Curve to distribute gravity bending along the hierarchy of the tree.";
		private static string MSG_NORMAL_MODE = " Mode to calculate normals on the sprout mesh:\n" +
			"1. PerSprout: default normals and relative to the each sprout position.\n" +
			"2. TreeOrigin: normals relative to the origin of the whole tree.\n" +
			"3. SproutsCenter: normals calculated from the center of the whole sprout mesh bounds.\n" +
			"4. SproutsBase: normals calculated from the bottom of the whole sprout mesh bounds.";
		private static string MSG_NORMAL_MODE_STRENGTH = "How much the normals transition from default to the selected mode.";
		private static string MSG_GRAVITY_BENDING_MULTIPLIER_AT_MIDDLE = "";
		private static string MSG_SIDE_GRAVITY_BENDING_AT_BASE = "";
		private static string MSG_SIDE_GRAVITY_BENDING_AT_TOP = "";
		private static string MSG_SIDE_GRAVITY_BENDING_SHAPE = "";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			sproutMeshGeneratorNode = target as SproutMeshGeneratorNode;

			SetPipelineElementProperty ("sproutMeshGeneratorElement");

			// Init Mesh List
			propSproutMeshes = GetSerializedProperty ("sproutMeshes");
			meshesList = new ReorderableList (serializedObject, propSproutMeshes, false, true, true, true);

			meshesList.draggable = false;
			meshesList.drawHeaderCallback += DrawMeshItemHeader;
			meshesList.drawElementCallback += DrawMeshItemElement;
			meshesList.onSelectCallback += OnSelectMeshItem;
			meshesList.onAddCallback += OnAddMeshItem;
			meshesList.onRemoveCallback += OnRemoveMeshItem;

			propNormalMode = GetSerializedProperty ("normalMode");
			propNormalModeStrength = GetSerializedProperty ("normalModeStrength");

			// Init mesh preview
			if (meshPreview == null) {
				meshPreview = new MeshPreview ();
				meshPreview.showPivot = true;
				meshPreview.onDrawHandles += OnPreviewMeshDrawHandles;
				meshPreview.onDrawGUI += OnPreviewMeshDrawGUI;
				pivotLabelStyle.normal.textColor = Color.yellow;
				gravityVectorLabelStyle.normal.textColor = Color.green;
				if (sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex > -1) {
					ShowPreviewMesh (sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex);
				}
			} else {
				meshPreview.Clear ();
			}
			meshPreview.CreateViewport ();
			//meshPreview.AddMesh (0, sproutLabComponent.GetMesh (null, 0), true);
			if (previewTitleGUIContent == null) {
				previewTitleGUIContent = new GUIContent ("Sprout Preview");
			}
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			sproutMeshGeneratorNode.selectedToolbar = GUILayout.Toolbar (sproutMeshGeneratorNode.selectedToolbar, new string[] {"Sprout Groups", "Mesh Data"});
			EditorGUILayout.Space ();

			EditorGUI.BeginChangeCheck ();

			if (sproutMeshGeneratorNode.selectedToolbar == 0) {
				// Log box.
				DrawLogBox ();
				if (sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex != meshesList.index &&
					sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex < meshesList.count) {
					meshesList.index = sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex;
				}
				meshesList.DoLayoutList ();
			} else {
				EditorGUILayout.PropertyField (propNormalMode);
				ShowHelpBox (MSG_NORMAL_MODE);
				if (propNormalMode.intValue != (int)SproutMeshGeneratorElement.NormalMode.PerSprout) {
					EditorGUILayout.Slider (propNormalModeStrength, 0f, 1f, "Normal Mode Strength");
					ShowHelpBox (MSG_NORMAL_MODE_STRENGTH);
				}
			}
			EditorGUILayout.Space ();

			if (EditorGUI.EndChangeCheck ()) {
				ApplySerialized ();
				var meshesEnumerator = previewMeshes.GetEnumerator ();
				while (meshesEnumerator.MoveNext ()) {
					DestroyImmediate (meshesEnumerator.Current.Value);
				}
				previewMeshes.Clear ();
				var materialsEnumerator = previewMaterials.GetEnumerator ();
				while (materialsEnumerator.MoveNext ()) {
					DestroyImmediate (materialsEnumerator.Current.Value);
				}
				previewMaterials.Clear ();
				ShowPreviewMesh (meshesList.index);
				UpdatePipeline (GlobalSettings.processingDelayVeryHigh);
				NodeEditorFramework.NodeEditor.RepaintClients ();
				SetUndoControlCounter ();
			}

			// Field descriptors option.
			DrawFieldHelpOptions ();
		}
		/// <summary>
		/// Raises the on disable event.
		/// </summary>
		private void OnDisable() {
			if (meshPreview != null) {
				meshPreview.Clear ();
			}
		}
		/// <summary>
		/// Event called when destroying this editor.
		/// </summary>
		private void OnDestroy() {
			var enumerator = previewMeshes.GetEnumerator ();
			while (enumerator.MoveNext ()) {
				DestroyImmediate (enumerator.Current.Value);
			}
			previewMeshes.Clear ();
			var matEnumerator = previewMaterials.GetEnumerator ();
			while (matEnumerator.MoveNext ()) {
				DestroyImmediate (matEnumerator.Current.Value);
			}
			previewMaterials.Clear ();
			meshPreview.Clear ();
			if (meshPreview.onDrawHandles != null) {
				meshPreview.onDrawHandles -= OnPreviewMeshDrawHandles;
				meshPreview.onDrawGUI -= OnPreviewMeshDrawGUI;
			}
		}
		#endregion

		#region Mesh Ordereable List
		/// <summary>
		/// Draws the list item header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawMeshItemHeader(Rect rect)
		{
			GUI.Label(rect, "Meshes");
		}
		/// <summary>
		/// Draws the list item element.
		/// </summary>
		/// <param name="rect">Rect to draw to.</param>
		/// <param name="index">Index of the item.</param>
		/// <param name="isActive">If set to <c>true</c> the item is active.</param>
		/// <param name="isFocused">If set to <c>true</c> the item is focused.</param>
		private void DrawMeshItemElement (Rect rect, int index, bool isActive, bool isFocused) {
			var sproutMesh = meshesList.serializedProperty.GetArrayElementAtIndex (index);

			int sproutGroupId = sproutMesh.FindPropertyRelative ("groupId").intValue;
			if (sproutGroupId > 0) {
				rect.y += 2;
				SproutGroups sproutGroups = 
					sproutMeshGeneratorNode.sproutMeshGeneratorElement.pipeline.sproutGroups;
				EditorGUI.DrawRect (new Rect (rect.x, rect.y, 
					EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), 
					sproutGroups.GetSproutGroupColor(sproutGroupId));
				rect.x += 22;
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight), 
					"Assigned to group " + sproutGroupId);
			} else {
				rect.y += 2;
				EditorGUI.DrawRect (new Rect (rect.x, rect.y, 
					EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), 
					Color.black);
				rect.x += 22;
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 
					150, EditorGUIUtility.singleLineHeight), "Unassigned mesh");
			}

			if (isActive) {
				if (index != sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex) {
					sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex = index;
					ShowPreviewMesh (index);
				}

				EditorGUILayout.Space ();
				SproutGroups sproutGroups = sproutMeshGeneratorNode.sproutMeshGeneratorElement.pipeline.sproutGroups;
				SproutMesh sproutMeshObj = sproutMeshGeneratorNode.sproutMeshGeneratorElement.sproutMeshes [index];

				// Sprout group.
				int sproutGroupIndex = EditorGUILayout.Popup ("Sprout Group",
					sproutMeshGeneratorNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupIndex (sproutMeshObj.groupId, true),
					sproutMeshGeneratorNode.pipelineElement.pipeline.sproutGroups.GetPopupOptions (true));
				int selectedSproutGroupId = 
					sproutMeshGeneratorNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupId (sproutGroupIndex);
				if (sproutMeshObj.groupId != selectedSproutGroupId) {
					if (sproutMeshGeneratorNode.sproutMeshGeneratorElement.GetSproutGroupsAssigned ().Contains (selectedSproutGroupId)) {
						Debug.LogWarning ("The sprout group has already been assigned to a mesh.");
					} else {
						sproutMesh.FindPropertyRelative ("groupId").intValue = selectedSproutGroupId;
					}
				}
				ShowHelpBox (MSG_SPROUT_GROUP);

				// Mode.
				EditorGUILayout.PropertyField (sproutMesh.FindPropertyRelative ("mode"));
				SproutMesh.Mode sproutMode = sproutMeshObj.mode;
				ShowHelpBox (MSG_MODE);
				/*
				if (sproutMode == SproutMesh.Mode.Billboard) {
					EditorGUILayout.HelpBox ("Billboard meshes are only available when using the Tree Creator shader.", MessageType.Info);
				}
				*/
				EditorGUILayout.Space ();

				if (sproutMode != SproutMesh.Mode.Mesh) {
					// Size and origin.
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("width"), 0f, 10f);
					ShowHelpBox (MSG_WIDTH);
					if (!sproutMesh.FindPropertyRelative ("overrideHeightWithTexture").boolValue) {
						EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("height"), 0f, 10f);
						ShowHelpBox (MSG_HEIGHT);
					}
					EditorGUILayout.PropertyField (sproutMesh.FindPropertyRelative ("overrideHeightWithTexture"), 
						new GUIContent ("Override Height with Texture", "Height is proportional to the assigned texture dimension ratio."));
					ShowHelpBox (MSG_OVERRIDE_HEIGHT_WITH_TEXTURE);
					EditorGUILayout.PropertyField (sproutMesh.FindPropertyRelative ("includeScaleFromAtlas"), 
						new GUIContent ("Include Scale from Atlas", "Apply scaling according to the mapping of a texture atlas."));
					ShowHelpBox (MSG_INCLUDE_SCALE_FROM_ATLAS);
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("pivotX"), 0f, 1f);
					ShowHelpBox (MSG_PIVOT_X);
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("pivotY"), 0f, 1f);
					ShowHelpBox (MSG_PIVOT_Y);
					EditorGUILayout.Space ();
				}
				/*
				if (sproutMode == SproutMesh.Mode.Billboard) {
					// Billboard mode.
					EditorGUILayout.PropertyField (sproutMesh.FindPropertyRelative ("billboardAtOrigin"));
					ShowHelpBox (MSG_BILLBOARD_AT_ORIGIN);
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("billboardRotationAtTop"), 
						-4f, 4f, "Rotation at Top");
					ShowHelpBox (MSG_BILLBOARD_ROTATION_AT_TOP);
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("billboardRotationAtBase"), 
						-4f, 4f, "Rotation at Base");
					ShowHelpBox (MSG_BILLBOARD_ROTATION_AT_BASE);
					EditorGUILayout.CurveField (sproutMesh.FindPropertyRelative ("billboardRotationCurve"),
						sproutGroups.GetSproutGroupColor (sproutGroupId), billboardRotationCurveRange, 
						new GUIContent ("Rotation Curve"));
					ShowHelpBox (MSG_BILLBOARD_ROTATION_CURVE);
				} else 
				*/
				if (sproutMode == SproutMesh.Mode.Mesh) {
					// Mesh mode.
					sproutMesh.FindPropertyRelative ("meshGameObject").objectReferenceValue =
					EditorGUILayout.ObjectField ("Mesh", sproutMesh.FindPropertyRelative ("meshGameObject").objectReferenceValue, typeof(GameObject), false);
					ShowHelpBox (MSG_MESH);
					EditorGUILayout.PropertyField (sproutMesh.FindPropertyRelative ("meshScale"), new GUIContent ("Mesh Scale"));
					ShowHelpBox (MSG_MESH_SCALE);
					EditorGUILayout.PropertyField (sproutMesh.FindPropertyRelative ("meshRotation"), new GUIContent ("Mesh Rotation"));
					ShowHelpBox (MSG_MESH_ROTATION);
					EditorGUILayout.PropertyField (sproutMesh.FindPropertyRelative ("meshOffset"), new GUIContent ("Mesh Offset"));
					ShowHelpBox (MSG_MESH_OFFSET);
				} else if (sproutMode == SproutMesh.Mode.PlaneX) {
					// Plane X mode.
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("depth"), -3f, 3f, "Depth");
					ShowHelpBox (MSG_DEPTH);
				} else if (sproutMode == SproutMesh.Mode.GridPlane) {
					// Grid plane mode.
					EditorGUILayout.IntSlider (sproutMesh.FindPropertyRelative ("resolutionWidth"), 1, 10, "Grid Width Resolution");
					ShowHelpBox (MSG_RESOLUTION_WIDTH);
					EditorGUILayout.IntSlider (sproutMesh.FindPropertyRelative ("resolutionHeight"), 1, 10, "Grid Height Resolution");
					ShowHelpBox (MSG_RESOLUTION_HEIGHT);
					EditorGUILayout.Space ();
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("gravityBendingAtBase"), 
						-1f, 1f, "Gravity Bending at Base");
					ShowHelpBox (MSG_GRAVITY_BENDING_AT_BASE);
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("gravityBendingAtTop"), 
						-1f, 1f, "Gravity Bending at Top");
					ShowHelpBox (MSG_GRAVITY_BENDING_AT_TOP);
					EditorGUILayout.CurveField (sproutMesh.FindPropertyRelative ("gravityBendingCurve"),
						sproutGroups.GetSproutGroupColor (sproutGroupId), gravityBendingCurveRange, 
						new GUIContent ("Gravity Bending Curve"));
					ShowHelpBox (MSG_GRAVITY_BENDING_CURVE);
					EditorGUILayout.Space ();
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("gravityBendingMultiplierAtMiddle"), 
						-1f, 1f, "Bending Middle Multiplier");
					ShowHelpBox (MSG_GRAVITY_BENDING_MULTIPLIER_AT_MIDDLE);
					EditorGUILayout.Space ();
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("sideGravityBendingAtBase"), 
						-1f, 1f, "Side Gravity at Base");
					ShowHelpBox (MSG_SIDE_GRAVITY_BENDING_AT_TOP);
					EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("sideGravityBendingAtTop"), 
						-1f, 1f, "Side Gravity at Top");
					ShowHelpBox (MSG_SIDE_GRAVITY_BENDING_AT_BASE);
					EditorGUILayout.CurveField (sproutMesh.FindPropertyRelative ("sideGravityBendingShape"),
						sproutGroups.GetSproutGroupColor (sproutGroupId), gravityBendingCurveRange, 
						new GUIContent ("Gravity Bending Curve"));
					ShowHelpBox (MSG_SIDE_GRAVITY_BENDING_SHAPE);
				}

				// Scale.
				EditorGUILayout.Space ();
				EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("scaleAtTop"), 0f, 5f);
				ShowHelpBox (MSG_SCALE_AT_TOP);
				EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("scaleAtBase"), 0f, 5f);
				ShowHelpBox (MSG_SCALE_AT_BASE);
				EditorGUILayout.CurveField (sproutMesh.FindPropertyRelative ("scaleCurve"),
					sproutGroups.GetSproutGroupColor(sproutGroupId), scaleCurveRange);
				ShowHelpBox (MSG_SCALE_CURVE);

				// Horizontal alignment.
				EditorGUILayout.Space ();
				EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("horizontalAlignAtTop"), -1f, 1f);
				ShowHelpBox (MSG_HORIZONTAL_ALIGN_AT_TOP);
				EditorGUILayout.Slider (sproutMesh.FindPropertyRelative ("horizontalAlignAtBase"), -1f, 1f);
				ShowHelpBox (MSG_HORIZONTAL_ALIGN_AT_BASE);
				EditorGUILayout.CurveField (sproutMesh.FindPropertyRelative ("horizontalAlignCurve"),
					sproutGroups.GetSproutGroupColor(sproutGroupId), scaleCurveRange);
				ShowHelpBox (MSG_HORIZONTAL_ALIGN_CURVE);
			}
		}
		/// <summary>
		/// Raises the select mesh item event.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnSelectMeshItem (ReorderableList list)
		{
			Undo.RecordObject (sproutMeshGeneratorNode.sproutMeshGeneratorElement, "Sprout Mesh selected");
			sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex = list.index;
			ShowPreviewMesh (list.index);
		}
		/// <summary>
		/// Adds a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddMeshItem (ReorderableList list)
		{
			if (sproutMeshGeneratorNode.sproutMeshGeneratorElement.CanAddSproutMesh ()) {
				SproutMesh sproutMesh = new SproutMesh ();
				Undo.RecordObject (sproutMeshGeneratorNode.sproutMeshGeneratorElement, "Sprout Mesh added");
				sproutMeshGeneratorNode.sproutMeshGeneratorElement.AddSproutMesh (sproutMesh);
			}
		}
		/// <summary>
		/// Removes a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnRemoveMeshItem (ReorderableList list)
		{
			int undoGroup = Undo.GetCurrentGroup ();
			Undo.SetCurrentGroupName ("Sprout Mesh removed");
			Undo.RecordObject (sproutMeshGeneratorNode.sproutMeshGeneratorElement, "Sprout Mesh removed");
			sproutMeshGeneratorNode.sproutMeshGeneratorElement.RemoveSproutMesh (list.index);
			Undo.RecordObject (sproutMeshGeneratorNode.sproutMeshGeneratorElement, "Sprout Mesh removed");
			sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex = -1;
			Undo.CollapseUndoOperations (undoGroup);
		}
		#endregion

		#region Mesh Preview
		/// <summary>
		/// Determines whether this instance has preview GUI.
		/// </summary>
		/// <returns><c>true</c> if this instance has preview GU; otherwise, <c>false</c>.</returns>
		public override bool HasPreviewGUI () {
			if (sproutMeshGeneratorNode.selectedToolbar == 1) return false;
			if (meshPreviewEnabled &&
				sproutMeshGeneratorNode.sproutMeshGeneratorElement.sproutMeshes.Count > 0 &&
				sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex > -1) {
				int index = sproutMeshGeneratorNode.sproutMeshGeneratorElement.selectedMeshIndex;
				SproutMesh sproutMesh = sproutMeshGeneratorNode.sproutMeshGeneratorElement.sproutMeshes [index];
				/*
				if (sproutMesh.mode == SproutMesh.Mode.Billboard) {
					return false;
				} else
				*/
				if (sproutMesh.mode == SproutMesh.Mode.Mesh && sproutMesh.meshGameObject == null) {
					return false;
				}
				return true;
			}
			return false;
		}
		/// <summary>
		/// Gets the preview title.
		/// </summary>
		/// <returns>The preview title.</returns>
		public override GUIContent GetPreviewTitle () {
			return previewTitleGUIContent;
		}
		/// <summary>
		/// Raises the interactive preview GUI event.
		/// </summary>
		/// <param name="r">Rect to draw to.</param>
		/// <param name="background">Background.</param>
		public override void OnInteractivePreviewGUI (Rect r, GUIStyle background) {
			//Mesh renderer missing?
			if(meshPreview == null)	{
				//EditorGUI.DropShadowLabel is used often in these preview areas - it 'fits' well.
				EditorGUI.DropShadowLabel (r, "Mesh Renderer Required");
			}
			else
			{
				meshPreview.RenderViewport (r, background);
				//Rect toolboxRect = new Rect (0, 0, 20, EditorGUIUtility.singleLineHeight);
				Rect toolboxRect = new Rect (r);
				toolboxRect.height = EditorGUIUtility.singleLineHeight;
				//GUI.Button (toolboxRect, "??");
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Shaded")) {
					meshPreview.showWireframe = false;
				}
				if (GUILayout.Button ("Wireframe")) {
					meshPreview.showWireframe = true;
				}
				GUILayout.EndHorizontal ();
			}
		}
		/// <summary>
		/// Get a preview mesh for a SproutMesh.
		/// </summary>
		/// <returns>Mesh for previewing.</returns>
		public Mesh GetPreviewMesh (SproutMesh sproutMesh, SproutMap.SproutMapArea sproutMapArea) {
			SproutMeshBuilder.GetInstance ().globalScale = 1f;
			bool isTwoSided = TreeFactory.GetActiveInstance ().materialManager.IsSproutTwoSided ();
			return SproutMeshBuilder.GetPreview (sproutMesh, isTwoSided, sproutMapArea);
		}
		/// <summary>
		/// Gets the sprout map assigned to a group id.
		/// </summary>
		/// <param name="groupId">Group id.</param>
		/// <returns>Sprout map.</returns>
		SproutMap GetSproutMap (int groupId) {
			if (groupId > -1) {
				SproutMapperElement sproutMapperElement = 
					(SproutMapperElement)sproutMeshGeneratorNode.sproutMeshGeneratorElement.GetDownstreamElement (PipelineElement.ClassType.SproutMapper);
				if (sproutMapperElement != null) {
					SproutMap sproutMap = sproutMapperElement.GetSproutMap (groupId);
					return sproutMap;
				}
			}
			SproutMap defaultSproutMap = new SproutMap ();
			return defaultSproutMap;
		}
		/// <summary>
		/// Show a preview mesh.
		/// </summary>
		/// <param name="index">Index.</param>
		public void ShowPreviewMesh (int index) {
			Mesh mesh = null;
			Material material = null;
			if (!previewMeshes.ContainsKey (index) || previewMeshes [index] == null) {
				SproutMesh sproutMesh = sproutMeshGeneratorNode.sproutMeshGeneratorElement.sproutMeshes [index];
				SproutMap sproutMap = GetSproutMap (sproutMesh.groupId);
				SproutMap.SproutMapArea sproutMapArea = sproutMap.GetMapArea ();
				mesh = GetPreviewMesh (sproutMesh, sproutMapArea);
				if (sproutMap != null) {
					material = MaterialManager.GetPreviewLeavesMaterial (sproutMap, sproutMapArea);
					previewMaterials.Add (index, material);
				}
				previewMeshes.Add (index, mesh);
			} else {
				mesh = previewMeshes [index];
				if (previewMaterials.ContainsKey (index)) {
					material = previewMaterials [index];
				}
			}
			meshPreview.Clear ();
			meshPreview.CreateViewport ();
			mesh.RecalculateBounds();
			if (material != null) {
				meshPreview.AddMesh (0, mesh, material, true);
			} else {
				meshPreview.AddMesh (0, mesh, true);
			}
			if (!autoZoomUsed) {
				autoZoomUsed = true;
				meshPreview.CalculateZoom (mesh);
			}
		}
		/// <summary>
		/// Draw additional handles on the mesh preview area.
		/// </summary>
		/// <param name="r">Rect</param>
		/// <param name="camera">Camera</param>
		public void OnPreviewMeshDrawHandles (Rect r, Camera camera) {
			Handles.color = Color.green;
			Handles.ArrowHandleCap (0,
				Vector3.zero, 
				Quaternion.LookRotation (Vector3.down), 
				1f * MeshPreview.GetHandleSize (Vector3.zero, camera), 
				EventType.Repaint);
		}
		/// <summary>
		/// Draws GUI elements on the mesh preview area.
		/// </summary>
		/// <param name="r">Rect</param>
		/// <param name="camera">Camera</param>
		public void OnPreviewMeshDrawGUI (Rect r, Camera camera) {
			r.y += EditorGUIUtility.singleLineHeight;
			GUI.Label (r, "[Pivot]", pivotLabelStyle);
			r.y += EditorGUIUtility.singleLineHeight;
			GUI.Label (r, "[Gravity]", gravityVectorLabelStyle);
		}
		#endregion
	}
}