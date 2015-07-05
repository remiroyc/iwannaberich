using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPlanSubscription : MonoBehaviour
{
    public PlayerPlan Plan;
    public bool Enable = false;

    public void Activation(Toggle toggle)
    {
        Enable = toggle.isOn;
    }

    

}