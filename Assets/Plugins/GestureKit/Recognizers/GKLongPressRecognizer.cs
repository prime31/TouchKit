using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;



public class GKLongPressRecognizer : AbstractGestureRecognizer
{
	public float minimumPressDuration = 0.5f;
	public float allowableMovement = 10f;
	public int numberOfTapsRequired = 1;
	public int numberOfTouchesRequired = 1;
	
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
			if( state == GestureRecognizerState.Possible )
				state = GestureRecognizerState.Recognized;
		}
	}
	
	
	public override void touchesBegan( List<Touch> touches )
	{
		if( touches.Count >= numberOfTouchesRequired )
		{
			if( touches[0].tapCount >= numberOfTapsRequired )
			{
				if( !_waiting && state == GestureRecognizerState.Possible )
				{
					_beginLocation = touches[0].position;
					_waiting = true;
					GestureKit.instance.StartCoroutine( beginGesture() );
				}
			}
		}
	}
	
	
	public override void touchesMoved( List<Touch> touches )
	{
		if( state == GestureRecognizerState.Began || state == GestureRecognizerState.Possible )
		{
			// did we move too far?
			var moveDistance = Vector2.Distance( touches[0].position, _beginLocation );
			if( moveDistance > allowableMovement )
			{
				state = GestureRecognizerState.Failed;
				_waiting = false;
			}
		}
	}
	
	
	public override void touchesEnded( List<Touch> touches )
	{
		state = GestureRecognizerState.Failed;	
		_waiting = false;
	}

}
