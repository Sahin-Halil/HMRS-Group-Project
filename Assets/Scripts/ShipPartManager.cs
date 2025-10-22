using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipPartManager : MonoBehaviour
{
    [SerializeField] private float parts = 0;
    [SerializeField] private TMP_Text shipPartText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shipPartText.text = "ShipPart Count: " + parts.ToString();
    }

    public void addPart() {
        parts++;
    }
}
