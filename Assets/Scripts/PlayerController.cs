using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    private CharacterController characterController;
    
    [SerializeField] private float speed;
    [SerializeField] private Vector2 move;

    [SerializeField] private float mouseSense;
    [SerializeField] private Vector2 mouse;
    [SerializeField] private float lookHorizontal = 0;
    [SerializeField] private float lookVertical = 0;

    private void OnMove(InputValue value) {
        move = value.Get<Vector2>();
    }
    private void OnLook(InputValue value) { 
        Debug.Log("here is the coords += " + value.Get<Vector2>());
        mouse = value.Get<Vector2>();
        lookHorizontal -= mouse.y * mouseSense;
        lookVertical += mouse.x * mouseSense;
    }

    // Awake is called before the first frame update
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        speed = 5f;
        mouseSense = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        characterController.SimpleMove(new Vector3(move.x, 0, move.y) * speed);
        //transform.Rotate(new Vector3(0, mouse.x, -mouse.y) * mouseSense * Time.deltaTime, Space.World);
        transform.rotation = Quaternion.Euler(lookHorizontal, lookVertical, 0);
    }
}
