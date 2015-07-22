using UnityEngine;
using System.Collections;

public class RObject : MonoBehaviour , IStopable {

	protected bool isStop;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Stop() {
		isStop = true;
	}

	public void Play() {
		isStop = false;
	}
}

public interface IStopable
{
	void Stop();
	void Play();
}