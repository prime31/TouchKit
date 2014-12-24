using UnityEngine;
using System.Collections;


/// <summary>
/// this demo will create a virtual d-pad and two buttons. the virtual d-pad will have a left, right and up button. the up
/// button will overlap the left and right buttons. the reason for this is to show one way to handle 5 directional d-pad
/// (left, up-left, up, up-right, right) letting the player slide their finger around. It alsow demonstrates how the TKAnyTouchRecognizer
/// does not "eat" touches and allows them to bleed down through to recognizers below them.
///
/// note that we are working with a 16:9 design time resolution since that is the most popular on mobile and a good base. Note also
/// that we have set shouldAutoUpdateTouches to false and we are manually calling updateTouches. This lets us process the touches
/// exactly when we want: right before we use them.
/// </summary>
public class DemoTwo : MonoBehaviour
{
	private VirtualControls _controls;


	void Start()
	{
		_controls = new VirtualControls();
		_controls.createDebugQuads();
	}


	void Update()
	{
		_controls.update();
	}


	void OnGUI()
	{
		showLabelAndValue( "Left: ", _controls.leftDown.ToString() );
		showLabelAndValue( "Right: ", _controls.rightDown.ToString() );
		showLabelAndValue( "Up: ", _controls.upDown.ToString() );

		GUILayout.Space( 4 );

		showLabelAndValue( "Attack: ", _controls.attackDown.ToString() );
		showLabelAndValue( "Jump: ", _controls.jumpDown.ToString() );
	}


	void showLabelAndValue( string label, string value )
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label( label, GUILayout.Width( 50 ) );
			GUILayout.Label( value );
		}
		GUILayout.EndHorizontal();
	}

}
