# Mumei.CodeGen

Provides an easy to use api for generating code.

## Example

```csharp
var classBuilder = new ClassSyntaxBuilder(
  "SomeClass",
  SyntaxVisibility.Public | SyntaxVisibility.Sealed
).AddInterfaceImplementation<ISomeInterface>();

var backingField = classBuilder
    .AddField<List<string>>("_someBackingField")
    .SetVisibility(SyntaxVisibility.Private);

classBuilder.AddMethod<int>(
  "Sum",
  (int a1, int a2, BlockSyntaxBuilder b) => {
    b.Return(() => a1 + a2);
  }
);

classBuilder.AddProperty<List<string>>(
  "SomeProperty"
)
.DefineBackingFieldGetter(backingField)
.DefineSetter((b) => {
  b.If(() => b.SetterImplicitValue == null, (ifBody) => {
    ifBody.Throw(() => new ArgumentNullException(b.SetterImplicitValue));
  });
      
  b.Assign(backingField, b.SetterImplicitValue);
});

)

var textWriter = new TypeAwareTextWriter();
fileBuilder.WriteAsString(textWriter);
```

```csharp
using System.Collections.Generic;
using SomeInterface.Namespace;

public sealed class SomeClass : ISomeInterface {
  private List<string> _someBackingField;
  
  public List<string> SomeProperty {
    get {
      return 42;
    }
    set {
      if (value == null) {
        throw new ArgumentNullException(value);
      }
      
      _someBackingField = value;
    }
  }

  public int Sum(int a1, int a2) {
    return a1 + a2;
  }
}
```