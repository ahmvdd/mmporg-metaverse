using UnityEngine;
using System.Collections.Generic;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }

    private Dictionary<string, bool> collectables = new Dictionary<string, bool>();
    private readonly object _lock = new object();

    void Awake()
    {
        Instance = this;
    }

    public bool TryCollect(string bonusId, out string resultMessage)
    {
        lock (_lock)
        {
            if (collectables.ContainsKey(bonusId) && collectables[bonusId])
            {
                collectables[bonusId] = false;
                resultMessage = $"SUCCESS|{bonusId}";
                return true;
            }
            resultMessage = $"FAILED|{bonusId}";
            return false;
        }
    }

    public void OnCollectOK(string playerId, string bonusId)
    {
        if (playerId == NetworkManager.Instance?.PlayerId)
            ScoreManager.Instance?.AddScore(1);

        Bonus[] allBonuses = FindObjectsByType<Bonus>(FindObjectsSortMode.None);
        foreach (Bonus b in allBonuses)
        {
            if (b.BonusId == bonusId)
            {
                b.gameObject.SetActive(false);
                break;
            }
        }
    }

    public void OnCollectDenied(string playerId, string bonusId)
    {
        Debug.Log($"Collect refusé : {playerId} -> {bonusId}");
    }
}
