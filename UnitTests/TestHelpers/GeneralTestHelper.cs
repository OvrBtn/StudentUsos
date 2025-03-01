using System.Reflection;

namespace UnitTests.TestHelpers
{
    public static class GeneralTestHelper
    {
        public static T DeepCopyReflection<T>(T obj)
        {
            var newObj = Activator.CreateInstance<T>();
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(newObj, prop.GetValue(obj));
                }
            }
            return newObj;
        }
    }
}
