using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace FullRig
{
	public class FullDemoInputs
	{
		public enum DemoInputs
		{
			// Full Sail
			Shoot,
			ShootRay,
			Patch,
			RemoveImpact,

			// Full Rig
			ChangeFOVEnabled,   // if ( Input.GetKey(KeyCode.V) )
			LookAroundEnabled,  // Input.GetMouseButton(1)
			IncreaseRopeLength, //  Input.GetKey(KeyCode.Alpha2) )
			DecreaseRopeLength, // Input.GetKey(KeyCode.Alpha1)
			MoveLeft,           // Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)
			MoveRight,          // if ( Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) )
			MouseLookEnabled,   // Input.GetKeyDown(KeyCode.F4)
			Quit,               // if ( Input.GetKeyDown(KeyCode.Escape) )
			ShowControls,       // Input.GetKeyDown(KeyCode.F1)
			MoveRopesEnabled,	// Input.GetMouseButton(0) || Input.GetKey(KeyCode.M)
		}

		public static Vector2 GetMousePosition()
		{
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.position.ReadValue();
#else
			return Input.mousePosition;
#endif
		}

		public static float GetMouseDelta(string dir)//x/y
		{
#if ENABLE_INPUT_SYSTEM
            switch (dir.ToLower())
            {
                case "x":
                    return Mouse.current.delta.x.ReadValue();
                case "y":
                    return Mouse.current.delta.y.ReadValue();
            }
            return 0;
#else
			return Input.GetAxis($"Mouse {dir}");
#endif
		}

		public static float GetMouseScrollVertical()
		{
#if ENABLE_INPUT_SYSTEM
            return Mouse.current.scroll.ReadValue().normalized.y;
#else
			return Input.GetAxis("Mouse ScrollWheel");
#endif
		}

		/// <summary>
		/// Get the input for the demo regardless on input system used.
		/// </summary>
		/// <param name="input">Any Demo Input</param>
		/// <returns>True if the key has been pressed.</returns>
		public static bool GetInput(DemoInputs input)
		{
#if ENABLE_INPUT_SYSTEM
            switch (input)
            {
                case DemoInputs.Shoot:				return Mouse.current.leftButton.wasPressedThisFrame;
                case DemoInputs.ShootRay:			return Keyboard.current.spaceKey.wasPressedThisFrame;
                case DemoInputs.Patch:				return Mouse.current.rightButton.wasPressedThisFrame;
                case DemoInputs.RemoveImpact:		return Keyboard.current.rKey.wasPressedThisFrame;
                case DemoInputs.ChangeFOVEnabled:	return Keyboard.current.vKey.wasPressedThisFrame;
                case DemoInputs.LookAroundEnabled:	return Mouse.current.leftButton.isPressed;
                case DemoInputs.IncreaseRopeLength:	return Keyboard.current.oem2Key.wasPressedThisFrame;
                case DemoInputs.DecreaseRopeLength:	return Keyboard.current.oem1Key.wasPressedThisFrame;
                case DemoInputs.MoveLeft:			return Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.lKey.wasPressedThisFrame;
                case DemoInputs.MoveRight:			return Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame;
                case DemoInputs.MouseLookEnabled:	return Keyboard.current.f4Key.wasPressedThisFrame;
                case DemoInputs.Quit:				return Keyboard.current.escapeKey.wasPressedThisFrame;
                case DemoInputs.ShowControls:		return Keyboard.current.f1Key.wasPressedThisFrame;
                case DemoInputs.MoveRopesEnabled:	return Keyboard.current.mKey.isPressed;
            }
#else
			switch ( input )
			{
				case DemoInputs.Shoot:				return Input.GetMouseButtonDown(0);
				case DemoInputs.ShootRay:			return Input.GetKeyDown(KeyCode.Space);
				case DemoInputs.Patch:				return Input.GetMouseButtonDown(1);
				case DemoInputs.RemoveImpact:		return Input.GetKeyDown(KeyCode.R);
				case DemoInputs.ChangeFOVEnabled:	return Input.GetKey(KeyCode.V);
				case DemoInputs.LookAroundEnabled:	return Input.GetMouseButton(1);
				case DemoInputs.IncreaseRopeLength:	return Input.GetKey(KeyCode.Alpha2);
				case DemoInputs.DecreaseRopeLength:	return Input.GetKey(KeyCode.Alpha1);
				case DemoInputs.MoveLeft:			return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
				case DemoInputs.MoveRight:			return Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
				case DemoInputs.MouseLookEnabled:	return Input.GetKeyDown(KeyCode.F4);
				case DemoInputs.Quit:				return Input.GetKeyDown(KeyCode.Escape);
				case DemoInputs.ShowControls:		return Input.GetKeyDown(KeyCode.F1);
				case DemoInputs.MoveRopesEnabled:	return Input.GetKeyDown(KeyCode.M);
			}
#endif
			return false;
		}
	}

}