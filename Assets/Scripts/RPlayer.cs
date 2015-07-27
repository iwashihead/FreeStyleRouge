using UnityEngine;
using System.Collections;

/// <summary>
/// 8方向、時計周りな感じ
/// </summary>
public enum RDirection8
{
	back		= 0,
	back_r		= 1,
	right		= 2,
	front_r		= 3,
	front		= 4,
	front_l		= 5,
	left		= 6,
	back_l		= 7,
}

/// <summary>
/// 4方向、時計周りな感じ
/// </summary>
public enum RDirection4
{
	None = 0,
	up,
	right,
	down,
	left,

	ALL,
}

[RequireComponent(typeof(Animator))]
public class RPlayer : RObject {

	/// <summary>
	/// キャラの方向
	/// </summary>
	public int direction;



	private Animator _animator;

	void Awake() {
		_animator = GetComponent<Animator>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		// 向きの更新、Animatorにパラメタをセット
		_animator.SetInteger("direction", direction);
	}
}
