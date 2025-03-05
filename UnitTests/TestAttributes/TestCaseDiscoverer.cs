using Xunit.Abstractions;
using Xunit.Sdk;

namespace UnitTests.TestAttributes;

public class TestCaseDiscoverer : ITraitDiscoverer
{
    const string Key = "Category";
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        string testCase;
        var attributeInfo = traitAttribute as ReflectionAttributeInfo;
        var testCaseAttribute = attributeInfo?.Attribute as CategoryAttribute;
        if (testCaseAttribute != null)
        {
            testCase = testCaseAttribute.Category;
        }
        else
        {
            var constructorArguments = traitAttribute.GetConstructorArguments().ToArray();
            testCase = constructorArguments[0].ToString();
        }
        yield return new KeyValuePair<string, string>(Key, testCase);
    }
}