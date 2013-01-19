using UnityEngine;
using System.Collections;



/// <summary>
/// replacement for the Unity Rect class that is GestureKit and resolution-aware. Creating one while on a retina device will automatically double all values
/// if GestureKit.autoUpdateRectsForRetina is true and GestureKit.isRetina is true.
/// </summary>
public struct TKRect
{
	public float x;
	public float y;
	public float width;
	public float height;
	
	public float xMin { get { return x; } }
	public float xMax { get { return x + width; } }
	public float yMin { get { return y; } }
	public float yMax { get { return y + height; } }
	
	
	public TKRect( float x, float y, float width, float height )
	{
		var multiplier = TouchKit.instance.retinaMultiplier;
		
		this.x = x * multiplier;
		this.y = y * multiplier;
		this.width = width * multiplier;
		this.height = height * multiplier;
	}
	
	
	public TKRect copyWithExpansion( float allSidesExpansion )
	{
		allSidesExpansion *= TouchKit.instance.retinaMultiplier;
		return copyWithExpansion( allSidesExpansion, allSidesExpansion );
	}
	
	
	public TKRect copyWithExpansion( float xExpansion, float yExpansion )
	{
		xExpansion *= TouchKit.instance.retinaMultiplier;
		yExpansion *= TouchKit.instance.retinaMultiplier;
		return new TKRect( x - xExpansion, y - yExpansion, width + ( xExpansion + yExpansion ), height + ( xExpansion + yExpansion ) );
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
