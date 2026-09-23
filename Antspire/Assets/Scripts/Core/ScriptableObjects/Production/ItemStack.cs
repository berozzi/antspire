using System;
using UnityEngine;

/// <summary>
/// Stos przedmiotu - wiąże definicję zasobu (ResourceDef) z jego ilością.
/// Używany m.in. jako element tablicy wejściowej w recepturach produkcji.
/// </summary>
[Serializable]
public class ItemStack
{
    [SerializeField] private ResourceDef resource;
    [SerializeField] private int amount = 1;

    /// <summary>Definicja zasobu w tym stosie.</summary>
    public ResourceDef Resource => resource;

    /// <summary>Ilość danego zasobu.</summary>
    public int Amount => amount;

    /// <summary>Zwróć definicję zasobu w tym stosie.</summary>
    public bool IsEmpty => resource == null || amount <= 0;
}