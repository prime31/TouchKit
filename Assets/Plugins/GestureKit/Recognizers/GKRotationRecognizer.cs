using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// 
/// </summary>
public class GKRotationRecognizer : GKAbstractGestureRecognizer
{
	public event Action<GKRotationRecognizer> gestureRecognizedEvent;
	
	public float deltaRotation = 0;
	public float minimumDeltaRotationToRecognize = 0;
	
	private float _previousRotation;
	
	
	private float distanceBetweenTrackedTouches()
	{
		return Vector2.Distance( _trackingTouches[0].position, _trackingTouches[1].position );
	}
	
	
	private float angleBetweenTouches()
	{
		var fromLine = _trackingTouches[1].position - _trackingTouches[0].position;
		var toLine = new Vector2( 1, 0 );
 
		var angle = Vector2.Angle( fromLine, toLine );
		var cross = Vector3.Cross( fromLine, toLine );
 
		// did we wrap around?
		if( cross.z > 0 )
			angle = 360f - angle;
 
		return angle;
	}
	
	
	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}
	
	
	internal override void touchesBegan( List<GKTouch> touches )
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
				deltaRotation = 0;
				_previousRotation = angleBetweenTouches();
				state = GKGestureRecognizerState.Began;
			}
		}
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing || state == GKGestureRecognizerState.Began )
		{
			var currentRotation = angleBetweenTouches();
			deltaRotation = Mathf.DeltaAngle( currentRotation, _previousRotation );
			_previousRotation = currentRotation;
			state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		// remove any completed touches
		for( int i = 0; i < touches.Count; i++ )
		{
			if( touches[i].phase == TouchPhase.Ended )
				_trackingTouches.Remove( touches[i] );
		}
		
		// if we still have a touch left continue to wait for another. no touches means its time to reset
		if( _trackingTouches.Count == 1 )
		{
			state = GKGestureRecognizerState.Possible;
			deltaRotation = 0;
		}
		else
		{
			state = GKGestureRecognizerState.Failed;
		}
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, rotation: {3}", this.GetType(), state, touchLocation(), deltaRotation );
	}

}
