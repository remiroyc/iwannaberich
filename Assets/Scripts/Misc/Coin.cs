using UnityEngine;

public class Coin : MonoBehaviour
{

    private float _time = 0;
    public float MaxLifeTime = 6;
    public bool AutoDestruction = true;

    void Start()
    {
        _time = Time.time;
    }

    private void Update()
    {
        if (AutoDestruction)
        {
            if (Time.time - _time >= MaxLifeTime)
            {
                Destroy(transform.gameObject);
            }
        }
        var angle = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(angle.x, angle.y + 10, angle.z);
    }

}
