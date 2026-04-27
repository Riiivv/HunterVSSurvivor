using UnityEngine;
using UnityEngine.AI;

public class HunterAI : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }

    [Header("State")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("References")]
    public Transform player;
    public Transform[] patrolPoints;

    [Header("Ranges")]
    public float detectionRange = 15f;
    public float attackRange = 0.8f;

    [Header("Movement")]
    public float patrolSpeed = 2.5f;
    public float chaseSpeed = 4f;
    public float rotationSpeed = 8f;

    [Header("Idle")]
    public float idleTime = 1f;

    [Header("Flocking")]
    public float flockRadius = 4f;
    public float separationWeight = 0.5f;
    public float alignmentWeight = 0.1f;
    public float cohesionWeight = 0.1f;

    private NavMeshAgent agent;
    private int patrolIndex = 0;
    private float idleTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.isStopped = false;

        ChangeState(EnemyState.Patrol);
        GoToPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chase);
        }
        else if (currentState == EnemyState.Chase || currentState == EnemyState.Attack)
        {
            ChangeState(EnemyState.Patrol);
            GoToPatrolPoint();
        }

        switch (currentState)
        {
            case EnemyState.Idle:
                DoIdle();
                break;

            case EnemyState.Patrol:
                DoPatrol();
                break;

            case EnemyState.Chase:
                DoChase();
                break;

            case EnemyState.Attack:
                DoAttack();
                break;
        }

        RotateEnemy();
    }

    void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        if (newState == EnemyState.Idle)
        {
            agent.isStopped = true;
            idleTimer = idleTime;
        }
        else if (newState == EnemyState.Patrol)
        {
            agent.isStopped = false;
            agent.speed = patrolSpeed;
        }
        else if (newState == EnemyState.Chase)
        {
            agent.isStopped = false;
            agent.speed = chaseSpeed;
        }
        else if (newState == EnemyState.Attack)
        {
            agent.isStopped = true;
        }
    }

    void DoIdle()
    {
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            patrolIndex++;

            if (patrolIndex >= patrolPoints.Length)
            {
                patrolIndex = 0;
            }

            ChangeState(EnemyState.Patrol);
            GoToPatrolPoint();
        }
    }

    void DoPatrol()
    {
        if (patrolPoints.Length == 0) return;

        agent.isStopped = false;
        agent.speed = patrolSpeed;

        if (!agent.hasPath)
        {
            GoToPatrolPoint();
        }

        if (!agent.pathPending && agent.remainingDistance <= 0.5f)
        {
            ChangeState(EnemyState.Idle);
        }
    }

    void DoChase()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(player.position, out hit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void DoAttack()
    {
        agent.isStopped = true;

        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        GameManager.instance.LoseGame();
    }

    void GoToPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(patrolPoints[patrolIndex].position, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void RotateEnemy()
    {
        if (currentState == EnemyState.Attack) return;

        Vector3 direction = agent.velocity;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    Vector3 GetFlockingDirection()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, flockRadius);

        Vector3 separation = Vector3.zero;
        Vector3 alignment = Vector3.zero;
        Vector3 cohesion = Vector3.zero;

        int count = 0;

        foreach (Collider col in nearby)
        {
            if (col.gameObject == gameObject) continue;

            HunterAI other = col.GetComponent<HunterAI>();

            if (other == null) continue;

            count++;

            Vector3 away = transform.position - other.transform.position;
            away.y = 0f;

            if (away.sqrMagnitude > 0.01f)
            {
                separation += away.normalized / away.magnitude;
            }

            Vector3 otherForward = other.transform.forward;
            otherForward.y = 0f;
            alignment += otherForward;

            Vector3 otherPosition = other.transform.position;
            otherPosition.y = transform.position.y;
            cohesion += otherPosition;
        }

        if (count == 0)
        {
            return Vector3.zero;
        }

        alignment /= count;
        cohesion = (cohesion / count) - transform.position;
        cohesion.y = 0f;

        Vector3 flock =
            separation.normalized * separationWeight +
            alignment.normalized * alignmentWeight +
            cohesion.normalized * cohesionWeight;

        if (flock.magnitude > 1f)
        {
            flock.Normalize();
        }

        return flock;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("YOU LOSE");
        }
    }
}