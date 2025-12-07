using System.Runtime.CompilerServices;
using Mumei.Common.Internal;

namespace Mumei.CodeGen.Rendering.CSharp;

public readonly struct ExpressionFragment(string value) : IRenderFragment {
    public static ExpressionFragment Null { get; } = new("null");
    public static ExpressionFragment Default { get; } = new("default");

    public string Value => value;

    public static implicit operator ExpressionFragment(string value) {
        return new ExpressionFragment(value);
    }

    public static implicit operator ExpressionFragment(InterpolatedStringHandler value) {
        return new ExpressionFragment(value.GetValue());
    }

    [InterpolatedStringHandler]
    public ref struct InterpolatedStringHandler {
        private ArrayBuilder<char> _builder = default;

        public InterpolatedStringHandler(int literalLength, int formattedCount) {
            _builder = new ArrayBuilder<char>(literalLength + formattedCount * 8);
        }

        public void AppendLiteral(ReadOnlySpan<char> s) {
            _builder.AddRange(s);
        }

        public void AppendFormatted(ReadOnlySpan<char> s) {
            _builder.AddRange(s);
        }

        public void AppendFormatted(Type type, string? format = "") {
            if (format == "typeof") {
                _builder.AddRange("typeof(".AsSpan());
                _builder.AddRange(type.FullName.AsSpan());
                _builder.Add(')');
                return;
            }

            _builder.AddRange("global::");
            _builder.AddRange(type.FullName.AsSpan());
        }

        public string GetValue() {
            return _builder.ToStringAndFree();
        }
    }

    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(value);
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }
}