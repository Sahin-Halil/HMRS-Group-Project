using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;

    // Movement
    [SerializeField] private float speed;
    [SerializeField] private Vector2 move;

    // Mouse look
    [SerializeField] private float mouseSense;
    [SerializeField] private Vector2 mouse;
    [SerializeField] private float lookHorizontal = 0;
    [SerializeField] private float lookVertical = 0;

    // Called when movement input is detected
    private void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    // Called when mouse input is detected
    private void OnLook(InputValue value)
    {
        mouse = value.Get<Vector2>();
        lookHorizontal = Mathf.Clamp(lookHorizontal - (mouse.y * mouseSense), -90f, 90f);
        lookVertical = Mathf.Clamp(lookVertical + (mouse.x * mouseSense), -90f, 90f);
    }

    // Runs before Start, used for setup
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        speed = 5f;
        mouseSense = 0.5f;
    }

    // Called every frame
    void Update()
    {
        characterController.SimpleMove(new Vector3(move.x, 0, move.y) * speed);
        transform.rotation = Quaternion.Euler(lookHorizontal, lookVertical, 0);
    }
}
