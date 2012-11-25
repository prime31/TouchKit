using UnityEngine;
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


public class GKSwipeRecognizer : AbstractGestureRecognizer
{
	public int numberOfTouchesRequired = 1;
	public float timeToSwipe = 0.5f;	
	public float allowedVariance = 35.0f;
	public float minimumDistance = 40.0f;
	public GKSwipeDirection swipesToDetect = GKSwipeDirection.All;
	public float swipeVelocity { get; private set; }
	public GKSwipeDirection completedSwipeDirection;
	
	private GKTouchTracker[] _touchInfoArray;
	
		
	internal class GKTouchTracker
	{
		internal enum GKSwipeDetectionStatus
		{
			Waiting,
			Failed,
			Done
		}
		
		
		public GKSwipeRecognizer parentFixThis;
		public Vector2 startPoint;
		public float startTime;
		public GKSwipeDirection swipeDetectionState; // The current swipes that are still possibly valid
		public GKSwipeDirection completedSwipeDirection; // If a successful swipe occurs, this will be the type
		public GKSwipeDirection swipesToDetect; // Bitmask of SwipeDirections with the swipes that should be looked for
		public GKSwipeDetectionStatus swipeDetectionStatus; // Current status of the detector
		
		
		public GKTouchTracker( GKSwipeDirection swipesToDetect )
		{
			this.swipesToDetect = swipesToDetect;
			startPoint = Vector2.zero;
			startTime = 0.0f;
			swipeDetectionState = GKSwipeDirection.Horizontal;
			completedSwipeDirection = 0;
			swipeDetectionStatus = GKSwipeDetectionStatus.Waiting;
		}
		
	
		public void resetWithTouch( Touch touch )
		{
			// Initialize the detectionState only with the swipe types we want to listen for
			swipeDetectionState = swipesToDetect;
			startPoint = touch.position;
			startTime = Time.time;
			swipeDetectionStatus = GKSwipeDetectionStatus.Waiting;
		}
		
		
		public bool processWithTouch( Touch touch )
		{
			// If we already completed the swipe detection or if none are availalbe get out of here
			if( swipeDetectionStatus != GKSwipeDetectionStatus.Waiting )
				return false;
	
			// If we have a time stipulation and we exceeded it stop listening for swipes
			if( parentFixThis.timeToSwipe > 0.0f && ( Time.time - startTime ) > parentFixThis.timeToSwipe )
			{
				swipeDetectionStatus = GKSwipeDetectionStatus.Failed;
				return false;
			}
			
	
			// When dealing with standalones (non touch-based devices) we need to be careful what we examaine
			// We filter out all touches (mouse movements really) that didnt move
	#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
			if( touch.deltaPosition.x != 0.0f || touch.deltaPosition.y != 0.0f )
			{
	#endif

				// Check the delta move positions.  We can rule out at least 2 directions
				if( touch.deltaPosition.x > 0.0f )
					swipeDetectionState &= ~GKSwipeDirection.Left;
				if( touch.deltaPosition.x < 0.0f )
					swipeDetectionState &= ~GKSwipeDirection.Right;
				
				if( touch.deltaPosition.y < 0.0f )
					swipeDetectionState &= ~GKSwipeDirection.Up;			
				if( touch.deltaPosition.y > 0.0f )
					swipeDetectionState &= ~GKSwipeDirection.Down;
	
	#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
			}
	#endif
			
			//Debug.Log( string.Format( "swipeStatus: {0}", swipeDetectionState ) );
			
			// Grab the total distance moved in both directions
			float xDeltaAbs = Mathf.Abs( startPoint.x - touch.position.x );
			float yDeltaAbs = Mathf.Abs( startPoint.y - touch.position.y );
			
			// Only check for swipes in directions that havent been ruled out yet
			if( ( swipeDetectionState & GKSwipeDirection.Left ) != 0 )
			{
				if( xDeltaAbs > parentFixThis.minimumDistance )
				{
					if( yDeltaAbs < parentFixThis.allowedVariance )
					{
						completedSwipeDirection = GKSwipeDirection.Left;
						parentFixThis.swipeVelocity = xDeltaAbs / parentFixThis.timeToSwipe;
						return true;
					}
					
					// We exceeded our variance so this swipe is no longer allowed
					swipeDetectionState &= ~GKSwipeDirection.Left;
				}
			}
	
			// Right check
			if( ( swipeDetectionState & GKSwipeDirection.Right ) != 0 )
			{
				if( xDeltaAbs > parentFixThis.minimumDistance )
				{
					if( yDeltaAbs < parentFixThis.allowedVariance )
					{
						completedSwipeDirection = GKSwipeDirection.Right;
						parentFixThis.swipeVelocity = xDeltaAbs / parentFixThis.timeToSwipe;
						return true;
					}
					
					// We exceeded our variance so this swipe is no longer allowed
					swipeDetectionState &= ~GKSwipeDirection.Right;
				}
			}
			
			// Up check
			if( ( swipeDetectionState & GKSwipeDirection.Up ) != 0 )
			{
				if( yDeltaAbs > parentFixThis.minimumDistance )
				{
					if( xDeltaAbs < parentFixThis.allowedVariance )
					{
						completedSwipeDirection = GKSwipeDirection.Up;
						parentFixThis.swipeVelocity = yDeltaAbs / parentFixThis.timeToSwipe;
						return true;
					}
					
					// We exceeded our variance so this swipe is no longer allowed
					swipeDetectionState &= ~GKSwipeDirection.Up;
				}
			}
			
			// Down check
			if( ( swipeDetectionState & GKSwipeDirection.Down ) != 0 )
			{
				if( yDeltaAbs > parentFixThis.minimumDistance )
				{
					if( xDeltaAbs < parentFixThis.allowedVariance )
					{
						completedSwipeDirection = GKSwipeDirection.Down;
						parentFixThis.swipeVelocity = yDeltaAbs / parentFixThis.timeToSwipe;
						return true;
					}
					
					// We exceeded our variance so this swipe is no longer allowed
					swipeDetectionState &= ~GKSwipeDirection.Down;
				}
			}
			
			return false;
		}
		
	}
	
	
	public GKSwipeRecognizer()
	{
		// we track up to 5 touches by default
		_touchInfoArray = new GKTouchTracker[5];
	}
	
	
	public override void touchesBegan( List<Touch> touches )
	{
		foreach( var t in touches )
		{
			if( _touchInfoArray[t.fingerId] == null )
			{
				_touchInfoArray[t.fingerId] = new GKTouchTracker( swipesToDetect );
				_touchInfoArray[t.fingerId].parentFixThis = this;
			}
			
			_touchInfoArray[t.fingerId].resetWithTouch( t );
		}
		
		if( state == GestureRecognizerState.Possible )
			state = GestureRecognizerState.Began;
	}
	
	
	public override void touchesMoved( List<Touch> touches )
	{
		if( state == GestureRecognizerState.Began )
		{
			foreach( var t in touches )
			{
				if( _touchInfoArray[t.fingerId].processWithTouch( t ) )
				{
					completedSwipeDirection = _touchInfoArray[t.fingerId].completedSwipeDirection;
					state = GestureRecognizerState.Recognized;
					break;
				}
				else if( _touchInfoArray[t.fingerId].swipeDetectionStatus == GKTouchTracker.GKSwipeDetectionStatus.Failed )
				{
					state = GestureRecognizerState.Failed;
					break;
				}
			}
		}
	}
	
	
	public override void touchesEnded( List<Touch> touches )
	{
		reset();
	}
	
	
	public override string ToString()
	{
		return string.Format( "{0}, swipe direction: {1}", base.ToString(), completedSwipeDirection );
	}
}
