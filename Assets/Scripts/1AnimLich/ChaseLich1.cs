using UnityEngine.AI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChaseLich1 : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;
    float attackRange = 10;
    float chaseRange = 20;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        agent.speed = 4;

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(player.position);
        float distance = Vector3.Distance(animator.transform.position, player.position);


        if (distance < attackRange)
            animator.SetBool("IsAttacking", true);

        if (distance > 20)
            animator.SetBool("IsChasing", false);

    }


    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent.SetDestination(agent.transform.position);
        agent.speed = 2;
    }
}
