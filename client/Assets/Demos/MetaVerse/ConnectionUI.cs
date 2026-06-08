using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionUI : MonoBehaviour
{
    public TMP_InputField IPInput;
    public TMP_InputField PortInput;
    public Button ConnectButton;
    public Button HostButton;

    void Start()
    {
        ConnectButton.onClick.AddListener(OnConnectClick);
        HostButton.onClick.AddListener(OnHostClick);
        IPInput.text = "127.0.0.1";
        PortInput.text = "5555";
    }

    public void OnConnectClick()
    {
        string ip = IPInput.text;
        int port = int.Parse(PortInput.text);
        NetworkManager.Instance.Connect(ip, port);
        gameObject.SetActive(false);
    }

    public void OnHostClick()
    {
        int port = int.Parse(PortInput.text);
        GameServer.Instance.StartServer(port);
        NetworkManager.Instance.Connect("127.0.0.1", port);
        gameObject.SetActive(false);
    }
}