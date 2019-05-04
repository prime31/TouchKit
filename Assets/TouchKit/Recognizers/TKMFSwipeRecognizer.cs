using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Class used to track finger swipes. Supports multiple fingers
/// </summary>    
public class TKMFSwipe {
	public List<Vector2> points = new List<Vector2>();
	public float startTime;
	public float swipeVelocity;
	public TKSwipeDirection direction;

	public Vector2 StartPoint {
		get{ return points.FirstOrDefault(); }
	}

	public Vector2 EndPoint {
		get { return points.LastOrDefault(); }
	}

	public Vector2 VectorDirection {
		get { return (EndPoint - StartPoint).normalized; }
	}
}

public class TKMFSwipeRecognizer : TKAbstractGestureRecognizer {
	/// <summary>
    /// The event that fires when a swipe is recognized.
    /// </summary>
    public event System.Action<TKMFSwipe> gestureRecognizedEvent;

    /// <summary>
    /// The maximum amount of time for the motion to be considered a swipe.
    /// Setting to 0f will disable the time restriction completely.
    /// </summary>
    public float timeToSwipe = 0.5f;

    /// <summary>
    /// The maximum number of simultaneous touches (fingers) on the screen to trigger
    /// this swipe recognizer. Default is 2.
    /// </summary>
    public int maximumNumberOfTouches = 2;

    /// <summary>
    /// If true, will trigger on the frame that the criteria for a swipe are first met.
    /// If false, will only trigger on completion of the motion, when the touch is lifted.
    /// </summary>
    public bool triggerWhenCriteriaMet = true;


    /// <summary>
    /// The minimum distance in centimeters that the gesture has to make to be considered
    /// a proper swipe, based on resolution and pixel density. Default is 2cm.
    /// </summary>
    private float _minimumDistance = 2f;

    /// <summary>
    /// A dictionary keyed on touches that represents the swipe data for each individual
	/// finger one the screen
    /// </summary>
//    private List<Vector2> fingers = new List<Vector2>();
	private Dictionary<TKTouch, TKMFSwipe> fingers = new Dictionary<TKTouch, TKMFSwipe>();

    public TKMFSwipeRecognizer() : this(2f)
    { }

    public TKMFSwipeRecognizer(float minimumDistanceCm)
    {
        this._minimumDistance = minimumDistanceCm;
    }


    private bool CheckForSwipeCompletion(TKTouch touch) {
        //Grab the swipe tracking we're dealing with
		TKMFSwipe swipe = fingers[touch];

		// if we have a time stipulation and we exceeded it stop listening for swipes, fail
        if (timeToSwipe > 0.0f && (Time.time - swipe.startTime) > timeToSwipe)
            return false;

        // if we don't have at least two points to test yet, then fail
		if (this.fingers[touch].points.Count < 2)
            return false;

        // the ideal distance in pixels from the start to the finish
		float idealDistance = Vector2.Distance(swipe.StartPoint, swipe.EndPoint);

        // the ideal distance in centimeters, based on the screen pixel density
        float idealDistanceCM = idealDistance / TouchKit.instance.ScreenPixelsPerCm;

        // if the distance moved in cm was less than the minimum,
        if (idealDistanceCM < this._minimumDistance)
            return false;

        // add up distances between all points sampled during the gesture to get the real distance
        float realDistance = 0f;
        for (int i = 1; i < this.fingers[touch].points.Count; i++)
            realDistance += Vector2.Distance(this.fingers[touch].points[i], this.fingers[touch].points[i - 1]);

        // if the real distance is 10% greater than the ideal distance, then fail
        // this weeds out really irregular "lines" and curves from being considered swipes
        if (realDistance > idealDistance * 1.1f)
            return false;

        // the speed in cm/s of the swipe
        swipe.swipeVelocity = idealDistanceCM / (Time.time - swipe.startTime);

        // turn the slope of the ideal swipe line into an angle in degrees
		Vector2 v2 = (swipe.EndPoint - swipe.StartPoint).normalized;
        float swipeAngle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
        if (swipeAngle < 0)
            swipeAngle = 360 + swipeAngle;
        swipeAngle = 360 - swipeAngle;

        // depending on the angle of the line, give a logical swipe direction
        if (swipeAngle >= 292.5f && swipeAngle <= 337.5f)
            swipe.direction = TKSwipeDirection.UpRight;
        else if (swipeAngle >= 247.5f && swipeAngle <= 292.5f)
            swipe.direction = TKSwipeDirection.Up;
        else if (swipeAngle >= 202.5f && swipeAngle <= 247.5f)
            swipe.direction = TKSwipeDirection.UpLeft;
        else if (swipeAngle >= 157.5f && swipeAngle <= 202.5f)
            swipe.direction = TKSwipeDirection.Left;
        else if (swipeAngle >= 112.5f && swipeAngle <= 157.5f)
            swipe.direction = TKSwipeDirection.DownLeft;
        else if (swipeAngle >= 67.5f && swipeAngle <= 112.5f)
            swipe.direction = TKSwipeDirection.Down;
        else if (swipeAngle >= 22.5f && swipeAngle <= 67.5f)
            swipe.direction = TKSwipeDirection.DownRight;
        else // swipeAngle >= 337.5f || swipeAngle <= 22.5f
            swipe.direction = TKSwipeDirection.Right;

        return true;
    }

	internal override void fireRecognizedEvent () {}

	//TODO call this manually
    internal virtual void FireRecognizedEvent(TKTouch touch) {
		//Remove tracking
		_trackingTouches.Remove(touch);
		TKMFSwipe swipingFinger = fingers[touch];
		fingers.Remove(touch);

		if (gestureRecognizedEvent != null)
			gestureRecognizedEvent(swipingFinger);

    }

    internal override bool touchesBegan(List<TKTouch> touches) {
		foreach (var touch in touches)
		if (fingers.Count < maximumNumberOfTouches) {
			for (int i = 0; i < touches.Count; i++) {
				if (!fingers.ContainsKey(touches[i])) {
					this._trackingTouches.Add(touches[i]); //Add the touch to further tracking updates
					Debug.Log("SWIPE Adding Touch " + touches[i].ToString());
					fingers.Add(touches[i], new TKMFSwipe()); //Add the touch to internal swipe tracking
					fingers[touches[i]].points.Add(touches[i].position); //Update position
					fingers[touches[i]].startTime = Time.time; //timestamp
				}
	        }
			state = TKGestureRecognizerState.Began;
			return true;
		}
        return false;
    }

    internal override void touchesMoved(List<TKTouch> touches) {
		for (int i = 0; i < touches.Count; i++) {
			//Update points
			fingers[touches[i]].points.Add(touches[i].position);

			// if we're triggering when the criteria is met, then check for completion every frame
			if (triggerWhenCriteriaMet && CheckForSwipeCompletion(touches[i])) {
				FireRecognizedEvent(touches[i]);
				state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
			}
		}
    }

    internal override void touchesEnded(List<TKTouch> touches) {
		for (int i = 0; i < touches.Count; i++) {
			if (!fingers.ContainsKey(touches[i]))
				continue;
			
			//Update points
			fingers[touches[i]].points.Add(touches[i].position);
		
			// last frame, one last check for recognition
			if (CheckForSwipeCompletion(touches[i])) {
				FireRecognizedEvent(touches[i]);
				state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
			}

			Debug.Log("SWIPE Removing Touch " + touches[i].ToString());

			//Remove from touch tracking
			fingers.Remove(touches[i]);
			_trackingTouches.Remove(touches[i]);
		}
    }
}
