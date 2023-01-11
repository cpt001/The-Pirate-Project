using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

using Broccoli.Base;
using Broccoli.Pipe;
using Broccoli.Builder;
using Broccoli.Component;
using Broccoli.Factory;

namespace Broccoli.TreeNodeEditor
{
	/// <summary>
	/// Sprout lab node editor.
	/// </summary>
	[CustomEditor(typeof(SproutLabNode))]
	public class SproutLabNodeEditor : BaseNodeEditor {
		#region Vars
		/// <summary>
		/// The sprout lab node.
		/// </summary>
		public SproutLabNode sproutLabNode;

		ReorderableList sproutCompositesList;
		MeshPreview meshPreview;
		SerializedProperty propSproutComposites;
		/// <summary>
		/// The changes are to be applied on the pipeline.
		/// </summary>
		private bool changesForPipeline = false;
		private int selectedLOD = 0;

		private SproutLabComponent sproutLabComponent = null;
		private GUIContent previewTitleGUIContent;
		#endregion

		#region Messages
		//private static string MSG_GIRTH_AT_BASE = "Girth to be used at the base of the tree trunk.";
		#endregion

		#region Events
		/// <summary>
		/// Actions to perform on the concrete class when the enable event is raised.
		/// </summary>
		protected override void OnEnableSpecific () {
			sproutLabNode = target as SproutLabNode;
			SetPipelineElementProperty ("sproutLabElement");

			// Set component.
			sproutLabComponent = 
				(SproutLabComponent)TreeFactory.GetActiveInstance ().componentManager.GetFactoryComponent (sproutLabNode.sproutLabElement);

			// Sprout composites list.
			propSproutComposites = GetSerializedProperty ("sproutComposites");
			sproutCompositesList = new ReorderableList (serializedObject, propSproutComposites, false, true, true, true);
			sproutCompositesList.draggable = false;
			sproutCompositesList.drawHeaderCallback += DrawSproutCompositeHeader;
			sproutCompositesList.drawElementCallback += DrawSproutCompositeElement;
			sproutCompositesList.onSelectCallback += OnSelectSproutCompositeItem;
			sproutCompositesList.onAddCallback += OnAddSproutCompositeItem;
			sproutCompositesList.onRemoveCallback += OnRemoveSproutCompositeItem;

			// Init mesh preview
			if (meshPreview == null) {
				meshPreview = new MeshPreview ();
			} else {
				meshPreview.Clear ();
			}
			meshPreview.CreateViewport ("LOD0");
			meshPreview.CreateViewport ("LOD1");
			meshPreview.CreateViewport ("LOD2");
			meshPreview.AddMesh (0, sproutLabComponent.GetMesh (null, 0), true);
			meshPreview.AddMesh (1, sproutLabComponent.GetMesh (null, 1), true);
			meshPreview.AddMesh (2, sproutLabComponent.GetMesh (null, 2), true);

			//
			if (previewTitleGUIContent == null) {
				previewTitleGUIContent = new GUIContent ("Sprout Lab Preview");
			}
		}
		/// <summary>
		/// Raises the inspector GUI event.
		/// </summary>
		public override void OnInspectorGUI() {
			CheckUndoRequest ();

			UpdateSerialized ();

			// Log box.
			DrawLogBox ();

			// SEED OPTIONS
			DrawSeedOptions ();

			// HELP OPTIONS
			DrawFieldHelpOptions ();
			EditorGUILayout.Space ();

			changesForPipeline = false;
			if (sproutLabNode.sproutLabElement.selectedCompositeIndex != sproutCompositesList.index &&
				sproutLabNode.sproutLabElement.selectedCompositeIndex < sproutCompositesList.count) {
				sproutCompositesList.index = sproutLabNode.sproutLabElement.selectedCompositeIndex;
			}
			sproutCompositesList.DoLayoutList ();

			if (GUILayout.Button ("Show Sprout Lab")) {
				TreeFactoryEditorWindow.editorWindow.SetEditorView (TreeFactoryEditorWindow.EditorView.SproutsLab);
				TreeFactoryEditorWindow.editorWindow.Repaint ();
			}

			if (GUILayout.Button ("Test TextureBuilder")) {
				TextureBuilder tb = new TextureBuilder ();
				// Get tree mesh.
				GameObject previewTree = TreeFactory.GetActiveInstance ().previewTree.obj;
				tb.BeginUsage (previewTree);
				//tb.shader = GetUnlitShader ();
				tb.GetTexture (new Plane (Vector3.up, Vector3.zero), Vector3.forward, "Assets/sproutTexture.png");
				tb.shader = GetNormalShader ();
				tb.GetTexture (new Plane (Vector3.up, Vector3.zero), Vector3.forward, "Assets/sproutTextureNormals.png");
				tb.EndUsage ();
			}

			if (GUILayout.Button ("Take Catalog Screenshot")) {
				TextureBuilder tb = new TextureBuilder ();
				// Get tree mesh.
				GameObject previewTree = TreeFactory.GetActiveInstance ().previewTree.obj;
				tb.BeginUsage (previewTree);
				tb.textureSize = new Vector2 (1024, 1024);
				tb.GetTexture (new Plane (Vector3.back, Vector3.zero), Vector3.up, "Assets/treeThumb.png");
				tb.EndUsage ();
			}

			/*
			float girthAtBase = propGirthAtBase.floatValue;
			EditorGUILayout.Slider (propGirthAtBase, 0.01f, 3.5f, "Girth at Base");
			ShowHelpBox (MSG_GIRTH_AT_BASE);
			EditorGUILayout.Space ();

			float girthAtTop = propGirthAtTop.floatValue;
			EditorGUILayout.Slider (propGirthAtTop, 0.01f, 3.5f, "Girth at Top");
			ShowHelpBox (MSG_GIRTH_AT_TOP);
			EditorGUILayout.Space ();

			bool curveChanged = false;
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.CurveField (propGirthCurve, Color.green, girthCurveRange);
			ShowHelpBox (MSG_CURVE);
			if (EditorGUI.EndChangeCheck ()) {
				curveChanged = true;
			}
			EditorGUILayout.Space ();

			bool hierarchyScaleChanged = false;
			EditorGUI.BeginChangeCheck ();
			EditorGUILayout.PropertyField (propHierarchyScalingEnabled);
			ShowHelpBox (MSG_HIERARCHY_SCALING_ENABLED);
			EditorGUILayout.Space ();

			if (propHierarchyScalingEnabled.boolValue) {
				EditorGUILayout.Slider (propMaxHierarchyScaling, 0.01f, 1f, "Hierarchy Scaling");
				ShowHelpBox (MSG_MAX_HIERARCHY_SCALING);
				EditorGUILayout.Space ();
			}

			if (EditorGUI.EndChangeCheck () && 
				propMaxHierarchyScaling.floatValue >= propMinHierarchyScaling.floatValue) {
				hierarchyScaleChanged = true;
			}
			*/

			if (changesForPipeline) {
				ApplySerialized ();
				UpdatePipeline (GlobalSettings.processingDelayVeryHigh, true);
				sproutLabNode.sproutLabElement.Validate ();
				SetUndoControlCounter ();
			}

			/*
			if (girthAtBase != propGirthAtBase.floatValue ||
				girthAtTop != propGirthAtTop.floatValue ||
				curveChanged || hierarchyScaleChanged) {
				UpdatePipeline (GlobalSettings.processingDelayHigh);
				sproutLabNode.GirthTransformElement.Validate ();
				SetUndoControlCounter ();

			}
			EditorGUILayout.Space ();
			*/
		}
		Shader GetUnlitShader () {
			return Shader.Find ("Broccoli/Billboard Unlit");
		}
		Shader GetNormalShader () {
			return Shader.Find ("Broccoli/Billboard Normals");
		}
		#endregion

