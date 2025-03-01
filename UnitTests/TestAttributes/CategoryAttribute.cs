using Xunit.Sdk;

namespace UnitTests.TestAttributes
{
    //source https://kenbonny.net/creating-a-custom-xunit-trait

    [TraitDiscoverer("UnitTests.TestAttributes.TestCaseDiscoverer", "UnitTests")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class CategoryAttribute : Attribute, ITraitAttribute
    {
        public string Category { get; set; }
        public CategoryAttribute(Categories category)
        {
            Category = category.ToString();
        }
    }
}
