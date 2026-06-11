using UnityEngine;
using System.Globalization;

public class Bonus : MonoBehaviour
{
    public LayerMask CollisionLayers;
    public int Points = 1;
    public string BonusId;

    void Start()
    {
        BonusId = string.Format(CultureInfo.InvariantCulture,
            "{0}_{1:F2}_{2:F2}", gameObject.name,
            transform.position.x, transform.position.z);
    }

    void Update() { }

    private bool ShouldHandleObject(Collider other)
    {
        return (CollisionLayers.value & (1 << other.gameObject.layer)) > 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ShouldHandleObject(other)) return;
        NetworkManager.Instance?.Send(
            $"COLLECT|{NetworkManager.Instance.PlayerId}|{BonusId}"
        );
    }
}