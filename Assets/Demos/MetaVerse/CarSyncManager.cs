using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class CarSyncManager : MonoBehaviour
{
    public static CarSyncManager Instance;

    private List<GameObject> cars = new List<GameObject>();
    private Vector3[] lastPositions;
    private float[] lastRotations;
    private float sendTimer = 0f;
    private const float SendInterval = 0.1f;
    private const float PosThreshold = 0.02f;
    private const float RotThreshold = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject go in allObjects)
        {
            if (go.name.StartsWith("Car"))
                cars.Add(go);
        }
        lastPositions = new Vector3[cars.Count];
        lastRotations = new float[cars.Count];
        for (int i = 0; i < cars.Count; i++)
        {
            lastPositions[i] = cars[i].transform.position;
            lastRotations[i] = cars[i].transform.eulerAngles.y;
        }
        Debug.Log($"CarSyncManager : {cars.Count} voitures trouvées");
    }

    void Update()
    {
        if (GameServer.Instance == null || !GameServer.Instance.IsRunning) return;

        sendTimer += Time.deltaTime;
        if (sendTimer < SendInterval) return;
        sendTimer = 0f;

        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i] == null) continue;
            Vector3 pos = cars[i].transform.position;
            float rotY = cars[i].transform.eulerAngles.y;

            if (Vector3.Distance(pos, lastPositions[i]) < PosThreshold &&
                Mathf.Abs(Mathf.DeltaAngle(rotY, lastRotations[i])) < RotThreshold)
                continue;

            lastPositions[i] = pos;
            lastRotations[i] = rotY;

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