using UnityEngine;

public class KeyCard : MonoBehaviour
{
    public int cardNumber; // For tracking unique cards

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (KeyCardManager.Instance != null)
            {
                KeyCardManager.Instance.CollectCard(cardNumber);
            }

            gameObject.SetActive(false);
        }
    }
}