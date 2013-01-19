using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// detects a long press. The gesture is considered recognized when a touch has been down for minimumPressDuration and if it has moved less than allowableMovement
/// </summary>
public class GKLongPressRecognizer : GKAbstractGestureRecognizer
{
	public event Action<GKLongPressRecognizer> gestureRecognizedEvent;
	public event Action<GKLongPressRecognizer> gestureCompleteEvent; // fired when after a successful long press the finger is lifted
	
	public float minimumPressDuration = 0.5f;
	public float allowableMovement = 10f;
	
	private Vector2 _beginLocation;
	private bool _waiting;
	
	
	
	public GKLongPressRecognizer(){}
	
	
	public GKLongPressRecognizer( float minimumPressDuration, float allowableMovement )
	{
		this.minimumPressDuration = minimumPressDuration;
		this.allowableMovement = allowableMovement;
	}
	
	
	private IEnumerator beginGesture()
	{
		var endTime = Time.time + minimumPressDuration;
		
		// wait for our time to elapse or to be cancelled
		while( _waiting && Time.time < endTime )
			yield return null;
		
		// if our time elapsed it means we were not cancelled
		if( Time.time >= endTime )
		{
			if( state == GKGestureRecognizerState.Began )
				state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
		
		_waiting = false;
	}
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	
	internal override bool touchesBegan( List<GKTouch> touches )
	{
		if( !_waiting && state == GKGestureRecognizerState.Possible )
		{
			_beginLocation = touches[0].position;
			_waiting = true;
			GestureKit.instance.StartCoroutine( beginGesture() );
			_trackingTouches.Add( touches[0] );
			state = GKGestureRecognizerState.Began;
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began || state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			// did we move too far?
			var moveDistance = Vector2.Distance( touches[0].position, _beginLocation );
			if( moveDistance > allowableMovement )
			{
				// fire the complete event if we had previously recognized a long press
				if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing && gestureCompleteEvent != null )
					gestureCompleteEvent( this );
						
				state = GKGestureRecognizerState.Failed;
				_waiting = false;
			}
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		// fire the complete event if we had previously recognized a long press
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing && gestureCompleteEvent != null )
			gestureCompleteEvent( this );
		
		state = GKGestureRecognizerState.Failed;	
		_waiting = false;
	}

}
