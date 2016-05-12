using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public partial class TouchKit : MonoBehaviour
{
	[HideInInspector]
	public bool simulateTouches = true;
	[HideInInspector]
	public bool simulateMultitouch = true;
	[HideInInspector]
	public bool drawTouches = false;
	[HideInInspector]
	public bool drawDebugBoundaryFrames = false;

	/// <summary>
	/// lets TouchKit know if it should scale all rects and distances based on the designTimeResolution
	/// </summary>
	public bool autoScaleRectsAndDistances = true;

	/// <summary>
	/// if false, TouchKit will not do anything in Update. You will need to call updateTouches yourself.
	/// </summary>
	public bool shouldAutoUpdateTouches = true;

	private Vector2 _designTimeResolution = new Vector2( 320, 180 ); // 16:9 is a decent starting point for aspect ratio
	/// <summary>
	/// all TKRect sizes should be based on this screen size. They will be adjusted at runtime if autoUpdateRects is true
	/// </summary>
	public Vector2 designTimeResolution { 
		get {
			return _designTimeResolution;
		} 
		set {
			_designTimeResolution = value;
			setupRuntimeScale();
		}	
	}
	public int maxTouchesToProcess = 2;

	/// <summary>
	/// used at runtime to scale any TKRects as they are made for the current screen size
	/// </summary>
	public Vector2 runtimeScaleModifier { get; private set; }

	/// <summary>
	/// used at runtime to modify distances
	/// </summary>
	public float runtimeDistanceModifier { get; private set; }

	/// <summary>
	/// used at runtime to translate a pixel value into Unity units. Just multiply the pixel value by the pixelsToUnityUnitsMultiplier.
	/// Note that this will only work for orthographic cameras!
	/// </summary>
	public Vector2 pixelsToUnityUnitsMultiplier { get; private set; }


	private List<TKAbstractGestureRecognizer> _gestureRecognizers = new List<TKAbstractGestureRecognizer>( 5 );
	private TKTouch[] _touchCache;
	private List<TKTouch> _liveTouches = new List<TKTouch>( 2 );
	private bool _shouldCheckForLostTouches = false; // used internally to ensure we dont check for lost touches too often

	private const float inchesToCentimeters = 2.54f;

	public float ScreenPixelsPerCm
	{
		get
		{
			float fallbackDpi = 72f;
			#if UNITY_ANDROID
				// Android MDPI setting fallback
				// http://developer.android.com/guide/practices/screens_support.html
				fallbackDpi = 160f;
			#elif (UNITY_WP8 || UNITY_WP8_1 || UNITY_WSA || UNITY_WSA_8_0)
				// Windows phone is harder to track down
				// http://www.windowscentral.com/higher-resolution-support-windows-phone-7-dpi-262
				fallbackDpi = 92f;
			#elif UNITY_IOS
				// iPhone 4-6 range
				fallbackDpi = 326f;
			#endif

			return Screen.dpi == 0f ? fallbackDpi / inchesToCentimeters : Screen.dpi / inchesToCentimeters;
		}
	}

	private static TouchKit _instance = null;
	public static TouchKit instance
	{
		get
		{
			if( !_instance )
			{
				// check if there is a GO instance already available in the scene graph
				_instance = FindObjectOfType( typeof( TouchKit ) ) as TouchKit;

				// nope, create a new one
				if( !_instance )
				{
					var obj = new GameObject( "TouchKit" );
					_instance = obj.AddComponent<TouchKit>();
					DontDestroyOnLoad( obj );
				}

				// prep the scalers. for the distance scaler we just use an average of the width and height scales
				var aCamera = Camera.main ?? Camera.allCameras[0];
				if( aCamera.orthographic )
				{
					setupPixelsToUnityUnitsMultiplierWithCamera( aCamera );
				}
				else
				{
					_instance.pixelsToUnityUnitsMultiplier = Vector2.one;
				}

				_instance.setupRuntimeScale();
			}

			return _instance;
		}
	}

	protected void setupRuntimeScale()
	{
		_instance.runtimeScaleModifier = new Vector2( Screen.width / _instance.designTimeResolution.x, Screen.height / _instance.designTimeResolution.y );
		_instance.runtimeDistanceModifier = ( _instance.runtimeScaleModifier.x + _instance.runtimeScaleModifier.y ) / 2f;

		if( !_instance.autoScaleRectsAndDistances )
		{
			_instance.runtimeScaleModifier = Vector2.one;
			_instance.runtimeDistanceModifier = 1f;
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


	private void internalUpdateTouches()
	{
		// the next couple sections are disgustingly, appallingly, horrendously horrible due to all the #ifs but it
		// helps when testing in the editor
		#if UNITY_EDITOR
		// check to see if the Unity Remote is active
		if( shouldProcessMouseInput() )
		{
		#endif

			#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER || UNITY_WEBGL

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
			for( var i = 0; i < _gestureRecognizers.Count; i++ )
				_gestureRecognizers[i].recognizeTouches( _liveTouches );
			_liveTouches.Clear();
		}
	}


	#region MonoBehaviour

	private void Awake()
	{
		// prep our TKTouch cache so we avoid excessive allocations
		_touchCache = new TKTouch[maxTouchesToProcess];
		for( int i = 0; i < maxTouchesToProcess; i++ )
			_touchCache[i] = new TKTouch( i );
	}


	private void Update()
	{
		if( shouldAutoUpdateTouches )
			internalUpdateTouches();
	}


	private void OnApplicationQuit()
	{
		_instance = null;
		Destroy( gameObject );
	}

	#endregion


	#region Public API

	/// <summary>
	/// TouchKit will attempt to use the first camera to set this up. If that is not the camera you would like used
	/// just call this method with your HUD (or standard orthographic) camera.
	/// </summary>
	/// <param name="cam">Cam.</param>
	public static void setupPixelsToUnityUnitsMultiplierWithCamera( Camera cam )
	{
		if( !cam.orthographic )
		{
			Debug.LogError( "Attempting to setup unity pixel-to-units modifier with a non-orthographic camera" );
			return;
		}

		var screenSizeUnityUnits = new Vector2( cam.aspect * cam.orthographicSize * 2f, cam.orthographicSize * 2f );
		_instance.pixelsToUnityUnitsMultiplier = new Vector2
		(
			screenSizeUnityUnits.x / (float)Screen.width,
			screenSizeUnityUnits.y / (float)Screen.height
		);
	}


	public static void updateTouches()
	{
        if( _instance == null )
        	return;

        _instance.internalUpdateTouches();
	}


	public static void addGestureRecognizer( TKAbstractGestureRecognizer recognizer )
	{
		// add, then sort and reverse so the higher zIndex items will be on top
		instance._gestureRecognizers.Add( recognizer );

		if( recognizer.zIndex > 0 )
		{
			_instance._gestureRecognizers.Sort();
			_instance._gestureRecognizers.Reverse();
		}
	}


	public static void removeGestureRecognizer( TKAbstractGestureRecognizer recognizer )
	{
		if( _instance == null )
			return;

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
        if( _instance == null )
        	return;

		instance._gestureRecognizers.Clear();
	}

	#endregion

}
