using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// セルのデータ
/// マップ上の1マス分の背景データ
/// </summary>
//[System.SerializableAttribute]
//[System.Serializable]
public class RCell : RObject {
	/// <summary>
	/// UVアニメーション1コマ分のスプライト数
	/// この数の倍数に満たない場合はアニメーションさせない
	/// </summary>
	private static readonly int TILE_SPRITE_NUM = 20;

	public int x;
	public int y;

	/// <summary>
	/// 地形名
	/// </summary>
	public string cellName;

	/// <summary>
	/// セルの種別ID IDが同じなら同種
	/// </summary>
	public int id;

	/// <summary>
	/// 簡単な説明、ゲーム中に表示されるかどうかは未定
	/// </summary>
	public string desc;

	/// <summary>
	/// 移動可能かどうか
	/// </summary>
	public bool isMoveable = true;

	/// <summary>
	/// 水の属性を持つかどうか、水辺でも移動可能などのスキルに影響
	/// </summary>
	public bool isWater = false;

	/// <summary>
	/// タイルマップに対応しているかどうか
	/// </summary>
	public bool isTileMap = false;

	/// <summary>
	/// UVアニメーション数 [最大でも3程度]
	/// アニメーションしない時は1以下
	/// </summary>
	public int uvAnimationNum = 1;

	/// <summary>
	/// UVアニメーション時間
	/// 1コマあたりの時間
	/// </summary>
	public float uiAnimationTime = 0.25f;

	// タイルマップ用
	// (タイルマップは1種類20のSpriteが必要 5コマ分だが、それをさらに4分割する必要があるため20)
	public List<Sprite> spriteList;


	private float timer;
	private int animIndex;

	/// <summary>
	/// 描画するレンダラーと表示している画像のindexをセットで管理する
	/// indexはtileMapのindexを意味する
	/// </summary>
	private List<Pair<SpriteRenderer, int>> renderers;

	public void SetSprite(int rendererIndex, int spriteIndex)
	{
		if (renderers == null || rendererIndex >= renderers.Count) return;
		if (isTileMap && (spriteList == null || spriteList.Count >= spriteIndex)) return;

		renderers[rendererIndex].First.sprite = spriteList[spriteIndex];
	}


	void Awake()
	{
		
	}

	void Start()
	{
		Initialize();
	}

	void Update()
	{
		
	}


