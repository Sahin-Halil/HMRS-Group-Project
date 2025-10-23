using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPartManager : MonoBehaviour
{
    // Ship part tracking and UI reference
    [SerializeField] private float parts = 0;
    [SerializeField] private TMP_Text shipPartText;

    // Updates ship part count display each frame
    void Update()
    {
        shipPartText.text = "ShipPart Count: " + parts.ToString();
    }

    // Increases collected ship part count
    public void addPart()
    {
        parts++;
    }

    // Returns current number of collected ship parts
    public float getParts()
    {
        return parts;
    }
}
