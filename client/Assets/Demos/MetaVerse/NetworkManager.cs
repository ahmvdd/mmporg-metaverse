using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public string ServerIP = "127.0.0.1";
    public int ServerPort = 5555;
    public GameObject RemotePlayerPrefab;
    public GameObject BonusPrefab;
    public TCPClient NetworkClient;
    public string PlayerId { get; private set; }

    Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();

    private readonly Queue<string> messageQueue = new Queue<string>();
    private readonly object queueLock = new object();

    // Cibles d'interpolation des voitures distantes (mises à jour à la réception)
    private readonly Dictionary<int, Vector3> carTargetPos = new Dictionary<int, Vector3>();
    private readonly Dictionary<int, float> carTargetRot = new Dictionary<int, float>();

    private float positionSendTimer = 0f;
    private const float PositionSendInterval = 0.05f; // 20 Hz

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        PlayerId = "player_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    void Start()
    {
        if (NetworkClient == null)
            NetworkClient = GetComponent<TCPClient>();
    }

    void Update()
    {
        while (true)
        {
            string msg = null;
            lock (queueLock)
            {
                if (messageQueue.Count > 0)
                    msg = messageQueue.Dequeue();
            }
            if (msg == null) break;
            ProcessMessage(msg);
        }

        // Interpolation des voitures chaque frame (pas seulement à la réception)
        if (CarSyncManager.Instance != null)
        {
            foreach (var kvp in carTargetPos)
            {
                GameObject car = CarSyncManager.Instance.GetCar(kvp.Key);
                if (car == null) continue;
                car.transform.position = Vector3.Lerp(car.transform.position, kvp.Value, 15f * Time.deltaTime);
                if (carTargetRot.TryGetValue(kvp.Key, out float rot))
                    car.transform.rotation = Quaternion.Lerp(car.transform.rotation, Quaternion.Euler(0, rot, 0), 15f * Time.deltaTime);
            }
        }
    }

    public bool Connect(string ip, int port)
    {
        NetworkClient.DestinationIP = ip;
        NetworkClient.DestinationPort = port;
        bool ok = NetworkClient.Connect(OnMessageReceived, () =>
        {
            Send($"CONNECT|{PlayerId}");
            Debug.Log($"Connecté en tant que {PlayerId}");
        });
        if (!ok)
            Debug.LogWarning("Connexion échouée.");
        return ok;
    }

    public void Send(string message)
    {
        if (NetworkClient != null && NetworkClient.IsConnected)
            NetworkClient.SendTCPMessage(message);
    }

    public void SendPosition(Vector3 pos, float rotY)
    {
        Send(string.Format(CultureInfo.InvariantCulture,
            "MOVE|{0}|{1:F2}|{2:F2}|{3:F2}|{4:F2}",
            PlayerId, pos.x, pos.y, pos.z, rotY));
    }

    void OnMessageReceived(string message)
    {
        lock (queueLock)
        {
            messageQueue.Enqueue(message);
        }
    }

    void ProcessMessage(string message)
    {
        string[] parts = message.Trim().Split('|');
        if (parts.Length < 2) return;

        string type = parts[0];

        switch (type)
        {
            case "COLLECT_OK":
                if (parts.Length < 3) return;
                CollectableManager.Instance?.OnCollectOK(parts[1], parts[2]);
                return;

            case "COLLECT_DENIED":
                if (parts.Length < 3) return;
                CollectableManager.Instance?.OnCollectDenied(parts[1], parts[2]);
                return;

            case "CAR_MOVE":
                if (parts.Length < 6) return;
                if (!int.TryParse(parts[1].Replace("car_", ""), out int carIndex)) return;
                carTargetPos[carIndex] = new Vector3(
                    float.Parse(parts[2], CultureInfo.InvariantCulture),
                    float.Parse(parts[3], CultureInfo.InvariantCulture),
                    float.Parse(parts[4], CultureInfo.InvariantCulture)
                );
                carTargetRot[carIndex] = float.Parse(parts[5], CultureInfo.InvariantCulture);
                return;

            case "BONUS_SPAWN":
                if (parts.Length < 4) return;
                float bx = float.Parse(parts[1], CultureInfo.InvariantCulture);
                float by = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float bz = float.Parse(parts[3], CultureInfo.InvariantCulture);
                BonusSpawner.Instance?.SpawnAt(bx, by, bz);
                return;
        }

        string id = parts[1];
        if (id == PlayerId) return;

        switch (type)
        {
            case "CONNECT":
                Debug.Log($"Joueur distant annoncé : {id}");
                break;

            case "MOVE":
                if (parts.Length < 6) return;
                float x = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float y = float.Parse(parts[3], CultureInfo.InvariantCulture);
                float z = float.Parse(parts[4], CultureInfo.InvariantCulture);
                float rot = float.Parse(parts[5], CultureInfo.InvariantCulture);

                if (!remotePlayers.ContainsKey(id))
                {
                    GameObject go = Instantiate(RemotePlayerPrefab,
                        new Vector3(x, y, z), Quaternion.identity);

                    // Supprimer les composants locaux : empêche les collisions voiture
                    // d'affecter le score du joueur local via le singleton ScoreManager
                    PlayerController pc = go.GetComponent<PlayerController>();
                    if (pc != null) Destroy(pc);
                    if (go.TryGetComponent(out Rigidbody rb))
                        rb.isKinematic = true;

                    remotePlayers[id] = go;
                }

                RemotePlayer rp = remotePlayers[id].GetComponent<RemotePlayer>();
                if (rp != null)
                    rp.SetTarget(x, y, z, rot);
                break;

            case "SCORE":
                if (parts.Length < 3) return;
                if (remotePlayers.TryGetValue(id, out GameObject scoreTarget))
                {
                    if (int.TryParse(parts[2], out int remoteScore))
                        scoreTarget.GetComponent<RemotePlayer>()?.UpdateScore(remoteScore);
                }
                break;

            case "GAME_OVER":
                ScoreManager.Instance?.OnOpponentWon();
                break;

            case "DISCONNECT":
                if (remotePlayers.ContainsKey(id))
                {
                    Destroy(remotePlayers[id]);
                    remotePlayers.Remove(id);
                }
                break;
        }
    }

    public void Disconnect()
    {
        if (NetworkClient != null && NetworkClient.IsConnected)
            Send($"DISCONNECT|{PlayerId}");
        NetworkClient?.Close();

        foreach (var go in remotePlayers.Values)
            if (go != null) Destroy(go);
        remotePlayers.Clear();
    }

    void OnDisable()
    {
        Disconnect();
    }
}