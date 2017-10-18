using UnityEngine;
using System;
using System.Collections.Generic;


/// <summary>
/// TouchKit Analog  Recognizer to allow for an invisible analog which is 
/// usually best when working with mobile devices to avoid having a cluncky UI
/// in the way of what's important to the player
/// </summary>
public class TKAnalogRecognizer : TKAbstractGestureRecognizer
{
    public event Action<TKAnalogRecognizer> gestureRecognizedEvent;
    public event Action<TKAnalogRecognizer> gestureCompleteEvent;

    public AnimationCurve inputCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
    public Vector2 value;
    public Vector2 touchPadCenter;

    /// <summary>
    /// How far out to extend from the center of the rect or the initial tap position
    /// </summary>
    public float touchPadRadius = 50f;

    /// <summary>
    /// If false, the center of the analog will be wherever the initial touch is inside the rect
    /// </summary>
    public bool useRectOrigin = false;

    /// <summary>
    /// if false, the radius will determine how far out from the center to go to get to 1 or -1
    /// </summary>
    public bool useRectSize = false;

    /// <summary>
    /// only necessary if there is a rect in the same space waiting for a tap gesture
    /// </summary>
    public bool ignoreInitialTouch = false;


    /// <summary>
    /// the constructor ensures we have a frame to work with for this recognizer
    /// by default: use initial tap as analog center and default radius
    /// </summary>
    public TKAnalogRecognizer(TKRect frame)
    {
        this.boundaryFrame = frame;
        this.ignoreInitialTouch = false;
        this.useRectOrigin = false;
        this.useRectSize = false;
    }

    /// <summary>
    /// ignoring the initial touch is only necessary if there is a rect in the same space waiting for a tap gesture
    /// </summary>
    public TKAnalogRecognizer(TKRect frame, bool ignoreInitialTouch)
    {
        this.boundaryFrame = frame;
        this.ignoreInitialTouch = ignoreInitialTouch;
        this.useRectOrigin = false;
        this.useRectSize = false;
    }

    /// <summary>
    /// change radius size
    /// </summary>
    public TKAnalogRecognizer(TKRect frame, float radius)
    {
        this.boundaryFrame = frame;
        this.touchPadRadius = radius;
        this.ignoreInitialTouch = false;
        this.useRectOrigin = false;
        this.useRectSize = false;
    }

    /// <summary>
    /// set the analog radius and use the rect's origin as the center point
    /// </summary>
    public TKAnalogRecognizer(TKRect frame, float radius, bool useRectOrigin)
    {
        this.boundaryFrame = frame;
        this.touchPadRadius = radius;
        this.useRectOrigin = useRectOrigin;
        this.ignoreInitialTouch = false;
        this.useRectSize = false;
    }

    /// <summary>
    /// use rect origin as center point and rect size to determine analog position
    /// </summary>
    public TKAnalogRecognizer(TKRect frame, bool useRectOrigin, bool useRectSize)
    {
        this.boundaryFrame = frame;
        this.useRectOrigin = useRectOrigin;
        this.useRectSize = useRectSize;
        this.ignoreInitialTouch = false;
    }

    internal override bool touchesBegan(List<TKTouch> touches)
    {
        if (state == TKGestureRecognizerState.Possible)
        {
            for (var i = 0; i < touches.Count; i++)
            {
                // only add touches in the Began phase
                if (touches[i].phase == TouchPhase.Began)
                    _trackingTouches.Add(touches[i]);
            }

            if (_trackingTouches.Count > 0)
            {
                state = TKGestureRecognizerState.Began;

                // call through to touchesMoved so we set the value and set the state to RecognizedAndStillRecognizing which triggers the recognized event
                touchesMoved(touches, ignoreInitialTouch);
            }
        }
        return false;
    }

    internal override void touchesMoved(List<TKTouch> touches)
    {

        if (state == TKGestureRecognizerState.RecognizedAndStillRecognizing || state == TKGestureRecognizerState.Began)
        {
            var currentLocation = touchLocation();
            value = currentLocation - (useRectOrigin ? boundaryFrame.Value.center : touchPadCenter);

            // normalize from 0 - 1 and clamp
            value.x = Mathf.Clamp(value.x / (useRectSize ? (boundaryFrame.Value.width * 0.5f) : touchPadRadius), -1f, 1f);
            value.y = Mathf.Clamp(value.y / (useRectSize ? (boundaryFrame.Value.height * 0.5f) : touchPadRadius), -1f, 1f);

            // apply our inputCurve
            value.x = inputCurve.Evaluate(Mathf.Abs(value.x)) * Mathf.Sign(value.x);
            value.y = inputCurve.Evaluate(Mathf.Abs(value.y)) * Mathf.Sign(value.y);

            state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
        }
    }
    
    /// <summary>
    /// If also has a tap recognizer we want to ignore the first touch as to not be confused in using the analog
    /// </summary>
    internal void touchesMoved(List<TKTouch> touches, bool ignoreInitialTouch)
    {
        if (state == TKGestureRecognizerState.Began && ignoreInitialTouch)
        {
            //cashe starting touch position and ignore initial touch in case it's a tap
            touchPadCenter = startTouchLocation();
        }
        else
        {
            // call through to touchesMoved so we set the value and set the state to RecognizedAndStillRecognizing which triggers the recognized event
            touchesMoved(touches);
        }
    }

    internal override void touchesEnded(List<TKTouch> touches)
    {
        // remove any completed touches
        for (var i = 0; i < touches.Count; i++)
        {
            if (touches[i].phase == TouchPhase.Ended)
                _trackingTouches.Remove(touches[i]);
        }

        // if we had previously been recognizing fire our complete event
        if (state == TKGestureRecognizerState.RecognizedAndStillRecognizing)
        {
            if (gestureCompleteEvent != null)
                gestureCompleteEvent(this);
        }

        value = Vector2.zero;
        state = TKGestureRecognizerState.FailedOrEnded;
    }

    internal override void fireRecognizedEvent()
    {
        if (gestureRecognizedEvent != null)
            gestureRecognizedEvent(this);
    }

    public override string ToString()
    {
        return string.Format("[{0}] state: {1}, value: {2}", this.GetType(), state, value);
    }
}