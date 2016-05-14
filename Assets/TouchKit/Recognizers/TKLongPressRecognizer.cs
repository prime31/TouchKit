using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// detects a long press. The gesture is considered recognized when a touch has been down for minimumPressDuration and if it has moved less than allowableMovement
/// </summary>
public class TKLongPressRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKLongPressRecognizer> gestureRecognizedEvent;
	public event Action<TKLongPressRecognizer> gestureCompleteEvent; // fired when after a successful long press the finger is lifted

	public float minimumPressDuration = 0.5f;
	public int requiredTouchesCount = -1;
	public float allowableMovementCm = 1f;

	Vector2 _beginLocation;
	bool _waiting;



	public TKLongPressRecognizer()
	{}


	public TKLongPressRecognizer( float minimumPressDuration, float allowableMovement, int requiredTouchesCount )
	{
		this.minimumPressDuration = minimumPressDuration;
		this.allowableMovementCm = allowableMovement;
		this.requiredTouchesCount = requiredTouchesCount;
	}


	IEnumerator beginGesture()
	{
		var endTime = Time.time + minimumPressDuration;

		// wait for our time to elapse or to be cancelled
		while( _waiting && Time.time < endTime )
			yield return null;

		// if our time elapsed it means we were not cancelled
		if( Time.time >= endTime )
		{
			if( state == TKGestureRecognizerState.Began )
				state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
		}

		_waiting = false;
	}


	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}


	internal override bool touchesBegan( List<TKTouch> touches )
	{
		if (!_waiting && state == TKGestureRecognizerState.Possible && (requiredTouchesCount == -1 || touches.Count == requiredTouchesCount))
		{
			_beginLocation = touches[0].position;
			_waiting = true;

			TouchKit.instance.StartCoroutine( beginGesture() );
			_trackingTouches.Add(touches[0]);
			state = TKGestureRecognizerState.Began;
		}
		else if (requiredTouchesCount != -1)
		{
			_waiting = false;
		}

		return false;
	}


	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Began || state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			// did we move too far?
			var moveDistance = Vector2.Distance(touches[0].position, _beginLocation) / TouchKit.instance.ScreenPixelsPerCm;
			if (moveDistance > allowableMovementCm)
			{
				// fire the complete event if we had previously recognized a long press
				if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing && gestureCompleteEvent != null )
					gestureCompleteEvent( this );

				state = TKGestureRecognizerState.FailedOrEnded;
				_waiting = false;
			}
		}
	}


	internal override void touchesEnded( List<TKTouch> touches )
	{
		// fire the complete event if we had previously recognized a long press
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing && gestureCompleteEvent != null )
			gestureCompleteEvent( this );

		state = TKGestureRecognizerState.FailedOrEnded;
		_waiting = false;
	}

}
