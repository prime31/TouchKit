using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GestureKit : MonoBehaviour
{
	private List<AbstractGestureRecognizer> _gestureRecognizers = new List<AbstractGestureRecognizer>();
	private List<Touch> _trackingTouches = new List<Touch>();
	
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
	// used to track mouse movement and fake touches
	private Vector2? lastMousePosition;
#endif
	
	
	private static GestureKit _instance = null;
	public static GestureKit instance
	{
		get
		{
			if( !_instance )
			{
				// check if there is a GO instance already available in the scene graph
				_instance = FindObjectOfType( typeof( GestureKit ) ) as GestureKit;

				// nope, create a new one
				if( !_instance )
				{
					var obj = new GameObject( "GestureKit" );
					_instance = obj.AddComponent<GestureKit>();
					DontDestroyOnLoad( obj );
				}
			}

			return _instance;
		}
	}
	
	
	private void Update()
	{
		_trackingTouches.Clear();
		
		// get all touches and examine them
		// only do our touch processing if we have some touches
		if( Input.touchCount > 0 )
		{
			// send all the touches to the relevant gesture recognizers
			for( int i = 0; i < Input.touchCount; i++ )
			{
				var touch = Input.GetTouch( i );
				_trackingTouches.Add( touch );
			}
		}
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		else
		{
			if( Input.GetMouseButtonDown( 0 ) )
				_trackingTouches.Add( MouseToTouch.createTouchFromInput( GKMouseState.DownThisFrame, ref lastMousePosition ) );
			else if( Input.GetMouseButtonUp( 0 ) )
				_trackingTouches.Add( MouseToTouch.createTouchFromInput( GKMouseState.UpThisFrame, ref lastMousePosition ) );
			else if( Input.GetMouseButton( 0 ) )
				_trackingTouches.Add( MouseToTouch.createTouchFromInput( GKMouseState.HeldDown, ref lastMousePosition ) );
		}
#endif
		
		// recognize gestures if we have a touch
		if( _trackingTouches.Count > 0 )
		{
			// get all gesture recognizers filtering for those that are valid for the current Rect
			foreach( var recognizer in _gestureRecognizers )
				recognizer.recognizeTouches( _trackingTouches );
		}
	}

		
	#region Public API
	
	public static void addGestureRecognizer( AbstractGestureRecognizer recognizer )
	{
		instance._gestureRecognizers.Add( recognizer );
	}
	
	
	public static void removeGestureRecognizer( AbstractGestureRecognizer recognizer )
	{
		if( !_instance._gestureRecognizers.Contains( recognizer ) )
		{
			Debug.LogError( "Trying to remove gesture recognizer that has not been added: " + recognizer );
			return;
		}
		
		instance._gestureRecognizers.Remove( recognizer );
	}
	
	
	public static void removeAllGestureRecognizers()
	{
		instance._gestureRecognizers.Clear();
	}
	
	#endregion

}
