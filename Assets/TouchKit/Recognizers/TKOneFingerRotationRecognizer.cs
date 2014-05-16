using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// detects a rotation around an object with a single finger. The target objects position must be provided in screen coordinates.
/// </summary>
public class TKOneFingerRotationRecognizer : TKRotationRecognizer
{
	public new event Action<TKOneFingerRotationRecognizer> gestureRecognizedEvent;
	public new event Action<TKOneFingerRotationRecognizer> gestureCompleteEvent;
	
	/// <summary>
	/// this should be the center point in screen coordinates of the object that is being rotated
	/// </summary>
	public Vector2 targetPosition;
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	
	internal override bool touchesBegan( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Possible )
		{
			_trackingTouches.Add( touches[0] );
			
			deltaRotation = 0;
			_previousRotation = angleBetweenPoints( targetPosition, _trackingTouches[0].position );
			state = TKGestureRecognizerState.Began;
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing || state == TKGestureRecognizerState.Began )
		{
			var currentRotation = angleBetweenPoints( targetPosition, _trackingTouches[0].position );
			deltaRotation = Mathf.DeltaAngle( currentRotation, _previousRotation );
			_previousRotation = currentRotation;
			state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
	}
	
	
	internal override void touchesEnded( List<TKTouch> touches )
	{
		// if we had previously been recognizing fire our complete event
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			if( gestureCompleteEvent != null )
				gestureCompleteEvent( this );
		}
		
		state = TKGestureRecognizerState.FailedOrEnded;
	}

}
