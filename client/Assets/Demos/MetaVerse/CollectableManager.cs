// (Logique des objets)
// Ce script doit être présent sur le serveur (ou un objet central de gestion dans votre scène Unity).

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

    void Start()
    {
        collectables.Add("BONUS_001", true);
        collectables.Add("BONUS_002", true);
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
        Debug.Log($"Joueur {playerId} a collecté {bonusId}");
        // Détruire l'objet dans la scène
        GameObject bonus = GameObject.Find(bonusId);
        if (bonus != null) Destroy(bonus);
    }

    public void OnCollectDenied(string playerId, string bonusId)
    {
        Debug.Log($"Joueur {playerId} n'a pas pu collecter {bonusId}");
    }
}