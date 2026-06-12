using UnityEngine;
using System.Collections.Generic;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }

    private readonly Dictionary<string, Bonus> bonusCache = new Dictionary<string, Bonus>();

    void Awake()
    {
        Instance = this;
    }

    public void RegisterBonus(Bonus b)
    {
        bonusCache[b.BonusId] = b;
    }

    public void OnCollectOK(string playerId, string bonusId)
    {
        if (playerId == NetworkManager.Instance?.PlayerId)
            ScoreManager.Instance?.AddScore(1);

        if (bonusCache.TryGetValue(bonusId, out Bonus b))
            StartCoroutine(RespawnBonus(b.gameObject, 5f));
    }

    public void OnCollectDenied(string playerId, string bonusId)
    {
        Debug.Log($"Collect refusé : {playerId} -> {bonusId}");
    }

    private System.Collections.IEnumerator RespawnBonus(GameObject bonus, float delay)
    {
        bonus.SetActive(false);
        yield return new WaitForSeconds(delay);
        bonus.SetActive(true);
    }
}
