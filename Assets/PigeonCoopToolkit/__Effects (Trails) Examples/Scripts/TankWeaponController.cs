using UnityEngine;
using System.Collections;

public class TankWeaponController : MonoBehaviour
{

    public TankProjectile ProjectilePrefab;
    public Transform Nozzle;

	
	// Update is called once per frame
	void Update () {
	    if(animation.isPlaying == false && Input.GetKeyDown(KeyCode.Space))
	    {
	        animation.Play();
            Instantiate(ProjectilePrefab, Nozzle.position, Nozzle.rotation);
	    }
	}
}
