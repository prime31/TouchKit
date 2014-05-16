using UnityEngine;
using System.Collections;


/// <summary>
/// this class shows one way to setup a virtual control setup (d-pad and two buttons). It illustrates a bunch of concepts
/// including recognizer layering (overlapping "up" button to get diagonals) and also how to detect only in this frame
/// if a button is down (attack button).
/// </summary>
public class VirtualControls
{
	public bool leftDown, upDown, rightDown, attackDown, jumpDown;

	private float buttonWidth = 24f;
	private float buttonHeight = 35f;

	private TKAnyTouchRecognizer _leftRecognizer;
	private TKAnyTouchRecognizer _rightRecognizer;
	private TKAnyTouchRecognizer _upRecognizer;

	private TKButtonRecognizer _jumpRecognizer;
	private TKButtonRecognizer _attackRecognizer;


	public VirtualControls()
	{
		setupRecognizers();
	}


	public void update()
	{
		// reset our state
		leftDown = upDown = rightDown = attackDown = jumpDown = false;

		// first update our touches then use our recognizers to set our state
		TouchKit.updateTouches();

		leftDown = _leftRecognizer.state == TKGestureRecognizerState.RecognizedAndStillRecognizing;
		rightDown = _rightRecognizer.state == TKGestureRecognizerState.RecognizedAndStillRecognizing;
		upDown = _upRecognizer.state == TKGestureRecognizerState.RecognizedAndStillRecognizing;

		jumpDown = _jumpRecognizer.state == TKGestureRecognizerState.RecognizedAndStillRecognizing;
	}


	void setupRecognizers()
	{
		// left button
		_leftRecognizer = new TKAnyTouchRecognizer( new TKRect( 0f, 0f, buttonWidth, buttonHeight ) );
		TouchKit.addGestureRecognizer( _leftRecognizer );


		// right button
		_rightRecognizer = new TKAnyTouchRecognizer( new TKRect( 25f, 0f, buttonWidth, buttonHeight ) );
		TouchKit.addGestureRecognizer( _rightRecognizer );


		// up button
		_upRecognizer = new TKAnyTouchRecognizer( new TKRect( 0f, 25f, 49f, 30f ) );
		TouchKit.addGestureRecognizer( _upRecognizer );


		// jump button
		_jumpRecognizer = new TKButtonRecognizer( new TKRect( TouchKit.instance.designTimeResolution.x - buttonWidth, 0, buttonWidth, buttonHeight ), 0f );
		_jumpRecognizer.zIndex = 1;
		TouchKit.addGestureRecognizer( _jumpRecognizer );


		// attack button
		_attackRecognizer = new TKButtonRecognizer( new TKRect( TouchKit.instance.designTimeResolution.x - buttonWidth * 2f, 0, buttonWidth, buttonHeight ), 0f );
		_attackRecognizer.zIndex = 1;
		_attackRecognizer.onSelectedEvent += ( r ) =>
		{
			attackDown = true;
		};
		TouchKit.addGestureRecognizer( _attackRecognizer );
	}

}
