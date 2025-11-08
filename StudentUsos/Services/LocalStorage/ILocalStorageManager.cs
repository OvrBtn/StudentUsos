namespace StudentUsos.Services.LocalStorage;

public interface ILocalStorageManager
{
    public bool ContainsData(LocalStorageKeys key);
    public string? GetString(LocalStorageKeys key);
    public bool TryGettingString(LocalStorageKeys key, out string value);
    public void SetString(LocalStorageKeys key, string value);
    public void Remove(LocalStorageKeys key);
    public void DeleteEverything();
}