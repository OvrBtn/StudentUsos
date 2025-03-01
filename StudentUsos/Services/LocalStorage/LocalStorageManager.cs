namespace StudentUsos.Services.LocalStorage
{
    public class LocalStorageManager : ILocalStorageManager
    {
        public static ILocalStorageManager Default { get; private set; }
        public LocalStorageManager()
        {
            Default = this;
        }

        public bool ContainsData(LocalStorageKeys data)
        {
            return Preferences.ContainsKey(data.ToString());
        }

        public string GetData(LocalStorageKeys data)
        {
            if (Preferences.ContainsKey(data.ToString()) == false) throw new Exception("No local data");
            var pref = Preferences.Get(data.ToString(), "");
            return pref;
        }

        public bool TryGettingData(LocalStorageKeys data, out string result)
        {
            var pref = Preferences.Get(data.ToString(), null);
            if (pref is null)
            {
                result = "";
                return false;
            }
            result = pref;
            return true;
        }

        public void SetData(LocalStorageKeys data, string value)
        {
            Preferences.Set(data.ToString(), value);
        }

        public void Remove(LocalStorageKeys data)
        {
            Preferences.Remove(data.ToString());
        }

        public void DeleteEverything()
        {
            //shouldn't be deleted, it's needed for BackwardCompatibility to work properly
            var resetedVersions = Preferences.Get(BackwardCompatibility.PrefencesEnum.ResetedVersions.ToString(), null);
            var previousAppVersionsToReset = Preferences.Get(BackwardCompatibility.PrefencesEnum.PreviousAppVersionsToReset.ToString(), null);

            Preferences.Clear();

            if (resetedVersions is not null) Preferences.Set(BackwardCompatibility.PrefencesEnum.ResetedVersions.ToString(), resetedVersions);
            if (previousAppVersionsToReset is not null) Preferences.Set(BackwardCompatibility.PrefencesEnum.PreviousAppVersionsToReset.ToString(), previousAppVersionsToReset);
        }

    }
}
