using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class TKTouchPadRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKTouchPadRecognizer> gestureRecognizedEvent;
	public event Action<TKTouchPadRecognizer> gestureCompleteEvent;

	public AnimationCurve inputCurve = AnimationCurve.Linear( 0.0f, 0.0f, 1.0f, 1.0f );
	public Vector2 value;


	/// <summary>
	/// the constructor ensures we have a frame to work with for this recognizer
	/// </summary>
	public TKTouchPadRecognizer( TKRect frame )
	{
		boundaryFrame = frame;
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
			for( var i = 0; i < touches.Count; i++ )
			{
				// only add touches in the Began phase
				if( touches[i].phase == TouchPhase.Began )
					_trackingTouches.Add( touches[i] );
			}

			if( _trackingTouches.Count > 0 )
			{
				state = TKGestureRecognizerState.Began;

				// call through to touchesMoved so we set the value and set the state to RecognizedAndStillRecognizing which triggers the recognized event
				touchesMoved( touches );
			}
		}

		return false;
	}


	internal override void touchesMoved( List<TKTouch> touches )
	{
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing || state == TKGestureRecognizerState.Began )
		{
			var currentLocation = touchLocation();
			value = currentLocation - boundaryFrame.Value.center;

			// normalize from 0 - 1 and clamp
			value.x = Mathf.Clamp( value.x / ( boundaryFrame.Value.width * 0.5f ), -1f, 1f );
			value.y = Mathf.Clamp( value.y / ( boundaryFrame.Value.height * 0.5f ), -1f, 1f );

			// apply our inputCurve
			value.x = inputCurve.Evaluate( Mathf.Abs( value.x ) ) * Mathf.Sign( value.x );
			value.y = inputCurve.Evaluate( Mathf.Abs( value.y ) ) * Mathf.Sign( value.y );

			state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
		}
	}


	internal override void touchesEnded( List<TKTouch> touches )
	{
		// remove any completed touches
		for( var i = 0; i < touches.Count; i++ )
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

		value = Vector2.zero;
		state = TKGestureRecognizerState.FailedOrEnded;
	}


	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, value: {2}", this.GetType(), state, value );
	}

}
