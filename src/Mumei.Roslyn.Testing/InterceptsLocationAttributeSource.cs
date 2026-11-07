namespace Mumei.Roslyn.Testing;

public static class InterceptsLocationAttributeSource {
    /// <summary>The file-local polyfill Mumei emits next to every interceptor.</summary>
    public const string Generated = """
                                    namespace System.Runtime.CompilerServices {
                                        [global::System.AttributeUsage(global::System.AttributeTargets.Method, AllowMultiple = true)]
                                        file sealed class InterceptsLocationAttribute : global::System.Attribute {
                                            public InterceptsLocationAttribute(int version, string data) { }
                                        }
                                    }
                                    """;
}
