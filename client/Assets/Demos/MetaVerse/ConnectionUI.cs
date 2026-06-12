using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionUI : MonoBehaviour
{
    public TMP_InputField IPInput;
    public TMP_InputField PortInput;
    public Button ConnectButton;
    public Button HostButton;
    public TMP_Text IPDisplayText;

    void Start()
    {
        Time.timeScale = 0f;
        ConnectButton.onClick.AddListener(OnConnectClick);
        HostButton.onClick.AddListener(OnHostClick);
        IPInput.text = "127.0.0.1";
        PortInput.text = "5555";
        if (IPDisplayText != null)
            IPDisplayText.gameObject.SetActive(false);
    }

    public void OnConnectClick()
    {
        if (!int.TryParse(PortInput.text, out int port))
        {
            Debug.LogWarning("Port invalide.");
            return;
        }
        string ip = IPInput.text.Trim();
        if (string.IsNullOrEmpty(ip)) { Debug.LogWarning("IP invalide."); return; }
        NetworkManager.Instance.Connect(ip, port);
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void OnHostClick()
    {
        if (!int.TryParse(PortInput.text, out int port))
        {
            Debug.LogWarning("Port invalide.");
            return;
        }
        GameServer.Instance.StartServer(port);
        NetworkManager.Instance.Connect("127.0.0.1", port);
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        if (IPDisplayText != null)
        {
            IPDisplayText.gameObject.SetActive(true);
            IPDisplayText.text = "Mes IPs :\n" + GameServer.Instance.GetLocalIPAddresses();
        }
    }
}