using System.Runtime.CompilerServices;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Qt.TwoStageBuilders.SynthesizedComponents;

public readonly struct ExpressionFragment(string value) : IRenderFragment {
    public void Render(IRenderTreeBuilder renderTree) {
        renderTree.Text(value);
    }

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
}