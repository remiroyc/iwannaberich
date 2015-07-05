using UnityEngine;
using System.Collections;

public class AudioVoiceManager : MonoBehaviour
{

    public AudioSource BackgroundMusic;
    private bool content = false, enerve1 = false, enerve2 = false;
    private bool voice = false;
    void Start()
    {

    }

    void Update()
    {

        if (GameManager.instance.Coins >= 600 && !content && !voice)
        {
            content = true;
            StartCoroutine(Content());
        }

        if (GameManager.instance.Coins <= -100 && !enerve1 && !voice)
        {
            enerve1 = true;
            StartCoroutine(Enerve());
        }

        if (GameManager.instance.Coins <= -200 && !enerve2 && !voice)
        {
            enerve2 = true;
            StartCoroutine(Enerve2());
        }

    }


    IEnumerator Content()
    {
        voice = true;
        BackgroundMusic.volume = 0.1f;
        AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audios/Voice/nokia"), transform.position, 0.8f);
        yield return new WaitForSeconds(3);
        AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audios/Voice/content"), transform.position, 1);
        yield return new WaitForSeconds(10);
        BackgroundMusic.volume = 1;
        voice = false;
    }

    IEnumerator Enerve()
    {
        voice = true;
        BackgroundMusic.volume = 0.2f;
        AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audios/Voice/enerve"), transform.position, 1);
        yield return new WaitForSeconds(10);
        BackgroundMusic.volume = 1;
        voice = false;
    }

    IEnumerator Enerve2()
    {
        voice = true;
        BackgroundMusic.volume = 0.2f;
        AudioSource.PlayClipAtPoint(Resources.Load<AudioClip>("Audios/Voice/enerve2"), transform.position, 1);
        yield return new WaitForSeconds(10);
        BackgroundMusic.volume = 1;
        voice = false;
    }

}
