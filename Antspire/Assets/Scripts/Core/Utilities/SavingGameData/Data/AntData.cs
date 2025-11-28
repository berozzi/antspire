using System;
using UnityEngine;
[Serializable]
public class AntData
{
    public string name;
    public int id;
    //public Vector3 position; // zapis bêdzie dostosowany do Cell position dziêki czemu unikniemy problemów z precyzj¹ float
    public Vector2Int position;
    public AntState state;
    public House assignedHouse;
    public Workplace assignedWorkplace;
    public int health;
    public int carriedResourcesCount;
}
