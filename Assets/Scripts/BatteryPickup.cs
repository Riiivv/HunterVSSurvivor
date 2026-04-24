using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    public int value = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerCollector collector = other.GetComponent<PlayerCollector>();
            if (collector != null)
            {
                collector.AddBattery(value);
                Destroy(gameObject);
            }
        }
    }
}