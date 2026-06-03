// (Logique des objets)
// Ce script doit être présent sur le serveur (ou un objet central de gestion dans votre scène Unity).

using UnityEngine;
using System.Collections.Generic;

public class CollectableManager : MonoBehaviour
{
    // Dictionnaire pour suivre l'état des bonus (ID -> Disponible)
    private Dictionary<string, bool> collectables = new Dictionary<string, bool>();
    private readonly object _lock = new object();

    void Start()
    {
        // Initialisation des bonus (exemple)
        collectables.Add("BONUS_001", true);
        collectables.Add("BONUS_002", true);
    }

    // Appelée par le serveur quand un message COLLECT arrive
    public bool TryCollect(string bonusId, out string resultMessage)
    {
        lock (_lock)
        {
            if (collectables.ContainsKey(bonusId) && collectables[bonusId])
            {
                collectables[bonusId] = false; // L'objet est maintenant pris
                resultMessage = $"SUCCESS|{bonusId}";
                return true;
            }
            
            resultMessage = $"FAILED|{bonusId}";
            return false;
        }
    }
}