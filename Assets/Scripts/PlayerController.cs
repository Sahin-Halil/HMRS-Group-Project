using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Components
    private CharacterController characterController;

    // Movement
    [SerializeField] private float standingSpeed;
    [SerializeField] private Vector3 move;
    [SerializeField] private float xMove;
    [SerializeField] private float yMove;

    // Mouse look
    [SerializeField] private float mouseSense;
    [SerializeField] private float xRotation;
    [SerializeField] private float yRotation;

    private bool crouchInput;
    private float crouchSpeed;
    private float crouchHeight;

    // Called when movement input is detected
    private void OnMove(InputValue value)
    {
        Vector2 moveInput = value.Get<Vector2>();
        if (!crouchInput)
        {
            xMove = moveInput.x * standingSpeed;
            yMove = moveInput.y * standingSpeed;
        }
        else{
            xMove = moveInput.x * crouchSpeed;
            yMove = moveInput.y * crouchSpeed;
        }
    }

    // Called when mouse input is detected
    private void OnLook(InputValue value)
    {
        Vector2 mouseInput = value.Get<Vector2>();
        //Debug.Log(mouse);
        //Debug.Log(direction);
        xRotation = xRotation + (mouseInput.x * mouseSense);
        yRotation = Mathf.Clamp(yRotation - (mouseInput.y * mouseSense), -90f, 90f);
    }

    private void OnCrouch() {
        crouchInput = !crouchInput;
        characterController.height -= crouchHeight;
        crouchHeight *= -1f;
    }

    // Runs when game starts, used for setup
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        standingSpeed = 5f;
        xMove = 0f;
        yMove = 0f;
        mouseSense = 0.5f;
        xRotation = 0f;
        yRotation = 0f;
        crouchInput = false;
        crouchSpeed = 0.5f * standingSpeed;
        crouchHeight = 0.25f * characterController.height;
    }

    // Called every frame
    void Update()
    {
        move = transform.right * xMove + transform.forward * yMove;
        characterController.SimpleMove(move);
        transform.rotation = Quaternion.Euler(yRotation, xRotation, 0f);
    }
}
