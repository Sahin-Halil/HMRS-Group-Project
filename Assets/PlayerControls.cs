using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerControls: MonoBehaviour
{
    private CharacterController controller;

    [SerializeField] private float speed;
    //[SerializeField] private PlayerCamera;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        speed = 10f;

    }

    // Update is called once per frame
    void Update() 
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed * Time.deltaTime;
        controller.Move(move);
    }
}
