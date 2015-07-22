using UnityEngine;
using System.Collections;

[System.Serializable]
public class RMapDefine {
	public string name;
	public MapType type;

	public RCell ground; 	// 地面
	public RCell wall;		// 壁
	public RCell water;		// 水辺・水路
	public RCell road;		// 道
	public RCell house;		// 家
	public RCell castle;	// 城
	public RCell gate;		// 柵
}

public enum MapType
{
	World,
	Town,
	Dungeon,
}