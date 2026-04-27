using UnityEngine;
using UnityEngine.AI;

public class HunterSimple : MonoBehaviour
{
    public Transform player;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player == null) return;

        agent.SetDestination(player.position);
    }
}