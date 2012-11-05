using UnityEngine;
using System;
using System.Collections;
using System.Reflection;



#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN || UNITY_WEBPLAYER
public enum GKMouseState
{
	UpThisFrame,
	DownThisFrame,
	HeldDown
};


/// <summary>
/// this class now exists only to allow standalones/web players to create Touch objects
/// </summary>
public struct MouseToTouch
{
	public static Touch createTouchFromInput( GKMouseState mouseState, ref Vector2? lastMousePosition )
	{
		var self = new Touch();
		ValueType valueSelf = self;
		var type = typeof( Touch );
		
		var currentMousePosition = new Vector2( Input.mousePosition.x, Input.mousePosition.y );
		
		// if we have a lastMousePosition use it to get a delta
		if( lastMousePosition.HasValue )
			type.GetField( "m_PositionDelta", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, currentMousePosition - lastMousePosition );
		
		if( mouseState == GKMouseState.DownThisFrame ) // equivalent to touchBegan
		{
			type.GetField( "m_Phase", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, TouchPhase.Began );
			lastMousePosition = Input.mousePosition;
		}
		else if( mouseState == GKMouseState.UpThisFrame ) // equivalent to touchEnded
		{
			type.GetField( "m_Phase", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, TouchPhase.Ended );
			lastMousePosition = null;
		}
		else // UIMouseState.HeldDown - equivalent to touchMoved/Stationary
		{
			type.GetField( "m_Phase", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, TouchPhase.Moved );
			lastMousePosition = Input.mousePosition;
		}
		
		// this will always be one. not sure the best way to properly handle this cleanly yet
		type.GetField( "m_TapCount", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, 1 );
		type.GetField( "m_Position", BindingFlags.Instance | BindingFlags.NonPublic ).SetValue( valueSelf, currentMousePosition );
		
		return (Touch)valueSelf;
	}
}
#endif