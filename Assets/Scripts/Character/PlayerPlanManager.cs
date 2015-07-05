using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

[Serializable]
public class SelectedPlayerPlan
{
    [SerializeField]
    public PlayerPlan Plan { get; set; }
    [SerializeField]
    public bool Enable;
}

public class PlayerPlanManager : MonoBehaviour
{
    public List<SelectedPlayerPlan> PlayerPlanSub;

    private static PlayerPlanManager _instance;
    public static PlayerPlanManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindObjectOfType<PlayerPlanManager>();

                //Tell unity not to destroy this object when loading a new scene!
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            //If I am the first instance, make me the Singleton
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            //If a Singleton already exists and you find
            //another reference in scene, destroy it!
            if (this != _instance)
                Destroy(this.gameObject);
        }
    }

    public void GeneratePlayerPlanSub()
    {
        PlayerPlanSub = new List<SelectedPlayerPlan>();
        var plans = FindObjectsOfType<PlayerPlanSubscription>();
        foreach (var plan in plans)
        {
            PlayerPlanSub.Add(new SelectedPlayerPlan()
            {
                Enable = plan.Enable,
                Plan = plan.Plan
            });
        }
    }

}
