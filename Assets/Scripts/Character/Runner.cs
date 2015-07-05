using UnityEngine;
using UnityEngine.UI;

public class Runner : MonoBehaviour
{

    public Button SwordButton;
    private bool _runEnable;

    void Start()
    {

    }

    void Update()
    {
       
    }

    public void ActivateFight()
    {
        SwordButton.gameObject.SetActive(true);
    }

    public void ActivateRun()
    {
        _runEnable = true;
        SwordButton.gameObject.SetActive(false);
    }


}
