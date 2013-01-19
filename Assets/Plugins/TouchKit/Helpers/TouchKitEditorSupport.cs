using UnityEngine;
using System.Collections;



#if UNITY_EDITOR
/// <summary>
/// this only exists in the editor to assist with testing and simulating touches and keeping the main class clean
/// </summary>
public partial class TouchKit
{
	private bool _isUnityRemoteActive = false; // hack to detect the Unity remote. Once you touch the screen once mouse input will be ignored
	private Vector3 _simulatedMultitouchStartPosition;
	private Vector3 _simulatedMousePosition;
	private bool _isSimulatingMultitouch;
	private bool _hasActiveSimulatedTouch; // are we currently simulating multitouch with active touches (mouse is down)?
	
	
	/// <summary>
	/// returns true if mouse input should be processed as touch input. it will be true when the Unity Remote is not active.
	/// </summary>
	private bool shouldProcessMouseInput()
	{
		// check to see if the Unity Remote is active
		if( !_isUnityRemoteActive && Input.touchCount > 0 )
		{
			Debug.LogWarning( "disabling mouse input because we detected a Unity Remote connected" );
			_isUnityRemoteActive = true;
		}
		
		
		// if Unity remote is not active and alt is being held down we are simulating multitouch
		if( !_isUnityRemoteActive && ( Input.GetKey( KeyCode.LeftAlt ) || Input.GetKeyUp( KeyCode.LeftAlt ) ) )
		{
			// record our start position when alt is pressed
			if( Input.GetKeyDown( KeyCode.LeftAlt ) )
			{
				_simulatedMultitouchStartPosition = Input.mousePosition;
				_isSimulatingMultitouch = true;
			}
			else if( Input.GetKeyUp( KeyCode.LeftAlt ) )
			{
				_hasActiveSimulatedTouch = false;
				_isSimulatingMultitouch = false;
			}
			else
			{
				// a mouse down now results in two touches being created. first we setup the position of the touches based on the original point when alt was pressed
				var radius = Vector3.Distance( Input.mousePosition, _simulatedMultitouchStartPosition );
				var angle = TKRotationRecognizer.angleBetweenPoints( _simulatedMultitouchStartPosition, Input.mousePosition );
				angle = Mathf.Deg2Rad * angle;
				
				var opposite = Mathf.Sin( angle ) * radius;
				var adjacent = Mathf.Cos( angle ) * radius;
				_simulatedMousePosition = new Vector3( _simulatedMultitouchStartPosition.x - adjacent, _simulatedMultitouchStartPosition.y - opposite );
				
				// if we get a mouse down event its time to populate the touches
				if( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButton( 0 ) )
				{
					_hasActiveSimulatedTouch = true;
					_liveTouches.Add( _touchCache[0].populateFromMouse() );
					_liveTouches.Add( _touchCache[1].populateFromMouseAtPosition( _simulatedMousePosition ) );
				}
				else
				{
					_hasActiveSimulatedTouch = false;
				}
			}
		}
		
		return !_isUnityRemoteActive;
	}
	
	
	// this is for debugging only while in the editor
	private void OnDrawGizmos()
	{
		if( !debugDrawBoundaryFrames )
			return;
		
		// if we are simulating multitouch, draw appropriate gizmos to show them
		if( _isSimulatingMultitouch )
		{
			var imageName = _hasActiveSimulatedTouch ? "greenPoint.png" : "redPoint.png";
			var mousePos = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( Input.mousePosition ) );
			Gizmos.DrawIcon( mousePos, imageName, false );
			
			mousePos = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( _simulatedMousePosition ) );
			Gizmos.DrawIcon( mousePos, imageName, false );
		}
		
		var colors = new Color[] { Color.red, Color.cyan, Color.red, Color.magenta, Color.yellow };
		int i = 0;
		
		foreach( var r in _gestureRecognizers )
		{
			if( r.boundaryFrame.HasValue )
			{
				debugDrawRect( r.boundaryFrame.Value, colors[i] );
				if( ++i >= colors.Length )
					i = 0;
			}
		}
	}
	
	
	private void debugDrawRect( TKRect rect, Color color )
	{
		var bl = new Vector3( rect.xMin, rect.yMin, 0 );
		var br = new Vector3( rect.xMax, rect.yMin, 0 );
		var tl = new Vector3( rect.xMin, rect.yMax, 0 );
		var tr = new Vector3( rect.xMax, rect.yMax, 0 );
		
		bl = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( bl ) );
		br = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( br ) );
		tl = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( tl ) );
		tr = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( tr ) );
		
		// draw four sides
		Debug.DrawLine( bl, br, color );
		Debug.DrawLine( br, tr, color );
		Debug.DrawLine( tr, tl, color );
		Debug.DrawLine( tl, bl, color );
		
		// make an "x" at the midpoint
		Debug.DrawLine( tl, br, color );
		Debug.DrawLine( bl, tr, color );
	}

}
#endif