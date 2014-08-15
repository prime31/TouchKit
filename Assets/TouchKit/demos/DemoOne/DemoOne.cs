using UnityEngine;
using System.Collections;


public class DemoOne : MonoBehaviour
{
	public Transform cube;
	private Vector2 _scrollPosition; // for the scroll view


	void OnGUI()
	{
		GUI.skin.button.padding = new RectOffset( 10, 10, 20, 20 );
		GUI.skin.button.fixedWidth = 250;


		GUILayout.BeginArea( new Rect( Screen.width - GUI.skin.button.fixedWidth - 20, 0, GUI.skin.button.fixedWidth + 20, Screen.height ) );
			_scrollPosition = GUILayout.BeginScrollView( _scrollPosition, GUILayout.Width( GUI.skin.button.fixedWidth + 20 ), GUILayout.Height( Screen.height ) );

		if( GUILayout.Button( "Add Curve Recognizer" ) ){
			var recognizer = new TKCurveRecognizer();

			recognizer.gestureRecognizedEvent += ( r ) => {
				cube.Rotate( Vector3.back, recognizer.deltaRotation );
				Debug.Log( "curve recognizer fired: " + r );
			};

			recognizer.gestureCompleteEvent += ( r ) => {
				Debug.Log( "curve completed.");
			};

			TouchKit.addGestureRecognizer( recognizer );
		}

		if( GUILayout.Button( "Add Tap Recognizer" ) )
		{
			var recognizer = new TKTapRecognizer();

			// we can limit recognition to a specific Rect, in this case the bottom-left corner of the screen
			recognizer.boundaryFrame = new TKRect( 0, 0, 50f, 50f );

			// we can also set the number of touches required for the gesture
			if( Application.platform == RuntimePlatform.IPhonePlayer )
				recognizer.numberOfTouchesRequired = 2;

			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "tap recognizer fired: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add Long Press Recognizer" ) )
		{
			var recognizer = new TKLongPressRecognizer();
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "long press recognizer fired: " + r );
			};
			recognizer.gestureCompleteEvent += ( r ) =>
			{
				Debug.Log( "long press recognizer finished: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add Pan Recognizer" ) )
		{
			var recognizer = new TKPanRecognizer();

			// when using in conjunction with a pinch or rotation recognizer setting the min touches to 2 smoothes movement greatly
			if( Application.platform == RuntimePlatform.IPhonePlayer )
				recognizer.minimumNumberOfTouches = 2;

			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Camera.main.transform.position -= new Vector3( recognizer.deltaTranslation.x, recognizer.deltaTranslation.y ) / 100;
				Debug.Log( "pan recognizer fired: " + r );
			};

			// continuous gestures have a complete event so that we know when they are done recognizing
			recognizer.gestureCompleteEvent += r =>
			{
				Debug.Log( "pan gesture complete" );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add Horizontal Swipe Recognizer" ) )
		{
			var recognizer = new TKSwipeRecognizer( TKSwipeDirection.Horizontal );
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "swipe recognizer fired: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add Pinch Recognizer" ) )
		{
			var recognizer = new TKPinchRecognizer();
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				cube.transform.localScale += Vector3.one * recognizer.deltaScale;
				Debug.Log( "pinch recognizer fired: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add Rotation Recognizer" ) )
		{
			var recognizer = new TKRotationRecognizer();
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				cube.Rotate( Vector3.back, recognizer.deltaRotation );
				Debug.Log( "rotation recognizer fired: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add Button Recognizer" ) )
		{
			var recognizer = new TKButtonRecognizer( new TKRect( 5f, 145f, 80f, 30f ), 10f );
			recognizer.zIndex = 1;
			recognizer.onSelectedEvent += ( r ) =>
			{
				Debug.Log( "button recognizer selected: " + r );
			};
			recognizer.onDeselectedEvent += ( r ) =>
			{
				Debug.Log( "button recognizer deselected: " + r );
			};
			recognizer.onTouchUpInsideEvent += ( r ) =>
			{
				Debug.Log( "button recognizer touch up inside: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add One Finger Rotation Recognizer" ) )
		{
			var recognizer = new TKOneFingerRotationRecognizer();
			recognizer.targetPosition = Camera.main.WorldToScreenPoint( cube.position );
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				cube.Rotate( Vector3.back, recognizer.deltaRotation );
				Debug.Log( "one finger rotation recognizer fired: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Add Any Touch Recognizer" ) )
		{
			var recognizer = new TKAnyTouchRecognizer( new TKRect( 10, 10, 80, 50 ) );
			recognizer.zIndex = 1;
			recognizer.onEnteredEvent += ( r ) =>
			{
				Debug.Log( "any touch entered: " + r );
			};
			recognizer.onExitedEvent += ( r ) =>
			{
				Debug.Log( "any touch exited: " + r );
			};
			TouchKit.addGestureRecognizer( recognizer );
		}


		if( GUILayout.Button( "Remove All Recognizers" ) )
		{
			TouchKit.removeAllGestureRecognizers();
		}


			GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
}
