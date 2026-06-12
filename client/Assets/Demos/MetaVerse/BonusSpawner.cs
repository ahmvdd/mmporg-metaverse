using UnityEngine;
using System.Globalization;

public class BonusSpawner : MonoBehaviour
{
    public static BonusSpawner Instance { get; private set; }

    public GameObject BonusPrefab;
    public float SpawnInterval = 8f;
    public int MaxBonuses = 5;
    public float SpawnRadius = 8f;
    public float SpawnHeight = 0.5f;

    private float timer = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (GameServer.Instance == null || !GameServer.Instance.IsRunning) return;

        timer += Time.deltaTime;
        if (timer >= SpawnInterval)
        {
            timer = 0f;
            TrySpawnBonus();
        }
    }

    void TrySpawnBonus()
    {
        Bonus[] existing = FindObjectsByType<Bonus>(FindObjectsSortMode.None);
        int activeCount = 0;
        foreach (Bonus b in existing)
            if (b.gameObject.activeSelf) activeCount++;

        if (activeCount >= MaxBonuses) return;

        Vector3 spawnPos;
        if (!TryGetRandomPlayerPosition(out spawnPos)) return;

        Vector2 offset = Random.insideUnitCircle * SpawnRadius;
        float x = spawnPos.x + offset.x;
        float z = spawnPos.z + offset.y;

        GameServer.Instance.SendToAll(string.Format(CultureInfo.InvariantCulture,
            "BONUS_SPAWN|{0:F2}|{1:F2}|{2:F2}", x, SpawnHeight, z));
    }

    bool TryGetRandomPlayerPosition(out Vector3 position)
    {
        PlayerController[] locals = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        RemotePlayer[] remotes = FindObjectsByType<RemotePlayer>(FindObjectsSortMode.None);

        int total = locals.Length + remotes.Length;
        if (total == 0) { position = Vector3.zero; return false; }

        int pick = Random.Range(0, total);
        position = pick < locals.Length
            ? locals[pick].transform.position
            : remotes[pick - locals.Length].transform.position;
        return true;
    }

    public void SpawnAt(float x, float y, float z)
    {
        if (BonusPrefab == null) return;
        Instantiate(BonusPrefab, new Vector3(x, y, z), Quaternion.identity);
    }
}
