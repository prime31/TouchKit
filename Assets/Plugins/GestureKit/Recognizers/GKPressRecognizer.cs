using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// detects a press and its release. The gesture is considered recognized when a touch has been down for minimumPressDuration and if it has moved less than allowableMovement.
/// The gesture is ended (gestureEndedEvent) when the finger is released or moves further than allowableMovement after initial recognition
/// </summary>
public class GKPressRecognizer : GKAbstractGestureRecognizer
{
	public event Action<GKPressRecognizer> gestureRecognizedEvent;
	public event Action<GKPressRecognizer> gestureCompleteEvent;
	
	public float minimumPressDuration = 0;
	public float allowableMovement = 10f;
	
	private Vector2 _beginLocation;
	private bool _waiting;
	
	
	/* State Definitions
	 * Began: a touch began and we will start tracking it
	 * RecognizedAndStillRecognizing: we have started the recognition and the gestureRecognizedEvent will fire
	 * Recognized: we started the recognition and the finger has either lifted or moved too far
	 */
	
	
	public GKPressRecognizer(){}
	
	
	public GKPressRecognizer( float minimumPressDuration, float allowableMovement )
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
	
	
	internal override void touchesBegan( List<GKTouch> touches )
	{
		if( !_waiting && state == GKGestureRecognizerState.Possible && touches[0].phase == TouchPhase.Began )
		{
			_beginLocation = touches[0].position;
			_trackingTouches.Add( touches[0] );
			
			// if minimumPressDuration is 0 we dont need the coroutine and we can count this as RecognizedAndStillRecognizing straight away
			if( minimumPressDuration == 0 )
			{
				state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
			}
			else
			{
				_waiting = true;
				GestureKit.instance.StartCoroutine( beginGesture() );
				state = GKGestureRecognizerState.Began;
			}
		}
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		// if we began or are still recognizing see if we have moved too far
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing || state == GKGestureRecognizerState.Began )
		{
			// did we move too far?
			var moveDistance = Vector2.Distance( touches[0].position, _beginLocation );
			if( moveDistance > allowableMovement )
			{
				// if we had previously been recognizing fire our complete event before failing
				if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
				{
					if( gestureCompleteEvent != null )
						gestureCompleteEvent( this );
				}

				state = GKGestureRecognizerState.Failed;
				_waiting = false;
			}
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		// if we had previously been recognizing fire our complete event before failing
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			if( gestureCompleteEvent != null )
				gestureCompleteEvent( this );
		}
		
		state = GKGestureRecognizerState.Failed;
		_waiting = false;
	}

}
