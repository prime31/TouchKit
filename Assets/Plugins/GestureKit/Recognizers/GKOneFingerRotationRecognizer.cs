using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;




public class GKOneFingerRotationRecognizer : GKRotationRecognizer
{
	public new event Action<GKOneFingerRotationRecognizer> gestureRecognizedEvent;
	public new event Action<GKOneFingerRotationRecognizer> gestureCompleteEvent;
	
	public Vector2 targetPosition;
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	
	internal override void touchesBegan( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Possible )
		{
			_trackingTouches.Add( touches[0] );
			
			deltaRotation = 0;
			_previousRotation = angleBetweenPoints( targetPosition, _trackingTouches[0].position );
			state = GKGestureRecognizerState.Began;
		}
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing || state == GKGestureRecognizerState.Began )
		{
			var currentRotation = angleBetweenPoints( targetPosition, _trackingTouches[0].position );
			deltaRotation = Mathf.DeltaAngle( currentRotation, _previousRotation );
			_previousRotation = currentRotation;
			state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		// if we had previously been recognizing fire our complete event
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			if( gestureCompleteEvent != null )
				gestureCompleteEvent( this );
		}
		
		state = GKGestureRecognizerState.Failed;
	}

}
