using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[System.Flags]
public enum GKSwipeDirection
{
    Left 		= ( 1 << 0 ),
    Right 		= ( 1 << 1 ),
    Up 			= ( 1 << 2 ),
    Down 		= ( 1 << 4 ),
    Horizontal 	= ( Left | Right ),
    Vertical 	= ( Up | Down ),
    All 		= ( Horizontal | Vertical )
}


public class GKSwipeRecognizer : GKAbstractGestureRecognizer
{
	public event Action<GKSwipeRecognizer> gestureRecognizedEvent;
	
	public float timeToSwipe = 0.5f;	
	public float allowedVariance = 35.0f;
	public float minimumDistance = 40.0f;
	public GKSwipeDirection swipesToDetect = GKSwipeDirection.All;
	public float swipeVelocity { get; private set; }
	public GKSwipeDirection completedSwipeDirection { get; private set; }
	
	// swipe state info
	private Vector2 _startPoint;
	private float _startTime;
	private GKSwipeDirection _swipeDetectionState; // the current swipes that are still possibly valid
	

	
	private bool checkForSwipeCompletion( GKTouch touch )
	{
		// if we have a time stipulation and we exceeded it stop listening for swipes
		if( timeToSwipe > 0.0f && ( Time.time - _startTime ) > timeToSwipe )
		{
			state = GKGestureRecognizerState.Failed;
			return false;
		}
		

		// when dealing with standalones (non touch-based devices) we need to be careful what we examaine
		// we filter out all touches (mouse movements really) that didnt move
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		if( touch.deltaPosition.x != 0.0f || touch.deltaPosition.y != 0.0f )
		{
#endif
			// check the delta move positions.  We can rule out at least 2 directions
			if( touch.deltaPosition.x > 0.0f )
				_swipeDetectionState &= ~GKSwipeDirection.Left;
			if( touch.deltaPosition.x < 0.0f )
				_swipeDetectionState &= ~GKSwipeDirection.Right;
			
			if( touch.deltaPosition.y < 0.0f )
				_swipeDetectionState &= ~GKSwipeDirection.Up;			
			if( touch.deltaPosition.y > 0.0f )
				_swipeDetectionState &= ~GKSwipeDirection.Down;

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		}
#endif
		
		//Debug.Log( string.Format( "swipeStatus: {0}", swipeDetectionState ) );
		
		// Grab the total distance moved in both directions
		var xDeltaAbs = Mathf.Abs( _startPoint.x - touch.position.x );
		var yDeltaAbs = Mathf.Abs( _startPoint.y - touch.position.y );
		
		// only check for swipes in directions that havent been ruled out yet
		// left check
		if( ( _swipeDetectionState & GKSwipeDirection.Left ) != 0 )
		{
			if( xDeltaAbs > minimumDistance )
			{
				if( yDeltaAbs < allowedVariance )
				{
					completedSwipeDirection = GKSwipeDirection.Left;
					swipeVelocity = xDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~GKSwipeDirection.Left;
			}
		}

		// right check
		if( ( _swipeDetectionState & GKSwipeDirection.Right ) != 0 )
		{
			if( xDeltaAbs > minimumDistance )
			{
				if( yDeltaAbs < allowedVariance )
				{
					completedSwipeDirection = GKSwipeDirection.Right;
					swipeVelocity = xDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~GKSwipeDirection.Right;
			}
		}
		
		// up check
		if( ( _swipeDetectionState & GKSwipeDirection.Up ) != 0 )
		{
			if( yDeltaAbs > minimumDistance )
			{
				if( xDeltaAbs < allowedVariance )
				{
					completedSwipeDirection = GKSwipeDirection.Up;
					swipeVelocity = yDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~GKSwipeDirection.Up;
			}
		}
		
		// cown check
		if( ( _swipeDetectionState & GKSwipeDirection.Down ) != 0 )
		{
			if( yDeltaAbs > minimumDistance )
			{
				if( xDeltaAbs < allowedVariance )
				{
					completedSwipeDirection = GKSwipeDirection.Down;
					swipeVelocity = yDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~GKSwipeDirection.Down;
			}
		}
		
		return false;
	}
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	

	internal override void touchesBegan( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Possible )
		{
			_swipeDetectionState = swipesToDetect;
			_startPoint = touches[0].position;
			_startTime = Time.time;
			_trackingTouches.Add( touches[0] );
			
			state = GKGestureRecognizerState.Began;
		}
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began )
		{
			if( checkForSwipeCompletion( touches[0] ) )
			{
				state = GKGestureRecognizerState.Recognized;
			}
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		state = GKGestureRecognizerState.Failed;
	}
	
	
	public override string ToString()
	{
		return string.Format( "{0}, swipe direction: {1}, swipe velocity: {2}", base.ToString(), completedSwipeDirection, swipeVelocity );
	}
}
