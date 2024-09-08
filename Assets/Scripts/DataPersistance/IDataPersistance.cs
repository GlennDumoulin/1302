public interface IDataPersistance<TData>
{
    void LoadData(TData data);

    void SaveData(ref TData data);
}
