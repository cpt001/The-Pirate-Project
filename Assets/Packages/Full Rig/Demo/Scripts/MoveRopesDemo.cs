using UnityEngine;
using static FullRig.FullDemoInputs;

public class MoveRopesDemo : MonoBehaviour
{
	Vector3 startPos;
	public float	radius	= 1.0f;
	public float	angle;
	public float	speed	= 1.0f;

	Vector3	movePos;
	Vector3 cmovePos;
	Vector3 movevel;
	float	moveDamp = 0.25f;

    void Start()
    {
		startPos = transform.position;    
    }

    void Update()
    {
		angle += Time.deltaTime * speed;
		angle = Mathf.Repeat(angle, Mathf.PI * 2.0f);

		Vector3 p = Vector3.zero;

		p.x = Mathf.Sin(angle) * radius;
		p.y = Mathf.Cos(angle) * radius;

		//if ( Input.GetMouseButton(0) || Input.GetKey(KeyCode.M) )
		if ( GetInput(DemoInputs.MoveRopesEnabled) )
		{
			movePos.x -= Input.GetAxis("Mouse X") * 0.1f;
			movePos.y += Input.GetAxis("Mouse Y") * 0.1f;
		}

		if ( movePos.x > 1.0f )
			movePos.x = 1.0f;

		if ( movePos.x < -4.0f )
			movePos.x = -4.0f;

		if ( movePos.y < -2.8f )
			movePos.x = -2.8f;

		if ( movePos.y > 2.8f )
			movePos.x = 2.8f;

		cmovePos = Vector3.SmoothDamp(cmovePos, movePos, ref movevel, moveDamp);

		transform.position = startPos + cmovePos;	// + p;
	}
}
