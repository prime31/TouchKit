using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class GKPanRecognizer : GKAbstractGestureRecognizer
{
	public Vector2 startPosition;
	
	
	public override void touchesBegan( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Possible )
		{
			state = GKGestureRecognizerState.Began;
			_trackingTouches.Add( touches[0] );			
			startPosition = touchLocation();
		}
	}
	
	
	public override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GKGestureRecognizerState.Began || state == GKGestureRecognizerState.RecognizedAndStillRecognizing )
			state = GKGestureRecognizerState.RecognizedAndStillRecognizing;
	}
	
	
	public override void touchesEnded( List<GKTouch> touches )
	{
		state = GKGestureRecognizerState.Failed;
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, startPosition: {3}, deltaPosition: {4}", this.GetType(), state, touchLocation(), startPosition, ( touchLocation() - startPosition ) );
	}

}
