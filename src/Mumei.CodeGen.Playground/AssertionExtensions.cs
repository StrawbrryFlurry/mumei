namespace Mumei.CodeGen.Playground;

public static class AssertionExtensions {
    extension<TTarget>(TTarget subject) { }

    extension(bool subject) {
        public bool Assert() {
            return subject;
        }
    }
}