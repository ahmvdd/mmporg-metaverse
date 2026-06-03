using UnityEngine;
using System.Collections.Generic;

public class CollectableManager : MonoBehaviour
{

    private HashSet<string> collectedObjects = new HashSet<string>();

    public bool TryCollect(string bonusId, out string result)
    {
        lock (collectedObjects)
        {
            if (!collectedObjects.Contains(bonusId))
            {
                collectedObjects.Add(bonusId);
                result = bonusId;
                return true;
            }
            result = "";
            return false;
        }
    }

    public void RequestCollect(string bonusId)
    {
        // Envoi au serveur pour validation
        Debug.Log($"Demande de collecte : {bonusId}");
    }
}