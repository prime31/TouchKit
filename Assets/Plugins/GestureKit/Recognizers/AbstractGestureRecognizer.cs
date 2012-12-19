using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public enum GestureRecognizerState
{
	Possible, // we havent started yet and we are still listening
	Began, // we have started and latched at least one finger
	Failed, // no go. failed to recognize
	RecognizedAndStillRecognizing, // this will fire the state changed event and allow a gesture to continue to recognize. useful for continuous gestures
	Recognized // successfully recognized and we are not a continuous recognizer
}


public abstract class AbstractGestureRecognizer
{
	public event Action<AbstractGestureRecognizer> gestureRecognizedEvent;
	
	public bool enabled = true;
	/// <summary>
	/// frame that the touch must be within to be recognized. null means full screen. note that Unity's origin is the bottom left
	/// </summary>
	public Rect? boundaryFrame = null;
	public int numberOfTouches
	{
		get { return _trackingTouches.Count; }
	}
	
	private GestureRecognizerState _state = GestureRecognizerState.Possible;
	public GestureRecognizerState state
	{
		get { return _state; }
		set
		{
			_state = value;
			
			if( _state == GestureRecognizerState.Recognized || _state == GestureRecognizerState.RecognizedAndStillRecognizing )
			{
				if( gestureRecognizedEvent != null )
					gestureRecognizedEvent( this );
			}
			
			if( _state == GestureRecognizerState.Recognized || _state == GestureRecognizerState.Failed )
				reset();
		}
	}
	
	/// <summary>
	/// stores all the touches we are currently tracking
	/// </summary>
	protected List<GKTouch> _trackingTouches = new List<GKTouch>();
	
	
	/// <summary>
	/// checks to see if the touch is currently being tracked by the recognizer
	/// </summary>
	protected bool isTrackingTouch( GKTouch t )
	{
		return _trackingTouches.Contains( t );
	}


	/// <summary>
	/// checks to see if any of the touches are currently being tracked by the recognizer
	/// </summary>
	protected bool isTrackingAnyTouch( List<GKTouch> touches )
	{
		for( int i = 0; i < touches.Count; i++ )
		{
			if( _trackingTouches.Contains( touches[i] ) )
				return true;
		}
		
		return false;
	}
	
	
	/// <summary>
	/// returns only the touches currently being tracked by the recognizer
	/// </summary>
	private List<GKTouch> subsetOfTouchesBeingTracked( List<GKTouch> touches )
	{
		List<GKTouch> trackedTouches = new List<GKTouch>();
		
		for( int i = 0; i < touches.Count; i++ )
		{
			if( isTrackingTouch( touches[i] ) )
				trackedTouches.Add( touches[i] );
		}
		
		return trackedTouches;
	}
	
	
	private bool shouldAttemptToRecognize
	{
		get
		{
			return ( enabled && state != GestureRecognizerState.Failed && state != GestureRecognizerState.Recognized );
		}
	}
	
	
	#region Public API
	
	public void recognizeTouches( List<GKTouch> touches )
	{
		if( !shouldAttemptToRecognize )
			return;
		
		for( var i = 0; i < touches.Count; i++ )
		{
			var touch = touches[i];
			switch( touch.phase )
			{
				case TouchPhase.Began:
					if( !boundaryFrame.HasValue || ( boundaryFrame.HasValue && boundaryFrame.Value.Contains( touch.position ) ) )
						touchesBegan( touches );
					break;
				case TouchPhase.Moved:
				{
					// limit touches sent to those that are being tracked
					var subsetOfTouches = subsetOfTouchesBeingTracked( touches );
					if( subsetOfTouches.Count > 0 )
						touchesMoved( subsetOfTouches );
					break;
				}
				case TouchPhase.Ended:
				case TouchPhase.Canceled:
				{
					// limit touches sent to those that are being tracked
					var subsetOfTouches = subsetOfTouchesBeingTracked( touches );
					if( subsetOfTouches.Count > 0 )
						touchesEnded( subsetOfTouches );
					break;
				}
			}
		}
	}
	
	
	public void reset()
	{
		_state = GestureRecognizerState.Possible;
		_trackingTouches.Clear();
	}
	
	
	/// <summary>
	/// returns the location of the touches. If there are multiple touches this will return the centroid of the location.
	/// </summary>
	public Vector2 touchLocation()
	{
	    var x = 0f;
	    var y = 0f;
	    var k = 0f;
	    
	    foreach( var touch in _trackingTouches )
		{
	        x += touch.position.x;
	        y += touch.position.y;
	        k++;
	    }
	    
	    if( k > 0 )
	        return new Vector2( x / k, y / k );
	    else
	        return Vector2.zero;
	}
	
	
	public virtual void touchesBegan( List<GKTouch> touches )
	{}
	
	
	public virtual void touchesMoved( List<GKTouch> touches )
	{}
	
	
	public virtual void touchesEnded( List<GKTouch> touches )
	{}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}", this.GetType(), state, touchLocation() );
	}
	
	#endregion
	
}
