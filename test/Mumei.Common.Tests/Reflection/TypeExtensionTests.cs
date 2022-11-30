using Mumei.Common.Reflection;
using Mumei.Common.Reflection.Members;

namespace Mumei.Common.Tests.Reflection;

public sealed class TypeExtensionTests {
  [Fact]
  public void IsRuntimeType_ReturnsTrue_WhenTypeIsRuntimeType() {
    var type = typeof(string);

    type.IsRuntimeType().Should().BeTrue();
  }

  [Fact]
  public void IsRuntimeType_ReturnsFalse_WhenTypeIsNotRuntimeType() {
    var mumeiType = ReflectionType.Create(
      "test",
      "ns",
      null,
      Type.EmptyTypes,
      Type.EmptyTypes,
      Array.Empty<MethodInfoSpec>(),
      Array.Empty<FieldInfoSpec>(),
      Array.Empty<PropertyInfoSpec>(),
      ReflectionModule.Create("", null)
    );

    mumeiType.IsRuntimeType().Should().BeFalse();
  }
}