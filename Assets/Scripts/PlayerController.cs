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
    [SerializeField] private Vector3 move;
    [SerializeField] private float moveHorizontal = 0f;
    [SerializeField] private float moveVertical = 0f;

    // Mouse look
    [SerializeField] private float mouseSense;
    [SerializeField] private Vector2 mouse;
    [SerializeField] private float lookHorizontal = 0f;
    [SerializeField] private float lookVertical = 0f;

    // Called when movement input is detected
    private void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        moveHorizontal = moveInput.x;
        moveVertical = moveInput.y;
        //Debug.Log((transform.right.z, transform.forward.x));
    }

    // Called when mouse input is detected
    private void OnLook(InputValue value)
    {
        mouse = value.Get<Vector2>();
        //Debug.Log(mouse);
        //Debug.Log(direction);
        lookHorizontal = lookHorizontal + (mouse.x * mouseSense);
        lookVertical = Mathf.Clamp(lookVertical - (mouse.y * mouseSense), -90f, 90f);
    }

    // Runs when game starts, used for setup
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
        move = transform.right * moveHorizontal + transform.forward * moveVertical;
        characterController.SimpleMove(move * speed);
        transform.rotation = Quaternion.Euler(lookVertical, lookHorizontal, 0f);
    }
}
