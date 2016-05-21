using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Flags]
public enum TKSwipeDirection
{
    Left = (1 << 0),
    Right = (1 << 1),
    Up = (1 << 2),
    Down = (1 << 3),

    UpLeft = (1 << 4),
    DownLeft = (1 << 5),
    UpRight = (1 << 6),
    DownRight = (1 << 7),

    Horizontal = (Left | Right),
    Vertical = (Up | Down),
    Cardinal = (Horizontal | Vertical),

    DiagonalUp = (UpLeft | UpRight),
    DiagonalDown = (DownLeft | DownRight),
    DiagonalLeft = (UpLeft | DownLeft),
    DiagonalRight = (UpRight | DownRight),
    Diagonal = (DiagonalUp | DiagonalDown),

    RightSide = (Right | DiagonalRight),
    LeftSide = (Left | DiagonalLeft),
    TopSide = (Up | DiagonalUp),
    BottomSide = (Down | DiagonalDown),

    All = (Cardinal | Diagonal)
}

public class TKSwipeRecognizer : TKAbstractGestureRecognizer
{
    public event System.Action<TKSwipeRecognizer> gestureRecognizedEvent;

    public float timeToSwipe = 0.5f;
    public float swipeVelocity { get; private set; }
    public TKSwipeDirection completedSwipeDirection { get; private set; }
    public int minimumNumberOfTouches = 1;
    public int maximumNumberOfTouches = 2;

    private float _minimumDistance = 2f;

    // swipe state info
    private List<Vector2> points = new List<Vector2>();
    private float startTime;

    public Vector2 startPoint
    {
        get { return this.points.FirstOrDefault(); }
    }

    public Vector2 endPoint
    {
        get { return this.points.LastOrDefault(); }
    }

    public TKSwipeRecognizer() : this(2f)
    { }

    public TKSwipeRecognizer(float minimumDistanceCm)
    {
        _minimumDistance = minimumDistanceCm;
    }

    private bool checkForSwipeCompletion(TKTouch touch)
    {
        // the ideal distance in pixels from the start to the finish
        float idealDistance = Vector2.Distance(startPoint, endPoint);

        // the ideal distance in centimeters, based on the screen pixel density
        float idealDistanceCM = idealDistance / TouchKit.instance.ScreenPixelsPerCm;

        // if the distance moved in cm was less than the minimum,
        // or if we don't have at least two points in the motion to test, then fail
        if (idealDistanceCM < _minimumDistance || points.Count < 2)
            return false;

        // add up all of the point-to-point distances sampled during the swipe motion
        float totalPointToPointDistance = 0f;

        for (int i = 1; i < points.Count; i++)
            totalPointToPointDistance += Vector2.Distance(points[i], points[i - 1]);

        // if the cumulative point-to-point distance is 10% greater than the ideal distance, fail
        if (totalPointToPointDistance > (idealDistance * 1.1f))
            return false;

        // the speed in cm/s of the swipe
        swipeVelocity = idealDistanceCM / (Time.time - startTime);

        // turn the slope of the ideal swipe line into an angle in degrees
        Vector2 v2 = (endPoint - startPoint).normalized;
        float swipeAngle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
        if (swipeAngle < 0)
            swipeAngle = 360 + swipeAngle;
        swipeAngle = 360 - swipeAngle;

        // depending on the angle of the line, give a logical swipe direction
        if (swipeAngle >= 337.5f || swipeAngle <= 22.5f)
            completedSwipeDirection = TKSwipeDirection.Right;
        else if (swipeAngle >= 292.5f && swipeAngle <= 337.5f)
            completedSwipeDirection = TKSwipeDirection.UpRight;
        else if (swipeAngle >= 247.5f && swipeAngle <= 292.5f)
            completedSwipeDirection = TKSwipeDirection.Up;
        else if (swipeAngle >= 202.5f && swipeAngle <= 247.5f)
            completedSwipeDirection = TKSwipeDirection.UpLeft;
        else if (swipeAngle >= 157.5f && swipeAngle <= 202.5f)
            completedSwipeDirection = TKSwipeDirection.Left;
        else if (swipeAngle >= 112.5f && swipeAngle <= 157.5f)
            completedSwipeDirection = TKSwipeDirection.DownLeft;
        else if (swipeAngle >= 67.5f && swipeAngle <= 112.5f)
            completedSwipeDirection = TKSwipeDirection.Down;
        else if (swipeAngle >= 22.5f && swipeAngle <= 67.5f)
            completedSwipeDirection = TKSwipeDirection.DownRight;

        return true;
    }

    internal override void fireRecognizedEvent()
    {
        if (gestureRecognizedEvent != null)
            gestureRecognizedEvent(this);
    }

    internal override bool touchesBegan(List<TKTouch> touches)
    {
        if (state == TKGestureRecognizerState.Possible)
        {
            // add any touches on screen
            for (int i = 0; i < touches.Count; i++)
                _trackingTouches.Add(touches[i]);

            // if the number of touches is within our constraints, begin tracking
            if (_trackingTouches.Count >= minimumNumberOfTouches && _trackingTouches.Count <= maximumNumberOfTouches)
            {
                points.Clear();
                points.Add(touches[0].position);

                startTime = Time.time;
                state = TKGestureRecognizerState.Began;
            }
        }
        return false;
    }

    internal override void touchesMoved(List<TKTouch> touches)
    {
        if(state == TKGestureRecognizerState.Began)
        {
            // if we have a time stipulation and we exceeded it, fail
            if (timeToSwipe > 0.0f && (Time.time - startTime) > timeToSwipe)
                state = TKGestureRecognizerState.FailedOrEnded;
            else
                points.Add(touches[0].position);
        }
    }

    internal override void touchesEnded(List<TKTouch> touches)
    {
        if (state == TKGestureRecognizerState.Began)
        {
            // if we haven't failed yet, add the final point and then check for swipe completion
            points.Add(touches[0].position);

            if (checkForSwipeCompletion(touches[0]))
            {
                state = TKGestureRecognizerState.Recognized;
                return;
            }
        }
        state = TKGestureRecognizerState.FailedOrEnded;
    }

    public override string ToString()
    {
        return string.Format("{0}, swipe direction: {1}, swipe velocity: {2}, start point: {3}, end point: {4}",
            base.ToString(), completedSwipeDirection, swipeVelocity, startPoint, endPoint);
    }
}
