public interface ISaveable
{
    object GetSaveData();
    void LoadDataFromSave(object data);
}
