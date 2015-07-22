using UnityEngine;
using System.Collections;

public class DRect {
	public int top;
	public int bottom;
	public int left;
	public int right;

	public DRect() { }
	public DRect(int top, int bottom, int left, int right)
	{
		this.top = top;
		this.bottom = bottom;
		this.left = left;
		this.right = right;
	}
}
