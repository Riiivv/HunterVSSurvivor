using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    public int batteryCount = 0;

    public void AddBattery(int amount)
    {
        batteryCount += amount;
        GameManager.instance.AddBattery(amount);
    }
}