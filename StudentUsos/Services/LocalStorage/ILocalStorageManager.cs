namespace StudentUsos.Services.LocalStorage;

public interface ILocalStorageManager
{
    public bool ContainsData(LocalStorageKeys data);
    public string? GetData(LocalStorageKeys data);
    public bool TryGettingData(LocalStorageKeys data, out string result);
    public void SetData(LocalStorageKeys data, string value);
    public void Remove(LocalStorageKeys data);
    public void DeleteEverything();
}