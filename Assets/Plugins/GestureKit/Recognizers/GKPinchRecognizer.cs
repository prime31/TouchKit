using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class GKPinchRecognizer : GKAbstractGestureRecognizer
{
	public float scale = 1;
	private float _intialDistance;
	
	
	private float distanceBetweenTrackedTouches()
	{
		return Vector2.Distance( _trackingTouches[0].position, _trackingTouches[1].position );
	}
	
	
	public override void touchesBegan( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Possible )
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
			}
			
			if( _trackingTouches.Count == 2 )
			{
				scale = 1;
				_intialDistance = distanceBetweenTrackedTouches();
				state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
			}
		}
	}
	
	
	public override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			//Debug.Log( "init: " + _intialDistance + ", curr: " + distanceBetweenTrackedTouches() + ", 1: " + _trackingTouches[0].fingerId + ", 2: " + _trackingTouches[1].fingerId );
			scale = distanceBetweenTrackedTouches() / _intialDistance;
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
		
		// if we still have a touch left continue to wait for another. not touches means its time to reset
		if( _trackingTouches.Count == 1 )
		{
			state = GKGestureRecognizerState.Possible;
			scale = 1;
		}
		else
		{
			state = GKGestureRecognizerState.Failed;
		}
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, scale: {3}", this.GetType(), state, touchLocation(), scale );
	}
}
