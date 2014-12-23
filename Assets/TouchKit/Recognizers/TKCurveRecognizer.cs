using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class TKCurveRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKCurveRecognizer> gestureRecognizedEvent;
	public event Action<TKCurveRecognizer> gestureCompleteEvent;

	public float reportRotationStep = 20f; //how much rotation (degrees) is needed for the recognized event to fire
	public float squareDistance = 10f; //squared distance of touhes being evaluated
	public float maxSharpnes = 50f; //maximum angle (degrees) a touch is allowed to change direction of movement

	public int minimumNumberOfTouches = 1;
	public int maximumNumberOfTouches = 2;

	//should be read only
	public float deltaRotation = 0f; //rotation since last reported

	private Vector2 _previousLocation;
	private Vector2 _deltaTranslation;//direction vector from previous to current location (current location being the last location far enough from the previous one)
	private Vector2 _previousDeltaTranslation;//last direction


	internal override void fireRecognizedEvent()
	{
		if( gestureRecognizedEvent != null )
			gestureRecognizedEvent( this );
	}


	internal override bool touchesBegan( List<TKTouch> touches )
	{
		// add new or additional touches to gesture (allows for two or more touches to be added or removed without ending the pan gesture)
		if( state == TKGestureRecognizerState.Possible ||
		   ( ( state == TKGestureRecognizerState.Began || state == TKGestureRecognizerState.RecognizedAndStillRecognizing ) && _trackingTouches.Count < maximumNumberOfTouches ) )
		{
			for( int i = 0; i < touches.Count; i++ )
			{
				// only add touches in the Began phase
				if( touches[i].phase == TouchPhase.Began )
				{
					_trackingTouches.Add( touches[i] );

					if( _trackingTouches.Count == maximumNumberOfTouches )
						break;
				}
			} // end for

			if( _trackingTouches.Count >= minimumNumberOfTouches )
			{
				_previousLocation = touchLocation();
				if( state != TKGestureRecognizerState.RecognizedAndStillRecognizing ){
					//initialize values, but stay in Possible state. use Began only when there's enough data
					state = TKGestureRecognizerState.Possible;
					deltaRotation = 0f;
					_deltaTranslation = Vector2.zero;
					_previousDeltaTranslation = Vector2.zero;
				}
			}
		}

		return false;
	}


	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.Possible ){
			//previous delta translation hasn't been set yet, need another itteration

			Vector2 currentLocation = touchLocation();
			Vector2 delta = currentLocation - _previousLocation;
			_deltaTranslation = delta;
			_previousLocation = currentLocation;
			_previousDeltaTranslation = _deltaTranslation;

			state = TKGestureRecognizerState.Began; //got enough data to begin recognizing in next move
		}
		else if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing || state == TKGestureRecognizerState.Began )
		{
			var currentLocation = touchLocation();

			var delta = currentLocation - _previousLocation;
			if( delta.sqrMagnitude >= 10f )
			{
				var a = Vector2.Angle( _previousDeltaTranslation, delta );

				if( a > maxSharpnes )
				{
					Debug.Log( "Curve is to sharp: " + a + "  max sharpnes set to:" + maxSharpnes );
					state = TKGestureRecognizerState.FailedOrEnded; //calls reset
				}
				else
				{
					_deltaTranslation = delta;

					float crossZ = Vector3.Cross( _previousDeltaTranslation, delta ).z;//  / (_previousDeltaTranslation.magnitude * delta.magnitude);
					if( crossZ > 0f )
						deltaRotation -= a;
					else
						deltaRotation += a;

					if( Mathf.Abs ( deltaRotation) >= reportRotationStep )
					{
						//if total rotation changed enough: recognize and reset total rotation
						state = TKGestureRecognizerState.RecognizedAndStillRecognizing; // fires recognized event
						deltaRotation = 0f;//reset total rotation
					}
					_previousLocation = currentLocation;
					_previousDeltaTranslation = _deltaTranslation;

				}
			}

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

		// if we still have a touch left continue. no touches means its time to reset
		if( _trackingTouches.Count >= minimumNumberOfTouches )
		{
			_previousLocation = touchLocation();
			state = TKGestureRecognizerState.RecognizedAndStillRecognizing; //fires recognized event
		}
		else
		{
			// if we had previously been recognizing fire our complete event
			if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
			{
				if( gestureCompleteEvent != null )
					gestureCompleteEvent( this );
			}
			state = TKGestureRecognizerState.FailedOrEnded; //calls reset
		}
	}


	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, trans: {2}, lastTrans: {3}, totalRot: {4}", this.GetType(), state, _deltaTranslation, _previousDeltaTranslation, deltaRotation );
	}

}
