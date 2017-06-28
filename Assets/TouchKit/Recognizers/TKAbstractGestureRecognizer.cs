using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public enum TKGestureRecognizerState
{
	Possible, // we havent started yet and we are still listening
	Began, // we have started and latched at least one finger
	FailedOrEnded, // no go. failed to recognize or we are done recognizing
	RecognizedAndStillRecognizing, // this will fire the state changed event and allow a gesture to continue to recognize. useful for continuous gestures
	Recognized // successfully recognized and we are not a continuous recognizer
}


public abstract class TKAbstractGestureRecognizer : IComparable<TKAbstractGestureRecognizer>
{
	public bool enabled = true;

	/// <summary>
	/// frame that the touch must be within to be recognized. null means full screen. note that Unity's origin is the bottom left
	/// </summary>
	public TKRect? boundaryFrame = null;

	/// <summary>
	/// zIndex of touch input. 0 by default. if a zIndex of greater than 0 uses a touch in touchesBegan it will not be passed to any other recognizers.
	/// useful if you have some full screen recognizers and you want to overlay a button/control
	/// </summary>
	public uint zIndex = 0;

	private TKGestureRecognizerState _state = TKGestureRecognizerState.Possible;
	public TKGestureRecognizerState state
	{
		get { return _state; }
		set
		{
			_state = value;

			if( _state == TKGestureRecognizerState.Recognized || _state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
				fireRecognizedEvent();

			if( _state == TKGestureRecognizerState.Recognized || _state == TKGestureRecognizerState.FailedOrEnded )
				reset();
		}
	}


	/// <summary>
	/// when true, touchesMoved will be called for ALL touches. By default, only the touches
	/// a recognizer is tracking (from touchesBegan) will be sent.
	/// </summary>
	protected bool alwaysSendTouchesMoved = false;

	/// <summary>
	/// stores all the touches we are currently tracking
	/// </summary>
	protected List<TKTouch> _trackingTouches = new List<TKTouch>();

	/// <summary>
	/// The subset of touches being tracked that is applicable to the current recognizer. This is kept around to avoid allocations at runtime.
	/// </summary>
	private List<TKTouch> _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer = new List<TKTouch>();

	/// <summary>
	/// stores whether we sent any of the phases to the recognizer. This is to avoid sending a phase twice in one frame.
	/// </summary>
	private bool _sentTouchesBegan;
	private bool _sentTouchesMoved;
	private bool _sentTouchesEnded;

	/// <summary>
	/// checks to see if the touch is currently being tracked by the recognizer
	/// </summary>
	protected bool isTrackingTouch( TKTouch t )
	{
		return _trackingTouches.Contains( t );
	}


	/// <summary>
	/// checks to see if any of the touches are currently being tracked by the recognizer
	/// </summary>
	protected bool isTrackingAnyTouch( List<TKTouch> touches )
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
	private bool populateSubsetOfTouchesBeingTracked( List<TKTouch> touches )
	{
		_subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Clear();

		for( int i = 0; i < touches.Count; i++ )
		{
			if( alwaysSendTouchesMoved || isTrackingTouch( touches[i] ) )
				_subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Add( touches[i] );
		}

		return _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Count > 0;
	}


	private bool shouldAttemptToRecognize
	{
		get
		{
			return ( enabled && state != TKGestureRecognizerState.FailedOrEnded && state != TKGestureRecognizerState.Recognized );
		}
	}


	#region Public API

	internal void recognizeTouches( List<TKTouch> touches )
	{
		if( !shouldAttemptToRecognize )
			return;

		// reset our state to avoid sending any phase more than once
		_sentTouchesBegan = _sentTouchesMoved = _sentTouchesEnded = false;

		// we loop backwards because the Began phase could end up removing a touch
		for( var i = touches.Count - 1; i >= 0; i-- )
		{
			var touch = touches[i];
			switch( touch.phase )
			{
				case TouchPhase.Began:
				{
					// only send touches began once and ensure that the touch is in the boundaryFrame if applicable
					if( !_sentTouchesBegan && isTouchWithinBoundaryFrame( touches[i] ) )
					{
						// if touchesBegan returns true and we have a zIndex greater than 0 we remove the touches with a phase of Began
						if( touchesBegan( touches ) && zIndex > 0 )
						{
							// if we remove more than one touch we have to be careful with our loop and make sure to decrement i appropriately
							var removedTouches = 0;
							for( var j = touches.Count - 1; j >= 0; j-- )
							{
								if( touches[j].phase == TouchPhase.Began )
								{
									touches.RemoveAt( j );
									removedTouches++;
								}
							}

							// if we removed more than 1 touch decrement i for each additional touch removed
							if( removedTouches > 0 )
								i -= ( removedTouches - 1 );
						}
						_sentTouchesBegan = true;
					}
					break;
				}
				case TouchPhase.Moved:
				{
					// limit touches sent to those that are being tracked
					if( !_sentTouchesMoved && populateSubsetOfTouchesBeingTracked( touches ) && _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Contains( touch ) )
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
					if( !_sentTouchesEnded && populateSubsetOfTouchesBeingTracked( touches ) && _subsetOfTouchesBeingTrackedApplicableToCurrentRecognizer.Contains( touch ) )
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
		_state = TKGestureRecognizerState.Possible;
		_trackingTouches.Clear();
	}


	internal bool isTouchWithinBoundaryFrame( TKTouch touch )
	{
		return !boundaryFrame.HasValue || boundaryFrame.Value.contains( touch.position );
	}


	/// <summary>
	/// returns the location of the touches. If there are multiple touches this will return the centroid of the location.
	/// </summary>
	public Vector2 touchLocation()
	{
	    var x = 0f;
	    var y = 0f;
	    var k = 0f;

		for( var i = 0; i < _trackingTouches.Count; i++ )
		{
			x += _trackingTouches[i].position.x;
			y += _trackingTouches[i].position.y;
			k++;
		}

	    if( k > 0 )
	        return new Vector2( x / k, y / k );
	    else
	        return Vector2.zero;
	}

	/// <summary>
	/// returns the start location of the touches. If there are multiple touches this will return the centroid of the location.
	/// </summary>
	public Vector2 startTouchLocation()
	{
		var x = 0f;
		var y = 0f;
		var k = 0f;
		
		for( var i = 0; i < _trackingTouches.Count; i++ )
		{
			x += _trackingTouches[i].startPosition.x;
			y += _trackingTouches[i].startPosition.y;
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
	internal virtual bool touchesBegan( List<TKTouch> touches )
	{
		return false;
	}


	internal virtual void touchesMoved( List<TKTouch> touches )
	{}


	internal virtual void touchesEnded( List<TKTouch> touches )
	{}


	internal abstract void fireRecognizedEvent();

	#endregion


	#region IComparable and ToString implementation

	public int CompareTo( TKAbstractGestureRecognizer other )
	{
		return zIndex.CompareTo( other.zIndex );
	}




	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, zIndex: {3}", this.GetType(), state, touchLocation(), zIndex );
	}

	#endregion
}
