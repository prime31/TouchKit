using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class GKTapRecognizer : GKAbstractGestureRecognizer
{
	public event Action<GKTapRecognizer> gestureRecognizedEvent;
	
	public int numberOfTapsRequired = 1;
	public int numberOfTouchesRequired = 1;
	// taps that last longer than this duration will be ignored
	public float maxDurationForTapConsideration = 0.5f;
	
	private float _touchBeganTime;
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	
	internal override bool touchesBegan( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Possible )
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
				state = GKGestureRecognizerState.Began;
				
				return true;
			}
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began )
		{
			// did we move?
			for( var i = 0; i < touches.Count; i++ )
			{
				if( touches[i].deltaPosition.sqrMagnitude > 5 )
				{
					state = GKGestureRecognizerState.Failed;
					break;
				}
			}
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
