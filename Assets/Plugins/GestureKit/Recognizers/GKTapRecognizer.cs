using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class GKTapRecognizer : GKAbstractGestureRecognizer
{
	public event Action<GKTapRecognizer> gestureRecognizedEvent;
	
	public int numberOfTapsRequired = 1;
	// taps that last longer than this duration will be ignored
	public float maxDurationForTapConsideration = 0.5f;
	
	private float _touchBeganTime;
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	
	internal override void touchesBegan( List<GKTouch> touches )
	{
		if( touches[0].tapCount >= numberOfTapsRequired )
		{
			if( state == GKGestureRecognizerState.Possible )
			{
				_trackingTouches.Add( touches[0] );
				_touchBeganTime = Time.time;
				state = GKGestureRecognizerState.Began;
			}
		}
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began )
		{
			// did we move?
			if( touches[0].deltaPosition.sqrMagnitude > 5 )
				state = GKGestureRecognizerState.Failed;
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began && ( Time.time <= _touchBeganTime + maxDurationForTapConsideration ) )
			state = GKGestureRecognizerState.Recognized;
		else
			state = GKGestureRecognizerState.Failed;
	}
	
}
