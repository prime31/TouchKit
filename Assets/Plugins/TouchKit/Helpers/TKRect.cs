using UnityEngine;
using System.Collections;



/// <summary>
/// replacement for the Unity Rect class that is TouchKit and resolution-aware. Creating one while on a retina device will automatically double all values
/// if TouchKit autoUpdateRects is true
/// </summary>
public struct TKRect
{
	public TKAnchor anchor;

	public float x;
	public float y;
	public float width;
	public float height;
	
	public float xMin { get { return x; } }
	public float xMax { get { return x + width; } }
	public float yMin { get { return y; } }
	public float yMax { get { return y + height; } }
	public Vector2 center { get { return new Vector2( x + ( width / 2f ), y + ( height / 2f ) ); } }
	
	
	public TKRect( float x, float y, float width, float height ) : this( x, y, width, height, TKAnchor.Center )
	{}


	public TKRect( float x, float y, float width, float height, TKAnchor anchor )
	{
		this.anchor = anchor;
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;

		updateRectWithRuntimeScaleModifier();
	}


	private void updateRectWithRuntimeScaleModifier()
	{
		var multiplier = TouchKit.instance.runtimeScaleModifier;

		var newWidth = width * multiplier.x;
		var newHeight = height * multiplier.y;

		Debug.Log( "multiplier: " + multiplier );
		Debug.Log( "old center: " + center );
		Debug.Log( "old y: " + y );
		Debug.Log( "old yMAX: " + yMax );

		// x and y vary based on our anchor
		switch( anchor )
		{
			case TKAnchor.TopLeft:
			case TKAnchor.MiddleLeft:
			case TKAnchor.BottomLeft:
				x *= multiplier.x;
				break;

			case TKAnchor.TopCenter:
			case TKAnchor.Center:
			case TKAnchor.BottomCenter:
				x *= multiplier.x;
				break;

			case TKAnchor.TopRight:
			case TKAnchor.MiddleRight:
			case TKAnchor.BottomRight:
				x *= multiplier.x;
				break;
		}

		switch( anchor )
		{
			case TKAnchor.TopLeft:
			case TKAnchor.TopCenter:
			case TKAnchor.TopRight:
				y *= multiplier.y;
				break;
			
			case TKAnchor.MiddleLeft:
			case TKAnchor.Center:
			case TKAnchor.MiddleRight:
				y *= multiplier.y;
				break;

			case TKAnchor.BottomLeft:
			case TKAnchor.BottomCenter:
			case TKAnchor.BottomRight:
				y *= multiplier.y;
				break;
		}

		// width and height are always just directly modified
		width = newWidth;
		height = newHeight;

		Debug.Log( "new center: " + center );
		Debug.Log( "screen center: " + Screen.width / 2f + ", " + Screen.height / 2f );
		Debug.Log( "new y: " + y );
		Debug.Log( "new yMAX: " + yMax );
	}
	
	
	public TKRect copyWithExpansion( float allSidesExpansion )
	{
		return copyWithExpansion( allSidesExpansion, allSidesExpansion );
	}
	
	
	public TKRect copyWithExpansion( float xExpansion, float yExpansion )
	{
		xExpansion *= TouchKit.instance.runtimeScaleModifier.x;
		yExpansion *= TouchKit.instance.runtimeScaleModifier.y;

		var rect = new TKRect();
		rect.x = this.x - xExpansion;
		rect.y = this.y - yExpansion;
		rect.width = this.width + ( xExpansion * 2f );
		rect.height = this.height + ( yExpansion * 2f );

		return rect;
		//TKRect( x - xExpansion, y - yExpansion, width + ( xExpansion + yExpansion ), height + ( xExpansion + yExpansion ) );
	}
	
	
	public bool contains( Vector2 point )
	{
		if( x <= point.x && y <= point.y && xMax >= point.x && yMax >= point.y )
			return true;
		return false;
	}
	
	
	public override string ToString()
	{
		return string.Format( "TKRect: x: {0}, xMax: {1}, y: {2}, yMax: {3}, width: {4}, height: {5}", x, xMax, y, yMax, width, height );
	}
	
}
