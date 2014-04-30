using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
/// <summary>
/// this only exists in the editor to assist with testing and simulating touches and keeping the main class clean
/// </summary>
public partial class TouchKit
{
	private Vector3? _simulatedMultitouchStartPosition;
	private Vector3 _simulatedMousePosition;
	private bool _hasActiveSimulatedTouch;
	private bool _hasActiveSimulatedMultitouch;
	private bool _hasUnityRemoteActive;
	
	
	/// <summary>
	/// returns true if mouse input should be processed as touch input. it will be true when the Unity Remote is not active.
	/// </summary>
	private bool shouldProcessMouseInput()
	{
		if (!simulateTouches)
			return false;
		
		// check to see if the Unity Remote is active
		if (Input.touchCount > 0)
		{
			Debug.LogWarning("disabling touch simulation because we detected a Unity Remote connected");
			simulateTouches = false;
			return false;
		}
		
		// if enabled and alt is being held down we are simulating pinching
		if (simulateMultitouch && (_hasActiveSimulatedMultitouch || Input.GetKey(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.LeftAlt)))
		{
			if (Input.GetKeyDown (KeyCode.LeftAlt))
			{
				_simulatedMultitouchStartPosition = Input.mousePosition;
			} else if (Input.GetKey(KeyCode.LeftShift)) {
				// calculate the last mouse position from the simulated position and shift the start position acordingly
				Vector3 lastMousePosition = _simulatedMultitouchStartPosition.Value + (_simulatedMultitouchStartPosition.Value - _simulatedMousePosition);
				Vector3 diff =  Input.mousePosition - lastMousePosition;
				_simulatedMultitouchStartPosition += diff;
			}
			
			if (Input.GetKey (KeyCode.LeftAlt) || Input.GetKeyUp (KeyCode.LeftAlt))
			{
				Vector3 diff = new Vector3(Input.mousePosition.x - _simulatedMultitouchStartPosition.Value.x, Input.mousePosition.y - _simulatedMultitouchStartPosition.Value.y);
				_simulatedMousePosition = _simulatedMultitouchStartPosition.Value - diff;
			}
			
			TouchPhase? touchPhase = null;
			if (Input.GetKey (KeyCode.LeftAlt) && Input.GetMouseButton(0))
			{
				// if we haven't started yet, add a touch began, else move
				if (!_hasActiveSimulatedMultitouch)
				{
					_hasActiveSimulatedMultitouch = true;
					touchPhase = TouchPhase.Began;
				} else {
					touchPhase = TouchPhase.Moved;
				}
			}
			
			if ((Input.GetKeyUp (KeyCode.LeftAlt) || Input.GetMouseButtonUp(0)) && _hasActiveSimulatedMultitouch)
			{
				touchPhase = TouchPhase.Ended;
				_hasActiveSimulatedMultitouch = false;
			}
			
			
			if (touchPhase.HasValue)
			{
				// we need to set up a second touch
				
				_liveTouches.Add(_touchCache[1].populateWithPosition(_simulatedMousePosition, touchPhase.Value));
			}
			
			if (Input.GetKeyUp(KeyCode.LeftAlt))
			{
				_simulatedMultitouchStartPosition = null;
			}
		}
		
		
		_hasActiveSimulatedTouch = Input.GetMouseButton(0);
		
		return true;
	}
	
	
	// this is for debugging only while in the editor
	private void OnDrawGizmos()
	{
		if (_instance == null)
			return;
		
		if (drawTouches)
		{
			// draw a green point for all active touches, including the touches from Unity remote
			foreach (TKTouch touch in _touchCache)
			{
				if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
				{
					var touchPos = Camera.main.ScreenToWorldPoint(Camera.main.transform.InverseTransformPoint(touch.position));
					Gizmos.DrawIcon(touchPos, "greenPoint.png", false);
				}
			}
			
			if (_simulatedMultitouchStartPosition.HasValue && !_hasActiveSimulatedTouch)
			{
				var mousePos = Camera.main.ScreenToWorldPoint(Camera.main.transform.InverseTransformPoint(Input.mousePosition));
				Gizmos.DrawIcon(mousePos, "redPoint.png", false);
				
				var simulatedPos = Camera.main.ScreenToWorldPoint(Camera.main.transform.InverseTransformPoint(_simulatedMousePosition));
				Gizmos.DrawIcon(simulatedPos, "redPoint.png", false);
			}
		}
		
		if (drawDebugBoundaryFrames)
		{
			var colors = new Color[]
			{
				Color.red,
				Color.cyan,
				Color.red,
				Color.magenta,
				Color.yellow
			};
			int i = 0;
			
			foreach (var r in _gestureRecognizers)
			{
				if (r.boundaryFrame.HasValue)
				{
					debugDrawRect(r.boundaryFrame.Value, colors [i]);
					if (++i >= colors.Length)
						i = 0;
				}
			}
		}
	}
	
	
	private void debugDrawRect(TKRect rect, Color color)
	{
		var bl = new Vector3(rect.xMin, rect.yMin, 0);
		var br = new Vector3(rect.xMax, rect.yMin, 0);
		var tl = new Vector3(rect.xMin, rect.yMax, 0);
		var tr = new Vector3(rect.xMax, rect.yMax, 0);
		
		bl = Camera.main.ScreenToWorldPoint(Camera.main.transform.InverseTransformPoint(bl));
		br = Camera.main.ScreenToWorldPoint(Camera.main.transform.InverseTransformPoint(br));
		tl = Camera.main.ScreenToWorldPoint(Camera.main.transform.InverseTransformPoint(tl));
		tr = Camera.main.ScreenToWorldPoint(Camera.main.transform.InverseTransformPoint(tr));
		
		// draw four sides
		Debug.DrawLine(bl, br, color);
		Debug.DrawLine(br, tr, color);
		Debug.DrawLine(tr, tl, color);
		Debug.DrawLine(tl, bl, color);
		
		// make an "x" at the midpoint
        Debug.DrawLine(tl, br, color);
        Debug.DrawLine(bl, tr, color);
    }
    
}
#endif