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
	private float _maxDurationForTapConsideration = 0.5f;
	private float _maxDeltaMovementForTapConsideration = 5f;

	private float _touchBeganTime;



	public TKTapRecognizer() : this( 0.5f, 5f )
	{}


	public TKTapRecognizer( float maxDurationForTapConsideration, float maxDeltaMovementForTapConsideration )
	{
		_maxDurationForTapConsideration = maxDurationForTapConsideration;
		_maxDeltaMovementForTapConsideration = maxDeltaMovementForTapConsideration * TouchKit.instance.runtimeDistanceModifier;
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
				if( touches[i].deltaPosition.sqrMagnitude > _maxDeltaMovementForTapConsideration )
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
			state = TKGestureRecognizerState.Recognized;
		else
			state = TKGestureRecognizerState.FailedOrEnded;
	}

}
