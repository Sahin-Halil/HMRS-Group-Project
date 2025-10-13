using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Color flashColour = Color.red;
    public float flashDuration = 0.1f;

    private Renderer objectRenderer;
    private Color originalColour;
    private HealthSystem healthSystem;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if(objectRenderer != null)
        {
            originalColour = objectRenderer.material.color;
        }

        healthSystem = GetComponent<HealthSystem>();
        if(healthSystem != null)
        {
            // Listener for health changes
            healthSystem.onHealthChanged.AddListener(OnDamageTaken);
        }
    }

    void OnDamageTaken(float currentHealth)
    {
        if(objectRenderer != null)
        {
            StartCoroutine(FlashRoutine());
        }
    }

    IEnumerator FlashRoutine()
    {
        objectRenderer.material.color = flashColour;
        yield return new WaitForSeconds(flashDuration);
        objectRenderer.material.color = originalColour;
    }
}
