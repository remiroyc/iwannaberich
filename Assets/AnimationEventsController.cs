using UnityEngine;

public class AnimationEventsController : MonoBehaviour
{
    public MeleeWeaponTrail swordTrail;
    public GameObject ripFx;
    public GameObject hitFX;
    public MeleeWeaponTrail trail;

    private AudioSource _audio;

    void Awake()
    {
        _audio = GetComponentInParent<AudioSource>();
        if (_audio == null)
        {
            Debug.LogError("AudioSource null");
        }
    }

    public void ComboHit(string attackType)
    {
        switch (attackType)
        {
            case "LateralHit1":
                trail.enabled = true;
                _audio.PlayOneShot(Resources.Load<AudioClip>("Audios/epee"));
                if (ripFx != null)
                {
                    Instantiate(ripFx, transform.position + transform.forward, Quaternion.Euler(new Vector3(0, 0, -50)));
                }
                break;
            case "LateralHit2":
            case "LateralHit3":
                trail.enabled = true;
                _audio.PlayOneShot(Resources.Load<AudioClip>("Audios/epee"));
                if (ripFx != null)
                {
                    Instantiate(ripFx, transform.position + transform.forward, Quaternion.Euler(new Vector3(0, 0, 130)));
                }
                break;
            case "LegHit1":
                trail.enabled = false;
                _audio.PlayOneShot(Resources.Load<AudioClip>("Audios/weakpunch"));
                if (hitFX != null)
                {
                    Instantiate(hitFX, transform.position + transform.forward, Quaternion.identity);
                }
                break;
            case "End":
                trail.enabled = false;
                break;
        }
    }


}
