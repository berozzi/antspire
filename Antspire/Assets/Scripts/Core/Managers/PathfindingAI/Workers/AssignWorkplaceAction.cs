using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AssignWorkplace", story: "Assigns [workplace] to [self] checks with [hasWorkplace]", category: "Action", id: "5952cd48d5a34a2962b424d669e369f2")]
public partial class AssignWorkplaceAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Workplace;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> HasWorkplace;

    float maxSearchDistance = 500f;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;
        if (Workplace != null && Workplace.Value != null)
        {
            HasWorkplace.Value = true;
            return Status.Success;
        }
        Workplace[] allWorkplaces = GameObject.FindObjectsByType<Workplace>(FindObjectsSortMode.None);
        Workplace closestWorkplace = null;
        float closestDistance = float.MaxValue;
        foreach (var workplace in allWorkplaces)
        {
            float distance = Vector3.Distance(
                Self.Value.transform.position,
                workplace.transform.position
            );

            if (distance < closestDistance && distance <= maxSearchDistance)
            {
                closestDistance = distance;
                closestWorkplace = workplace;
            }
        }

        if (closestWorkplace != null)
        {
            Workplace.Value = closestWorkplace.gameObject;
            HasWorkplace.Value = true;
            Debug.Log($"{Self.Value.name} znalazł miejsce pracy: {closestWorkplace.name}");
            return Status.Success;
        }
        else
        {
            HasWorkplace.Value = false;
            Debug.LogWarning($"{Self.Value.name} nie znalazł domu w promieniu {maxSearchDistance}");
            return Status.Failure;
        }
    }

    protected override void OnEnd()
    {
    }
}

