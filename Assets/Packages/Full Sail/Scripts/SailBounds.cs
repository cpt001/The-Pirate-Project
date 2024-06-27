using UnityEngine;

// Increases the bounds of the mesh so it is not culled from the view when heavily deformed.
namespace FullSail
{
	[HelpURL("http://www.macspeedee.com/full-sail/")]
	public class SailBounds : MonoBehaviour
	{
		public Vector3	size = new Vector3(20.0f, 10.0f, 20.0f);

		void Start()
		{
			MeshFilter mf = GetComponent<MeshFilter>();
			if ( mf )
			{
				Mesh mesh = mf.sharedMesh;
				if ( mesh )
				{
					Bounds b = mesh.bounds;
					b.size = size;
					mesh.bounds = b;
				}
			}
		}
	}
}