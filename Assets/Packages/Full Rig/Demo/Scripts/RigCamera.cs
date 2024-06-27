#define USEPOSTPROCESS	// If you dont have Post Process installed and get errors comment out this line

using UnityEngine;
using static FullRig.FullDemoInputs;

#if USEPOSTPROCESS
using UnityEngine.Rendering.PostProcessing;
#endif

namespace FullRig	//MegaFiers
{
	[ExecuteInEditMode]
	public class RigCamera : MonoBehaviour
	{
		public GameObject			target;
		public float				distance	= 10.0f;
		public float				xSpeed		= 2500.0f;
		public float				ySpeed		= 1200.0f;
		public float				zSpeed		= 10.0f;
		public float				yMinLimit	= -20.0f;
		public float				yMaxLimit	= 80.0f;
		public float				xMinLimit	= -20.0f;
		public float				xMaxLimit	= 20.0f;
		public Vector3				offset;
		public float				trantime	= 4.0f;
		public float				nx			= 0.0f;
		public float				ny			= 0.0f;
		public float				nz			= 0.0f;
		public float				delay		= 0.24f;
		public float				delayz		= 0.24f;
		float						x			= 0.0f;
		float						y			= 0.0f;
		float						vx			= 0.0f;
		float						vy			= 0.0f;
		float						vz			= 0.0f;
		float						t			= 0.0f;
		Vector3						tpos		= new Vector3();
		MeshRenderer				render;
		SkinnedMeshRenderer			srender;
		MeshFilter					filter;
		//CameraShake					shake;
		public float				shakeAmt	= 1.0f;
#if USEPOSTPROCESS
		DepthOfField				dof;
		public PostProcessProfile	postprocess;
		bool						havedof;
#endif
		GameObject[]				targets;
		int							currentIndex;
		Vector3						newpos		= Vector3.zero;
		public AnimationCurve		crv			= new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
		float						cdofdist;
		float						currentDistance;
		float						dz;
		float						dofdistance;
		public Gradient				coltest;
		public Material				groundMat;
		public Color[]				colors;
		public float				ctime		= 1.0f;
		public float				ct			= 10.0f;
		public Color				currentCol;
		public bool					slowfade;
		[Range(0, 1)]
		public float				fogLerp		= 0.25f;
		[Range(0, 1)]
		public float				ambientLerp	= 0.75f;
		public float				fadetime	= 20.0f;
		int							lastindex;
		public int					colIndex;
		public bool					doRaycast;
		public float				speed		= 1.0f;
		public bool					lookat;
		float						fov;
		float						cfov;
		float						fovvel;
		float						startfov;
		Vector3						mvel;
		bool						firstrun;
		public float				dofdamp		= 0.25f;
		public GameObject			controlsOn;
		public GameObject			controlsOff;

		static public int Compare(GameObject o1, GameObject o2)
		{
			if ( o1.transform.position.z < o2.transform.position.z )
				return 1;

			if ( o1.transform.position.z > o2.transform.position.z )
				return -1;

			return 0;
		}

		void Start()
		{
			newpos = transform.position;

			fov = Camera.main.fieldOfView;
			cfov = fov;
			startfov = fov;
			lastindex = 0;

			currentCol = colors[colIndex];

			targets = GameObject.FindGameObjectsWithTag("LookAt");

			System.Array.Sort(targets, Compare);

			for ( int i = 0; i < targets.Length; i++ )
			{
				if ( targets[i] == target )
				{
					currentIndex = i;
					break;
				}
			}

			//shake = GetComponent<CameraShake>();

			NewTarget(target);

			if ( target )
				tpos = target.transform.position;
			else
			{
				Vector3 angles = transform.eulerAngles;
				x = angles.y;
				y = angles.x;
			}

			vx = vy = vz = 0.0f;
			x = nx;
			y = ny;
			distance = nz;

			cdofdist = nz;
			currentDistance = nz;
			dofdistance = nz;
#if USEPOSTPROCESS
			if ( postprocess )
				havedof = postprocess.TryGetSettings<DepthOfField>(out dof);
#endif
			t = 8.0f;

			Application.targetFrameRate = -1;
		}

