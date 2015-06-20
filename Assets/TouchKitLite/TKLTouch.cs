using UnityEngine;
using System.Collections;


namespace Prime31 {

public class TKLTouch
{
	public readonly int fingerId;
	public Vector2 position;
	public Vector2 deltaPosition;
	public float deltaTime;
	public int tapCount;
	public TouchPhase phase = TouchPhase.Ended;

	public Vector2 previousPosition
	{
		get { return position - deltaPosition; }
	}

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	// used to track mouse movement and fake touches
	private Vector2? _lastPosition;
	private double _lastClickTime;
	private double _multipleClickInterval = 0.2;
#endif


	public TKLTouch( int fingerId )
	{
		// lock this TKTouch to the fingerId
		this.fingerId = fingerId;
	}


	public TKLTouch populateWithTouch( Touch touch )
	{
		position = touch.position;
		deltaPosition = touch.deltaPosition;
		deltaTime = touch.deltaTime;
		tapCount = touch.tapCount;

		// canceled and ended are the same to us
		if( touch.phase == TouchPhase.Canceled )
			phase = TouchPhase.Ended;
		else
			phase = touch.phase;

		return this;
	}


	/// <summary>
	/// populates the TKTouch with either mouse or touch input
	/// </summary>
	public void populate()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		populateFromMouse();
#else
		populateWithTouch( Input.touches[0] );
#endif
	}


	public bool hasMouseInput()
	{
		return Input.GetMouseButtonUp( 0 ) || Input.GetMouseButton( 0 );
	}


#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	public TKLTouch populateWithPosition( Vector3 currentPosition, TouchPhase touchPhase )
	{
		var currentPosition2d = new Vector2( currentPosition.x, currentPosition.y );

		// if we have a lastMousePosition use it to get a delta
		if( _lastPosition.HasValue )
			deltaPosition = currentPosition2d - _lastPosition.Value;
		else
			deltaPosition = new Vector2( 0, 0 );

		switch( touchPhase )
		{
		case TouchPhase.Began:
			phase = TouchPhase.Began;

			// check for multiple clicks
			if( Time.time < _lastClickTime + _multipleClickInterval )
				tapCount++;
			else
				tapCount = 1;
			_lastPosition = currentPosition2d;
			_lastClickTime = Time.time;
			break;
		case TouchPhase.Stationary:
		case TouchPhase.Moved:
			if( deltaPosition.sqrMagnitude == 0 )
				phase = TouchPhase.Stationary;
			else
				phase = TouchPhase.Moved;

			_lastPosition = currentPosition2d;
			break;
		case TouchPhase.Ended:
			phase = TouchPhase.Ended;
			_lastPosition = null;
			break;
		}

		position = currentPosition2d;

		return this;
	}


	public TKLTouch populateFromMouse()
	{
		// do we have some input to work with?
		if( hasMouseInput() )
		{
			TouchPhase phase = TouchPhase.Moved;
			if( Input.GetMouseButtonUp( 0 ) )
				phase = TouchPhase.Ended;
			if( Input.GetMouseButtonDown( 0 ) )
				phase = TouchPhase.Began;

			var currentMousePosition = new Vector2( Input.mousePosition.x, Input.mousePosition.y );
			this.populateWithPosition( currentMousePosition, phase );
		}

		return this;
	}
#endif


	public override string ToString()
	{
		return string.Format( "[TKLTouch] fingerId: {0}, phase: {1}, position: {2}", fingerId, phase, position );
	}

}}