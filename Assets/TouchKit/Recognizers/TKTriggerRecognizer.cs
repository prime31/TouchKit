using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks a single touch that starts within a given rect until that touch ends
/// </summary>
public class TKTriggerRecognizer : TKAbstractGestureRecognizer {
	public event Action<TKTouch> OnTriggerEvent;
	public event Action<TKTouch> OnTouchMovedEvent;
	public event Action<TKTouch> OnTouchEnd;

	public TKTouch touch {
		get { return _trackingTouches[0]; } 
	}
		
	public TKTriggerRecognizer (TKRect _bounds) {
		boundaryFrame = _bounds;
	}

	#region implement TKAbstractGestureRecognizer
	internal override void fireRecognizedEvent () {}

	internal override bool touchesBegan (List<TKTouch> touches) {
		if (_trackingTouches.Count == 0) {
			for( int i = 0; i < touches.Count; i++ ) {
				if (boundaryFrame.Value.contains(touches[i].position)) {
					_trackingTouches.Add(touches[i]);
					Debug.Log("TRIGGER Adding touchID " + touches[i].ToString());
					if (OnTriggerEvent != null)
						OnTriggerEvent(touch);

					state = TKGestureRecognizerState.Began;
					return true;
				}
			}
		}
		return false;
	}

	internal override void touchesMoved (List<TKTouch> touches) {
		if (OnTouchMovedEvent != null)
			OnTouchMovedEvent(touch);
		
		state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
	}

	internal override void touchesEnded (List<TKTouch> touches) {
		//FIXME I believe this is masking a bug lower in tocuhkit where a touch not applicable to this is coming in
		for( int i = 0; i < touches.Count; i++ ) {
//			if (touches[i] != touch)
//				continue;

			Debug.Log("TRIGGER Ending touchID " + touches[i].ToString());
			if (OnTouchEnd != null)
				OnTouchEnd(touch);

			state = TKGestureRecognizerState.FailedOrEnded;
		}
	}
	#endregion
}
