using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public enum GKGestureRecognizerState
{
	Possible, // we havent started yet and we are still listening
	Began, // we have started and latched at least one finger
	Failed, // no go. failed to recognize
	RecognizedAndStillRecognizing, // this will fire the state changed event and allow a gesture to continue to recognize. useful for continuous gestures
	Recognized // successfully recognized and we are not a continuous recognizer
}


public abstract class GKAbstractGestureRecognizer : IComparable<GKAbstractGestureRecognizer>
{
	public bool enabled = true;
	
	/// <summary>
	/// frame that the touch must be within to be recognized. null means full screen. note that Unity's origin is the bottom left
	/// </summary>
	public Rect? boundaryFrame = null;
	
	/// <summary>
	/// zIndex of touch input. 0 by default. if a zIndex of greater than 0 uses a touch in touchesBegan it will not be passed to any other recognizers.
	/// useful if you have some full screen recognizers and you want to overlay a button/control
	/// </summary>
	public uint zIndex = 0;
	
	private GKGestureRecognizerState _state = GKGestureRecognizerState.Possible;
	public GKGestureRecognizerState state
	{
		get { return _state; }
		set
		{
			_state = value;
			
			if( _state == GKGestureRecognizerState.Recognized || _state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
				fireRecognizedEvent();
			
			if( _state == GKGestureRecognizerState.Recognized || _state == GKGestureRecognizerState.Failed )
				reset();
		}
	}
	
	/// <summary>
	/// stores all the touches we are currently tracking
	/// </summary>
	protected List<GKTouch> _trackingTouches = new List<GKTouch>();
	
	/// <summary>
	/// The subset of touches being tracked that is applicable to the current recognizer. This is kept around to avoid allocations at runtime.
	/// </summary>
	private List<GKTouch> _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer = new List<GKTouch>();
	
	/// <summary>
	/// stores whether we sent any of the phases to the recognizer. This is to avoid sending a phase twice in one frame.
	/// </summary>
	private bool _sentTouchesBegan;
	private bool _sentTouchesMoved;
	private bool _sentTouchesEnded;
	
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
	/// populates the _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer with only the touches currently being tracked by the recognizer.
	/// returns true if there are any touches being tracked
	/// </summary>
	private bool populateSubsetOfTouchesBeingTracked( List<GKTouch> touches )
	{
		_subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Clear();
		
		for( int i = 0; i < touches.Count; i++ )
		{
			if( isTrackingTouch( touches[i] ) )
				_subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Add( touches[i] );
		}
		
		return _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Count > 0;
	}
	
	
	private bool shouldAttemptToRecognize
	{
		get
		{
			return ( enabled && state != GKGestureRecognizerState.Failed && state != GKGestureRecognizerState.Recognized );
		}
	}
	
	
	#region Public API
	
	internal void recognizeTouches( List<GKTouch> touches )
	{
		if( !shouldAttemptToRecognize )
			return;
		
		// reset our state to avoid sending any phase more than once
		_sentTouchesBegan = _sentTouchesMoved = _sentTouchesEnded = false;
		
		for( var i = 0; i < touches.Count; i++ )
		{
			var touch = touches[i];
			switch( touch.phase )
			{
				case TouchPhase.Began:
				{
					// only send touches began once and ensure that the touch is in the boundaryFrame if applicable
					if( !_sentTouchesBegan && ( !boundaryFrame.HasValue || boundaryFrame.Value.Contains( touch.position ) ) )
					{
						// if touchesBegan returns true and we have a zIndex greater than 0 we remove the touches with a phase of Began
						if( touchesBegan( touches ) && zIndex > 0 )
						{
							for( var j = touches.Count - 1; j >= 0; j-- )
							{
								if( touches[j].phase == TouchPhase.Began )
									touches.RemoveAt( j );
							}
						}
						_sentTouchesBegan = true;
					}
					break;
				}
				case TouchPhase.Moved:
				{
					// limit touches sent to those that are being tracked
					if( !_sentTouchesMoved && populateSubsetOfTouchesBeingTracked( touches ) )
					{
						touchesMoved( _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer );
						_sentTouchesMoved = true;
					}
					break;
				}
				case TouchPhase.Ended:
				case TouchPhase.Canceled:
				{
					// limit touches sent to those that are being tracked
					if( !_sentTouchesEnded && populateSubsetOfTouchesBeingTracked( touches ) )
					{
						touchesEnded( _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer );
						_sentTouchesEnded = true;
					}
					break;
				}
			}
		}
	}
	
	
	internal void reset()
	{
		_state = GKGestureRecognizerState.Possible;
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
	
	
	/// <summary>
	/// return true if a touch was used, false if none were. this is used by any recognizers that should swallow touches if on a higher than 0 zIndex
	/// </summary>
	internal virtual bool touchesBegan( List<GKTouch> touches )
	{
		return false;
	}
	
	
	internal virtual void touchesMoved( List<GKTouch> touches )
	{}
	
	
	internal virtual void touchesEnded( List<GKTouch> touches )
	{}
	
	
	internal abstract void fireRecognizedEvent();
	
	#endregion
	
	
	#region IComparable and ToString implementation
	
	public int CompareTo( GKAbstractGestureRecognizer other )
	{
		return zIndex.CompareTo( other.zIndex );
	}
	
	
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, zIndex: {3}", this.GetType(), state, touchLocation(), zIndex );
	}
	
	#endregion
}
