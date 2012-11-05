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
				state = GestureRecognizerState.Began;
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
		if( state == GestureRecognizerState.Began || state == GestureRecognizerState.Changed || state == GestureRecognizerState.Possible )
		{
			// did we move to far?
			var moveDistance = Vector2.Distance( touches[0].position, _beginLocation );
			if( moveDistance < allowableMovement )
			{
				// if we already began then we change. if we are still possible then we remain possible
				if( state == GestureRecognizerState.Began )
					state = GestureRecognizerState.Changed;
			}
			else
			{
				if( state == GestureRecognizerState.Began )
					state = GestureRecognizerState.Cancelled;
				else
					state = GestureRecognizerState.Failed;
				_waiting = false;
			}
		}
	}
	
	
	public override void touchesEnded( List<Touch> touches )
	{
		if( state == GestureRecognizerState.Began || state == GestureRecognizerState.Changed )
			state = GestureRecognizerState.Ended;
		
		_waiting = false;
	}

}
