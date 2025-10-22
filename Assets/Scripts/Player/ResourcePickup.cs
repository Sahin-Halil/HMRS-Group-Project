using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public int resourceValue = 1;
    public string resourceType = "Coin";

    [Header("Visual")]
    public float rotationSpeed = 50f;
    public float bobSpeed = 1f;
    public float bobHeight = 0.1f;

    private Vector3 startPosition;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player picked it up
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player collected {resourceType}! Value: {resourceValue}");
            Destroy(gameObject);
        }
    }
}
