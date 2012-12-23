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
	public event Action<GKRotationRecognizer> gestureCompleteEvent;
	
	public float deltaRotation = 0;
	public float minimumDeltaRotationToRecognize = 0;
	
	protected float _previousRotation;
	
	
	/// <summary>
	/// this is public due to its usefulness elsewhere. it should probably move to a helper class.
	/// </summary>
	public static float angleBetweenPoints( Vector2 position1, Vector2 position2 )
	{
		var fromLine = position2 - position1;
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
	
	
	internal override bool touchesBegan( List<GKTouch> touches )
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
			} // end for
			
			if( _trackingTouches.Count == 2 )
			{
				deltaRotation = 0;
				_previousRotation = angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position );
				state = GKGestureRecognizerState.Began;
			}
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing || state == GKGestureRecognizerState.Began )
		{
			var currentRotation = angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position );
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
		
		// if we had previously been recognizing fire our complete event
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			if( gestureCompleteEvent != null )
				gestureCompleteEvent( this );
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
