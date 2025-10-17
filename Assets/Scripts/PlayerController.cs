using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    private CharacterController characterController;
    [SerializeField] private float speed;
    [SerializeField] private Vector2 move;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        speed = 5f;
    }
    private void OnMove(InputValue value) {
        move = value.Get<Vector2>();
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        characterController.SimpleMove(new Vector3(move.x, 0, move.y) * speed);        
    }
}
