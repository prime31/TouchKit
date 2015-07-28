using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class TKPinchRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKPinchRecognizer> gestureRecognizedEvent;
	public event Action<TKPinchRecognizer> gestureCompleteEvent;

	/// <summary>
	/// the minimum amount of distance the two fingers must move apart before the gesture is recognized
	/// </summary>
	public float minimumScaleDistanceToRecognize = 0;
	public float deltaScale = 0;

	private float _intialDistance = 0; // represents the distance between two fingers when gesture was first officially recognized
	private float _firstDistance = 0; // first ever distance when two fingers hit the screen (not yet recognized)
	private float _previousDistance = 0;

	/// <summary>
	/// calculated, read-only property. Represents the scale accumulated since the gesture was initially recognized
	/// </summary>
	/// <value>The accumulated scale.</value>
	public float accumulatedScale
	{
		get
		{
			var currentDistance = distanceBetweenTrackedTouches();
			return currentDistance / _intialDistance;
		}
	}


	private float distanceBetweenTrackedTouches()
	{	
		// prevent NaN when the distance between the touches is zero -- only happens in editor
		var distance = Vector2.Distance( _trackingTouches[0].position, _trackingTouches[1].position );
		return ( Mathf.Max(0.0001f, distance) / TouchKit.instance.ScreenPixelsPerCm );
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
			// we need to have two touches to work with so we dont set state to Begin until then
			// latch the touches
			for( int i = 0; i < touches.Count; i++ )
			{
				// only add touches in the Began phase
				if( touches[i].phase == TouchPhase.Began )
				{
					_trackingTouches.Add( touches[i] );
					
					if( _trackingTouches.Count == 2 )
						break;
				}
			} // end for
			
			if( _trackingTouches.Count == 2 )
			{
				// gesture cannot be recognized until the two touches exceed the minimum scale threshold
				_firstDistance = distanceBetweenTrackedTouches();

				// if( minimumScaleDistanceToRecognize == 0 )
				// {
				// 	_intialDistance = _firstDistance;
				// 	state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
				// }
			}
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<TKTouch> touches )
	{
		// if the two fingers move far apart to exceed the minimum threshold, begin officially recognizing the gesture
		if( _trackingTouches.Count == 2 )
		{
			if ( state == TKGestureRecognizerState.Possible )
			{
				if( Mathf.Abs( distanceBetweenTrackedTouches() - _firstDistance ) >= minimumScaleDistanceToRecognize )
				{
					deltaScale = 0;
					_intialDistance = distanceBetweenTrackedTouches();
					_previousDistance = _intialDistance;
					state = TKGestureRecognizerState.Began;
				}
			}
			else if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing || state == TKGestureRecognizerState.Began )
			{
				var currentDistance = distanceBetweenTrackedTouches();
				deltaScale = ( currentDistance - _previousDistance ) / _intialDistance;
				_previousDistance = currentDistance;
				state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
			}
		}
	}
	
	
	internal override void touchesEnded( List<TKTouch> touches )
	{
		// remove any completed touches
		for( int i = 0; i < touches.Count; i++ )
		{
			if( touches[i].phase == TouchPhase.Ended )
				_trackingTouches.Remove( touches[i] );
		}
		
		// if we had previously been recognizing fire our complete event
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			if( gestureCompleteEvent != null )
				gestureCompleteEvent( this );
		}
		
		// if we still have a touch left continue to wait for another. no touches means its time to reset
		if( _trackingTouches.Count == 1 )
		{
			state = TKGestureRecognizerState.Possible;
			deltaScale = 0; // I don't know why this would be 1 instead of 0
		}
		else
		{
			state = TKGestureRecognizerState.FailedOrEnded;
		}
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, deltaScale: {2}", this.GetType(), state, deltaScale );
	}

}
