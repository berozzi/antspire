using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AntResting", story: "[Self] is having a break with [restTime]", category: "Action", id: "ec677f2e1381a551a749ff5352df21ba")]
public partial class AntRestingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> RestTime;
    Ant ant => Self.Value.GetComponent<Ant>();
    NavMeshAgent agent;
    float timer;

    protected override Status OnStart()
    {
        ant.SetState(AntState.Resting);
        timer = 20f;
        agent = Self.Value.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = Self.Value.AddComponent<NavMeshAgent>();
        }
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        agent.isStopped = true; // Stop movement while resting
        timer += Time.deltaTime;
        
        //UnityEngine.Debug.Log($"Ant is resting: {timer}/{RestTime.Value} seconds");
        if (timer >= RestTime.Value)
        { // Resume movement after resting
            agent.isStopped = false;
            return Status.Success;
        }
        return Status.Running;
    }


    protected override void OnEnd()
    {
    }
}

