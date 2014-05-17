#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor( typeof( TouchKit ) )]
public class TouchKitEditor : Editor
{
	private bool showDebug = true;
	private string status = "Touch Debugging";


	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		showDebug = EditorGUILayout.Foldout( showDebug, status );
		if( showDebug )
		{
			TouchKit touchKit = (TouchKit)target;
			touchKit.drawDebugBoundaryFrames = EditorGUILayout.Toggle( "Draw boundary frames", touchKit.drawDebugBoundaryFrames );
			touchKit.drawTouches = EditorGUILayout.Toggle( "Draw touches", touchKit.drawTouches );
			touchKit.simulateTouches = EditorGUILayout.Toggle( "Simulate touches", touchKit.simulateTouches );

			GUI.enabled = touchKit.simulateTouches;
			if( GUI.enabled || true )
			{
				var helpText = "Touches can be simulated in the editor or on the desktop with mouse clicks.";
				if( touchKit.simulateMultitouch )
					helpText += "\nTo simulate a two-finger gesture, press and hold the left alt key and move your mouse around. Shift the touches by additionally holding down left shift.";

				EditorGUILayout.HelpBox( helpText, MessageType.Info, true );
			}

			touchKit.simulateMultitouch = EditorGUILayout.Toggle( "Simulate multitouch", touchKit.simulateMultitouch );
		}

		if( GUI.changed )
			EditorUtility.SetDirty( target );
	}

}
#endif