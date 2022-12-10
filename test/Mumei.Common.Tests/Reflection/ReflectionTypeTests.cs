using System.Reflection;
using Mumei.Common.Reflection;
using Mumei.Common.Reflection.Members;

namespace Mumei.Common.Tests.Reflection;

public sealed class ReflectionTypeTests {
  [Fact]
  public void MakeGenericType_CreatesGenericRuntimeType_WhenCalleeIsRuntimeType() {
   // var reflectionType = ReflectionType.Create(
   //   "TestType",
   //   "Namespace",
   //   null,
   //   Type.EmptyTypes,
   //   Type.EmptyTypes,
   //   TypeAttributes.Class,
   //   Array.Empty<MethodInfoSpec>(),
   //   Array.Empty<FieldInfoSpec>(),
   //   Array.Empty<PropertyInfoSpec>(),
   //   ReflectionModule.Create(
   //     "", 
   //     ReflectionAssembly.Create("TestAssembly")
   //     )
   // );
   // 
   // var type = typeof(List<>).MakeGenericType(reflectionType);
   // // Proxy type builder uses the AssemblyQualifiedName
   // type.FullName.Should().Be("System.Collections.Generic.List`1[[Namespace.TestType, TestAssembly]]");
  }

  [Fact]
  public void MakeGenericType_CreatesGenericReflectionType_WhenCalleeIsReflectionType() {
  //  var reflectionType = ReflectionType.Create(
  //    "TestType",
  //    "Namespace",
  //    null,
  //    Type.EmptyTypes,
  //    Type.EmptyTypes,
  //    TypeAttributes.Class,
  //    Array.Empty<MethodInfoSpec>(),
  //    Array.Empty<FieldInfoSpec>(),
  //    Array.Empty<PropertyInfoSpec>(),
  //    ReflectionModule.Create("", null)
  //    );
  //  
  //  var type = reflectionType.MakeGenericType(typeof(int));
  //  type.FullName.Should().Be("Namespace.TestType`1[System.Int32]");
  }
  
}