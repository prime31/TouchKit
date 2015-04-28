using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class TKPinchRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKPinchRecognizer> gestureRecognizedEvent;
	public event Action<TKPinchRecognizer> gestureCompleteEvent;
	
	public float deltaScale = 0;
	private float _intialDistance;
	private float _previousDistance;
	
	
	private float distanceBetweenTrackedTouches()
	{
		return (Vector2.Distance(_trackingTouches[0].position, _trackingTouches[1].position) / TouchKit.instance.ScreenPixelsPerCm);
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
				deltaScale = 0;
				_intialDistance = distanceBetweenTrackedTouches();
				_previousDistance = _intialDistance;
				state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
			}
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			var currentDistance = distanceBetweenTrackedTouches();
			deltaScale = ( currentDistance - _previousDistance ) / _intialDistance;
			_previousDistance = currentDistance;
			state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
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
			deltaScale = 1;
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
