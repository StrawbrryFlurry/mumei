using Mumei.CodeGen.Rendering;
using Mumei.CodeGen.Rendering.CSharp;

namespace Mumei.CodeGen.Components;

internal sealed class RuntimeSyntheticLiteralExpression(object? value) : ISyntheticExpression, ISyntheticConstructable<ExpressionFragment> {
    public ExpressionFragment Construct(ICompilationUnitContext compilationUnit) {
        if (value is null) {
            return ExpressionFragment.Null;
        }

        if (value is string strValue) {
            return new ExpressionFragment($"{Strings.RawStringLiteral9}{strValue}{Strings.RawStringLiteral9}");
        }

        if (value is char charValue) {
            return new ExpressionFragment($"'{charValue}'");
        }

        if (value is bool boolValue) {
            return new ExpressionFragment(boolValue ? "true" : "false");
        }

        if (value is float floatValue) {
            return new ExpressionFragment($"{floatValue}f");
        }

        if (value is double doubleValue) {
            return new ExpressionFragment($"{doubleValue}d");
        }

        if (value is decimal decimalValue) {
            return new ExpressionFragment($"{decimalValue}m");
        }

        if (value is Enum enumValue) {
            var enumType = enumValue.GetType();
            var enumName = enumType.FullName ?? enumType.Name;
            var enumMemberName = enumValue.ToString();
            return new ExpressionFragment($"{enumName}.{enumMemberName}");
        }

        throw new NotSupportedException($"Unsupported literal type for runtime synthetic literal expression. Value: {value}");
    }
}