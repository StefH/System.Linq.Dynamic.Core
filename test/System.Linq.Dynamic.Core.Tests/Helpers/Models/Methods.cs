namespace System.Linq.Dynamic.Core.Tests.Helpers.Models
{
    public class Methods
    {
        public bool Method1(int value)
        {
            return value == 1;
        }

        public bool Method2(object value)
        {
            return value != null && (int)value == 1;
        }

        public bool Method3(object value)
        {
            Item item = value as Item;
            return item != null && item.Value == 1;
        }

        public class Item
        {
            public int Value { get; set; }
        }

        public static bool StaticGenericMethod<T>(T value) => value is Item item && item.Value == 1;
        public bool GenericMethod<T>(T value) => value is Item item && item.Value == 1;

    }

    public static class MethodsItemExtension
    {
        public class DummyFunctions { }
        public static DummyFunctions Functions => new DummyFunctions();

        public static T EfCoreCollate<T>(this DummyFunctions _, T value, string collation) => value;
    }
}
