using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeTarget : MonoBehaviour
{
	public FullRig.Rope	rope;

	public float	minLength = 1.0f;
	public float	maxLength = 10.0f;
	float clength;
	public float length;
	float vel;

	public float	damp = 0.25f;
	public float	speed = 1.0f;

    void Start()
    {
		if ( rope )
	        clength = length = rope.length;
    }

    void Update()
    {
		if ( rope )
		{
			length = Mathf.Clamp(length, minLength, maxLength);
			clength = Mathf.SmoothDamp(clength, length, ref vel, damp);
			rope.length = clength;
			rope.SetDirty();
		}
    }
}
