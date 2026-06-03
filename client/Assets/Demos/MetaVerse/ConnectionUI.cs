using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConnectionUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField IPField;
    public TMP_InputField PortField;
    public Button JoinButton;
    public TMP_Text ErrorText;
    public GameObject Panel;

    void Start()
    {
        IPField.text = "127.0.0.1";
        PortField.text = "5555";
        ErrorText.text = "";
        JoinButton.onClick.AddListener(OnJoinClicked);
    }

    void OnJoinClicked()
    {
        ErrorText.text = "";

        string ip = IPField.text.Trim();
        if (string.IsNullOrEmpty(ip))
        {
            ShowError("Veuillez saisir une adresse IP.");
            return;
        }

        if (!int.TryParse(PortField.text.Trim(), out int port) || port < 1 || port > 65535)
        {
            ShowError("Port invalide (1-65535).");
            return;
        }

        bool connected = NetworkManager.Instance.Connect(ip, port);
        if (connected)
        {
            Panel.SetActive(false);
        }
        else
        {
            ShowError("Connexion échouée. Vérifiez l'IP et le port.");
        }
    }

    void ShowError(string message)
    {
        ErrorText.text = message;
    }
}
