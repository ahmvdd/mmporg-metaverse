using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class CarSyncManager : MonoBehaviour
{
    public static CarSyncManager Instance;

    private List<GameObject> cars = new List<GameObject>();
    private float sendTimer = 0f;
    private float sendInterval = 0.1f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Trouver toutes les voitures automatiquement
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject go in allObjects)
        {
            if (go.name.StartsWith("Car"))
                cars.Add(go);
        }
        Debug.Log($"CarSyncManager : {cars.Count} voitures trouvées");
    }

    void Update()
    {
        if (GameServer.Instance == null || !GameServer.Instance.IsRunning) return;

        sendTimer += Time.deltaTime;
        if (sendTimer < sendInterval) return;
        sendTimer = 0f;

        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i] == null) continue;
            Vector3 pos = cars[i].transform.position;
            float rotY = cars[i].transform.eulerAngles.y;
            NetworkManager.Instance?.Send(string.Format(CultureInfo.InvariantCulture,
                "CAR_MOVE|car_{0}|{1:F2}|{2:F2}|{3:F2}|{4:F2}",
                i, pos.x, pos.y, pos.z, rotY));
        }
    }

    public GameObject GetCar(int index)
    {
        if (index >= 0 && index < cars.Count)
            return cars[index];
        return null;
    }
}