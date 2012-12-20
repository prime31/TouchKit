using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GKTapRecognizer : GKAbstractGestureRecognizer
{
	public int numberOfTapsRequired = 1;
	// taps that last longer than this duration will be ignored
	public float maxDurationForTapConsideration = 0.5f;
	
	private float _touchBeganTime;
	
	
	public override void touchesBegan( List<GKTouch> touches )
	{
		if( touches[0].tapCount >= numberOfTapsRequired )
		{
			if( state == GKGestureRecognizerState.Possible )
			{
				state = GKGestureRecognizerState.Began;
				_trackingTouches.Add( touches[0] );
				_touchBeganTime = Time.time;
			}
		}
	}
	
	
	public override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began )
		{
			// did we move?
			if( touches[0].deltaPosition.sqrMagnitude > 5 )
				state = GKGestureRecognizerState.Failed;
		}
	}
	
	
	public override void touchesEnded( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began && ( Time.time <= _touchBeganTime + maxDurationForTapConsideration ) )
			state = GKGestureRecognizerState.Recognized;
		else
			state = GKGestureRecognizerState.Failed;
	}
	
}
