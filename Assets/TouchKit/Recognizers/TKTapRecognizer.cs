using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class TKTapRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKTapRecognizer> gestureRecognizedEvent;

	public int numberOfTapsRequired = 1;
	public int numberOfTouchesRequired = 1;

	// taps that last longer than this duration will be ignored
	float _maxDurationForTapConsideration = 0.5f;

	float _maxDeltaMovementForTapConsideration = 1f;
	float _touchBeganTime;
	int _preformedTapsCount = 0;


	public TKTapRecognizer() : this( 0.5f, 1f )
	{}


	public TKTapRecognizer( float maxDurationForTapConsideration, float maxDeltaMovementForTapConsiderationCm )
	{
		_maxDurationForTapConsideration = maxDurationForTapConsideration;
		_maxDeltaMovementForTapConsideration = maxDeltaMovementForTapConsiderationCm;
	}


	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}


	internal override bool touchesBegan( List<TKTouch> touches )
	{
		if( Time.time > _touchBeganTime + _maxDurationForTapConsideration && _preformedTapsCount !=  0 && _preformedTapsCount < numberOfTapsRequired )
			state = TKGestureRecognizerState.FailedOrEnded;

		if( state == TKGestureRecognizerState.Possible )
		{
			for( int i = 0; i < touches.Count; i++ )
			{
				// only add touches in the Began phase
				if( touches[i].phase == TouchPhase.Began )
				{
					_trackingTouches.Add( touches[i] );

					if( _trackingTouches.Count == numberOfTouchesRequired )
						break;
				}
			} // end for

			if( _trackingTouches.Count == numberOfTouchesRequired )
			{
				_touchBeganTime = Time.time;
				_preformedTapsCount = 0;
				state = TKGestureRecognizerState.Began;

				return true;
			}
		}

		return false;
	}


	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Began )
		{
			// did we move?
			for( var i = 0; i < touches.Count; i++ )
			{
				if (
					((Math.Abs(touches[i].position.x - touches[i].startPosition.x) / TouchKit.instance.ScreenPixelsPerCm) > _maxDeltaMovementForTapConsideration) ||
					((Math.Abs(touches[i].position.y - touches[i].startPosition.y) / TouchKit.instance.ScreenPixelsPerCm) > _maxDeltaMovementForTapConsideration)
				)
				{
					state = TKGestureRecognizerState.FailedOrEnded;
					break;
				}
			}
		}
	}


	internal override void touchesEnded( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Began && ( Time.time <= _touchBeganTime + _maxDurationForTapConsideration ) )
		{
			++_preformedTapsCount;
			if( _preformedTapsCount == numberOfTapsRequired )
				state = TKGestureRecognizerState.Recognized;
		}
		else
		{
			state = TKGestureRecognizerState.FailedOrEnded;
		}
	}

}
