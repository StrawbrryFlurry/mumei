using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Mumei.Roslyn;

namespace Mumei.CodeGen.Rendering.CSharp;

[CollectionBuilder(typeof(AttributeListFragment), nameof(Initialize))]
public readonly struct AttributeListFragment(ImmutableArray<AttributeFragment> attributes) : IRenderFragment, IEnumerable<AttributeFragment> {
    public static AttributeListFragment Empty => new(ImmutableArray<AttributeFragment>.Empty);
    public ImmutableArray<AttributeFragment> Attributes => attributes;

    public bool IsEmpty => attributes.IsDefaultOrEmpty;

    public void Render(IRenderTreeBuilder renderTree) {
        if (IsEmpty) {
            return;
        }

        renderTree.List(Attributes.AsSpan());
    }

    IEnumerator<AttributeFragment> IEnumerable<AttributeFragment>.GetEnumerator() {
        return Attributes.GetEnumeratorInterfaceImplementation();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return Attributes.GetEnumeratorInterfaceImplementation();
    }

    public static AttributeListFragment Initialize(ReadOnlySpan<AttributeFragment> items) {
        if (items.Length == 0) {
            return Empty;
        }

        return new AttributeListFragment([..items]);
    }

    public override string ToString() {
        return DebugRenderer.Render(this);
    }
}