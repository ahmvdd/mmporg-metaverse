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

    TCPClient tcp;
    Dictionary<string, GameObject> remotePlayers = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        PlayerId = System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    void Start()
    {
        tcp = GetComponent<TCPClient>();
        tcp.DestinationIP = ServerIP;
        tcp.DestinationPort = ServerPort;
        bool ok = tcp.Connect(OnMessageReceived);
        if (ok)
        {
            tcp.SendTCPMessage($"CONNECT|{PlayerId}");
            Debug.Log($"Connecté au serveur en tant que {PlayerId}");
        }
        else
            Debug.LogWarning("Connexion au serveur échouée.");
    }

    public void SendPosition(Vector3 pos, float rotY)
    {
        if (!tcp.IsConnected) return;
        tcp.SendTCPMessage(string.Format(CultureInfo.InvariantCulture,
            "MOVE|{0}|{1}|{2}|{3}|{4}", PlayerId, pos.x, pos.y, pos.z, rotY));
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
                    Debug.Log($"Nouveau joueur distant : {id}");
                }
                break;

            case "MOVE":
                if (parts.Length < 6) return;
                float x   = float.Parse(parts[2], CultureInfo.InvariantCulture);
                float y   = float.Parse(parts[3], CultureInfo.InvariantCulture);
                float z   = float.Parse(parts[4], CultureInfo.InvariantCulture);
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
                    Debug.Log($"Joueur distant déconnecté : {id}");
                }
                break;
        }
    }

    void OnDisable()
    {
        if (tcp != null && tcp.IsConnected)
            tcp.SendTCPMessage($"DISCONNECT|{PlayerId}");
        tcp?.Close();
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (string.IsNullOrEmpty(PlayerId))
            PlayerId = "player_" + System.Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    public bool Connect(string ip, int port)
    {
        NetworkClient.DestinationIP = ip;
        NetworkClient.DestinationPort = port;
        bool ok = NetworkClient.Connect(OnMessageReceived);
        if (ok) SendConnect();
        return ok;
    }

    public void Send(string message)
    {
        if (NetworkClient != null && NetworkClient.IsConnected)
            NetworkClient.SendTCPMessage(message);
    }

    void SendConnect()
    {
        Send("CONNECT|" + PlayerId);
    }

    void OnMessageReceived(string message)
    {
        Debug.Log("[Server] " + message);
    }

    void OnDisable()
    {
        if (NetworkClient != null) NetworkClient.Close();
    }
}
