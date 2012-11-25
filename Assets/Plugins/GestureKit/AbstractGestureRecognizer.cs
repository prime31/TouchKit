using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public enum GestureRecognizerState
{
	Possible,
	Began,
	Failed,
	Recognized
}


public abstract class AbstractGestureRecognizer
{
	public event Action<AbstractGestureRecognizer> gestureStateChangedEvent;
	
	public bool enabled = true;
	/// <summary>
	/// frame that the touch must be within to be recognized. null means full screen
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
			if( _state == value )
				return;
			
			_state = value;
			
			if( _state == GestureRecognizerState.Recognized )
			{
				if( gestureStateChangedEvent != null )
					gestureStateChangedEvent( this );
			}
			
			if( _state == GestureRecognizerState.Recognized || _state == GestureRecognizerState.Failed )
				reset();
		}
	}
	
	private List<Touch> _trackingTouches;
	
	
	private bool shouldAttemptToRecognize
	{
		get
		{
			return ( enabled &&
            state != GestureRecognizerState.Failed &&
            state != GestureRecognizerState.Recognized );
		}
	}
	

	#region Public API
	
	public void recognizeTouches( List<Touch> touches )
	{
		if( !shouldAttemptToRecognize )
			return;
		
		_trackingTouches = touches;
		
		for( var i = 0; i < touches.Count; i++ )
		{
			var touch = touches[i];
			
			switch( touch.phase )
			{
				case TouchPhase.Began:
					touchesBegan( touches );
					break;
				case TouchPhase.Moved:
					touchesMoved( touches );
					break;
				case TouchPhase.Ended:
				case TouchPhase.Canceled:
					touchesEnded( touches );
					break;
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
	public Vector2 location()
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
	        return new Vector2( x/k, y/k );
	    else
	        return Vector2.zero;
	}
	
	
	public virtual void touchesBegan( List<Touch> touches )
	{}
	
	
	public virtual void touchesMoved( List<Touch> touches )
	{}
	
	
	public virtual void touchesEnded( List<Touch> touches )
	{}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}", this.GetType(), state );
	}
	
	#endregion
	
}
