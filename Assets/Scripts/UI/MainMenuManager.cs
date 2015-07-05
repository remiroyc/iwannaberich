using UnityEngine;

public class MainMenuManager : MonoBehaviour
{

    public PlayerPlanManager PlayerPlanManager;

    void Start()
    {
    }

    void Update()
    {
    }

    public void ContractMenu(Animation animScript)
    {
        var audioClip = Resources.Load<AudioClip>("Audios/click_open");
        var audioClickClip = Resources.Load<AudioClip>("Audios/click");
        AudioSource.PlayClipAtPoint(audioClickClip, transform.position, 1);
        AudioSource.PlayClipAtPoint(audioClip, transform.position, 1);
        animScript.Play("TitlePanelContract");
    }

    public void ChoosePlan()
    {
        PlayerPlanManager.GeneratePlayerPlanSub();
        Application.LoadLevel(1);
    }

}
