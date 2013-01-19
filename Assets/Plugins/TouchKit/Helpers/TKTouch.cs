using UnityEngine;
using System.Collections;



public class TKTouch
{
	public readonly int fingerId;
	public Vector2 position;
	public Vector2 deltaPosition;
	public float deltaTime;
	public int tapCount;
	public TouchPhase phase = TouchPhase.Ended;
	
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	// used to track mouse movement and fake touches
	private static Vector2? _lastMousePosition;
	private double _lastClickTime;
	private double _multipleClickInterval = 0.2;
#endif
	
	
	public TKTouch( int fingerId )
	{
		// lock this TKTouch to the fingerId
		this.fingerId = fingerId;
	}
	

	public TKTouch populateWithTouch( Touch touch )
	{
		position = touch.position;
		deltaPosition = touch.deltaPosition;
		deltaTime = touch.deltaTime;
		tapCount = touch.tapCount;
		phase = touch.phase;
		
		return this;
	}
	
	
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	
	/// <summary>
	/// seperating this out into a seperate method allows us to pass in a real mousePosition or a simulated mouse position when populating teh touch
	/// </summary>
	public TKTouch populateFromMouseAtPosition( Vector3 mousePosition )
	{
		// do we have some input to work with?
		if( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButton( 0 ) )
		{
			var currentMousePosition = new Vector2( mousePosition.x, mousePosition.y );
			
			// if we have a lastMousePosition use it to get a delta
			if( _lastMousePosition.HasValue )
				deltaPosition = currentMousePosition - _lastMousePosition.Value;
			
			if( Input.GetMouseButtonDown( 0 ) )
			{
				phase = TouchPhase.Began;
				_lastMousePosition = Input.mousePosition;
				
				// check for multiple clicks
				if( Time.time < _lastClickTime + _multipleClickInterval )
					tapCount++;
				else
					tapCount = 1;
				_lastClickTime = Time.time;
			}
			else if( Input.GetMouseButtonUp( 0 ) )
			{
				phase = TouchPhase.Ended;
				_lastMousePosition = null;
			}
			else if( Input.GetMouseButton( 0 ) )
			{
				phase = TouchPhase.Moved;
				_lastMousePosition = mousePosition;
			}
			
			position = currentMousePosition;
		}
		
		return this;
	}
	
	
	public TKTouch populateFromMouse()
	{
		return populateFromMouseAtPosition( Input.mousePosition );
	}
#endif
	
	
	public override string ToString()
	{
		return string.Format( "[TKTouch] fingerId: {0}, phase: {1}, position: {2}", fingerId, phase, position );
	}

}