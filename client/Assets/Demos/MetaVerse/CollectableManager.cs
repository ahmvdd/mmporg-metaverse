using UnityEngine;
using System.Collections.Generic;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance { get; private set; }

    private readonly object _lock = new object();

    void Awake()
    {
        Instance = this;
    }
    //changement ici aussi faut fixer au niveau du prefab bonus
    public void OnCollectOK(string playerId, string bonusId)
{
    if (playerId == NetworkManager.Instance?.PlayerId)
        ScoreManager.Instance?.AddScore(20);

    Bonus[] allBonuses = FindObjectsByType<Bonus>(FindObjectsSortMode.None);
    foreach (Bonus b in allBonuses)
    {
        if (b.BonusId == bonusId)
        {
            StartCoroutine(RespawnBonus(b.gameObject, 5f));
            break;
        }
    }
    }

    private System.Collections.IEnumerator RespawnBonus(GameObject bonus, float delay)
    {
        bonus.SetActive(false);
        yield return new WaitForSeconds(delay);
        bonus.SetActive(true);
    }

        public void OnCollectDenied(string playerId, string bonusId)
        {
            Debug.Log($"Collect refusé : {playerId} -> {bonusId}");
        }
    }