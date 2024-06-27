using System.Collections;
using UnityEngine;

namespace FullSail
{
	public class ImpactBall : MonoBehaviour
	{
#if false
		// Example of a OnCollisionEnter adding a hole to a sail, I dont use it because objects bounce off instead of going through
		private void OnCollisionEnter(Collision collision)
		{
			Sail sail = collision.collider.GetComponent<Sail>();

			if ( sail )
			{
				ContactPoint cp = collision.contacts[0];

				float size = Random.Range(0.1f, 0.4f);

				int ty = types[Random.Range(0, 4)];
				// Calc uv

				Vector3 lpos = hit.collider.transform.InverseTransformPoint(hit.point);
				lpos /= 10.0f;
				lpos.x = 1.0f - (lpos.x + 0.5f);
				lpos.y = 1.0f + lpos.y;

				sail.AddImpact(lpos.x, lpos.y, size, ty, true);
				sail.AddRipple(lpos.x, lpos.y, size * 10.0f);
			}
		}
#endif
		Rigidbody rb;

		private void Start()
		{
			rb = GetComponent<Rigidbody>();	
		}

		int[] types = new int[] { 0, 3, 6, 7, 5 };	// The damage types we can apply on impact, choosen at random by the method below

		private void FixedUpdate()
		{
			if ( rb )
			{
				Ray ray = new Ray(transform.position, rb.velocity.normalized);
				RaycastHit hit;

				// Check if we are about to hit this sail
				if ( Physics.Raycast(ray, out hit, rb.velocity.magnitude * Time.fixedDeltaTime) )
				{
					// We hit so get the sail component if there is one
					Sail sail = hit.collider.GetComponent<Sail>();

					// Did we hit a sail
					if ( sail )
					{
						// get size between min and max
						float size = Random.Range(0.05f, 0.2f);
						// Get an impact type
						int ty = types[Random.Range(0, 4)];

						// Calc uv
						Vector3 lpos = hit.collider.transform.InverseTransformPoint(hit.point);
						lpos /= 10.0f;
						lpos.x = 1.0f - (lpos.x + 0.5f);
						lpos.y = 1.0f + lpos.y;

						// Add the impact at the uv position, with size and the type
						sail.AddImpact(lpos.x, lpos.y, size, ty, true);
						// Set the impact location at the same pos, set the impact time and the amplitude of the impact
						// The Sail should have an ImpactLocation override
						sail.AddRipple(lpos.x, lpos.y, size * 10.0f);
					}
				}
			}
		}
	}
}