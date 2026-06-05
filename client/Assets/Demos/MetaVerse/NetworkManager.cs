using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public string ServerIP = "127.0.0.1";
    public int ServerPort = 5555;
    public GameObject RemotePlayerPrefab;
    public TCPClient NetworkClient;
    public string PlayerId { get; private set; }

    Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();

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

    public bool Connect(string ip, int port)
    {
        NetworkClient.DestinationIP = ip;
        NetworkClient.DestinationPort = port;
        bool ok = NetworkClient.Connect(OnMessageReceived);
        if (ok)
        {
            Send($"CONNECT|{PlayerId}");
            Debug.Log($"Connecté en tant que {PlayerId}");
        }
        else
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
        string[] parts = message.Trim().Split('|');
        if (parts.Length < 2) return;

        string type = parts[0];
        string id = parts[1];

        if (id == PlayerId) return;

        switch (type)
        {
            case "CONNECT":
                if (!remotePlayers.ContainsKey(id))
                {
                    GameObject go = Instantiate(RemotePlayerPrefab, Vector3.zero, Quaternion.identity);
                    remotePlayers[id] = go;
                    Debug.Log($"Joueur distant connecté : {id}");
                }
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
                    remotePlayers[id] = go;
                }
                remotePlayers[id].transform.position = Vector3.Lerp(
                    remotePlayers[id].transform.position,
                    new Vector3(x, y, z),
                    10f * Time.deltaTime
                );
                remotePlayers[id].transform.rotation = Quaternion.Lerp(
                    remotePlayers[id].transform.rotation,
                    Quaternion.Euler(0, rot, 0),
                    10f * Time.deltaTime
                );
                break;

            case "DISCONNECT":
                if (remotePlayers.ContainsKey(id))
                {
                    Destroy(remotePlayers[id]);
                    remotePlayers.Remove(id);
                }
                break;

            case "COLLECT_OK":
                if (parts.Length < 3) return;
                CollectableManager.Instance?.OnCollectOK(parts[1], parts[2]);
                break;

            case "COLLECT_DENIED":
                if (parts.Length < 3) return;
                CollectableManager.Instance?.OnCollectDenied(parts[1], parts[2]);
                break;
        }
    }

    void OnDisable()
    {
        if (NetworkClient != null && NetworkClient.IsConnected)
            Send($"DISCONNECT|{PlayerId}");
        NetworkClient?.Close();
    }
}
