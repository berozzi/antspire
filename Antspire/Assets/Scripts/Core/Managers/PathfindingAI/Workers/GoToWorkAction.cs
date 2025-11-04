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
    float workTimer = 0f;

    protected override Status OnStart()
    {
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
        workTimer += Time.deltaTime;
        if (Vector3.Distance(Self.Value.transform.position, Workplace.Value.transform.position) < 3f)
        {
            if (workTimer < WorkDuration.Value)
            {
                UnityEngine.Debug.Log($"[GoToWorkAction] {Self.Value.name} is working: {workTimer}/{WorkDuration.Value} seconds");
                return Status.Running;
            }
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

