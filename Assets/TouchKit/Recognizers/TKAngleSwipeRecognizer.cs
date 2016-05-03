using UnityEngine;
using System;
using System.Collections.Generic;

public class TKAngleSwipeRecognizer : TKAbstractGestureRecognizer
{

	public event Action<TKAngleSwipeRecognizer> gestureRecognizedEvent;

	private struct AngleListener
	{
		public float Varience;
		public Vector2 Direction;
		public Action<TKAngleSwipeRecognizer> Action;

		public AngleListener (Vector2 direction, float varience, Action<TKAngleSwipeRecognizer> action) : this ()
		{
			Varience = varience;
			Direction = direction;
			Action = action;
		}
	}

	private List<AngleListener> _angleRecognizedEvents = new List<AngleListener> ();

	public float timeToSwipe = 0.5f;

	public float swipeVelocity { get; private set; }

	public float swipeAngle { get; private set; }

	public Vector2 swipeVelVector { get; private set; }

	public float minimumDistance = 2f;

	// swipe state info
	private Vector2 _startPoint;
	private Vector2 _endPoint;
	private float _startTime;

	public Vector2 startPoint {
		get { return this._startPoint; }
	}

	public Vector2 endPoint {
		get { return this._endPoint; }
	}

	public TKAngleSwipeRecognizer () : this (2f)
	{
	}

	public TKAngleSwipeRecognizer (float minimumDistanceCm)
	{
		minimumDistance = minimumDistanceCm;
	}

	public void addAngleRecogizedEvents (Action<TKAngleSwipeRecognizer> action, Vector2 direction, float angleVarience)
	{
		_angleRecognizedEvents.Add (new AngleListener (direction, angleVarience, action));
	}

	public void removeAngleRecognizedEvents (Action<TKAngleSwipeRecognizer> action)
	{
		_angleRecognizedEvents.RemoveAll (
			(AngleListener listener) =>  {
				return listener.Action == action;
			});
	}

	public void removeAllAngleRecongnizedEvents ()
	{
		_angleRecognizedEvents.Clear ();
	}

	public void fireAngleRecognizedEvents ()
	{
		for (int i = 0, length = _angleRecognizedEvents.Count; i < length; i++) 
		{
			var angleEvent = _angleRecognizedEvents [i];
			if (angleEvent.Varience > Vector2.Angle (angleEvent.Direction, swipeVelVector)) 
			{
				angleEvent.Action (this);
			}
		}
	}

	private bool checkForSwipeCompletion (TKTouch touch)
	{
		// if we have a time stipulation and we exceeded it stop listening for swipes
		if (timeToSwipe > 0.0f && (Time.time - _startTime) > timeToSwipe) 
		{
			state = TKGestureRecognizerState.FailedOrEnded;
			return false;
		}

		// Grab the total distance moved in both directions
		var xDeltaAbsCm = Mathf.Abs (_startPoint.x - touch.position.x) / TouchKit.instance.ScreenPixelsPerCm;
		var yDeltaAbsCm = Mathf.Abs (_startPoint.y - touch.position.y) / TouchKit.instance.ScreenPixelsPerCm;

		_endPoint = touch.position;

		// Get the velocity
		swipeVelocity = Mathf.Sqrt (xDeltaAbsCm * xDeltaAbsCm + yDeltaAbsCm * yDeltaAbsCm);

		// Calculate the movement vector
		var displacement = endPoint - startPoint;
		// Calculate the angle with respect to the x axis
		swipeAngle = Mathf.Rad2Deg * Mathf.Atan2 (displacement.y, displacement.x);
		// Get it between 0 to 360
		if (swipeAngle < 0) 
		{
			swipeAngle += 360;
		}

		swipeVelVector = _endPoint - _startPoint;

		// If we have enough distance, then return true
		if (swipeVelocity > minimumDistance) 
		{
			return true;
		}

		return false;
	}


	internal override void fireRecognizedEvent ()
	{
		if (gestureRecognizedEvent != null) 
		{
			gestureRecognizedEvent (this);
		}
		fireAngleRecognizedEvents ();
	}


	internal override bool touchesBegan (List<TKTouch> touches)
	{
		if (state == TKGestureRecognizerState.Possible) 
		{
			_startPoint = touches [0].position;
			_startTime = Time.time;
			_trackingTouches.Add (touches [0]);

			state = TKGestureRecognizerState.Began;
		}
		return false;
	}


	internal override void touchesMoved (List<TKTouch> touches)
	{
		if (state == TKGestureRecognizerState.Began) 
		{
			if (checkForSwipeCompletion (touches [0])) 
			{
				state = TKGestureRecognizerState.Recognized;
			}
		}
	}


	internal override void touchesEnded (List<TKTouch> touches)
	{
		state = TKGestureRecognizerState.FailedOrEnded;
	}

	public override string ToString ()
	{
		return string.Format ("{0}, velocity: {1}, angle: {2}, start point: {3}, end point: {4}",
			base.ToString (), swipeVelocity, swipeAngle, startPoint, endPoint);
	}
}
