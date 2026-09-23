using UnityEngine;

/// <summary>
/// Receptura produkcji - definiuje nazwę budynku, wejście (input) i wyjście (output).
/// Wejście przyjmuje tablicę stosów przedmiotów (ItemStack).
/// </summary>
[CreateAssetMenu(fileName = "Production", menuName = "ScriptableObjects/Production/Production")]
public class Production : ScriptableObject
{
    public string buildingName;
    public ItemStack[] input = new ItemStack[0];
    public ItemStack output;
}