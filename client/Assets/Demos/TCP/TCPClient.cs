using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class TCPClient : MonoBehaviour
{
    public int DestinationPort = 25000;
    public string DestinationIP = "127.0.0.1";

    TcpClient tcp;
    IPEndPoint localEP;
    private readonly StringBuilder receiveBuffer = new StringBuilder();

    public delegate void TCPMessageReceive(string message);

    private TCPMessageReceive OnMessageReceive;


    public bool Connect(TCPMessageReceive handler, System.Action onConnected = null) {
        if (tcp != null) {
            Debug.LogWarning("Socket already initialized! Close it first.");
            return false;
        }
        OnMessageReceive = handler;
        tcp = new TcpClient();
        tcp.NoDelay = true;
        tcp.BeginConnect(DestinationIP, DestinationPort, OnConnectResult, onConnected);
        return true;
    }

    private void OnConnectResult(System.IAsyncResult result) {
        try {
            tcp.EndConnect(result);
            Debug.Log($"Connecté à {DestinationIP}:{DestinationPort}");
            (result.AsyncState as System.Action)?.Invoke();
        } catch (System.Exception ex) {
            Debug.LogWarning("Connexion échouée : " + ex.Message);
            CloseTCP();
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
            int bytesRead = tcp.GetStream().Read(data, 0, available);
            receiveBuffer.Append(Encoding.UTF8.GetString(data, 0, bytesRead));
        }

        // Traite uniquement les lignes complètes (terminées par \n)
        string buffered = receiveBuffer.ToString();
        int newlineIndex;
        while ((newlineIndex = buffered.IndexOf('\n')) >= 0)
        {
            string line = buffered.Substring(0, newlineIndex).Trim();
            buffered = buffered.Substring(newlineIndex + 1);
            if (line.Length > 0)
            {
                try { OnMessageReceive?.Invoke(line); }
                catch (System.Exception ex) { Debug.LogWarning("Error receiving TCP message: " + ex.Message); }
            }
        }
        receiveBuffer.Clear();
        receiveBuffer.Append(buffered);
    }

    private void CloseTCP() {
        if (tcp != null) {
            tcp.Close();
            tcp = null;            
        }
        OnMessageReceive = null;
    }

}
