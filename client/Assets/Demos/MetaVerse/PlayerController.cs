using UnityEngine;
using UnityEngine.InputSystem;
using System;

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
    const float MoveInterval = 0.05f; // 50ms

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

    void SendPosition()
    {
        if (NetworkManager.Instance == null) return;

        Vector3 pos = rb.position;
        float rotY = rb.rotation.eulerAngles.y;
        string msg = string.Format("MOVE|{0}|{1:F3}|{2:F3}|{3:F3}|{4:F3}",
            NetworkManager.Instance.PlayerId, pos.x, pos.y, pos.z, rotY);
        NetworkManager.Instance.Send(msg);
    }

    void OnDisable() {
      PlayerAction.Disable();
    }
}
