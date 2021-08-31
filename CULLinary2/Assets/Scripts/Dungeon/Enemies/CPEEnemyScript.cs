using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CPEEnemyScript : MonoBehaviour
{
    [SerializeField] private EnemyScript enemyScript;
    [SerializeField] private float distanceTriggered;
    
    // Variables for idle
    [SerializeField] private float idleTimer;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderTimer;

    // The minimum distance to wander about.
    [Tooltip("The minimum distance to wander about. Needed because of the stopping distance being large makes the enemy only wander a bit before stopping.")]
    [SerializeField] private float minDist;
    // Variables for goingBackToStart
    private float goingBackToStartTimer;

    public LineRenderer lineTest;

    // Variables for roaming
    private Vector3 roamPosition;

    // Variables for attacking
    private bool canAttack = true;
    [SerializeField] private float timeBetweenAttacks;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;
    private float timer;
    private Vector3 startingPosition;

    // The amount of distance to be considered from the final position, needs to be set to suitable value else mob will "WALK" forever
    [Tooltip("The minimum distance to stop in front of the player. Has to be equal to Stopping distance. Cannot use stopping distance directly else navmesh agent will keep bumping into player/")]
    private float reachedPositionDistance;
    // Start is called before the first frame update
    void Start()
    {
        // Get Variables from EnemyScript
        agent = enemyScript.getNavMeshAgent();
        animator = enemyScript.getAnimator();
        player = enemyScript.getPlayerReference();
        enemyScript.onEnemyRoaming += EnemyRoaming;
        enemyScript.onEnemyChase += EnemyChase;
        enemyScript.onEnemyIdle += EnemyIdle;
        enemyScript.onEnemyAttack += EnemyAttackPlayer;
        enemyScript.onEnemyReturn += EnemyReturn;
        reachedPositionDistance = agent.stoppingDistance;
        startingPosition = transform.position;
        timer = wanderTimer;
        goingBackToStartTimer = 0;
    }

    private void EnemyIdle()
	{
        animator.SetBool("isMoving", false);
        timer += Time.deltaTime;
        enemyScript.FindTarget();
        if (timer >= idleTimer)
        {
            Vector3 newPos = enemyScript.RandomNavSphere(startingPosition, wanderRadius, -1, minDist);
            agent.SetDestination(newPos);
            var points = new Vector3[2];

            points[0] = new Vector3(transform.position.x, transform.position.y, transform.position.z);
            points[1] = newPos;
            lineTest.SetPositions(points);
            timer = 0;
            enemyScript.setStateMachine(State.Roaming);
            roamPosition = newPos;
        }
    }

    private void EnemyRoaming()
	{
        animator.SetBool("isMoving", true);
        timer += Time.deltaTime;
        enemyScript.FindTarget();
        Vector3 distanceToFinalPosition = transform.position - roamPosition;
        Debug.Log(distanceToFinalPosition);
        //without this the eggplant wandering will be buggy as it may be within the Navmesh Obstacles itself
        if (timer >= wanderTimer || distanceToFinalPosition.magnitude < reachedPositionDistance)
        {
            timer = 0;
            Debug.Log("Has Set is moving to false");
            animator.SetBool("isMoving", false);
            enemyScript.setStateMachine(State.Idle);
        }
    }

    private void EnemyChase()
    {
        Vector3 playerPositionWithoutYOffset = new Vector3(player.position.x, transform.position.y, player.position.z);
        animator.SetBool("isMoving", true);
        float directionVector = Vector3.Distance(transform.position, playerPositionWithoutYOffset);
        var points = new Vector3[2];

        points[0] = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        points[1] = playerPositionWithoutYOffset;
        lineTest.SetPositions(points);
        if (directionVector <= reachedPositionDistance)
        {
            transform.LookAt(playerPositionWithoutYOffset);
            // Target within attack range
            enemyScript.setStateMachine(State.AttackTarget);
            // Add new state to attack player
        }
        else
        {
            Debug.Log("Setting destination to player offset");
            agent.SetDestination(playerPositionWithoutYOffset);
        }

        if (Vector3.Distance(transform.position, player.position) > enemyScript.getStopChaseDistance() + 0.1f)
        {
            // Too far, stop chasing
            enemyScript.setStateMachine(State.GoingBackToStart);
        }
    }

    private void EnemyAttackPlayer()
    {
        Vector3 playerPositionWithoutYOffset = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(playerPositionWithoutYOffset);
        animator.SetBool("isMoving", false);
        animator.ResetTrigger("attack");
        if (canAttack == true)
        {
            animator.SetTrigger("attack");
            canAttack = false;
            StartCoroutine(DelayFire());
        }
        float directionVector = Vector3.Distance(transform.position, playerPositionWithoutYOffset);
        if (directionVector > agent.stoppingDistance && enemyScript.getCanMoveDuringAttack())
        {
            // Target within attack range
            enemyScript.setStateMachine(State.ChaseTarget);
        }

    }

    private void EnemyReturn()
    {
        goingBackToStartTimer += Time.deltaTime;
        animator.SetBool("isMoving", true);
        transform.LookAt(startingPosition);
        agent.SetDestination(startingPosition);
        var points = new Vector3[2];

        points[0] = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        points[1] = startingPosition;
        lineTest.SetPositions(points);
        //|| goingBackToStartTimer > 4.0f
        Debug.Log(Vector3.Distance(transform.position, startingPosition));
        if (Vector3.Distance(transform.position, startingPosition) <= reachedPositionDistance)
        {
            Debug.Log("Reach start");
            // Reached Start Position
            animator.SetBool("isMoving", false);
            enemyScript.setStateMachine(State.Idle);
            goingBackToStartTimer = 0;
            // Reset timer for roaming / idle time
            timer = 0;
        }
    }


    private IEnumerator DelayFire()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);
        canAttack = true;
    }


}
