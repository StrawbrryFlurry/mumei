namespace Mumei.CodeGen.Qt;

public delegate void RenderFragment<TState>(IRenderTreeBuilder context, TState state);
public delegate void RenderFragment(IRenderTreeBuilder context);