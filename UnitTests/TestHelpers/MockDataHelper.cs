using System.Text.Json;

namespace UnitTests.TestHelpers;

public static class MockDataHelper
{
    public static string LoadFile(string fileName, string folder)
    {
        return LoadFile(fileName, "Features", folder, "MockData");
    }

    public static string LoadFile(string fileName, params string[] folders)
    {
        string basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

        string fullPath = Path.Combine(new[] { basePath }.Concat(folders).Append(fileName).ToArray());

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Test data file not found: {fullPath}");
        }

        return File.ReadAllText(fullPath);
    }

    public static T? LoadJsonDeserialized<T>(string fileName, string folder)
    {
        return LoadJsonDeserialized<T>(fileName, "Features", folder, "MockData");
    }

    public static T? LoadJsonDeserialized<T>(string fileName, params string[] folders)
    {
        string basePath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName;

        string fullPath = Path.Combine(new[] { basePath }.Concat(folders).Append(fileName).ToArray());

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Test data file not found: {fullPath}");
        }

        var json = File.ReadAllText(fullPath);

        return JsonSerializer.Deserialize<T>(json);
    }
}