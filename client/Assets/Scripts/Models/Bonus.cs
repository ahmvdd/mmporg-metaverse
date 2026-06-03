using UnityEngine;

public class Bonus : MonoBehaviour
{
    public LayerMask CollisionLayers;
    public int Points = 1;
    public string BonusId;

    void Start()
    {
        BonusId = gameObject.name + "_" + transform.position.x + "_" + transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private bool ShouldHandleObject(Collider other)
    {
        return (CollisionLayers.value & (1 << other.gameObject.layer)) > 0;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ShouldHandleObject(other)) { return; }

        CharacterScore cScore = other.gameObject.GetComponentInChildren<CharacterScore>();
        if (cScore != null)
        {
            cScore.AddScore(Points);
        }

        Destroy(gameObject);
    }
}
