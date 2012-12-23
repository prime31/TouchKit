using UnityEngine;
using System.Collections;


public class DemoOne : MonoBehaviour
{
	public Transform cube;
	private Vector2 _scrollPosition; // for the scroll view
	
	
	void Start()
	{
		// turn on debug drawing while in the editor
		GestureKit.instance.debugDrawBoundaryFrames = true;
	}
	
	
	void OnGUI()
	{
		//GUI.matrix = Matrix4x4.Scale( new Vector3( 1.5f, 1.5f, 1.5f ) );
		GUI.skin.button.padding = new RectOffset( 10, 10, 20, 20 );
		GUI.skin.button.fixedWidth = 250;
		
		
		GUILayout.BeginArea( new Rect( Screen.width - GUI.skin.button.fixedWidth - 20, 0, GUI.skin.button.fixedWidth + 20, Screen.height ) );
			_scrollPosition = GUILayout.BeginScrollView( _scrollPosition, GUILayout.Width( GUI.skin.button.fixedWidth + 20 ), GUILayout.Height( Screen.height ) );
		
		if( GUILayout.Button( "Add Tap Recognizer" ) )
		{
			var recognizer = new GKTapRecognizer();
			
			// we can limit recognition to a specific Rect, in this case the bottom-left corner of the screen
			recognizer.boundaryFrame = new GKRect( 50, 50, 400, 400 );
			
			// we can also set the number of touches required for the gesture
			if( Application.platform == RuntimePlatform.IPhonePlayer )
				recognizer.numberOfTouchesRequired = 2;
			
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
		
		
		if( GUILayout.Button( "Add Press Recognizer" ) )
		{
			var recognizer = new GKPressRecognizer( 0.3f, 10f );
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Debug.Log( "press recognizer fired: " + r );
			};
			recognizer.gestureCompleteEvent += r =>
			{
				Debug.Log( "press recognizer completed: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		

		if( GUILayout.Button( "Add Pan Recognizer" ) )
		{
			var recognizer = new GKPanRecognizer();
			
			// when using in conjunction with a pinch or rotation recognizer setting the min touches to 2 smoothes movement greatly
			if( Application.platform == RuntimePlatform.IPhonePlayer )
				recognizer.minimumNumberOfTouches = 2;
			
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				Camera.mainCamera.transform.position -= new Vector3( recognizer.deltaTranslation.x, recognizer.deltaTranslation.y ) / 100;
				Debug.Log( "pan recognizer fired: " + r );
			};
			
			// continuous gestures have a complete event so that we know when they are done recognizing
			recognizer.gestureCompleteEvent += r =>
			{
				Debug.Log( "pan gesture complete" );
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
				cube.transform.localScale += Vector3.one * recognizer.deltaScale;
				Debug.Log( "pinch recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Add Rotation Recognizer" ) )
		{
			var recognizer = new GKRotationRecognizer();
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				cube.Rotate( Vector3.back, recognizer.deltaRotation );
				Debug.Log( "rotation recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Add Button Recognizer" ) )
		{
			var recognizer = new GKButtonRecognizer( new GKRect( 300, 30, 278, 90 ), 20 );
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
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Add One Finger Rotation Recognizer" ) )
		{
			var recognizer = new GKOneFingerRotationRecognizer();
			recognizer.targetPosition = Camera.mainCamera.WorldToScreenPoint( cube.position );
			recognizer.gestureRecognizedEvent += ( r ) =>
			{
				cube.Rotate( Vector3.back, recognizer.deltaRotation );
				Debug.Log( "one finger rotation recognizer fired: " + r );
			};
			GestureKit.addGestureRecognizer( recognizer );
		}
		
		
		if( GUILayout.Button( "Remove All Recognizers" ) )
		{
			GestureKit.removeAllGestureRecognizers();
		}
		

			GUILayout.EndScrollView();
		GUILayout.EndArea();
	}
}
