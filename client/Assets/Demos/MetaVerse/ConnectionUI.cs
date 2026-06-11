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
        string ip = IPInput.text;
        int port = int.Parse(PortInput.text);
        NetworkManager.Instance.Connect(ip, port);
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void OnHostClick()
    {
        int port = int.Parse(PortInput.text);
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