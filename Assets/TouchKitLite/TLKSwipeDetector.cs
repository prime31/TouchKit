using UnityEngine;
using System.Collections.Generic;
using System;


namespace Prime31
{

public enum SwipeDirection
{
	Left = ( 1 << 0 ),
	Right = ( 1 << 1 ),
	Up = ( 1 << 2 ),
	Down = ( 1 << 4 ),
	Horizontal = ( Left | Right ),
	Vertical = ( Up | Down ),
	All = ( Horizontal | Vertical )
}


public class TLKSwipeDetector : MonoBehaviour
{
	public event Action<SwipeDirection> onSwipeDeteced;


	public float timeToSwipe = 0.5f;
	public float swipeVelocity;
	SwipeDirection completedSwipeDirection;

	public float _minimumDistance = 2f;
	public float _allowedVariance = 1.5f;
	public SwipeDirection _swipesToDetect = SwipeDirection.All;

	// swipe state info
	private Vector2 _startPoint;
	private float _startTime;
	private SwipeDirection _swipeDetectionState;
	// the current swipes that are still possibly valid
	bool _didCompleteDetection = true;


	void Update()
	{
		// dont process drags if we have no input
		if( TouchKitLite.instance.liveTouches.Count == 0 )
			return;

		var touch = TouchKitLite.instance.liveTouches[0];

		// touch down, possible chance for a swipe
		if( touch.phase == TouchPhase.Began )
		{
			_swipeDetectionState = _swipesToDetect;
			_startPoint = touch.position;
			_startTime = Time.time;
			_didCompleteDetection = false;
		}
		else if( touch.phase == TouchPhase.Moved )
		{
			if( !_didCompleteDetection && checkForSwipeCompletion( touch ) )
			{
				_didCompleteDetection = true;
				if( onSwipeDeteced != null )
					onSwipeDeteced( completedSwipeDirection );
			}
		}
	}


	private bool checkForSwipeCompletion( TKLTouch touch )
	{
		// if we have a time stipulation and we exceeded it stop listening for swipes
		if( timeToSwipe > 0.0f && ( Time.time - _startTime ) > timeToSwipe )
		{
			return false;
		}


		// when dealing with standalones (non touch-based devices) we need to be careful what we examaine
		// we filter out all touches (mouse movements really) that didnt move
		#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WEBGL
		if( touch.deltaPosition.x != 0.0f || touch.deltaPosition.y != 0.0f )
		{
			#endif
			// check the delta move positions.  We can rule out at least 2 directions
			if( touch.deltaPosition.x > 0.0f )
				_swipeDetectionState &= ~SwipeDirection.Left;
			if( touch.deltaPosition.x < 0.0f )
				_swipeDetectionState &= ~SwipeDirection.Right;

			if( touch.deltaPosition.y < 0.0f )
				_swipeDetectionState &= ~SwipeDirection.Up;			
			if( touch.deltaPosition.y > 0.0f )
				_swipeDetectionState &= ~SwipeDirection.Down;

			#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WEBGL
		}
		#endif

		//Debug.Log( string.Format( "swipeStatus: {0}", swipeDetectionState ) );

		// Grab the total distance moved in both directions
		var xDeltaAbsCm = Mathf.Abs( _startPoint.x - touch.position.x ) / TouchKitLite.instance.screenPixelsPerCm;
		var yDeltaAbsCm = Mathf.Abs( _startPoint.y - touch.position.y ) / TouchKitLite.instance.screenPixelsPerCm;

		// only check for swipes in directions that havent been ruled out yet
		// left check
		if( ( _swipeDetectionState & SwipeDirection.Left ) != 0 )
		{
			if( xDeltaAbsCm > _minimumDistance )
			{
				if( yDeltaAbsCm < _allowedVariance )
				{
					completedSwipeDirection = SwipeDirection.Left;
					swipeVelocity = xDeltaAbsCm / ( Time.time - _startTime );
					return true;
				}

				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~SwipeDirection.Left;
			}
		}

		// right check
		if( ( _swipeDetectionState & SwipeDirection.Right ) != 0 )
		{
			if( xDeltaAbsCm > _minimumDistance )
			{
				if( yDeltaAbsCm < _allowedVariance )
				{
					completedSwipeDirection = SwipeDirection.Right;
					swipeVelocity = xDeltaAbsCm / ( Time.time - _startTime );
					return true;
				}

				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~SwipeDirection.Right;
			}
		}

		// up check
		if( ( _swipeDetectionState & SwipeDirection.Up ) != 0 )
		{
			if( yDeltaAbsCm > _minimumDistance )
			{
				if( xDeltaAbsCm < _allowedVariance )
				{
					completedSwipeDirection = SwipeDirection.Up;
					swipeVelocity = yDeltaAbsCm / ( Time.time - _startTime );
					return true;
				}

				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~SwipeDirection.Up;
			}
		}

		// down check
		if( ( _swipeDetectionState & SwipeDirection.Down ) != 0 )
		{
			if( yDeltaAbsCm > _minimumDistance )
			{
				if( xDeltaAbsCm < _allowedVariance )
				{
					completedSwipeDirection = SwipeDirection.Down;
					swipeVelocity = yDeltaAbsCm / ( Time.time - _startTime );
					return true;
				}

				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~SwipeDirection.Down;
			}
		}

		return false;
	}
}
}
