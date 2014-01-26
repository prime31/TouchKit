using UnityEngine;
using System.Collections;



/// <summary>
/// replacement for the Unity Rect class that is TouchKit and resolution-aware. Creating one while on a retina device will automatically double all values
/// if TouchKit autoScaleRectsAndDistances is true
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

		x *= multiplier.x;
		y *= multiplier.y;
		width = width * multiplier.x;
		height = height * multiplier.y;
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
