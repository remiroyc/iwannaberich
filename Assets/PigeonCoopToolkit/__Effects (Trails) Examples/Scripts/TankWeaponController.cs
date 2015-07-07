using UnityEngine;

public class TankWeaponController : MonoBehaviour
{

    public TankProjectile ProjectilePrefab;
    public Transform Nozzle;

    void Update()
    {
        var localAnimation = GetComponent<Animation>();
        if (localAnimation.isPlaying == false && Input.GetKeyDown(KeyCode.Space))
        {
            localAnimation.Play();
            Instantiate(ProjectilePrefab, Nozzle.position, Nozzle.rotation);
        }
    }
}
