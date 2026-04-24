using UnityEngine;
using UnityEngine.AI;

public class HunterAI : MonoBehaviour
{
    public Transform player;
    public Transform[] patrolPoints;

    public float detectionRange = 6f;
    public float catchRange = 1.2f;

    public float separationRadius = 1.5f;
    public float separationStrength = 2f;

    private NavMeshAgent agent;
    private int currentPoint = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            Vector3 targetPos = player.position + GetSeparationOffset();
            agent.SetDestination(targetPos);
        }
        else
        {
            Patrol();
        }

        if (distanceToPlayer <= catchRange)
        {
            GameManager.instance.LoseGame();
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPoint++;
            if (currentPoint >= patrolPoints.Length)
            {
                currentPoint = 0;
            }

            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    Vector3 GetSeparationOffset()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 push = Vector3.zero;

        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;

            HunterAI otherHunter = col.GetComponent<HunterAI>();
            if (otherHunter != null)
            {
                Vector3 away = transform.position - otherHunter.transform.position;
                if (away != Vector3.zero)
                {
                    push += away.normalized;
                }
            }
        }

        return push * separationStrength;
    }
}