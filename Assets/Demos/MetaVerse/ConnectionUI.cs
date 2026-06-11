using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionUI : MonoBehaviour
{
    public TMP_InputField IPInput;
    public TMP_InputField PortInput;
    public Button ConnectButton;
    public Button HostButton;
    public TextMeshProUGUI StatusText; // optionnel : drag un Text dans l'Inspector

    void Start()
    {
        ConnectButton.onClick.AddListener(OnConnectClick);
        HostButton.onClick.AddListener(OnHostClick);
        IPInput.text = "";  // vide pour forcer le client à entrer l'IP de l'hôte
        PortInput.text = "5555";
    }

    public void OnConnectClick()
    {
        string ip = IPInput.text.Trim();
        if (string.IsNullOrEmpty(ip))
        {
            Debug.LogError("Entre l'IP de l'hôte !");
            if (StatusText) StatusText.text = "❌ Entre l'IP de l'hôte";
            return;
        }

        int port = int.Parse(PortInput.text);
        bool connected = NetworkManager.Instance.Connect(ip, port);

        if (connected)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"Connexion échouée vers {ip}:{port}");
            if (StatusText) StatusText.text = $"❌ Connexion échouée ({ip}:{port})";
        }
    }

    public void OnHostClick()
    {
        int port = int.Parse(PortInput.text);
        GameServer.Instance.StartServer(port);
        NetworkManager.Instance.Connect("127.0.0.1", port);
        gameObject.SetActive(false);
    }
}