	public void Initialize()
	{
		if (isTileMap) {
			// タイルマップの場合は子オブジェクトに4つのSpriteRendererを用意
			renderers = new List<Pair<SpriteRenderer, int>>();

			for (int i=0; i<4; i++)
			{
				GameObject go = new GameObject();
				go.name = "sprite0";
				SpriteRenderer ren = go.AddComponent<SpriteRenderer>();
				renderers.Add(new Pair<SpriteRenderer, int>(ren , -1));
				ren.transform.SetParent(transform);
				ren.transform.localScale = Vector3.one;
				switch (i)
				{
					case 0: ren.transform.localPosition = new Vector3(0f,	0.08f,	0f); break;
					case 1: ren.transform.localPosition = new Vector3(0.08f,0.08f,	0f); break;
					case 2: ren.transform.localPosition = new Vector3(0f,	0f,		0f); break;
					case 3: ren.transform.localPosition = new Vector3(0.08f,0f,		0f); break;
				}
			}

			if (spriteList == null || spriteList.Count < 20)
			{
				Debug.LogWarning("TileMap dont reach minimum size!");
			}
			else
			{
				bool upJoint		= ((RMap.Instance.GetCell(x, y-1)==null) ? id : RMap.Instance.GetCell(x, y-1).id) == id;
				bool downJoint		= ((RMap.Instance.GetCell(x, y+1)==null) ? id : RMap.Instance.GetCell(x, y+1).id) == id;
				bool rightJoint		= ((RMap.Instance.GetCell(x+1, y)==null) ? id : RMap.Instance.GetCell(x+1, y).id) == id;
				bool leftJoint		= ((RMap.Instance.GetCell(x-1, y)==null) ? id : RMap.Instance.GetCell(x-1, y).id) == id;
				bool upLeftJoint	= ((RMap.Instance.GetCell(x-1, y-1)==null) ? id : RMap.Instance.GetCell(x-1, y-1).id) == id;
				bool upRightJoint	= ((RMap.Instance.GetCell(x+1, y-1)==null) ? id : RMap.Instance.GetCell(x+1, y-1).id) == id;
				bool downLeftJoint	= ((RMap.Instance.GetCell(x-1, y+1)==null) ? id : RMap.Instance.GetCell(x-1, y+1).id) == id;
				bool downRightJoint	= ((RMap.Instance.GetCell(x+1, y+1)==null) ? id : RMap.Instance.GetCell(x+1, y+1).id) == id;

				// 左上の状態
				if (!upJoint && !leftJoint)									{ SetSprite(0,0); renderers[0].Second = 0; }
				else if (upJoint && !leftJoint)								{ SetSprite(0,4); renderers[0].Second = 4; }
				else if (!upJoint && leftJoint) 							{ SetSprite(0,8); renderers[0].Second = 8; }
				else if (upJoint && leftJoint && !upLeftJoint)				{ SetSprite(0,12); renderers[0].Second = 12; }
				else if (upJoint && leftJoint && upLeftJoint)				{ SetSprite(0,16); renderers[0].Second = 16; }
				else 														{ SetSprite(0,16); renderers[0].Second = 16; }

				// 右上の状態
				if (!upJoint && !rightJoint) 								{ SetSprite(1,1); renderers[1].Second = 1; }
				else if (upJoint && !rightJoint) 							{ SetSprite(1,5); renderers[1].Second = 5; }
				else if (!upJoint && rightJoint) 							{ SetSprite(1,9); renderers[1].Second = 9; }
				else if (upJoint && rightJoint && !upRightJoint)			{ SetSprite(1,13); renderers[1].Second = 13; }
				else if (upJoint && rightJoint && upRightJoint) 			{ SetSprite(1,17); renderers[1].Second = 17; }
				else 														{ SetSprite(1,17); renderers[1].Second = 17; }

				// 左下の状態
				if (!downJoint && !leftJoint) 								{ SetSprite(2,2); renderers[2].Second = 2; }
				else if (downJoint && !leftJoint) 							{ SetSprite(2,6); renderers[2].Second = 6; }
				else if (!downJoint && leftJoint) 							{ SetSprite(2,10); renderers[2].Second = 10; }
				else if (downJoint && leftJoint && !downLeftJoint) 			{ SetSprite(2,14); renderers[2].Second = 14; }
				else if (downJoint && leftJoint && downLeftJoint) 			{ SetSprite(2,18); renderers[2].Second = 18; }
				else 														{ SetSprite(2,18); renderers[2].Second = 18; }

				// 右下の状態
				if (!downJoint && !rightJoint)								{ SetSprite(3,3); renderers[3].Second = 3; }
				else if (downJoint && !rightJoint)							{ SetSprite(3,7); renderers[3].Second = 7; }
				else if (!downJoint && rightJoint) 							{ SetSprite(3,11); renderers[3].Second = 11; }
				else if (downJoint && rightJoint && !downRightJoint) 		{ SetSprite(3,15); renderers[3].Second = 15; }
				else if (downJoint && rightJoint && downRightJoint) 		{ SetSprite(3,19); renderers[3].Second = 19; }
				else 														{ SetSprite(3,19); renderers[3].Second = 19; }
			}
		}
		else {
			GameObject go = new GameObject();
			go.name = "sprite0";
			SpriteRenderer ren = go.AddComponent<SpriteRenderer>();
			renderers.Add(new Pair<SpriteRenderer, int>(ren, 0));
			ren.transform.SetParent(transform);
			ren.transform.localScale = Vector3.one;
			ren.transform.localPosition = new Vector3(0f, 0f, 0f);
			ren.sprite = spriteList[0];
		}

		// 共通の初期化処理
		timer = 0f;
		animIndex = 0;
	}

