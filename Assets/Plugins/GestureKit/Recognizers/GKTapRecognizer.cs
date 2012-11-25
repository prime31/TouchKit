using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GKTapRecognizer : AbstractGestureRecognizer
{
	public int numberOfTapsRequired = 1;
	public int numberOfTouchesRequired = 1;
	
	
	public override void touchesBegan( List<Touch> touches )
	{
		if( touches.Count >= numberOfTouchesRequired )
		{
			if( touches[0].tapCount >= numberOfTapsRequired )
			{
				if( state == GestureRecognizerState.Possible )
					state = GestureRecognizerState.Began;
			}
		}
	}
	
	
	public override void touchesMoved( List<Touch> touches )
	{
		if( state == GestureRecognizerState.Began )
		{
			// did we move?
			if( touches[0].deltaPosition.sqrMagnitude > 5 )
				state = GestureRecognizerState.Failed;
		}
	}
	
	
	public override void touchesEnded( List<Touch> touches )
	{
		if( state == GestureRecognizerState.Began )
			state = GestureRecognizerState.Recognized;
	}
	
}
