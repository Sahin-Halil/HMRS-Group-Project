using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    private float speed;
    private Vector2 motion;
    // Start is called before the first frame update
    void Start()
    {
        speed = 20;
    }

    // Update is called once per frame
    void Update()
    {
        motion = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        transform.Translate(motion * speed * Time.deltaTime);
    }
}