	public void Refresh()
	{
		if (isTileMap)
		{
			
			if (spriteList == null || spriteList.Count < 20)
			{
				Debug.LogWarning("TileMap dont reach minimum size!");
			}
			else
			{
				bool upJoint		= ((RMap.Instance.GetCell(x, y-1)==null) ? id : RMap.Instance.GetCell(x, y-1).id) == id;
				bool downJoint		= ((RMap.Instance.GetCell(x, y+1)==null) ? id : RMap.Instance.GetCell(x, y+1).id) == id;
				bool rightJoint		= ((RMap.Instance.GetCell(x+1, y)==null) ? id : RMap.Instance.GetCell(x+1, y).id) == id;
				bool leftJoint		= ((RMap.Instance.GetCell(x-1, y)==null) ? id : RMap.Instance.GetCell(x-1, y).id) == id;
				bool upLeftJoint	= ((RMap.Instance.GetCell(x-1, y-1)==null) ? id : RMap.Instance.GetCell(x-1, y-1).id) == id;
				bool upRightJoint	= ((RMap.Instance.GetCell(x+1, y-1)==null) ? id : RMap.Instance.GetCell(x+1, y-1).id) == id;
				bool downLeftJoint	= ((RMap.Instance.GetCell(x-1, y+1)==null) ? id : RMap.Instance.GetCell(x-1, y+1).id) == id;
				bool downRightJoint	= ((RMap.Instance.GetCell(x+1, y+1)==null) ? id : RMap.Instance.GetCell(x+1, y+1).id) == id;

				// 左上の状態
				if (!upJoint && !leftJoint)									{ SetSprite(0,0); renderers[0].Second = 0; }
				else if (upJoint && !leftJoint)								{ SetSprite(0,4); renderers[0].Second = 4; }
				else if (!upJoint && leftJoint) 							{ SetSprite(0,8); renderers[0].Second = 8; }
				else if (upJoint && leftJoint && !upLeftJoint)				{ SetSprite(0,12); renderers[0].Second = 12; }
				else if (upJoint && leftJoint && upLeftJoint)				{ SetSprite(0,16); renderers[0].Second = 16; }
				else 														{ SetSprite(0,16); renderers[0].Second = 16; }

				// 右上の状態
				if (!upJoint && !rightJoint) 								{ SetSprite(1,1); renderers[1].Second = 1; }
				else if (upJoint && !rightJoint) 							{ SetSprite(1,5); renderers[1].Second = 5; }
				else if (!upJoint && rightJoint) 							{ SetSprite(1,9); renderers[1].Second = 9; }
				else if (upJoint && rightJoint && !upRightJoint)			{ SetSprite(1,13); renderers[1].Second = 13; }
				else if (upJoint && rightJoint && upRightJoint) 			{ SetSprite(1,17); renderers[1].Second = 17; }
				else 														{ SetSprite(1,17); renderers[1].Second = 17; }

				// 左下の状態
				if (!downJoint && !leftJoint) 								{ SetSprite(2,2); renderers[2].Second = 2; }
				else if (downJoint && !leftJoint) 							{ SetSprite(2,6); renderers[2].Second = 6; }
				else if (!downJoint && leftJoint) 							{ SetSprite(2,10); renderers[2].Second = 10; }
				else if (downJoint && leftJoint && !downLeftJoint) 			{ SetSprite(2,14); renderers[2].Second = 14; }
				else if (downJoint && leftJoint && downLeftJoint) 			{ SetSprite(2,18); renderers[2].Second = 18; }
				else 														{ SetSprite(2,18); renderers[2].Second = 18; }

				// 右下の状態
				if (!downJoint && !rightJoint)								{ SetSprite(3,3); renderers[3].Second = 3; }
				else if (downJoint && !rightJoint)							{ SetSprite(3,7); renderers[3].Second = 7; }
				else if (!downJoint && rightJoint) 							{ SetSprite(3,11); renderers[3].Second = 11; }
				else if (downJoint && rightJoint && !downRightJoint) 		{ SetSprite(3,15); renderers[3].Second = 15; }
				else if (downJoint && rightJoint && downRightJoint) 		{ SetSprite(3,19); renderers[3].Second = 19; }
				else 														{ SetSprite(3,19); renderers[3].Second = 19; }
			}
		}
		else
		{
			SetSprite(0, 0);
		}
	}

	/// <summary>
	/// アニメーションを次のコマへ進める
	/// UVアニメーションの設定がされていない場合はなにもしない
	/// </summary>
	private void AnimationNext()
	{
		if (uvAnimationNum <= 1
			|| spriteList == null
			|| spriteList.Count <= 20
			|| renderers==null)
		{
			return;
		}

		// アニメーションの更新
		animIndex = (animIndex + 1) % uvAnimationNum;

		// 次のコマのアニメーションを行うために必要なSprite数に満たない場合はアニメーションしない
		if ((isTileMap && spriteList.Count < TILE_SPRITE_NUM * (animIndex+1))
			|| (!isTileMap && spriteList.Count < animIndex+1)) {
			Debug.LogWarning("Animation Sprite Not Enough!!");
			return;
		}

		for (int i=0; i<renderers.Count; i++)
		{
			SetSprite(i, renderers[i].Second + (isTileMap ? TILE_SPRITE_NUM : 1) * animIndex);
		}
	}
}
