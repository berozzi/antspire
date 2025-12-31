using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckAntWorkplace", story: "Checks if [Self] has [workplace] by [hasWorkplace]", category: "Conditions", id: "62ba5722c4853b4bc5b981c992dd0128")]
public partial class CheckAntWorkplaceCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Workplace;
    [SerializeReference] public BlackboardVariable<bool> HasWorkplace;

    public override bool IsTrue()
    {
        if (Workplace != null && Workplace.Value != null)
        {
            Debug.Log($"Ant has workplace: {Workplace.Value.name}");
            return true;
        }
        else
        {
            Debug.Log("Ant has no workplace - should find new one");
            return false;
        }
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