		public void NewTarget(GameObject targ)
		{
			if ( target != targ )
			{
				target = targ;
				t = 0.0f;
				tpos = newpos;

				ChangeCol();
			}
		}

		void DoColor()
		{
			if ( Application.isPlaying || slowfade )
			{
				if ( slowfade )
				{
					if ( Application.isPlaying )
					{
						ct += Time.deltaTime * speed;
						if ( ct > fadetime )
							ct = 0.0f;
					}

					currentCol = coltest.Evaluate(ct / fadetime);
				}
				else
				{
					if ( ct < ctime )
					{
						ct += Time.deltaTime;

						currentCol = Color.Lerp(colors[lastindex], colors[colIndex], ct / ctime);
					}
				}
			}

			RenderSettings.fogColor = Color.Lerp(currentCol, Color.white, fogLerp);
			// Need different values for ambient, or multiply them up to white
			RenderSettings.ambientLight = Color.Lerp(currentCol, Color.white, ambientLerp);
			if ( groundMat )
				groundMat.color = currentCol;
		}

		void ChangeCol()
		{
			if ( !slowfade )
			{
				lastindex = colIndex;
				colIndex++;
				if ( colIndex >= colors.Length )
					colIndex = 0;

				ct = 0.0f;
			}
		}

		void Update()
		{
			//if ( Input.GetKeyDown(KeyCode.F1) )
			if ( GetInput(DemoInputs.ShowControls) )
			{
				if ( controlsOn.activeInHierarchy )
				{
					controlsOn.SetActive(false);
					controlsOff.SetActive(true);
				}
				else
				{
					controlsOn.SetActive(true);
					controlsOff.SetActive(false);
				}
			}

			//if ( Input.GetKeyDown(KeyCode.Escape) )
			if ( GetInput(DemoInputs.Quit) )
				Application.Quit();

			DoColor();

			//if ( Input.GetKeyDown(KeyCode.F4) )
			if ( GetInput(DemoInputs.MouseLookEnabled) )
				lookat = !lookat;

			//if ( Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) )
			if ( GetInput(DemoInputs.MoveRight) )
			{
				currentIndex++;
				if ( currentIndex >= targets.Length )
					currentIndex = targets.Length - 1;

				if ( targets.Length > 0 )
					NewTarget(targets[currentIndex]);
			}

			//if ( Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) )
			if ( GetInput(DemoInputs.MoveLeft) )
			{
				currentIndex--;
				if ( currentIndex < 0 )
					currentIndex = 0;

				if ( targets.Length > 0 )
					NewTarget(targets[currentIndex]);
			}

			if ( target )
			{
				RopeTarget r = target.GetComponent<RopeTarget>();

				if ( r )
				{
					//if ( Input.GetKey(KeyCode.Alpha1) )
					if ( GetInput(DemoInputs.DecreaseRopeLength) )
					{
						r.length -= r.speed * Time.deltaTime;
					}

					//if ( Input.GetKey(KeyCode.Alpha2) )
					if ( GetInput(DemoInputs.IncreaseRopeLength) )
					{
						r.length += r.speed * Time.deltaTime;
					}
				}

				if ( !lookat )
				{
					//if ( Input.GetMouseButton(1) )
					if ( GetInput(DemoInputs.LookAroundEnabled) )
					{
						//nx = x + Input.GetAxis("Mouse X") * xSpeed * 0.02f;
						//ny = y - Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
						nx = x + GetMouseDelta("X") * xSpeed * 0.02f;
						ny = y - GetMouseDelta("Y") * ySpeed * 0.02f;
					}

					// NOTE: If you get an exception for this line it means you dont have a scroll wheel input setup in
					// the input manager, go to Edit/Project Settings/Input and set the Mouse ScrollWheel setting to use 3rd mouse axis
					//if ( Input.GetKey(KeyCode.V) )
					if ( GetInput(DemoInputs.ChangeFOVEnabled) )
					{
						//fov -= Input.GetAxis("Mouse ScrollWheel") * zSpeed * 0.5f;
						fov -= GetMouseScrollVertical() * zSpeed * 0.5f;
					}
					else
					{
						//nz = nz - (Input.GetAxis("Mouse ScrollWheel") * zSpeed);
						nz = nz - (GetMouseScrollVertical() * zSpeed);
					}

					if ( Application.isPlaying )
					{
						x = Mathf.SmoothDamp(x, nx, ref vx, delay);
						y = Mathf.SmoothDamp(y, ny, ref vy, delay);
						distance = Mathf.SmoothDamp(distance, nz, ref vz, delayz);
					}
					else
					{
						x = nx;
						y = ny;
						distance = nz;
					}

					y = ClampAngle(y, yMinLimit, yMaxLimit);

					if ( distance < 1.0f )
					{
						distance = 1.0f;
						nz = 1.0f;
					}

					Vector3 c = target.transform.position + offset;

					if ( t < trantime )
						t += Time.deltaTime;

					if ( !firstrun )
					{
						firstrun = true;
						newpos = c;
					}

					newpos = Vector3.SmoothDamp(newpos, c, ref mvel, 0.25f);

					Quaternion rotation = Quaternion.Euler(y, x, 0.0f);
					Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + newpos;

					Quaternion srot = Quaternion.identity;

					//if ( shake && Application.isPlaying )
					//{
						//Vector3 rot = Vector3.zero;
						//shake.DoUpdate(ref position, ref rot, shakeAmt);
						//srot = Quaternion.Euler(rot);
					//}

					if ( doRaycast )
					{
						RaycastHit hit;
						Ray ray = new Ray(newpos, (position - newpos).normalized);

						if ( Physics.Raycast(ray, out hit, (position - newpos).magnitude, 1 << 10) )
							position = hit.point + (Vector3.up * 0.2f);
					}
					else
					{
						//if ( position.y < 0.2f )
							//position.y = 0.2f;
					}

					transform.rotation = rotation * srot;
					transform.position = position;
				}
				else
				{
					//if ( Input.GetKey(KeyCode.V) )
					if ( GetInput(DemoInputs.ChangeFOVEnabled) )
						fov -= Input.GetAxis("Mouse ScrollWheel") * zSpeed * 0.5f;

					Vector3 c = target.transform.position + offset;

					newpos = Vector3.SmoothDamp(newpos, c, ref mvel, 0.25f);

					Quaternion srot = Quaternion.identity;
					//if ( shake && Application.isPlaying )
					//{
						//Vector3 rot = Vector3.zero;
						//Vector3 p = Vector3.zero;
						//shake.DoUpdate(ref p, ref rot, shakeAmt);
						//srot = Quaternion.Euler(rot);
					//}

					Quaternion rotation = Quaternion.LookRotation(newpos - transform.position);
					transform.rotation = rotation * srot;
				}
				UpdateDOF();

				fov = Mathf.Clamp(fov, 8.0f, 45.0f);
				cfov = Mathf.SmoothDamp(cfov, fov, ref fovvel, 0.25f);
				Camera.main.fieldOfView = cfov;
			}
		}

		static float ClampAngle(float angle, float min, float max)
		{
			if ( angle < -360.0f )
				angle += 360.0f;

			if ( angle > 360.0f )
				angle -= 360.0f;

			return Mathf.Clamp(angle, min, max);
		}

		public void SetDOF(float dist)
		{
#if USEPOSTPROCESS
			if ( havedof )
				dof.focusDistance.value = dist;
#endif
		}

		float GetDOFDist(Ray ray)
		{
			RaycastHit hit;

			if ( Physics.Raycast(ray, out hit) )
				dofdistance = hit.distance;

			return dofdistance;
		}

		public void UpdateDOF()
		{
			if ( doRaycast )
			{
				Ray ray = new Ray();

				ray.origin = transform.position;
				ray.direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;

				dofdistance = GetDOFDist(ray);

				currentDistance = Mathf.SmoothDamp(currentDistance, dofdistance, ref dz, dofdamp);
				SetDOF(currentDistance);
			}
			else
				SetDOF(distance);
		}
	}
}