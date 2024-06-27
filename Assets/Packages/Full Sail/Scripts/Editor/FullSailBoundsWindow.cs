using UnityEditor;
using UnityEngine;

// This script allows you to alter the bounds for a mesh. This is needed by Full Sail as the mesh is deformed by a shader and do could be culled
// from the view if its bounds box is very thin. Not used presently as cant save changes. Use SailBounds script instead
#if false
namespace FullSail
{
	public class FullSailBoundsWindow : EditorWindow
	{
		public Vector2		scroll;
		public Mesh			mesh;
		public GameObject	sailMeshPrefab;
		Texture				logoImage;

		[MenuItem("Window/Full Sail/Sail Bounds")]
		static void Init()
		{
			EditorWindow.GetWindow(typeof(FullSailBoundsWindow), false, "Sail Bounds");
		}

		void OnGUI()
		{
			float w = EditorGUIUtility.currentViewWidth - 12.0f;

			if ( logoImage == null )
				logoImage = (Texture)Resources.Load<Texture>("Editor/FullSailLogo");

			if ( logoImage )
			{
				float h1 = (float)logoImage.height / ((float)logoImage.width / ((float)position.width - 0));
				GUILayout.Box(logoImage, GUILayout.Width(position.width), GUILayout.Height(h1));
			}

			scroll = EditorGUILayout.BeginScrollView(scroll);

			//mesh = (Mesh)EditorGUILayout.ObjectField("Sail Mesh", mesh, typeof(Mesh), false);
			sailMeshPrefab = (GameObject)EditorGUILayout.ObjectField("Sail Mesh Prefab", sailMeshPrefab, typeof(GameObject), true);

			if ( sailMeshPrefab )
			{
				MeshFilter mf = sailMeshPrefab.GetComponent<MeshFilter>();

				if ( mf )
				{
					Mesh mesh = mf.sharedMesh;
					Bounds b = mesh.bounds;

					b = EditorGUILayout.BoundsField("Bounds", b);
					if ( GUI.changed )
					{
						mesh.bounds = b;
					}
				}
			}

			EditorGUILayout.EndScrollView();
		}
	}
}
#endif