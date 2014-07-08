using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class TKPanRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKPanRecognizer> gestureRecognizedEvent;
	public event Action<TKPanRecognizer> gestureCompleteEvent;
	
	public Vector2 deltaTranslation;
	public int minimumNumberOfTouches = 1;
	public int maximumNumberOfTouches = 2;
	
	private Vector2 _previousLocation;
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	

	internal override bool touchesBegan( List<TKTouch> touches )
	{
		// add new or additional touches to gesture (allows for two or more touches to be added or removed without ending the pan gesture)
		if( state == TKGestureRecognizerState.Possible || ( ( state == TKGestureRecognizerState.Began || state == TKGestureRecognizerState.RecognizedAndStillRecognizing ) && _trackingTouches.Count < maximumNumberOfTouches ) )
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
			} // end for
			
			if( _trackingTouches.Count >= minimumNumberOfTouches )
			{
				_previousLocation = touchLocation();
				if( state != TKGestureRecognizerState.RecognizedAndStillRecognizing )
				{
					state = TKGestureRecognizerState.Began;
				}
			}
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing || state == TKGestureRecognizerState.Began )
		{
			var currentLocation = touchLocation();
			deltaTranslation = currentLocation - _previousLocation;
			_previousLocation = currentLocation;
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

		// if we still have a touch left continue. no touches means its time to reset
		if( _trackingTouches.Count >= minimumNumberOfTouches )
		{
			_previousLocation = touchLocation();
			state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
		else
		{
			// if we had previously been recognizing fire our complete event
			if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
			{
				if( gestureCompleteEvent != null ) {
					gestureCompleteEvent( this );
				}
			}

			state = TKGestureRecognizerState.FailedOrEnded;
		}
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, deltaTranslation: {3}", this.GetType(), state, touchLocation(), deltaTranslation );
	}

}
