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
			recognizer.boundaryFrame = new Rect( 0, 0, 300, 300 );
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "tap recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Add Long Press Recognizer" ) )
		{
			var recognizer = new GKLongPressRecognizer();
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "long press recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		

		if( GUILayout.Button( "Add Pan Recognizer" ) )
		{
			var recognizer = new GKPanRecognizer();
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "pan recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}

		
		if( GUILayout.Button( "Add Horizontal Swipe Recognizer" ) )
		{
			var recognizer = new GKSwipeRecognizer();
			recognizer.swipesToDetect = GKSwipeDirection.Horizontal;
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "swipe recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Add Pinch Recognizer" ) )
		{
			var recognizer = new GKPinchRecognizer();
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "pinch recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Remove All Recognizers" ) )
		{
			GestureKit.removeAllGestureRecognizers();
		}

	}
}
