namespace Mumei.CodeGen.Qt.Tests;

internal sealed class InterceptsLocationAttributeSource {
    public const string Generated = """
                                    #pragma warning disable
                                    namespace System.Runtime.CompilerServices {
                                        [global::System.AttributeUsageAttribute(global::System.AttributeTargets.Method, AllowMultiple = true)]
                                        file sealed class InterceptsLocationAttribute(int version, string data) : global::System.Attribute;
                                    }
                                    #pragma warning enable
                                    """;
}