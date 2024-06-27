using UnityEngine;

// Simple example script that fires an object at the sails to test impacts.
// It also shows how an impact can be added directly using the AddImpact method in the sail component
// And also how a hole can be repaired or removed from the sail
namespace FullSail
{
	public class TestImpacts : MonoBehaviour
	{
		[Range(0, 1)]
		public float		minSize	= 0.1f;		// Min size used when making a hole from impact
		[Range(0, 1)]
		public float		maxSize = 0.3f;		// Max size used when making a hole from impact
		public GameObject	ball;				// object to fire, should have a 

		void Update()
		{
			if ( Input.GetMouseButtonDown(0) )
			{
				GameObject b = Instantiate(ball);
				ball.transform.position = transform.position;
				Rigidbody rb = b.GetComponent<Rigidbody>();

				Ray ray1 = Camera.main.ScreenPointToRay(Input.mousePosition);

				rb.AddForce(ray1.direction * 1600.0f, ForceMode.Impulse);
			}

			// ray cast to mouse, if we hit a collider with a sail add an impact
			if ( Input.GetKeyDown(KeyCode.Space) )
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if ( Physics.Raycast(ray, out hit, 100) )
				{
					Sail sail = hit.collider.GetComponent<Sail>();
					if ( sail )
					{
						sail.AddImpact(hit.textureCoord.x, hit.textureCoord.y, Random.Range(minSize, maxSize), 0);

						Vector4 loc = new Vector4(hit.textureCoord.x, hit.textureCoord.y, Time.timeSinceLevelLoad, 0.5f);
						sail.SetValue(SailParamID.ImpactLocation, loc);
					}
				}
			}

			if ( Input.GetMouseButtonDown(1) )
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if ( Physics.Raycast(ray, out hit, 100) )
				{
					Sail sail = hit.collider.GetComponent<Sail>();
					if ( sail )
					{
						sail.PatchImpact();
					}
				}
			}

			if ( Input.GetKeyDown(KeyCode.R) )
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;
				if ( Physics.Raycast(ray, out hit, 100) )
				{
					Sail sail = hit.collider.GetComponent<Sail>();
					if ( sail )
						sail.RemoveImpact();
				}
			}
		}
	}
}