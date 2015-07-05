using UnityEngine;
using System.Collections;

public class DeathColliderManager : MonoBehaviour {
    private bool _isPlayerDead;
    private GameObject currentPlayer;
    public GameObject respawnPoint;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        
	}

    void OnTriggerEnter(Collider player)
    {
        if (player.tag == "Player")
        {
            
            GameOverManager.instance.GameOver(respawnPoint);
        }
    }
}
