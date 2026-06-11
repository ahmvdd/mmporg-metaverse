using UnityEngine;
using System.Net.Sockets;

public class TCPClient : MonoBehaviour
{
    public int DestinationPort = 25000;
    public string DestinationIP = "127.0.0.1";

    TcpClient tcp;

    public delegate void TCPMessageReceive(string message);

    private TCPMessageReceive OnMessageReceive;

    public bool Connect(TCPMessageReceive handler) {
        if (tcp != null) {
            Debug.LogWarning("Socket already initialized! Close it first.");
            return false;
        }
        try {
            tcp = new TcpClient();

            // Timeout 3 secondes au lieu de freezer 20-30s 
            System.IAsyncResult result = tcp.BeginConnect(DestinationIP, DestinationPort, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(System.TimeSpan.FromSeconds(3));

            if (!success || !tcp.Connected)
            {
                Debug.LogWarning($"Timeout — impossible de joindre {DestinationIP}:{DestinationPort}");
                CloseTCP();
                return false;
            }

            tcp.EndConnect(result);
            OnMessageReceive = handler;
            Debug.Log($"Connecté à {DestinationIP}:{DestinationPort}");
            return true;
        } catch (System.Exception ex)
        {
            Debug.LogWarning("Erreur connexion: " + ex.Message);
            CloseTCP();
            return false;
        }
    }

    public void SendTCPMessage(string message) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message + "\n");
        SendTCPBytes(bytes);
    }

    public void Close() {
        CloseTCP();
    }

    public bool IsConnected {
        get {
            return (tcp != null && tcp.Connected);
        }
    }

    private void SendTCPBytes(byte[] bytes) {
        if (tcp == null) {
            return;
        }

        try {
            tcp.GetStream().Write(bytes, 0, bytes.Length);
        } catch (SocketException e)
        {
            Debug.LogWarning(e.Message);
        }
    }

    void OnDisable() {
        CloseTCP();
    }

    void Update() {
        ReceiveTCP();
    }

    private void ReceiveTCP() {
        if (tcp == null) { return; }

        while (tcp.Available > 0)
        {
            int available = tcp.Available;
            byte[] data = new byte[available];
            int read = tcp.GetStream().Read(data, 0, available);

            try
            {
                if (read > 0)
                    ParseString(data, read);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("Error receiving TCP message: " + ex.Message);
            }
        }
    }

    private void ParseString(byte[] bytes, int length = -1) {
        string raw = System.Text.Encoding.UTF8.GetString(bytes, 0, length < 0 ? bytes.Length : length);
        foreach (string line in raw.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.Length > 0)
                OnMessageReceive.Invoke(trimmed);
        }
    }

    private void CloseTCP() {
        if (tcp != null) {
            tcp.Close();
            tcp = null;
        }
        OnMessageReceive = null;
    }
}