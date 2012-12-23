using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GestureKit : MonoBehaviour
{
	public bool debugDrawBoundaryFrames = false;
	public static int maxTouchesToProcess = 2;
	
	private List<GKAbstractGestureRecognizer> _gestureRecognizers = new List<GKAbstractGestureRecognizer>();
	private GKTouch[] _touchCache;
	private List<GKTouch> _liveTouches = new List<GKTouch>();
	private bool _shouldCheckForLostTouches = false; // used to ensure we dont check for lost touches too often
	
#if UNITY_EDITOR
	private bool _isUnityRemoteActive = false; // hack to detect the Unity remote. Once you touch the screen once mouse input will be ignored
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
	
	
	/// <summary>
	/// Unity often misses the Ended phase of touches so this method will look out for that
	/// </summary>
	private void addTouchesUnityForgotToEndToLiveTouchesList()
	{
		for( int i = 0; i < _touchCache.Length; i++ )
		{
			if( _touchCache[i].phase != TouchPhase.Ended )
			{
				Debug.LogWarning( "found touch Unity forgot to end with phase: " + _touchCache[i].phase );
				_touchCache[i].phase = TouchPhase.Ended;
				_liveTouches.Add( _touchCache[i] );
			}
		}
	}
	
	
	#region MonoBehaviour
	
	private void OnApplicationQuit()
	{
		_instance = null;
	}
	
	
	private void Awake()
	{
#if UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
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
		// the next 10 or so lines is disgustingly, appallingly, horrendously horrible but it helps when testing
#if UNITY_EDITOR
		// check to see if the Unity Remote is active
		if( !_isUnityRemoteActive && Input.touchCount > 0 )
		{
			Debug.LogWarning( "disabling mouse input because we detected a Unity Remote connected" );
			_isUnityRemoteActive = true;
		}
		
		if( !_isUnityRemoteActive )
		{
#endif
		
	#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
			
			// we only need to process if we have some interesting input this frame
			if( Input.GetMouseButtonUp( 0 ) || Input.GetMouseButton( 0 ) )
				_liveTouches.Add( _touchCache[0].populateFromMouse() );
	
	#endif
		
#if UNITY_EDITOR
		}
#endif
		
		// get all touches and examine them. only do our touch processing if we have some touches
		if( Input.touchCount > 0 )
		{
			_shouldCheckForLostTouches = true;
			
			var maxTouchIndexToExamine = Mathf.Min( Input.touches.Length, maxTouchesToProcess );
			for( var i = 0; i < maxTouchIndexToExamine; i++ )
			{
				var touch = Input.touches[i];
				if( touch.fingerId < maxTouchesToProcess )
					_liveTouches.Add( _touchCache[touch.fingerId].populateWithTouch( touch ) );
			}
		}
		else
		{
			// we guard this so that we only check once after all the touches are lifted
			if( _shouldCheckForLostTouches )
			{
				addTouchesUnityForgotToEndToLiveTouchesList();
				_shouldCheckForLostTouches = false;
			}
		}

		// pass on the touches to all the recognizers
		if( _liveTouches.Count > 0 )
		{
			foreach( var recognizer in _gestureRecognizers )
				recognizer.recognizeTouches( _liveTouches );
			
			_liveTouches.Clear();
		}
	}
	
	
#if UNITY_EDITOR
	
	// this is for debugging only while in the editor
	private void OnDrawGizmos()
	{
		if( !debugDrawBoundaryFrames )
			return;

		var colors = new Color[] { Color.red, Color.cyan, Color.red, Color.magenta, Color.yellow };
		int i = 0;
		
		foreach( var r in _gestureRecognizers )
		{
			if( r.boundaryFrame.HasValue )
			{
				debugDrawRect( r.boundaryFrame.Value, colors[i] );
				if( ++i >= colors.Length )
					i = 0;
			}
		}
	}
	
	
	private void debugDrawRect( Rect rect, Color color )
	{
		var bl = new Vector3( rect.xMin, rect.yMin, 0 );
		var br = new Vector3( rect.xMax, rect.yMin, 0 );
		var tl = new Vector3( rect.xMin, rect.yMax, 0 );
		var tr = new Vector3( rect.xMax, rect.yMax, 0 );
		
		bl = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( bl ) );
		br = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( br ) );
		tl = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( tl ) );
		tr = Camera.main.ScreenToWorldPoint( Camera.main.transform.InverseTransformPoint( tr ) );
		
		// draw four sides
		Debug.DrawLine( bl, br, color );
		Debug.DrawLine( br, tr, color );
		Debug.DrawLine( tr, tl, color );
		Debug.DrawLine( tl, bl, color );
		
		// make an "x" at the midpoint
		Debug.DrawLine( tl, br, color );
		Debug.DrawLine( bl, tr, color );
	}
	
#endif
	
	#endregion
	
		
	#region Public API
	
	public static void addGestureRecognizer( GKAbstractGestureRecognizer recognizer )
	{
		instance._gestureRecognizers.Add( recognizer );
	}
	
	
	public static void removeGestureRecognizer( GKAbstractGestureRecognizer recognizer )
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
