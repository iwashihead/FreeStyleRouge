using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// マップ定義データのリスト
/// </summary>
public class RMapDefineData : ScriptableObject {
	public List<RMapDefine> world;
	public List<RMapDefine> town;
	public List<RMapDefine> dungeon;

	public RMapDefineData ()
	{
		hideFlags = HideFlags.None;

		if (world == null) world = new List<RMapDefine>();
		if (town == null) town = new List<RMapDefine>();
		if (dungeon == null) dungeon = new List<RMapDefine>();
	}
}