		#region Mesh Preview
		/// <summary>
		/// Determines whether this instance has preview GUI.
		/// </summary>
		/// <returns><c>true</c> if this instance has preview GU; otherwise, <c>false</c>.</returns>
		public override bool HasPreviewGUI () {
			return sproutLabNode.sproutLabElement.selectedCompositeIndex != -1;
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
				if (GUILayout.Button ("LOD0")) {
					selectedLOD = 0;
					meshPreview.SelectViewport (selectedLOD);
				}
				if (GUILayout.Button ("LOD1")) {
					selectedLOD = 1;
					meshPreview.SelectViewport (selectedLOD);
				}
				if (GUILayout.Button ("LOD2")) {
					selectedLOD = 2;
					meshPreview.SelectViewport (selectedLOD);
				}
				GUILayout.EndHorizontal ();
			}
		}
		#endregion

		#region Sprouts Composites List
		/// <summary>
		/// Draws the list item header.
		/// </summary>
		/// <param name="rect">Rect.</param>
		private void DrawSproutCompositeHeader(Rect rect)
		{
			GUI.Label(rect, "Sprout Composites");
		}
		/// <summary>
		/// Draws the sprout map element.
		/// </summary>
		/// <param name="rect">Rect.</param>
		/// <param name="index">Index.</param>
		/// <param name="isActive">If set to <c>true</c> is active.</param>
		/// <param name="isFocused">If set to <c>true</c> is focused.</param>
		private void DrawSproutCompositeElement (Rect rect, int index, bool isActive, bool isFocused) {
			var sproutCompositeProp = sproutCompositesList.serializedProperty.GetArrayElementAtIndex (index);
			int sproutGroupId = sproutCompositeProp.FindPropertyRelative ("groupId").intValue;
			if (sproutGroupId > 0) {
				rect.y += 2;
				SproutGroups sproutGroups = sproutLabNode.sproutLabElement.pipeline.sproutGroups;
				EditorGUI.DrawRect (new Rect (rect.x, rect.y, 
					EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), 
					sproutGroups.GetSproutGroupColor(sproutGroupId));
				rect.x += 22;
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 
					150, EditorGUIUtility.singleLineHeight), "Assigned to group " + sproutGroupId);
			} else {
				rect.y += 2;
				EditorGUI.DrawRect (new Rect (rect.x, rect.y, 
					EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight), 
					Color.black);
				rect.x += 22;
				EditorGUI.LabelField (new Rect (rect.x, rect.y, 
					150, EditorGUIUtility.singleLineHeight), "Unassigned composite");
			}

			if (isActive) {
				if (index != sproutLabNode.sproutLabElement.selectedCompositeIndex) {
					sproutLabNode.sproutLabElement.selectedCompositeIndex = index;
				}
				EditorGUILayout.Space ();

				// Sprout group.
				EditorGUI.BeginChangeCheck ();
				int sproutGroupIndex = EditorGUILayout.Popup ("Sprout Group",
					sproutLabNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupIndex (sproutGroupId, true),
					sproutLabNode.pipelineElement.pipeline.sproutGroups.GetPopupOptions (true));
				int selectedSproutGroupId = 
					sproutLabNode.pipelineElement.pipeline.sproutGroups.GetSproutGroupId (sproutGroupIndex);
				if (EditorGUI.EndChangeCheck() && sproutGroupId != selectedSproutGroupId) {
					if (sproutLabNode.sproutLabElement.GetSproutGroupsAssigned ().Contains (selectedSproutGroupId)) {
						Debug.LogWarning ("The sprout group has already been assigned to a material.");
					} else {
						sproutCompositeProp.FindPropertyRelative ("groupId").intValue = selectedSproutGroupId;
						changesForPipeline = true;
					}
				}

				/*
				EditorGUI.BeginChangeCheck ();
				// Mode.
				SproutMap.Mode sproutMapMode = (SproutMap.Mode)sproutCompositeProp.FindPropertyRelative ("mode").enumValueIndex;
				EditorGUILayout.PropertyField(sproutCompositeProp.FindPropertyRelative ("mode"));
				if ((SproutMap.Mode)sproutCompositeProp.FindPropertyRelative ("mode").enumValueIndex == SproutMap.Mode.MaterialOverride) {
					// Changes for mode.
					if (EditorGUI.EndChangeCheck () ||
						sproutMapMode != (SproutMap.Mode)sproutCompositeProp.FindPropertyRelative ("mode").enumValueIndex) {
						changesForPipeline = true;
					} else {
						DrawSproutMapElementMaterialOverrideMode (sproutCompositeProp, index);
					}
				} else if ((SproutMap.Mode)sproutCompositeProp.FindPropertyRelative ("mode").enumValueIndex == SproutMap.Mode.Material) {
					// Changes for mode.
					if (EditorGUI.EndChangeCheck () ||
						sproutMapMode != (SproutMap.Mode)sproutCompositeProp.FindPropertyRelative ("mode").enumValueIndex) {
						changesForPipeline = true;
					} else {
						DrawSproutMapElementMaterialMode (sproutCompositeProp);
					}
				} else if ((SproutMap.Mode)sproutCompositeProp.FindPropertyRelative ("mode").enumValueIndex == SproutMap.Mode.Texture) {
					// Changes for mode.
					if (EditorGUI.EndChangeCheck () ||
						sproutMapMode != (SproutMap.Mode)sproutCompositeProp.FindPropertyRelative ("mode").enumValueIndex) {
						changesForPipeline = true;
					} else {
						DrawSproutMapElementTextureMode (sproutMapProp, index);
					}
				}
				*/
			}
		}
		/// <summary>
		/// Adds a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnAddSproutCompositeItem(ReorderableList list)
		{
			if (sproutLabNode.sproutLabElement.CanAddSproutComposite ()) {
				SproutComposite sproutComposite = new SproutComposite ();
				Undo.RecordObject (sproutLabNode.sproutLabElement, "Sprout Composite added");
				sproutLabNode.sproutLabElement.AddSproutComposite (sproutComposite);
			}
		}
		/// <summary>
		/// Event called when a composite is selected from the list.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnSelectSproutCompositeItem (ReorderableList list)
		{
			Undo.RecordObject (sproutLabNode.sproutLabElement, "Sprout Lab selected");
			sproutLabNode.sproutLabElement.selectedCompositeIndex = list.index;
		}
		/// <summary>
		/// Removes a list item.
		/// </summary>
		/// <param name="list">List.</param>
		private void OnRemoveSproutCompositeItem(ReorderableList list)
		{
			int undoGroup = Undo.GetCurrentGroup ();
			Undo.SetCurrentGroupName ("Sprout Composite removed");
			Undo.RecordObject (sproutLabNode.sproutLabElement, "Sprout Composite removed");
			sproutLabNode.sproutLabElement.sproutComposites.RemoveAt (list.index);
			Undo.RecordObject (sproutLabNode.sproutLabElement, "Sprout Composite removed");
			sproutLabNode.sproutLabElement.selectedCompositeIndex = -1;
			Undo.CollapseUndoOperations (undoGroup);
			//changesForMeshes = true;
		}
		#endregion
	}
}