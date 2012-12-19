using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class GKPanRecognizer : AbstractGestureRecognizer
{
	public Vector2 startPosition;
	
	
	public override void touchesBegan( List<GKTouch> touches )
	{
		if( state == GestureRecognizerState.Possible )
		{
			state = GestureRecognizerState.Began;
			
			// latch the touches
			for( int i = 0; i < touches.Count; i++ )
			{
				if( !isTrackingTouch( touches[i] ) )
					_trackingTouches.Add( touches[i] );
			}
			
			startPosition = touchLocation();
		}
	}
	
	
	public override void touchesMoved( List<GKTouch> touches )
	{
		if( state == GestureRecognizerState.Began || state == GestureRecognizerState.RecognizedAndStillRecognizing )
			state = GestureRecognizerState.RecognizedAndStillRecognizing;
	}
	
	
	public override void touchesEnded( List<GKTouch> touches )
	{
		state = GestureRecognizerState.Failed;
	}
	
	
	public override string ToString()
	{
		return string.Format( "[{0}] state: {1}, location: {2}, startPosition: {3}, deltaPosition: {4}", this.GetType(), state, touchLocation(), startPosition, ( touchLocation() - startPosition ) );
	}

}
