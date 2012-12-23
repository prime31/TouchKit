using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class GestureKit : MonoBehaviour
{
	public bool debugDrawBoundaryFrames = false;
	public bool isRetina { get; private set; } // are we running on a retina device?
	public static bool autoUpdateRectsForRetina = true; // automatically doubles rect width/height
	public static int maxTouchesToProcess = 2;
	
	/// <summary>
	/// helper that will return 1 for non-retina and 2 for retina devices. useful for setting/modifying anything in screen coordinates.
	/// </summary>
	public int retinaMultiplier
	{
		get
		{
			if( isRetina && autoUpdateRectsForRetina )
				return 2;
			return 1;
		}
	}
	
	private List<GKAbstractGestureRecognizer> _gestureRecognizers = new List<GKAbstractGestureRecognizer>();
	private GKTouch[] _touchCache;
	private List<GKTouch> _liveTouches = new List<GKTouch>();
	private bool _shouldCheckForLostTouches = false; // used to ensure we dont check for lost touches too often
	
	
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
#if UNITY_IPHONE
		// check to see if we are on a retina device
		if( iPhone.generation == iPhoneGeneration.iPad3Gen || iPhone.generation == iPhoneGeneration.iPadUnknown || iPhone.generation == iPhoneGeneration.iPhone4
			|| iPhone.generation == iPhoneGeneration.iPhone4S || iPhone.generation == iPhoneGeneration.iPhone5 || iPhone.generation == iPhoneGeneration.iPodTouch4Gen
			|| iPhone.generation == iPhoneGeneration.iPodTouch5Gen || iPhone.generation == iPhoneGeneration.iPodTouchUnknown )
			isRetina = true;
#endif
		
		// prep our GKTouch cache so we avoid excessive allocations
		_touchCache = new GKTouch[maxTouchesToProcess];
		for( int i = 0; i < maxTouchesToProcess; i++ )
			_touchCache[i] = new GKTouch( i );
	}
	
	
	private void Update()
	{
		// the next couple sections are disgustingly, appallingly, horrendously horrible but it helps when testing in the editor
#if UNITY_EDITOR
		// check to see if the Unity Remote is active
		if( shouldProcessMouseInput() )
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

	#endregion
	
		
	#region Public API
	
	public static void addGestureRecognizer( GKAbstractGestureRecognizer recognizer )
	{
		// add, then sort and reverse so the higher zIndex items will be on top
		instance._gestureRecognizers.Add( recognizer );
		
		if( recognizer.zIndex > 0 )
		{
			_instance._gestureRecognizers.Sort();
			_instance._gestureRecognizers.Reverse();
		}
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
