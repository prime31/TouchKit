using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GestureKit : MonoBehaviour
{
	public static int maxTouchesToProcess = 2;
	
	private List<AbstractGestureRecognizer> _gestureRecognizers = new List<AbstractGestureRecognizer>();
	private GKTouch[] _touchCache;
	private List<GKTouch> _liveTouches = new List<GKTouch>();
	
	
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
	
	
	private void OnApplicationQuit()
	{
		_instance = null;
	}
	
	
	private void Awake()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		// we only need one touch on mouse driven platforms
		maxTouchesToProcess = 1;
#endif
		
		// prep our GKTouch cache so we avoid excessive allocations
		_touchCache = new GKTouch[maxTouchesToProcess];
		for( int i = 0; i < maxTouchesToProcess; i++ )
			_touchCache[i] = new GKTouch( i );
	}
	
	
	private void Update()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
		
		// we only need to process if we have some interesting input this frame
		if( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButton( 0 ) )
			_liveTouches.Add( _touchCache[0].populateFromMouse() );
		
#else
		
		// get all touches and examine them. only do our touch processing if we have some touches
		if( Input.touchCount > 0 )
		{
			var maxTouchIndexToExamine = Mathf.Max( Input.touches.Length, maxTouchesToProcess );
			for( var i = 0; i < maxTouchIndexToExamine; i++ )
				_liveTouches.Add( _touchCache[Input.touches[i].fingerId].populateWithTouch( Input.touches[i] ) );
		}
		
#endif
		
		// pass on the touches to all the recognizers
		if( _liveTouches.Count > 0 )
		{
			foreach( var recognizer in _gestureRecognizers )
				recognizer.recognizeTouches( _liveTouches );
			
			_liveTouches.Clear();
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
		
		recognizer.reset();
		instance._gestureRecognizers.Remove( recognizer );
	}
	
	
	public static void removeAllGestureRecognizers()
	{
		instance._gestureRecognizers.Clear();
	}
	
	#endregion

}
