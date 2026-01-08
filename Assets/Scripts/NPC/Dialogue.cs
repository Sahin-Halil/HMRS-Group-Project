using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Dialogue : MonoBehaviour
{
    [Header("Dialogue Content")]
    [SerializeField] private string characterName;
    [SerializeField] [TextArea(3, 10)] private string dialogueText;
    [SerializeField] private float displayDuration = 5f;

    [Header("UI References")]
    [SerializeField] private GameObject dialogueBoxUI;

    private UIManager uiManager;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI textField;
    private float currentDisplayTime = 0f;
    private bool isDisplaying = false;
    private bool wasActiveBeforePause = false;
    private bool hasPlayed = false;
    private static bool dialogueActive = false;
    private static Queue<Dialogue> dialogueQueue = new Queue<Dialogue>();

    private void Start()
    {
        // Find UIManager
        uiManager = Object.FindFirstObjectByType<UIManager>();
        
        if (uiManager == null)
        {
            Debug.LogWarning("UIManager not found in scene! Pause functionality may not work.");
        }

        // Find the text components in dialogueBoxUI children
        if (dialogueBoxUI != null)
        {
            Transform nameTransform = dialogueBoxUI.transform.Find("CharacterName");
            if (nameTransform != null)
            {
                nameText = nameTransform.GetComponent<TextMeshProUGUI>();
            }
            
            Transform dialogueTransform = dialogueBoxUI.transform.Find("DialogueText");
            if (dialogueTransform != null)
            {
                textField = dialogueTransform.GetComponent<TextMeshProUGUI>();
            }

            // Ensure dialogue box is hidden at start
            dialogueBoxUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isDisplaying) return;

        // Check if game is paused
        if (uiManager != null && uiManager.getPauseState())
        {
            // If paused and dialogue was active, hide it and remember state
            if (dialogueBoxUI.activeSelf)
            {
                dialogueBoxUI.SetActive(false);
                wasActiveBeforePause = true;
            }
            return; // Don't update timer while paused
        }
        else
        {
            // If unpaused and it was active before pause, show it again
            if (wasActiveBeforePause)
            {
                dialogueBoxUI.SetActive(true);
                wasActiveBeforePause = false;
            }
        }

        // Update display timer
        currentDisplayTime += Time.deltaTime;

        if (currentDisplayTime >= displayDuration)
        {
            EndDialogue();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hasPlayed) return;

        hasPlayed = true;

        if (!dialogueActive)
        {
            dialogueActive = true;
            StartDialogue();
        }
        else
        {
            dialogueQueue.Enqueue(this);
        }
    }

    private void StartDialogue()
    {
        if (string.IsNullOrEmpty(dialogueText))
        {
            Debug.LogWarning("No dialogue text set for " + gameObject.name);
            return;
        }

        isDisplaying = true;
        currentDisplayTime = 0f;
        
        // Update UI elements
        if (nameText != null)
            nameText.text = characterName;
        
        if (textField != null)
            textField.text = dialogueText;

        // Show dialogue box if not paused
        if (dialogueBoxUI != null && (uiManager == null || !uiManager.getPauseState()))
        {
            dialogueBoxUI.SetActive(true);
        }
    }

    private void EndDialogue()
    {
        isDisplaying = false;
        currentDisplayTime = 0f;
        wasActiveBeforePause = false;

        if (dialogueBoxUI != null)
            dialogueBoxUI.SetActive(false);

        dialogueActive = false;

        if (dialogueQueue.Count > 0)
        {
            Dialogue next = dialogueQueue.Dequeue();
            dialogueActive = true;
            next.isDisplaying = true;
            next.currentDisplayTime = 0f;
            
            if (next.nameText != null)
                next.nameText.text = next.characterName;
            
            if (next.textField != null)
                next.textField.text = next.dialogueText;

            if (next.dialogueBoxUI != null)
                next.dialogueBoxUI.SetActive(true);
        }
    }
}