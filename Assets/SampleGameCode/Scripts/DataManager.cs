using UnityEngine;
using System.Collections;

public class DataManager : SingletonObject<DataManager> {
	public RCellData cellData;
	public RMapDefineData mapDefineData;


	public int seed {
		get {
			if (isInit == false) {
				Initialize();
			}
			return _seed;
		}
	}
	private int _seed = -1;


	bool isInit;

	void Awake()
	{
		
	}

	void Initialize()
	{
		if (isInit) return;

		// TODO : 初期化
		isInit = true;
		_seed = (int)System.DateTime.Now.Ticks;
	}
}
