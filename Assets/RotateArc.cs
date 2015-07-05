using UnityEngine;
using System.Collections;

public class RotateArc : MonoBehaviour {

    public float speed = 2f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        this.transform.Rotate(Vector3.back, speed * Time.deltaTime);
	}
}
