using UnityEngine;
using System.Collections;


/// <summary>
/// this demo will create a virtual d-pad and two buttons. the virtual d-pad will have a left, right and up button. the up
/// button will overlap the left and right buttons. the reason for this is to show one way to handle 5 directional d-pad
/// (left, up-left, up, up-right, right) letting the player slide their finger around. It alsow demonstrates how the TKAnyTouchRecognizer
/// does not "eat" touches and allows them to bleed down through to recognizers below them.
///
/// note that we are working with a 16:9 design time resolution since that is the most popular on mobile and a good base.
/// </summary>
public class DemoTwo : MonoBehaviour
{
	private float buttonWidth = 24f;
	private float buttonHeight = 40f;

	void Start()
	{
		setupDpad();
		setupJumpButton();
		setupAttackButton();
	}


	void setupDpad()
	{
		// left button
		var recognizer = new TKAnyTouchRecognizer( new TKRect( 0f, 0f, buttonWidth, buttonHeight ) );
		recognizer.onEnteredEvent += ( r ) =>
		{
			Debug.Log( "left pressed: " + r );
		};
		recognizer.onExitedEvent += ( r ) =>
		{
			Debug.Log( "left released: " + r );
		};
		TouchKit.addGestureRecognizer( recognizer );


		// right button
		recognizer = new TKAnyTouchRecognizer( new TKRect( 25f, 0f, buttonWidth, buttonHeight ) );
		recognizer.onEnteredEvent += ( r ) =>
		{
			Debug.Log( "right pressed: " + r );
		};
		recognizer.onExitedEvent += ( r ) =>
		{
			Debug.Log( "right released: " + r );
		};
		TouchKit.addGestureRecognizer( recognizer );


		// up button
		recognizer = new TKAnyTouchRecognizer( new TKRect( 0f, 25f, 49f, 20f ) );
		recognizer.onEnteredEvent += ( r ) =>
		{
			Debug.Log( "up pressed: " + r );
		};
		recognizer.onExitedEvent += ( r ) =>
		{
			Debug.Log( "up released: " + r );
		};
		TouchKit.addGestureRecognizer( recognizer );
	}


	void setupJumpButton()
	{
		var recognizer = new TKButtonRecognizer( new TKRect( TouchKit.instance.designTimeResolution.x - buttonWidth, 0, buttonWidth, buttonHeight ), 0f );
		recognizer.zIndex = 1;
		recognizer.onSelectedEvent += ( r ) =>
		{
			Debug.Log( "jump pressed: " + r );
		};
		recognizer.onDeselectedEvent += ( r ) =>
		{
			Debug.Log( "jump released: " + r );
		};
		recognizer.onTouchUpInsideEvent += ( r ) =>
		{
			Debug.Log( "jump released: " + r );
		};
		TouchKit.addGestureRecognizer( recognizer );
	}


	void setupAttackButton()
	{
		var recognizer = new TKButtonRecognizer( new TKRect( TouchKit.instance.designTimeResolution.x - buttonWidth * 2f, 0, buttonWidth, buttonHeight ), 0f );
		recognizer.zIndex = 1;
		recognizer.onSelectedEvent += ( r ) =>
		{
			Debug.Log( "attack pressed: " + r );
		};
		recognizer.onDeselectedEvent += ( r ) =>
		{
			Debug.Log( "attack released: " + r );
		};
		recognizer.onTouchUpInsideEvent += ( r ) =>
		{
			Debug.Log( "attack released: " + r );
		};
		TouchKit.addGestureRecognizer( recognizer );
	}
}
