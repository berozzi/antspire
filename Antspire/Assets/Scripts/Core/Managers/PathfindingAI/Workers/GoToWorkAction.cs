using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GoToWork", story: "[Self] goes to [workplace] with [workDuration]", category: "Action", id: "46a85ef63938769799fef9bc0028688b")]
public partial class GoToWorkAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Workplace;
    [SerializeReference] public BlackboardVariable<float> WorkDuration;

    NavMeshAgent agent;
    float workTimer;

    protected override Status OnStart()
    {
        workTimer = 0f;
        agent = Self.Value.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = Self.Value.AddComponent<NavMeshAgent>();
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self == null || Workplace == null) return Status.Failure;
        agent.SetDestination(Workplace.Value.transform.position);
        
        float distanceToWorkplace = Vector3.Distance(Self.Value.transform.position, Workplace.Value.transform.position);

        if (distanceToWorkplace >= 3f)
        {
            // Still traveling
            agent.SetDestination(Workplace.Value.transform.position);
            return Status.Running;
        }
        else
        {
            // Arrived at workplace - now work
            agent.isStopped = true;
            workTimer += Time.deltaTime;

            if (workTimer < WorkDuration.Value)
            {
                Debug.Log($"Working: {workTimer}/{WorkDuration.Value}");
                return Status.Running;
            }
            return Status.Success;
        }
    }

    protected override void OnEnd()
    {
    }
}

