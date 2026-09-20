using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Zarządza wyprawami mrówek po surowce (szkielet - do rozbudowy).
public class ExpeditionManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ResourceManager resourceManager;

    [Header("Domyślne ustawienia wypraw")]
    [SerializeField] private float defaultDurationSeconds = 10f;
    [SerializeField] private ResourceType defaultResourceType = ResourceType.Wood;
    [SerializeField] private int defaultRewardPerAnt = 1;

    private readonly List<ActiveExpedition> activeExpeditions = new List<ActiveExpedition>();

    public event Action<ExpeditionResult> OnExpeditionCompleted;

    private class ActiveExpedition
    {
        public int AntCount;
        public ResourceType ResourceType;
        public int RewardPerAnt;
        public float RemainingTime;
    }

    /// Wysyła mrówki na wyprawę po zasoby.
    public void StartExpedition(
        int antCount,
        ResourceType resourceType = default,
        int rewardPerAnt = 0,
        float duration = 0f)
    {
        if (antCount <= 0)
            return;

        var expedition = new ActiveExpedition
        {
            AntCount = antCount,
            ResourceType = resourceType == default ? defaultResourceType : resourceType,
            RewardPerAnt = rewardPerAnt <= 0 ? defaultRewardPerAnt : rewardPerAnt,
            RemainingTime = duration <= 0f ? defaultDurationSeconds : duration
        };

        activeExpeditions.Add(expedition);
        StartCoroutine(RunExpedition(expedition));
    }

    public int ActiveExpeditionCount => activeExpeditions.Count;

    private IEnumerator RunExpedition(ActiveExpedition expedition)
    {
        yield return new WaitForSeconds(expedition.RemainingTime);

        activeExpeditions.Remove(expedition);

        int reward = expedition.AntCount * expedition.RewardPerAnt;
        if (resourceManager != null)
            resourceManager.AddResource(expedition.ResourceType, reward);

        OnExpeditionCompleted?.Invoke(new ExpeditionResult
        {
            AntCount = expedition.AntCount,
            ResourceType = expedition.ResourceType,
            Amount = reward
        });
    }
}

public struct ExpeditionResult
{
    public int AntCount;
    public ResourceType ResourceType;
    public int Amount;
}