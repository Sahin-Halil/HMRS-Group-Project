using UnityEngine;

public class LavaDamageController : MonoBehaviour
{
    public float damagePerSecond = 100f;   

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthSystem hp = other.GetComponent<HealthSystem>();
            if (hp != null)
            {
                Debug.Log("hello");
                hp?.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
        else if (other.CompareTag("Enemy"))
        {
            EnemyHealth hp = other.GetComponent<EnemyHealth>();
            hp?.TakeDamage(damagePerSecond * Time.deltaTime);
        }
    }
}
