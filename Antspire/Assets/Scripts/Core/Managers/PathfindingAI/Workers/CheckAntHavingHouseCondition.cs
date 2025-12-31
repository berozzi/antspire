using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CheckAntHavingHouse", story: "Checks if [self] has [home] by [hasHome]", category: "Conditions", id: "9661c32e56eb9421b67065b516ac73a9")]
public partial class CheckAntHavingHouseCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Home;
    [SerializeReference] public BlackboardVariable<bool> HasHome;

    public override bool IsTrue()
    {
        if (Home != null && Home.Value != null)
        {
            Debug.Log($"Ant has home: {Home.Value.name}");
            return true;
        }
        else
        {
            Debug.Log("Ant has NO home - should find new one");
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
