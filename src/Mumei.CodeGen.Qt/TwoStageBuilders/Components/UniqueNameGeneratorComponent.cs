namespace Mumei.CodeGen.Qt.TwoStageBuilders.Components;

internal sealed class UniqueNameGeneratorComponent {
    private int _nextMajorId = 0;
    private int _nextMinorId = 0;

    public string MakeUnique(string s) {
        var id = $"{s}_{_nextMajorId}__{_nextMinorId}";
        if (++_nextMinorId >= 10) {
            _nextMinorId = 0;
            _nextMajorId++;
        }

        return id;
    }
}