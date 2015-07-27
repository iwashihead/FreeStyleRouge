using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// <para>部屋とその領域のデータクラス</para>
/// <para>例）白:壁 黒:通路</para>
/// <para>□ □ □ □ □ □ □ □ □</para>
/// <para>□ □ ■ ■ ■ ■ ■ □ □</para>
/// <para>■ ■ ■ ■ ■ ■ ■ □ □</para>
/// <para>□ □ ■ ■ ■ ■ ■ □ □</para>
/// <para>□ □ ■ ■ ■ ■ ■ ■ ■</para>
/// <para>□ □ ■ ■ ■ ■ ■ □ □</para>
/// <para>□ □ □ □ □ □ □ □ □</para>
/// <para></para>
/// <para>この部屋のパラメタはそれぞれこうなる(この領域がマップ全体の左上位置とした場合)</para>
/// <para>top : 0 </para>
/// <para>bottom : 6</para>
/// <para>left : 0</para>
/// <para>right : 8</para>
/// <para>roomTop : 1</para>
/// <para>roomLeft : 2</para>
/// <para>roomWidth : 5</para>
/// <para>roomHeight : 5</para>
/// <para>topRoad : -1 (部屋の上側に道はないので-1)</para>
/// <para>bottomRoad : -1 (部屋の下側に道はないので-1)</para>
/// <para>leftRoad : 2</para>
/// <para>rightRoad : 4</para>
/// </summary>
public class DArea {
	/// <summary>領域の上位置</summary>
	public int top;
	/// <summary>領域の下位置</summary>
	public int bottom;
	/// <summary>領域の左位置</summary>
	public int left;
	/// <summary>領域の右位置</summary>
	public int right;


	/// <summary>部屋の上位置</summary>
	public int roomTop;
	/// <summary>部屋の左位置</summary>
	public int roomLeft;
	/// <summary>部屋の横幅</summary>
	public int roomWidth;
	/// <summary>部屋の縦幅</summary>
	public int roomHeight;


	/// <summary>部屋の上側への道の位置(部屋ではなく領域の上からの位置) 道を作成していない場合は-1</summary>
	public int topRoad;
	/// <summary>部屋の下側への道の位置(部屋ではなく領域の上からの位置) 道を作成していない場合は-1</summary>
	public int bottomRoad;
	/// <summary>部屋の左側への道の位置(部屋ではなく領域の左からの位置) 道を作成していない場合は-1</summary>
	public int leftRoad;
	/// <summary>部屋の右側への道の位置(部屋ではなく領域の左からの位置) 道を作成していない場合は-1</summary>
	public int rightRoad;

	/// <summary>
	/// 通路で繋がれた部屋
	/// </summary>
	public List<DArea> connectedArea = new List<DArea>();

	/// <summary>
	/// 横幅
	/// </summary>
	public int Width {
		get { return Mathf.Abs(right - left); }
	}

	/// <summary>
	/// 縦幅
	/// </summary>
	public int Height {
		get { return Mathf.Abs(bottom - top); }
	}

	/// <summary>
	/// 部屋に通っている道の本数
	/// </summary>
	public int RoadCount {
		get { return topRoad + bottomRoad + leftRoad + rightRoad; }
	}

	public override string ToString ()
	{
		return string.Format ("[DRect: Width={0}, Height={1}, top={2}, bottom={3}, , left={4}, right={5} topRoad={6}, bottomRoad={7}, leftRoad={8}, rightRoad={9}]",
			Width, Height, top, bottom, left, right, topRoad, bottomRoad, leftRoad, rightRoad);
	}

	public DArea() { }
	public DArea(int left, int top, int right, int bottom)
	{
		this.top = top;
		this.bottom = bottom;
		this.left = left;
		this.right = right;

		topRoad = -1;
		bottomRoad = -1;
		leftRoad = -1;
		rightRoad = -1;
	}

	/// <summary>
	/// 連結グループを全て取得する
	/// </summary>
	public List<DArea> ConnectGroup
	{
		get {
			List<DArea> connectGroup = new List<DArea>();
			// 再帰的にすべて取得
			GetRecursive( ref connectGroup, this );
			return connectGroup;
		}
	}

	void GetRecursive( ref List<DArea> connectGroup, DArea area )
	{
		if (connectGroup.Contains(area) == false) {
			connectGroup.Add( area );
			foreach (DArea a in area.connectedArea) {
				GetRecursive( ref connectGroup, a);
			}
		}
	}


	/// <summary>
	/// 指定した方向に隣接する部屋のリストを取得する
	/// </summary>
	/// <returns>隣接する部屋リスト.</returns>
	/// <param name="areas">部屋のリスト.</param>
	/// <param name="area">元となる部屋.</param>
	/// <param name="direction">隣接方向</param>
	public static List<DArea> GetNextAreas(List<DArea> areas, DArea area, RDirection4 direction)
	{
		if (areas == null || area == null || areas.Count <= 1) {
			return new List<DArea>();
		}

		List<DArea> ret = new List<DArea>();

		for (int i=0; i<areas.Count; i++)
		{
			if (areas[i]==area) continue;// 元部屋は無視

			// 位置が同じ部屋を返す
			switch (direction)
			{
			case RDirection4.up:
				if (area.top == areas[i].bottom)
					if (!(area.left > areas[i].right && area.right < areas[i].left))
						ret.Add(areas[i]);
				break;
			case RDirection4.right:
				if (area.right == areas[i].left)
					if (!(area.top > areas[i].bottom && area.bottom < areas[i].top))
						ret.Add(areas[i]);
				break;
			case RDirection4.down:
				if (area.bottom == areas[i].top)
					if (!(area.left > areas[i].right && area.right < areas[i].left))
						ret.Add(areas[i]);
				break;
			case RDirection4.left:
				if (area.left == areas[i].right) 
					if (!(area.top > areas[i].bottom && area.bottom < areas[i].top))
						ret.Add(areas[i]);
				break;
			case RDirection4.ALL:
				if ((area.top == areas[i].bottom) || (area.bottom == areas[i].top))
				{
					if (area.right < areas[i].left || area.left > areas[i].right) {
					}
					else {
						ret.Add(areas[i]);
					}
				}

				if ((area.right == areas[i].left) || (area.left == areas[i].right))
				{
					if (area.top > areas[i].bottom || area.bottom < areas[i].top) {
					}
					else {
						ret.Add(areas[i]);
					}
				}
				break;
			case RDirection4.None:
				return ret;
			}
		}

		return ret;
	}
}
