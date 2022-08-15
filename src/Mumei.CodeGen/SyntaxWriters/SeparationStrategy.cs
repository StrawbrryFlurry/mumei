namespace Mumei.CodeGen.SyntaxWriters;

public abstract class SeparationStrategy {
  public static readonly SeparationStrategy None = new SeparationStrategyNone();
  public static readonly SeparationStrategy Space = new SeparationStrategySpace();
  public static readonly SeparationStrategy NewLine = new SeparationStrategyNewLine();
  public abstract void WriteSeparator(ISyntaxWriter writer);
}

public class SeparationStrategyNone : SeparationStrategy {
  public override void WriteSeparator(ISyntaxWriter writer) { }
}

public class SeparationStrategySpace : SeparationStrategy {
  public override void WriteSeparator(ISyntaxWriter writer) {
    writer.Write(" ");
  }
}

public class SeparationStrategyNewLine : SeparationStrategy {
  public override void WriteSeparator(ISyntaxWriter writer) {
    writer.WriteLine("");
  }
}