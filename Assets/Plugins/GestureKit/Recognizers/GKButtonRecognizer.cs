using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;



public class GKButtonRecognizer : GKAbstractGestureRecognizer
{
	private enum GKButtonState
	{
		Highlighted
	}
	
	public event Action<GKButtonRecognizer> onSelectedEvent;
	public event Action<GKButtonRecognizer> onDeselectedEvent;
	public event Action<GKButtonRecognizer> onTouchUpInsideEvent;
	
	private GKRect _defaultFrame;
	private GKRect _highlightedFrame;
	
	
	/* State Definitions
	 * Began: a touch started out on the button but has moved off. We still track it in case it comes back on the button
	 * RecognizedAndStillRecognizing: a touch is currently down on the button
	 * Recognized: 
	 */
	
	#region Constructors
	
	/// <summary>
	/// the contstructors ensure we have a frame to work with for button recognizers
	/// </summary>
	public GKButtonRecognizer( GKRect defaultFrame ) : this( defaultFrame, 30 )
	{}
	
	
	public GKButtonRecognizer( GKRect defaultFrame, float highlightedExpansion ) : this( defaultFrame, defaultFrame.copyWithExpansion( highlightedExpansion ) )
	{}
	
	
	public GKButtonRecognizer( GKRect defaultFrame, GKRect highlightedFrame )
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
	/// called when a touch ends (if the button was already highlighted) or if a tracked touch exists the highlighted frame
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
	
	
	#region GKAbstractGestureRecognizer
	
	// we do nothing here. all event will be handled internally
	internal override void fireRecognizedEvent() {}
	
	
	internal override bool touchesBegan( List<GKTouch> touches )
	{
		// grab the first touch that begins on us
		if( state == GKGestureRecognizerState.Possible && touches[0].phase == TouchPhase.Began )
		{
			_trackingTouches.Add( touches[0] );
			state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
			onSelected();
			
			return true;
		}
		
		return false;
	}
	
	
	internal override void touchesMoved( List<GKTouch> touches )
	{
		// check to see if the touch is still in our frame
		var isTouchInFrame = boundaryFrame.Value.contains( touches[0].position );
		
		// if we are in the Began phase than we should switch to RecognizedAndStillRecognizing (highlighted) if the touch is in our frame
		if( state == GKGestureRecognizerState.Began && isTouchInFrame )
		{
			state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
			onSelected();
		}
		else if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing && !isTouchInFrame ) // if the touch exits the frame and we were highlighted deselect now
		{
			state = GKGestureRecognizerState.Began;
			onDeselected();
		}
	}
	
	
	internal override void touchesEnded( List<GKTouch> touches )
	{
		// if we were previously highlighted (RecognizedAndStillRecognizing) we have an official touch
		if( state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
			onTouchUpInside();
		
		// reset the boundary frame
		boundaryFrame = _defaultFrame;
		
		state = GKGestureRecognizerState.Failed;
	}
	
	#endregion

}
