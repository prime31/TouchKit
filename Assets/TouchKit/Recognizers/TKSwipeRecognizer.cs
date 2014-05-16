using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


[System.Flags]
public enum TKSwipeDirection
{
    Left 		= ( 1 << 0 ),
    Right 		= ( 1 << 1 ),
    Up 			= ( 1 << 2 ),
    Down 		= ( 1 << 4 ),
    Horizontal 	= ( Left | Right ),
    Vertical 	= ( Up | Down ),
    All 		= ( Horizontal | Vertical )
}


public class TKSwipeRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKSwipeRecognizer> gestureRecognizedEvent;
	
	public float timeToSwipe = 0.5f;	
	public float swipeVelocity { get; private set; }
	public TKSwipeDirection completedSwipeDirection { get; private set; }
	
	private float _allowedVariance = 35.0f;
	private float _minimumDistance = 40.0f;
	private TKSwipeDirection _swipesToDetect = TKSwipeDirection.All;
	
	// swipe state info
	private Vector2 _startPoint;
	private Vector2 _endPoint;
	private float _startTime;
	private TKSwipeDirection _swipeDetectionState; // the current swipes that are still possibly valid
	
	public Vector2 startPoint
	{
		get
		{
			return this._startPoint;
		}
	}

	public Vector2 endPoint
	{
		get
		{
			return this._endPoint;
		}
	}
	
	
	public TKSwipeRecognizer() : this( 40f, 35f )
	{}
	
	
	public TKSwipeRecognizer( TKSwipeDirection swipesToDetect ) : this( swipesToDetect, 40f, 35f )
	{}
	
	
	public TKSwipeRecognizer( float minimumDistance, float allowedVariance ) : this( TKSwipeDirection.All, minimumDistance, allowedVariance )
	{}
	
	
	public TKSwipeRecognizer( TKSwipeDirection swipesToDetect, float minimumDistance, float allowedVariance )
	{
		_swipesToDetect = swipesToDetect;
		_minimumDistance = minimumDistance * TouchKit.instance.runtimeDistanceModifier;
		_allowedVariance = allowedVariance * TouchKit.instance.runtimeDistanceModifier;
	}

	
	private bool checkForSwipeCompletion( TKTouch touch )
	{
		// if we have a time stipulation and we exceeded it stop listening for swipes
		if( timeToSwipe > 0.0f && ( Time.time - _startTime ) > timeToSwipe )
		{
			state = TKGestureRecognizerState.FailedOrEnded;
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
				_swipeDetectionState &= ~TKSwipeDirection.Left;
			if( touch.deltaPosition.x < 0.0f )
				_swipeDetectionState &= ~TKSwipeDirection.Right;
			
			if( touch.deltaPosition.y < 0.0f )
				_swipeDetectionState &= ~TKSwipeDirection.Up;			
			if( touch.deltaPosition.y > 0.0f )
				_swipeDetectionState &= ~TKSwipeDirection.Down;

#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		}
#endif
		
		//Debug.Log( string.Format( "swipeStatus: {0}", swipeDetectionState ) );
		
		// Grab the total distance moved in both directions
		var xDeltaAbs = Mathf.Abs( _startPoint.x - touch.position.x );
		var yDeltaAbs = Mathf.Abs( _startPoint.y - touch.position.y );

		_endPoint = touch.position;

		// only check for swipes in directions that havent been ruled out yet
		// left check
		if( ( _swipeDetectionState & TKSwipeDirection.Left ) != 0 )
		{
			if( xDeltaAbs > _minimumDistance )
			{
				if( yDeltaAbs < _allowedVariance )
				{
					completedSwipeDirection = TKSwipeDirection.Left;
					swipeVelocity = xDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~TKSwipeDirection.Left;
			}
		}

		// right check
		if( ( _swipeDetectionState & TKSwipeDirection.Right ) != 0 )
		{
			if( xDeltaAbs > _minimumDistance )
			{
				if( yDeltaAbs < _allowedVariance )
				{
					completedSwipeDirection = TKSwipeDirection.Right;
					swipeVelocity = xDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~TKSwipeDirection.Right;
			}
		}
		
		// up check
		if( ( _swipeDetectionState & TKSwipeDirection.Up ) != 0 )
		{
			if( yDeltaAbs > _minimumDistance )
			{
				if( xDeltaAbs < _allowedVariance )
				{
					completedSwipeDirection = TKSwipeDirection.Up;
					swipeVelocity = yDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~TKSwipeDirection.Up;
			}
		}
		
		// cown check
		if( ( _swipeDetectionState & TKSwipeDirection.Down ) != 0 )
		{
			if( yDeltaAbs > _minimumDistance )
			{
				if( xDeltaAbs < _allowedVariance )
				{
					completedSwipeDirection = TKSwipeDirection.Down;
					swipeVelocity = yDeltaAbs / ( Time.time - _startTime );
					return true;
				}
				
				// We exceeded our variance so this swipe is no longer allowed
				_swipeDetectionState &= ~TKSwipeDirection.Down;
			}
		}
		
		return false;
	}
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	

	internal override bool touchesBegan( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Possible )
		{
			_swipeDetectionState = _swipesToDetect;
			_startPoint = touches[0].position;
			_startTime = Time.time;
			_trackingTouches.Add( touches[0] );
			
			state = TKGestureRecognizerState.Began;
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Began )
		{
			if( checkForSwipeCompletion( touches[0] ) )
			{
				state = TKGestureRecognizerState.Recognized;
			}
		}
	}
	
	
	internal override void touchesEnded( List<TKTouch> touches )
	{
		state = TKGestureRecognizerState.FailedOrEnded;
	}
	
	
	public override string ToString()
	{
		return string.Format( "{0}, swipe direction: {1}, swipe velocity: {2}, start point: {3}, end point: {4}",
			base.ToString(), completedSwipeDirection, swipeVelocity, startPoint, endPoint );
	}
}
