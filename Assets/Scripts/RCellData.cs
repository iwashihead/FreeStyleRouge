using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// セルデータのリスト
/// </summary>
public class RCellData : ScriptableObject {
	public List<RCell> list = null;

	public RCellData ()
	{
		hideFlags = HideFlags.None;
		if (list == null) {
			list = new List<RCell>();
		}
	}
}
