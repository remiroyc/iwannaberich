using UnityEngine;
using System.Collections;

public class FallingTileManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter(Collision collision)
    {
        Destroy(this);
    }
}
