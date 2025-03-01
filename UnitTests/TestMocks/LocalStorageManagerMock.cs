using StudentUsos.Services.LocalStorage;

namespace UnitTests.TestMocks
{
    public class LocalStorageManagerMock : ILocalStorageManager
    {
        Dictionary<string, string> dictionary = new();
        public bool ContainsData(LocalStorageKeys data)
        {
            return dictionary.ContainsKey(data.ToString());
        }

        public void DeleteEverything()
        {
            dictionary = new();
        }

        public string GetData(LocalStorageKeys data)
        {
            return dictionary[data.ToString()];
        }

        public void Remove(LocalStorageKeys data)
        {
            dictionary.Remove(data.ToString());
        }

        public void SetData(LocalStorageKeys data, string value)
        {
            dictionary[data.ToString()] = value;
        }

        public bool TryGettingData(LocalStorageKeys data, out string result)
        {
            if (dictionary.ContainsKey(data.ToString()))
            {
                result = dictionary[data.ToString()];
                return true;
            }
            result = string.Empty;
            return false;
        }
    }
}
