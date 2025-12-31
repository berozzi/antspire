using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GoToHome", story: "[Self] goes to [home]", category: "Action", id: "2bf9e9d2114bc9f50c691444d4c18f3f")]
public partial class GoToHomeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Home;
    Ant ant => Self.Value.GetComponent<Ant>();
    NavMeshAgent agent;

    protected override Status OnStart()
    {
        ant.SetState(AntState.MovingToHouse);
        agent = Self.Value.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = Self.Value.AddComponent<NavMeshAgent>();
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        agent.isStopped = false;
        if (Self == null || Home == null) return Status.Failure;
        // Ustaw cel tylko raz
        if (agent.destination != Home.Value.transform.position)
        {
            agent.SetDestination(Home.Value.transform.position);
            //Debug.Log($"{Self.Value.name} idzie do domu: {Home.Value.name}");
        }

        if (Vector3.Distance(Self.Value.transform.position, Home.Value.transform.position) < 3f)
        {
              
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

