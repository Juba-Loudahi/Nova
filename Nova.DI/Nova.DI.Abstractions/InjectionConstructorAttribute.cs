namespace Nova.DI;

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class InjectionConstructorAttribute : Attribute
{
}
