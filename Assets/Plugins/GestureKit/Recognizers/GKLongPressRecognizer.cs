using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;



public class GKLongPressRecognizer : GKAbstractGestureRecognizer
{
	public event Action<GKLongPressRecognizer> gestureRecognizedEvent;
	
	public float minimumPressDuration = 0.5f;
	public float allowableMovement = 10f;
	
	private Vector2 _beginLocation;
	private bool _waiting;
	
	
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
				state = GKGestureRecognizerState.Recognized;
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
		if( !_waiting && state == GKGestureRecognizerState.Possible )
		{
			_beginLocation = touches[0].position;
			_waiting = true;
			GestureKit.instance.StartCoroutine( beginGesture() );
			_trackingTouches.Add( touches[0] );
			state = GKGestureRecognizerState.Began;
		}
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began )
		{
			// did we move too far?
			var moveDistance = Vector2.Distance( touches[0].position, _beginLocation );
			if( moveDistance > allowableMovement )
			{
				state = GKGestureRecognizerState.Failed;
				_waiting = false;
			}
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		state = GKGestureRecognizerState.Failed;	
		_waiting = false;
	}

}
