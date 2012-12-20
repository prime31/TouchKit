using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class GKPanRecognizer : GKAbstractGestureRecognizer
{
	public Vector2 deltaTranslation;
	public int minimumNumberOfTouches = 1;
	public int maximumNumberOfTouches = 2;
	
	private Vector2 _previousLocation;
	
	
	public override void touchesBegan( List<GKTouch> touches )
	{
		for( int i = 0; i < touches.Count; i++ )
		{
			// only add touches in the Began phase
			if( touches[i].phase == TouchPhase.Began )
			{
				_trackingTouches.Add( touches[i] );
				
				if( _trackingTouches.Count == maximumNumberOfTouches )
					break;
			}

			if( _trackingTouches.Count >= minimumNumberOfTouches )
			{
				_previousLocation = touchLocation();
				state = GKGestureRecognizerState.Began;
			}
		}
	}
	
	
	public override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began || state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			var currentLocation = touchLocation();
			deltaTranslation = currentLocation - _previousLocation;
			_previousLocation = currentLocation;
			state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
	}
	
	
	public override void touchesEnded( List<GKTouch> touches )
	{
		// remove any completed touches
		for( int i = 0; i < touches.Count; i++ )
		{
			if( touches[i].phase == TouchPhase.Ended )
				_trackingTouches.Remove( touches[i] );
		}
		
		// if we still have a touch left continue. no touches means its time to reset
		if( _trackingTouches.Count == 1 )
		{
			state = GKGestureRecognizerState.Began;
		}
		else
		{
			state = GKGestureRecognizerState.Failed;
		}
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, deltaTranslation: {3}", this.GetType(), state, touchLocation(), deltaTranslation );
	}

}
