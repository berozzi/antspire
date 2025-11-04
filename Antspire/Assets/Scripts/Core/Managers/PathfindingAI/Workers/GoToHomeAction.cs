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

    NavMeshAgent agent;

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
        if (Self == null || Home == null) return Status.Failure;
        agent.SetDestination(Home.Value.transform.position);
        // myœlê ¿e mo¿na tutaj ogarn¹æ odpoczynek w domu
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

