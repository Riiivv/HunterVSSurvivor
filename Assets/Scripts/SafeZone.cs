using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.instance.TryWinGame();
        }
    }
}