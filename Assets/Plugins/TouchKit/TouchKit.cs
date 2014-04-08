using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// TK touch state.
/// </summary>
public enum TKTouchState
{
	TouchBegin,     //when user first time touch an object with collider2d
	TouchMoveEnter, //when user move his/her finger int an object
	TouchMoveLeave, //when user leave his/her finger from an object but still touching
	TouchEnded,     //when user stop touching on an object
	TouchCanceled   //when system canceled touching event
}

public partial class TouchKit : MonoBehaviour
{
	public bool debugDrawBoundaryFrames = false;
	public bool autoScaleRectsAndDistances = true;

	/// <summary>
	/// all TKRect sizes should be based on this screen size. They will be adjusted at runtime if autoUpdateRects is true
	/// </summary>
	public Vector2 designTimeResolution = new Vector2( 320, 180 ); // 16:9 is a decent starting point for aspect ratio
	public int maxTouchesToProcess = 2;

	/// <summary>
	/// used at runtime to scale any TKRects as they are made for the current screen size
	/// </summary>
	public Vector2 runtimeScaleModifier { get; private set; }

	/// <summary>
	/// used at runtime to modify distances
	/// </summary>
	public float runtimeDistanceModifier { get; private set; }
	
	private List<TKAbstractGestureRecognizer> _gestureRecognizers = new List<TKAbstractGestureRecognizer>();
	private TKTouch[] _touchCache;
	private List<TKTouch> _liveTouches = new List<TKTouch>();
	private bool _shouldCheckForLostTouches = false; // used internally to ensure we dont check for lost touches too often

	//Touch event Detection
	private Dictionary<int,Dictionary<GameObject, TKTouchState>>  coveredObjects;
	public bool isTouchEventDetectionEnabled = true;
	
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
				_instance.runtimeScaleModifier = new Vector2( Screen.width / _instance.designTimeResolution.x, Screen.height / _instance.designTimeResolution.y );
				_instance.runtimeDistanceModifier = ( _instance.runtimeScaleModifier.x + _instance.runtimeScaleModifier.y ) / 2f;

				if( !_instance.autoScaleRectsAndDistances )
				{
					_instance.runtimeScaleModifier = Vector2.one;
					_instance.runtimeDistanceModifier = 1f;
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
		Destroy( gameObject );
	}
	
	
	private void Awake()
	{
		// prep our TKTouch cache so we avoid excessive allocations
		coveredObjects = new Dictionary<int, Dictionary<GameObject, TKTouchState>> ();

		_touchCache = new TKTouch[maxTouchesToProcess];
		for( int i = 0; i < maxTouchesToProcess; i++ )
			_touchCache[i] = new TKTouch( i );
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
			if(isTouchEventDetectionEnabled)
				DetectTouchEvent();

			foreach( var recognizer in _gestureRecognizers )
				recognizer.recognizeTouches( _liveTouches );

			_liveTouches.Clear();
		}
	}

	void DetectTouchEvent()
	{
		if(_liveTouches == null)
			return;

		foreach(TKTouch touch in _liveTouches)
		{
			if(touch.phase == TouchPhase.Began)
			{
				if(!coveredObjects.ContainsKey(touch.fingerId))
					coveredObjects.Add(touch.fingerId, new Dictionary<GameObject, TKTouchState>());

				var ray = Camera.main.ScreenToWorldPoint(touch.position);
				var hit = Physics2D.Raycast(ray, Vector2.zero);

				if(hit.collider != null)
				{
					var obj = hit.collider.gameObject;
					coveredObjects[touch.fingerId].Add(obj, TKTouchState.TouchBegin);
					obj.SendMessage(TKTouchState.TouchBegin.ToString(), touch, SendMessageOptions.DontRequireReceiver);
				}
			}
			else if(touch.phase == TouchPhase.Moved)
			{
				var ray = Camera.main.ScreenToWorldPoint(touch.position);
				var hit = Physics2D.Raycast(ray, Vector2.zero);

				Dictionary<GameObject, bool> hittedObjects = new Dictionary<GameObject, bool>();

				if(hit.collider != null)
				{
					hittedObjects.Add(hit.collider.gameObject, true);
				}

				var s = coveredObjects[touch.fingerId];
				GameObject[] keys = new GameObject[s.Keys.Count];
				s.Keys.CopyTo(keys, 0);
				foreach(var key in keys)
				{
					if(hittedObjects.ContainsKey(key))
					{
						if(s[key] != TKTouchState.TouchMoveEnter)
						{
							s[key] = TKTouchState.TouchMoveEnter;
							key.SendMessage(TKTouchState.TouchMoveEnter.ToString(), touch, SendMessageOptions.DontRequireReceiver);
						}
					}
					else
					{
						if(s[key] != TKTouchState.TouchMoveLeave)
						{
							s[key] = TKTouchState.TouchMoveLeave;
							key.SendMessage(TKTouchState.TouchMoveLeave.ToString(), touch, SendMessageOptions.DontRequireReceiver);
						}
					}

				}

				foreach(var key in hittedObjects.Keys)
				{
					if(!coveredObjects[touch.fingerId].ContainsKey(key))
					{
						coveredObjects[touch.fingerId].Add(key, TKTouchState.TouchMoveEnter);
						key.SendMessage(TKTouchState.TouchMoveEnter.ToString(), touch, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
			else if(touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
			{
				var ray = Camera.main.ScreenToWorldPoint(touch.position);
				var hit = Physics2D.Raycast(ray, Vector2.zero);
				
				Dictionary<GameObject, bool> hittedObjects = new Dictionary<GameObject, bool>();

				if(hit.collider != null)
				{
					hittedObjects.Add(hit.collider.gameObject, true);
				}

				foreach(var key in hittedObjects.Keys)
				{
					TKTouchState message = (touch.phase == TouchPhase.Ended) ? TKTouchState.TouchEnded : TKTouchState.TouchCanceled;
					key.SendMessage(message.ToString(), touch, SendMessageOptions.DontRequireReceiver);
				}

				var s = coveredObjects[touch.fingerId];
				foreach(var key in s.Keys)
				{
					if(!hittedObjects.ContainsKey(key))
					{
						key.SendMessage(TKTouchState.TouchCanceled.ToString(), touch, SendMessageOptions.DontRequireReceiver);
					}
				}

				coveredObjects[touch.fingerId].Clear();
			}
		}
	}

	#endregion
	
		
	#region Public API
	
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
