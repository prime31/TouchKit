using UnityEngine;
using System.Collections;


public class DemoOne : MonoBehaviour
{
	void OnGUI()
	{
		GUI.matrix = Matrix4x4.Scale( new Vector3( 1.5f, 1.5f, 1.5f ) );
		GUI.skin.button.padding = new RectOffset( 0, 0, 10, 10 );
		GUI.skin.button.fixedWidth = 200;
		
		
		if( GUILayout.Button( "Add Tap Recognizer" ) )
		{
			var recognizer = new GKTapRecognizer();
			recognizer.gestureStateChangedEvent += ( r ) =>
			{
				if( r.state == GestureRecognizerState.Ended )
					Debug.Log( "tap recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Add Long Press Recognizer" ) )
		{
			var recognizer = new GKLongPressRecognizer();
			recognizer.gestureStateChangedEvent += ( r ) =>
			{
				// when a long press recognizer begins, that means the delay has elapsed
				if( r.state == GestureRecognizerState.Began )
					Debug.Log( "long press recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Remove All Recognizers" ) )
		{
			GestureKit.removeAllGestureRecognizers();
		}

	}
}
