using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FullRig
{
	public class RopeManager : MonoBehaviour
	{
		Rope[]	ropes;

		// Start is called before the first frame update
		void Start()
		{
			ropes = GameObject.FindObjectsOfType<Rope>();
		}

		// Update is called once per frame
		void Update()
		{
			for ( int i = 0; i < ropes.Length; i++ )
			{
				ropes[i].RopeUpdate();
			}
		}

		private void FixedUpdate()
		{
			for ( int i = 0; i < ropes.Length; i++ )
			{
				ropes[i].RopeFixedUpdate();
			}
		}
	}
}