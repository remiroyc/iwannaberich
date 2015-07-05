using UnityEngine;
using System.Collections;

public class AnimationEventsController : MonoBehaviour {
    public MeleeWeaponTrail swordTrail;
    public GameObject ripFx;
    public GameObject hitFX;
    public MeleeWeaponTrail trail;

    void Start()
    {
        
    }

	public void ComboHit(string attackType)
    {
        switch (attackType)
        {
            case "LateralHit1":
                trail.enabled = true;
                audio.PlayOneShot(Resources.Load<AudioClip>("Audios/epee"));
                Instantiate(ripFx, this.transform.position + this.transform.forward, Quaternion.Euler(new Vector3(0,0,-50)));
                break;
            case "LateralHit2":
            case "LateralHit3":
                trail.enabled = true;
                audio.PlayOneShot(Resources.Load<AudioClip>("Audios/epee"));
                Instantiate(ripFx, this.transform.position + this.transform.forward, Quaternion.Euler(new Vector3(0, 0, 130)));
                break;
            case "LegHit1":
                trail.enabled = false;
                audio.PlayOneShot(Resources.Load<AudioClip>("Audios/weakpunch"));
                Instantiate(hitFX, this.transform.position + this.transform.forward, Quaternion.identity);
                break; 
            case "End":
                trail.enabled = false;
                break;
        }
    }


}
