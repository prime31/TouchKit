using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public enum GestureRecognizerState
{
	Possible,
	Began,
	Changed,
	Ended,
	Cancelled,
	
	Failed,
	
	Recognized = Ended
}


public abstract class AbstractGestureRecognizer
{
	public event Action<AbstractGestureRecognizer> gestureStateChangedEvent;
	
	public bool enabled = true;
	public bool delayTouchesBegan = false;
	public bool delayTouchesEnded = true;
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
			GestureStateTransition? transition = null;
			
			for( var i = 0; i < 9; i++ )
			{
				if( _allowedTransitions[i].fromState == _state && _allowedTransitions[i].toState == value )
				{
					transition = _allowedTransitions[i];
					break;
				}
			}
			
			if( transition == null )
			{
				Debug.LogError( "Invalid state transition from: " + _state + " to " + value );
				return;
			}
			
			// we have a valid transition
			_state = value;
			
			if( transition.Value.shouldNotify )
			{
				if( gestureStateChangedEvent != null )
					gestureStateChangedEvent( this );
			}
			
			if( transition.Value.shouldReset )
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
            state != GestureRecognizerState.Cancelled && 
            state != GestureRecognizerState.Ended );
		}
	}
	
	private static GestureStateTransition[] _allowedTransitions;
	
	// internal class used to verify state transitions
	private struct GestureStateTransition
	{
		public GestureRecognizerState fromState;
		public GestureRecognizerState toState;
		public bool shouldNotify;
		public bool shouldReset;
		
		
		public GestureStateTransition( GestureRecognizerState fromState, GestureRecognizerState toState, bool shouldNotify, bool shouldReset )
		{
			this.fromState = fromState;
			this.toState = toState;
			this.shouldNotify = shouldNotify;
			this.shouldReset = shouldReset;
		}
	}
	
	
	#region Constructor
	
	static AbstractGestureRecognizer()
	{
		// setup the state transitions
		_allowedTransitions = new GestureStateTransition[9];
		_allowedTransitions[0] = new GestureStateTransition( GestureRecognizerState.Possible, GestureRecognizerState.Recognized, true, true );
		_allowedTransitions[1] = new GestureStateTransition( GestureRecognizerState.Possible, GestureRecognizerState.Failed, false, true );
		_allowedTransitions[2] = new GestureStateTransition( GestureRecognizerState.Possible, GestureRecognizerState.Began, true, false );
		_allowedTransitions[3] = new GestureStateTransition( GestureRecognizerState.Began, GestureRecognizerState.Changed, true, false );
		_allowedTransitions[4] = new GestureStateTransition( GestureRecognizerState.Began, GestureRecognizerState.Cancelled, true, true );
		_allowedTransitions[5] = new GestureStateTransition( GestureRecognizerState.Began, GestureRecognizerState.Ended, true, true );
		_allowedTransitions[6] = new GestureStateTransition( GestureRecognizerState.Changed, GestureRecognizerState.Changed, true, false );
		_allowedTransitions[7] = new GestureStateTransition( GestureRecognizerState.Changed, GestureRecognizerState.Cancelled, true, true );
		_allowedTransitions[8] = new GestureStateTransition( GestureRecognizerState.Changed, GestureRecognizerState.Ended, true, true );
	}
	
	#endregion
	
	
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
