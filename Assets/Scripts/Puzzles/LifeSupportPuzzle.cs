using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LifeSupportPuzzle : PuzzleConsole
{
    // Puzzle Settings and UI
    public int resourcesPerType = 2;

    public Transform resourcesContainer;
    public Transform slotsContainer;
    public GameObject resourceButtonPrefab;
    public GameObject slotPrefab;
    public TMP_Text instructionText;

    // Resource Settings
    public Color oxygenColor = Color.cyan;
    public Color nitrogenColor = Color.yellow;
    public Color co2Color = Color.gray;

    private List<ResourceButton> allResources;
    private List<ResourceSlot> allSlots;
    private ResourceButton selectedResource = null;

    protected override void OnPuzzleStart()
    {
        allResources = new List<ResourceButton>();
        allSlots = new List<ResourceSlot>();
        selectedResource = null;

        GeneratePuzzle();

        if (instructionText != null)
        {
            instructionText.text = "Click a resource, then click its matching slot!";
        }
    }

    void GeneratePuzzle()
    {
        foreach (Transform child in resourcesContainer)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in slotsContainer)
        {
            Destroy(child.gameObject);
        }
        allResources.Clear();
        allSlots.Clear();

        // Create slots for each resource type
        CreateSlot(ResourceType.Oxygen, "O2", oxygenColor);
        CreateSlot(ResourceType.Nitrogen, "N2", nitrogenColor);
        CreateSlot(ResourceType.CO2, "CO2", co2Color);

        // Create shuffled resources
        List<ResourceType> resourceTypes = new List<ResourceType>();
        for (int i = 0; i < resourcesPerType; i++)
        {
            resourceTypes.Add(ResourceType.Oxygen);
            resourceTypes.Add(ResourceType.Nitrogen);
            resourceTypes.Add(ResourceType.CO2);
        }

        for (int i = 0; i < resourceTypes.Count; i++)
        {
            ResourceType temp = resourceTypes[i];
            int randomIndex = Random.Range(i, resourceTypes.Count);
            resourceTypes[i] = resourceTypes[randomIndex];
            resourceTypes[randomIndex] = temp;
        }

        foreach (ResourceType type in resourceTypes)
        {
            CreateResource(type);
        }
    }

    void CreateResource(ResourceType type)
    {
        GameObject obj = Instantiate(resourceButtonPrefab, resourcesContainer);
        ResourceButton resource = obj.GetComponent<ResourceButton>();

        if (resource != null)
        {
            Color color = GetColorForType(type);
            string label = GetLabelForType(type);
            resource.Initialize(type, color, label, this);
            allResources.Add(resource);
        }
    }

    void CreateSlot(ResourceType type, string label, Color color)
    {
        GameObject obj = Instantiate(slotPrefab, slotsContainer);
        ResourceSlot slot = obj.GetComponent<ResourceSlot>();

        if (slot != null)
        {
            slot.Initialize(type, label, color, this);
            allSlots.Add(slot);
        }
    }

    Color GetColorForType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Oxygen: return oxygenColor;
            case ResourceType.Nitrogen: return nitrogenColor;
            case ResourceType.CO2: return co2Color;
            default: return Color.white;
        }
    }

    string GetLabelForType(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Oxygen: return "O2";
            case ResourceType.Nitrogen: return "N2";
            case ResourceType.CO2: return "CO2";
            default: return "?";
        }
    }

    public void OnResourceClicked(ResourceButton resource)
    {
        if (selectedResource != null)
        {
            selectedResource.SetSelected(false);
        }

        selectedResource = resource;
        resource.SetSelected(true);
    }

    public void OnSlotClicked(ResourceSlot slot)
    {
        if (selectedResource == null)
        {
            return;
        }

        if (slot.IsFilled())
        {
            return;
        }

        if (selectedResource.resourceType == slot.requiredType)
        {
            slot.FillSlot(selectedResource);
            selectedResource.gameObject.SetActive(false);
            allResources.Remove(selectedResource);
            selectedResource = null;

            CheckSolution();
        }
        else
        {
            if (instructionText != null)
            {
                StartCoroutine(ShowWrongMessage());
            }
            selectedResource.SetSelected(false);
            selectedResource = null;
        }
    }

    void CheckSolution()
    {
        bool allFilled = true;

        foreach (ResourceSlot slot in allSlots)
        {
            if (!slot.IsFilled())
            {
                allFilled = false;
                break;
            }
        }

        if (allFilled)
        {
            StartCoroutine(PuzzleComplete());
        }
    }

    IEnumerator PuzzleComplete()
    {
        if (instructionText != null)
        {
            instructionText.text = "SUCCESS! Filters aligned!";
            instructionText.color = Color.green;
        }

        yield return new WaitForSeconds(2f);

        PuzzleSolved();
    }

    IEnumerator ShowWrongMessage()
    {
        string originalText = instructionText.text;
        Color originalColor = instructionText.color;

        instructionText.text = "WRONG! That doesn't match!";
        instructionText.color = Color.red;

        yield return new WaitForSeconds(1.5f);

        instructionText.text = originalText;
        instructionText.color = originalColor;
    }

    protected override void OnPuzzleComplete()
    {
        // Cleanup
    }
}

public enum ResourceType
{
    Oxygen,
    Nitrogen,
    CO2
}