using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class TKButtonRecognizer : TKAbstractGestureRecognizer
{
	public event Action<TKButtonRecognizer> onSelectedEvent;
	public event Action<TKButtonRecognizer> onDeselectedEvent;
	public event Action<TKButtonRecognizer> onTouchUpInsideEvent;

	private TKRect _defaultFrame;
	private TKRect _highlightedFrame;


	/* State Definitions
	 * Began: a touch started out on the button but has moved off. We still track it in case it comes back on the button
	 * RecognizedAndStillRecognizing: a touch is currently down on the button
	 * Recognized:
	 */

	#region Constructors

	/// <summary>
	/// the constructors ensure we have a frame to work with for button recognizers
	/// </summary>
	public TKButtonRecognizer( TKRect defaultFrame ) : this( defaultFrame, 40f )
	{}


	public TKButtonRecognizer( TKRect defaultFrame, float highlightedExpansion ) : this( defaultFrame, defaultFrame.copyWithExpansion( highlightedExpansion ) )
	{}


	public TKButtonRecognizer( TKRect defaultFrame, TKRect highlightedFrame )
	{
		_defaultFrame = defaultFrame;
		_highlightedFrame = highlightedFrame;
		boundaryFrame = _defaultFrame;
	}

	#endregion


	#region Button methods. Subclasses can override these or you can skip subclassing and just listen to the events

	/// <summary>
	/// called when a touch has began on the button or reentered the frame
	/// </summary>
	protected virtual void onSelected()
	{
		// while selected, we use a highlighted frame to allow the touch to move a bit and still remain selected
		boundaryFrame = _highlightedFrame;

		if( onSelectedEvent != null )
			onSelectedEvent( this );
	}


	/// <summary>
	/// called when a touch ends (if the button was already highlighted) or if a tracked touch exits the highlighted frame
	/// </summary>
	protected virtual void onDeselected()
	{
		if( onDeselectedEvent != null )
			onDeselectedEvent( this );
	}


	/// <summary>
	/// called if a tracked touch ends while the button is highlighted
	/// </summary>
	protected virtual void onTouchUpInside()
	{
		if( onTouchUpInsideEvent != null )
			onTouchUpInsideEvent( this );
	}

	#endregion


	#region TKAbstractGestureRecognizer

	// we do nothing here. all events will be handled internally
	internal override void fireRecognizedEvent() {}


	internal override bool touchesBegan( List<TKTouch> touches )
	{
		// grab the first touch that begins on us
		if( state == TKGestureRecognizerState.Possible )
		{
			for( int i = 0; i < touches.Count; i++ )
			{
				// only add touches in the Began phase
				if( touches[i].phase == TouchPhase.Began )
				{
					_trackingTouches.Add( touches[i] );
					state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
					onSelected();

					return true;
				}
			}
		}

		return false;
	}


	internal override void touchesMoved( List<TKTouch> touches )
	{
		for( int i = 0; i < touches.Count; i++ )
		{
			if( touches[i].phase == TouchPhase.Stationary )
			{
				// check to see if the touch is still in our frame
				var isTouchInFrame = isTouchWithinBoundaryFrame( touches[i] );

				// if we are in the Began phase than we should switch to RecognizedAndStillRecognizing (highlighted) if the touch is in our frame
				if( state == TKGestureRecognizerState.Began && isTouchInFrame )
				{
					state = TKGestureRecognizerState.RecognizedAndStillRecognizing;
					onSelected();
				}
				else if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing && !isTouchInFrame ) // if the touch exits the frame and we were highlighted deselect now
				{
					state = TKGestureRecognizerState.FailedOrEnded;
					onDeselected();
				}
			}
		}
	}


	internal override void touchesEnded( List<TKTouch> touches )
	{
		// if we were previously highlighted (RecognizedAndStillRecognizing) we have an official touch
		if( state == TKGestureRecognizerState.RecognizedAndStillRecognizing )
			onTouchUpInside();

		// reset the boundary frame
		boundaryFrame = _defaultFrame;

		state = TKGestureRecognizerState.FailedOrEnded;
	}

	#endregion

}
