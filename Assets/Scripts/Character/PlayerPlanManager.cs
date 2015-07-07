using System;
using System.Collections.Generic;
using UnityEngine;
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
                _instance = FindObjectOfType<PlayerPlanManager>();
                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != _instance)
            {
                Destroy(gameObject);
            }
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
