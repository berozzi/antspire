using UnityEngine;

[System.Serializable]
public class PassiveIncomeSource
{
    [SerializeField] private string sourceName;
    [SerializeField] private float baseIncomePerSecond;
    [SerializeField] private bool isActive = true;

    // jeœli bêdziemy chcieli dodaæ tymczasowe boosty dla budynków to mo¿na dodaæ tutaj dodatkowe pola typu temporaryMultiplier itp.
    // dodatkowo zmieniæ GetIncomePerSecond aby uwzglêdnia³o te boosty

    public string SourceName => sourceName;
    public bool IsActive => isActive;

    public PassiveIncomeSource(string name, float baseIncome)
    {
        sourceName = name;
        baseIncomePerSecond = baseIncome;
    }
    // Zwraæa dochód na sekundê
    public float GetIncomePerSecond()
    {
        if (!isActive) return 0f;

        return baseIncomePerSecond;
    }

    // Ustawia mno¿nik efektywnoœci
    public void UpgradeBaseIncome(float upgradeAmount)
    {
        baseIncomePerSecond += upgradeAmount;
    }
    // Ustawia czy Ÿród³o jest aktywne
    public void SetActive(bool active)
    {
        isActive = active;
    }
}
