using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;



/// <summary>
/// マップの基底クラス
/// ワールドマップ
/// タウンマップ
/// ダンジョンマップ
/// 
/// などへ継承していく予定
/// </summary>
public class RMap : SingletonObject<RMap> {
	public enum MapType
	{
		World,
		Dungeon,
		Town,
	}

	public MapType mapType;
	public int width;
	public int height;

	public RCell[,] cells;
	public List<DArea> areas;

	/// <summary>
	/// ダンジョンマップの種類
	/// </summary>
	public enum DungeonType
	{
		Normal = 0,// 通常の不思議のダンジョン風
		SingleRoom,// 大部屋

		MAX
	}

	/// <summary>
	/// タウンマップの種類
	/// </summary>
	public enum TownType
	{
		Normal = 0,

		MAX
	}

	/// <summary>
	/// ワールドマップの種類
	/// </summary>
	public enum WorldType
	{
		Normal = 0,

		MAX
	}

	/// <summary>
	/// セルのタイプ、壁とか道とか
	/// </summary>
	public enum CellType
	{
		WALL = 99,
		ROAD_1 = 0,
		ROAD_2 = 1,

		WATER_1 = 10,
		WATER_2 = 11,

	}

	/// <summary>
	/// 領域の最低幅数 縦横共通
	/// </summary>
	static readonly int AREA_MIN_SIZE = 8;

	/// <summary>
	/// 領域の最低個数
	/// </summary>
	static readonly int AREA_MIN_NUM = 3;

	/// <summary>
	/// 部屋数が最低限に達している時に分割を行わない可能性(0..9)
	/// 数値　* 10% の確率で分割しなくなります
	/// 例) 3なら30%の確率で分割しない
	/// </summary>
	static readonly int AREA_DONT_SPLIT = 6;

	/// <summary>
	/// 部屋の最低幅数 縦横共通
	/// </summary>
	static readonly int ROOM_MIN_SIZE = 5;

	/// <summary>
	/// 部屋に道を繋ぐ確率（0..100）%
	/// </summary>
	static readonly int ROOM_CONNECT_PROB = 10;


	void Awake () {
	}

	// Use this for initialization
	void Start () {
		Initialize();
	}
	
	// Update is called once per frame
	void Update () {
		// テスト用
		if (Input.GetKeyDown(KeyCode.G)) {// GenerateのGだよ！
			cells = Generate((int)System.DateTime.Now.Ticks, mapType);
		}
	}


	public void Initialize()
	{
		cells = Generate(DataManager.Instance.seed, mapType/*DataManagerのどこかを見るようにいづれ変更*/);
	}

