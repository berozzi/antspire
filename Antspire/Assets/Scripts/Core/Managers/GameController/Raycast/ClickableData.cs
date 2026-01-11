using System.Collections.Generic;
using UnityEngine;

public class ClickableData
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    public Sprite Icon { get; set; }
    public Dictionary<string, string> Stats { get; set; }
}