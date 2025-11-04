using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AssignHouseToAnt", story: "Action assigns [home] to [self] checks with [hasHome]", category: "Action", id: "9c180e3c822a4b6b21a04fe8e41faeb0")]
public partial class AssignHouseToAntAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Home;
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<bool> HasHome;
    
    float maxSearchDistance = 500f;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Self == null || Self.Value == null)
            return Status.Failure;

        // Sprawd� czy ju� ma dom
        if (Home != null && Home.Value != null)
        {
            HasHome.Value = true;
            return Status.Success;
        }

        // Szukaj domu
        House[] allHouses = GameObject.FindObjectsByType<House>(FindObjectsSortMode.None);

        House closestHouse = null;
        float closestDistance = float.MaxValue;

        foreach (var house in allHouses)
        {
            float distance = Vector3.Distance(
                Self.Value.transform.position,
                house.transform.position
            );

            if (distance < closestDistance && distance <= maxSearchDistance)
            {
                closestDistance = distance;
                closestHouse = house;
            }
        }

        if (closestHouse != null)
        {
            Home.Value = closestHouse.gameObject;
            HasHome.Value = true;
            Debug.Log($"{Self.Value.name} znalazł dom: {closestHouse.name}");
            return Status.Success;
        }
        else
        {
            HasHome.Value = false;
            Debug.LogWarning($"{Self.Value.name} nie znalazł domu w promieniu {maxSearchDistance}");
            return Status.Failure;
        }
    }

    protected override void OnEnd()
    {
    }
}

