namespace Mumei.CodeGen.Components;

/// <summary>
/// Declares a method, field, property or other member as an output member for the synthetic type.
/// By default, any members of a synthetic type definition are considered implementation details and
/// therefore not generated into the final type emitted to the compiler. Marking a member with this attribute
/// indicates that the member should be included in the final implementation.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.GenericParameter | AttributeTargets.Constructor)]
public sealed class OutputAttribute : Attribute {
    public string? Name { get; set; }
}

/// <summary>
/// Declares a property as an input member for the synthetic type definition.
/// When the type is constructed, the compiler will ensure that the property is assigned during construction.
/// Nullable properties may be left unassigned.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class InputAttribute : Attribute;

/// <summary>
/// Declares a type parameter as bindable in a synthetic type definition
/// meaning it is not part of the actual type declaration but instead
/// will resolve to a concrete type when the type is constructed.
/// E.g.:
/// <code>
///  public class AwesomeClassDef{[Bindable] T> : SyntheticClassDefinition{AwesomeClassDef{T>> {
///     [Output]
///     public T Value { get; set; }
///     <br />
///     [Input]
///     public ISyntheticType ActualType { get; set; }
///     <br />
///     public override void SetupDynamic(ISyntheticClassBuilder{AwesomeClassDef{T>> classBuilder) {
///         classBuilder.Bind{T}(ActualType);
///     }
///  }
/// </code>
/// Would emit a class similar to:
/// <code>
/// public class AwesomeClass {
///   public string Value { get; set; }
/// }
/// </code>
/// Assuming the type parameter T was bound to string during construction.
/// <remarks>
/// The compiler will ensure that all bindable type parameters are bound during construction.
/// All type parameters not explicitly marked as an output via <see cref="OutputAttribute"/> are considered bindable by default.
/// </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.GenericParameter)]
public sealed class BindableAttribute : Attribute;