using UnityEngine;
using UnityEngine.InputSystem;
using System.Globalization;

public enum CharacterPlayer {
    Player1,
    Player2
}

public class PlayerController : MonoBehaviour
{
    public CharacterPlayer Player = CharacterPlayer.Player1;
    public float WalkSpeed = 3;
    public float RotateSpeed = 250;

    Animator Anim;
    MetaverseInput inputs;
    InputAction PlayerAction;
    Rigidbody rb;

    float moveTimer;
    const float MoveInterval = 0.05f;

    // Zone route
    private bool isOnRoad = false;
    private float roadTimer = 0f;
    private float roadScoreInterval = 1f;

    // Voiture
    private bool isHitByCar = false;
    private float carHitCooldown = 0f;
    private const float CarHitCooldownDuration = 1.5f;
    private float carStuckTimer = 0f;
    private float carStuckPenaltyInterval = 1f;

    void Start()
    {
        Anim = GetComponent<Animator>();
        inputs = new MetaverseInput();
        switch (Player) {
            case CharacterPlayer.Player1:
                PlayerAction = inputs.Player1.Move;
                break;
            case CharacterPlayer.Player2:
                PlayerAction = inputs.Player2.Move;
                break;
        }
        PlayerAction.Enable();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 vec = PlayerAction.ReadValue<Vector2>();
        Anim.SetFloat("Walk", vec.y);

        rb.MovePosition(rb.position + transform.forward * WalkSpeed * Time.fixedDeltaTime * vec.y);
        rb.MoveRotation(rb.rotation * Quaternion.AngleAxis(RotateSpeed * Time.fixedDeltaTime * vec.x, Vector3.up));

        moveTimer += Time.fixedDeltaTime;
        if (moveTimer >= MoveInterval)
        {
            moveTimer = 0f;
            SendPosition();
        }
    }

    void Update()
    {
        if (carHitCooldown > 0f)
            carHitCooldown -= Time.deltaTime;

        // Si collé à une voiture
        if (isHitByCar)
        {
            carStuckTimer += Time.deltaTime;
            if (carStuckTimer >= carStuckPenaltyInterval)
            {
                carStuckTimer = 0f;
                ScoreManager.Instance?.AddScore(-5);
                Debug.Log("Collé à une voiture ! -5 points");
            }
            return; // Pas de points route tant que collé
        }

        // Timer route
        if (isOnRoad)
        {
            roadTimer += Time.deltaTime;
            if (roadTimer >= roadScoreInterval)
            {
                roadTimer = 0f;
                ScoreManager.Instance?.AddScore(1);
            }
        }
        else
        {
            roadTimer = 0f;
        }
    }

    // Détection zone route
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoneRoute"))
            isOnRoad = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZoneRoute"))
            isOnRoad = false;
    }

    // Collision voiture
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("Car"))
        {
            if (carHitCooldown <= 0f)
            {
                carHitCooldown = CarHitCooldownDuration;
                ScoreManager.Instance?.AddScore(-30);
                Debug.Log("Touché par une voiture ! -30 points");
            }
            isHitByCar = true;
            carStuckTimer = 0f;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("Car"))
            isHitByCar = true;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name.StartsWith("Car"))
        {
            isHitByCar = false;
            carStuckTimer = 0f;
        }
    }

    void SendPosition()
    {
        if (NetworkManager.Instance == null) return;
        Vector3 pos = rb.position;
        float rotY = rb.rotation.eulerAngles.y;
        string msg = string.Format(CultureInfo.InvariantCulture,
            "MOVE|{0}|{1:F3}|{2:F3}|{3:F3}|{4:F3}",
            NetworkManager.Instance.PlayerId, pos.x, pos.y, pos.z, rotY);
        NetworkManager.Instance.Send(msg);
    }

    void OnDisable()
    {
        PlayerAction.Disable();
    }
}