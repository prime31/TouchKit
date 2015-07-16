using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



/// <summary>
///
/// </summary>
public class TKRotationRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKRotationRecognizer> gestureRecognizedEvent;
	public event Action<TKRotationRecognizer> gestureCompleteEvent;
	
	public float deltaRotation = 0;
	public float minimumRotationToRecognize = 0;
	
	protected float _previousRotation = 0;
	protected float _firstRotation = 0; // first ever rotation angle when 2 fingers hit the screen
	protected float _initialRotation = 0; // rotation angle when gesture is officially recognized

	public float accumulatedRotation
	{
		get
		{
			if( _trackingTouches.Count == 2 )
				return Mathf.DeltaAngle( angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position ), _initialRotation );
			return 0;
		}
	}

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
	
	
	internal override bool touchesBegan( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Possible )
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
				if( minimumRotationToRecognize == 0 )
				{
					deltaRotation = 0;
					_previousRotation = angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position );
					state = TKGestureRecognizerState.Began;
				}
				else
				{
					// gesture cannot be recognized until the the rotation angle exceeds the minimum threshold
					_firstRotation = angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position );
				}
			}
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<TKTouch> touches )
	{
		// if the gesture has passed the minimum delta rotation threshold, begin officially recognizing the gesture
		if( state == TKGestureRecognizerState.Possible && _trackingTouches.Count == 2 )
		{
			var cr = angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position );
			var delta = Mathf.Abs( Mathf.DeltaAngle( cr, _firstRotation ) );

			if( delta > minimumRotationToRecognize )
			{
				_initialRotation = cr; // the starting rotation when the gesture was first recognized

				deltaRotation = 0;
				_previousRotation = angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position );
				state = TKGestureRecognizerState.Began;
			}
		}

		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing || state == TKGestureRecognizerState.Began )
		{
			var currentRotation = angleBetweenPoints( _trackingTouches[0].position, _trackingTouches[1].position );
			deltaRotation = Mathf.DeltaAngle( currentRotation, _previousRotation );
			_previousRotation = currentRotation;
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
		
		// if we had previously been recognizing fire our complete event
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
		{
			if( gestureCompleteEvent != null )
				gestureCompleteEvent( this );
		}
		
		// if we still have a touch left continue to wait for another. no touches means its time to reset
		if( _trackingTouches.Count == 1 )
		{
			state = TKGestureRecognizerState.Possible;
			deltaRotation = 0;
		}
		else
		{
			state = TKGestureRecognizerState.FailedOrEnded;
			_initialRotation = 0;
		}
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, rotation: {3}", this.GetType(), state, touchLocation(), deltaRotation );
	}

}