	public RCell[,] Generate(int seed, MapType type)
	{
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
		sw.Start();

		// ランダムの初期化
		Random.seed = seed;
		mapType = type;
		Debug.Log("UnityRandom set seed = " + seed);

		RCell[,] ret = new RCell[width, height];
		int[,] _cell = new int[width, height];

		// 部屋タイプ設定
		int roomType = 0;
		switch (mapType)
		{
			case MapType.World:
				roomType = Random.Range(0, (int)WorldType.MAX + 1); break;
			case MapType.Town:
				roomType = Random.Range(0, (int)TownType.MAX + 1); break;
			case MapType.Dungeon:
				roomType = Random.Range(0, (int)DungeonType.MAX + 1); break;
		}


		// 不思議のダンジョン風
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				_cell[x, y] = (int)CellType.WALL;
			}
		}

		DArea rect = new DArea(0, 0, width, height);
		areas = new List<DArea>();

		// 1.部屋の分割
		SplitRoom(rect, true);

		int roomNo = 1;
		foreach (DArea area in areas)
		{
			for (int x=area.left; x<area.right; x++)
			{
				for (int y=area.top; y<area.bottom; y++)
				{
					// 範囲外チェック
					if (x < 0 || x >= _cell.GetLength(0) || y < 0 || y >= _cell.GetLength(1)) continue;

					_cell[x, y] = roomNo;
				}
			}
			roomNo++;
		}

		// 2.部屋の作成
		CreateRoom(ref _cell);

		Check(_cell);//デバッグ表示

		// 3.部屋同士を道で繋ぐ
		CreateRoad(ref _cell);

		Check(_cell);//デバッグ表示

		// 4.アイテムの生成

		// 5.プレイヤーの生成

		sw.Stop();
		Debug.Log("generate ms : " + (sw.ElapsedMilliseconds));

		return ret;
	}

	/// <summary>
	/// デバッグ用 ログ表示
	/// </summary>
	void Check(int[,] _cell)
	{
		string dbg = "";

		if (areas != null)
		{
			for (int i=0; i<areas.Count; i++) {
				int start_y		= areas[i].roomTop;
				int end_y		= start_y		+ areas[i].roomHeight;
				int start_x		= areas[i].roomLeft;
				int end_x		= start_x		+ areas[i].roomWidth;
				for (int y = start_y; y < end_y; y++)
				{
					for (int x = start_x; x < end_x; x++)
					{
						_cell[x, y] = (int)CellType.ROAD_1;
					}
				}
			}
		}

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				if (_cell[x, y] == (int)CellType.ROAD_1)
				{
					dbg += "|_";
				}
				else
				{
					dbg += _cell[x, y].ToString() + " ";
				}
			}
			dbg += "\n";
		}
		Debug.LogWarning(dbg);
	}

	/// <summary>
	/// すべての部屋を道で繋ぐ
	/// </summary>
	void CreateRoad(ref int[,] cells)
	{
		// 開始部屋をランダムに選ぶ
		DArea first = areas[ Random.Range(0, areas.Count) ];

		for (int i=0; i<areas.Count-1; i++)
		{
			if (RoadGen(areas[i], areas[i+1], ref cells))
			{
				// 隣接していなくて道の生成に失敗した
				List<DArea> nexts = DArea.GetNextAreas(areas, areas[i], RDirection4.ALL);

				if (nexts.Count > 0) {
					RoadGen(areas[i], nexts[Random.Range(0, nexts.Count)], ref cells);
				}
			}
		}

		// 繋がっていない部屋が無いか調べ、全ての部屋が繋がるよう道を生成する
		List<DArea> connectGroup = areas[0].ConnectGroup;
		while (areas.Count != connectGroup.Count)
		{
			if (areas.Count != connectGroup.Count)
			{
				List<DArea> diff = new List<DArea>(areas);
				foreach (DArea a in connectGroup)
				{
					diff.Remove(a);
				}

				foreach (DArea a in diff)
				{
					List<DArea> nexts = DArea.GetNextAreas(areas, a, RDirection4.ALL);

					if (nexts.Count > 0) {
						RoadGen(a, nexts[Random.Range(0, nexts.Count)], ref cells);
					}
				}
			}
			connectGroup = areas[0].ConnectGroup;
		}
	}

	/// <summary>
	/// 部屋と部屋を道で繋ぐ
	/// </summary>
	/// <returns><c>true</c>道の生成に失敗した場合はtrueを返す<c>false</c>道の生成に成功した場合はfalseを返す.</returns>
	/// <param name="frm">道を繋ぐ元の部屋.</param>
	/// <param name="to">道を繋ぐ先の部屋.</param>
	/// <param name="cells">セルデータ.</param>
	bool RoadGen(DArea frm, DArea to, ref int[,] cells)
	{
		if (frm.top == to.bottom)
		{// 上方向

			// 下の部屋から上へ道を繋ぐ
			if (frm.topRoad < 0) {
				frm.topRoad = Random.Range(frm.roomLeft, frm.roomLeft + frm.roomWidth);
			}
			int frmPos = frm.topRoad;
			
			for (int y = frm.top; y<frm.roomTop; y++)
			{
				cells[frmPos, y] = (int)CellType.ROAD_1;
			}

			// 上の部屋から下へ道を繋ぐ
			if (to.bottomRoad < 0) {
				to.bottomRoad = Random.Range(to.roomLeft, to.roomLeft + to.roomWidth);
			}
			int toPos = to.bottomRoad;

			for (int y = to.roomTop+to.roomHeight; y<to.bottom; y++)
			{
				cells[toPos, y] = (int)CellType.ROAD_1;
			}

			// 最後に横に繋ぐ
			int lower = Mathf.Min(frmPos, toPos);
			int bigger = Mathf.Max(frmPos, toPos);
			for (int x = lower; x <= bigger; x++)
			{
				cells[x, to.bottom-1] = (int)CellType.ROAD_1;
			}
		}
		else if (frm.right == to.left)
		{// 右方向

			// 左の部屋から右へ道を繋ぐ

			if (frm.rightRoad < 0) {
				frm.rightRoad = Random.Range(frm.roomTop, frm.roomTop + frm.roomHeight);
			}
			int frmPos = frm.rightRoad;

			for (int x = frm.roomLeft+frm.roomWidth; x<frm.right; x++)
			{
				cells[x, frmPos] = (int)CellType.ROAD_1;
			}

			// 右の部屋から左へ道を繋ぐ
			if (to.leftRoad < 0) {
				to.leftRoad = Random.Range(to.roomTop, to.roomTop + to.roomHeight);
			}
			int toPos = to.leftRoad;

			for (int x = to.left; x <= to.roomLeft; x++)
			{
				cells[x, toPos] = (int)CellType.ROAD_1;
			}

			// 最後に横に繋ぐ
			int lower = Mathf.Min(frmPos, toPos);
			int bigger = Mathf.Max(frmPos, toPos);
			for (int y = lower; y <= bigger; y++)
			{
				cells[frm.right-1, y] = (int)CellType.ROAD_1;
			}
		}
		else if (frm.bottom == to.top)
		{// 下方向

			// 上の部屋から下へ道を繋ぐ
			if (frm.bottomRoad < 0) {
				frm.bottomRoad = Random.Range(frm.roomLeft, frm.roomLeft + frm.roomWidth);
			}
			int frmPos = frm.bottomRoad;

			for (int y = frm.roomTop+frm.roomHeight; y<frm.bottom; y++)
			{
				cells[frmPos, y] = (int)CellType.ROAD_1;
			}

			// 下の部屋から上へ道を繋ぐ
			if (to.topRoad < 0) {
				to.topRoad = Random.Range(to.roomLeft, to.roomLeft + to.roomWidth);
			}
			int toPos = to.topRoad;

			for (int y = to.top; y<to.roomTop; y++)
			{
				cells[toPos, y] = (int)CellType.ROAD_1;
			}

			// 最後に横に繋ぐ
			int lower = Mathf.Min(frmPos, toPos);
			int bigger = Mathf.Max(frmPos, toPos);
			for (int x = lower; x <= bigger; x++)
			{
				cells[x, frm.bottom-1] = (int)CellType.ROAD_1;
			}
		}
		else if (frm.left == to.right)
		{// 左方向

			// 右の部屋から左へ道を繋ぐ
			if (frm.leftRoad < 0) {
				frm.leftRoad = Random.Range(frm.roomTop, frm.roomTop + frm.roomHeight);
			}
			int frmPos = frm.leftRoad;

			for (int x = frm.left; x<frm.roomLeft; x++)
			{
				cells[x, frmPos] = (int)CellType.ROAD_1;
			}

			// 右の部屋から左へ道を繋ぐ
			if (to.rightRoad < 0) {
				to.rightRoad = Random.Range(to.roomTop, to.roomTop + to.roomHeight);
			}
			int toPos = to.rightRoad;

			for (int x = to.roomLeft+to.roomWidth; x < to.right; x++)
			{
				cells[x, toPos] = (int)CellType.ROAD_1;
			}

			// 最後に横に繋ぐ
			int lower = Mathf.Min(frmPos, toPos);
			int bigger = Mathf.Max(frmPos, toPos);
			for (int y = lower; y <= bigger; y++)
			{
				cells[to.right-1, y] = (int)CellType.ROAD_1;
			}
		}
		else
		{
			// 隣接していない！
			return true;
		}

		// 道登録
		frm.connectedArea.Add(to);
		to.connectedArea.Add(frm);

		return false;
	}


	/// <summary>
	/// 部屋の分割
	/// 分割処理は再帰的に行われ、部屋数がすでに最低限数作られた場合
	/// ランダムで分割を行わないこともあります( 部屋の大きさにランダム性を持たせるため )
	/// </summary>
	/// <param name="rects">部屋のリスト.</param>
	/// <param name="parent">親部屋データ.</param>
	void SplitRoom(DArea parent, bool addParent)
	{
		if (parent == null) { return; }
		if (areas == null) { areas = new List<DArea>(); }

		// 親部屋を追加
		if (addParent) {
			areas.Add(parent);
		}

		if (areas.Count > AREA_MIN_NUM) {
			// 部屋数が最低限に達している場合、確率で部屋分割を実行しない
			if (Random.Range(0, 10) < AREA_DONT_SPLIT) {
				return;
			}
		}

		// 親部屋のサイズをチェック (部屋を作る最低限のサイズがあるかどうか)
		if (parent.Width < AREA_MIN_SIZE*2 && parent.Height < AREA_MIN_SIZE*2) {
			return;
		}

		// 縦横どちらの方向に分割する?  0:横, 1:縦
		int slight;
		if (parent.Width < AREA_MIN_SIZE*2) {// 横方向はサイズ不足で分割不可能 => 横方向に決定
			slight = 0;
		} else if (parent.Height < AREA_MIN_SIZE*2) {// 縦方向はサイズ不足で分割不可能 => 縦方向に決定
			slight = 1;
		} else {// どちらも最低サイズを充している場合はランダム
			slight = Random.Range(0, 2);
		}

		// 子部屋
		DArea child;

		// 分割位置
		int s_pos;
		if (slight == 0) {// 横に割る -> 上下に別れる
			s_pos = Random.Range(parent.top + AREA_MIN_SIZE, parent.bottom - AREA_MIN_SIZE);

			if (Random.Range(0, 2) == 0)
			{
				// 上:親 下:子
				child = new DArea(parent.left, s_pos, parent.right, parent.bottom);
				parent.bottom = s_pos;
			}
			else {
				// 上:子 下:親
				child = new DArea(parent.left, parent.top, parent.right, s_pos);
				parent.top = s_pos;
			}

//			Debug.Log("--- split Holizontal ---\nParent : " + parent.ToString() + "\nChild : "+ child.ToString());
		} else {// 縦に割る -> 左右に別れる
			s_pos = Random.Range(parent.left + AREA_MIN_SIZE, parent.right - AREA_MIN_SIZE);

			if (Random.Range(0, 2) == 0)
			{
				// 左:親 右:子
				child = new DArea(s_pos, parent.top, parent.right, parent.bottom);
				parent.right = s_pos;
			}
			else {
				// 左:子 右:親
				child = new DArea(parent.left, parent.top, s_pos, parent.bottom);
				parent.left = s_pos;
			}

//			Debug.Log("--- split Vartical ---\nParent : " + parent.ToString() + "˜\nChild : "+ child.ToString());
		}

		// 再帰処理
		SplitRoom(child, true);
		SplitRoom(parent, false);
	}

	void CreateRoom(ref int[,] _cells)
	{
		if (areas == null) return;
		foreach (DArea area in areas)
		{
			// 部屋のサイズを決める
			int l_max = area.Width - ROOM_MIN_SIZE - 2;
			area.roomLeft = Random.Range(1, l_max) + area.left;

			int t_max = area.Height - ROOM_MIN_SIZE - 2;
			area.roomTop = Random.Range(1, t_max) + area.top;

			int w_max = area.Width - (area.roomLeft - area.left) - 2;
			area.roomWidth = Random.Range(ROOM_MIN_SIZE, w_max);

			int h_max = area.Height - (area.roomTop - area.top) - 2;
			area.roomHeight = Random.Range(ROOM_MIN_SIZE, h_max);

//			Debug.Log(string.Format("room create --- top: {0}  left: {1}  height: {2}  width: {3}", area.roomTop, area.roomLeft, area.roomHeight, area.roomWidth));

			for (int y = area.roomTop; y < area.roomTop+area.roomHeight; y++)
			{
				for (int x = area.roomLeft; x < area.roomLeft+area.roomWidth; x++)
				{
					_cells[x, y] = (int)CellType.ROAD_1;
				}
			}
		}
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