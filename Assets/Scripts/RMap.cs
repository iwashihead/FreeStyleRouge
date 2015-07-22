using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// マップの基底クラス
/// ワールドマップ
/// タウンマップ
/// ダンジョンマップ
/// 
/// などへ継承していく予定
/// </summary>
public class RMap : SingletonObject<RMap> {

	public int width;
	public int height;

	public RCell[,] cells;

	void Awake () {
	}

	// Use this for initialization
	void Start () {
		Initialize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	public void Initialize()
	{
		cells = new RCell[width, height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				GameObject go = new GameObject(string.Format("Cell ({0},{1})", x, y));
				cells[x, y] = go.AddComponent<RCell>();
				cells[x, y].x = x;
				cells[x, y].y = y;

				// 位置を決めたりする ここから＞このコードはGenerateへもってく
			}
		}
	}

	public RCell[,] Generate(int seed)
	{
		RCell[,] ret = new RCell[width, height];

		// 不思議のダンジョン風
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
//				ret[x, y] = ;
			}
		}


		return ret;
	}

	public RCell GetCell(int x, int y) {
		if (cells == null) {
			Debug.LogError("Not Generated!");
			return null;
		}
		if (x < 0 || x >= cells.GetLength(0) || y < 0 || y >= cells.GetLength(0)) {
			return null;
		}
		return cells[x, y];
	}
}