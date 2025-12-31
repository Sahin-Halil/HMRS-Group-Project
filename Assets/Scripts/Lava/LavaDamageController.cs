using UnityEngine;

public class LavaDamageController : MonoBehaviour
{
    public float damagePerSecond = 25f;   

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Damage player if next to lavas
        HealthSystem hp = other.GetComponent<HealthSystem>();
        if (hp != null)
        {
            Debug.Log("hello");
            hp?.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
