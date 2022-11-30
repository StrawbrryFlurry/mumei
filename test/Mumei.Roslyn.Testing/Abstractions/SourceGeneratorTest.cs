using System.Collections.Immutable;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Testing.Model;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Testing;

public abstract class SourceGeneratorTest<TVerifier> : AnalyzerTest<TVerifier>
  where TVerifier : IVerifier, new() {
  protected override IEnumerable<DiagnosticAnalyzer> GetDiagnosticAnalyzers() {
    return Enumerable.Empty<DiagnosticAnalyzer>();
  }

  /// <summary>
  ///   Returns the source generators being tested - to be implemented in non-abstract class.
  /// </summary>
  /// <returns>The <see cref="ISourceGenerator" /> to be used.</returns>
  protected abstract IEnumerable<ISourceGenerator> GetSourceGenerators();

  protected abstract GeneratorDriver CreateGeneratorDriver(
    Project project,
    ImmutableArray<ISourceGenerator> sourceGenerators
    );

  protected override async Task RunImplAsync(CancellationToken cancellationToken) {
    var analyzers = GetDiagnosticAnalyzers().ToArray();
    var defaultDiagnostic = GetDefaultDiagnostic(analyzers);
    var supportedDiagnostics = analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics).ToImmutableArray();
    var fixableDiagnostics = ImmutableArray<string>.Empty;
    var testState = TestState.WithInheritedValuesApplied(null, fixableDiagnostics).WithProcessedMarkup(MarkupOptions,
      defaultDiagnostic, supportedDiagnostics, fixableDiagnostics, DefaultFilePath);

    var diagnostics = await VerifySourceGeneratorAsync(testState, Verify, cancellationToken).ConfigureAwait(false);
    await VerifyDiagnosticsAsync(
      new EvaluatedProjectState(testState, ReferenceAssemblies).WithAdditionalDiagnostics(diagnostics),
      testState.AdditionalProjects.Values
        .Select(additionalProject => new EvaluatedProjectState(additionalProject, ReferenceAssemblies))
        .ToImmutableArray(), testState.ExpectedDiagnostics.ToArray(), Verify.PushContext("Diagnostics of test state"),
      cancellationToken).ConfigureAwait(false);
  }

  protected override async Task<Compilation> GetProjectCompilationAsync(Project project, IVerifier verifier,
    CancellationToken cancellationToken) {
    var (finalProject, diagnostics) =
      await ApplySourceGeneratorAsync(GetSourceGenerators().ToImmutableArray(), project, verifier, cancellationToken)
        .ConfigureAwait(false);
    return (await finalProject.GetCompilationAsync(cancellationToken).ConfigureAwait(false))!;
  }

  /// <summary>
  ///   Called to test a C# source generator when applied on the input source as a string.
  /// </summary>
  /// <param name="testState">The effective input test state.</param>
  /// <param name="verifier">The verifier to use for test assertions.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> that the task will observe.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
  protected async Task<ImmutableArray<Diagnostic>> VerifySourceGeneratorAsync(SolutionState testState,
    IVerifier verifier, CancellationToken cancellationToken) {
    return await VerifySourceGeneratorAsync(Language, GetSourceGenerators().ToImmutableArray(), testState,
      ApplySourceGeneratorAsync, verifier.PushContext("Source generator application"), cancellationToken);
  }

  private async Task<ImmutableArray<Diagnostic>> VerifySourceGeneratorAsync(
    string language,
    ImmutableArray<ISourceGenerator> sourceGenerators,
    SolutionState testState,
    Func<ImmutableArray<ISourceGenerator>, Project, IVerifier, CancellationToken,
      Task<(Project project, ImmutableArray<Diagnostic> diagnostics)>> getFixedProject,
    IVerifier verifier,
    CancellationToken cancellationToken) {
    var project = await CreateProjectAsync(new EvaluatedProjectState(testState, ReferenceAssemblies),
      testState.AdditionalProjects.Values
        .Select(additionalProject => new EvaluatedProjectState(additionalProject, ReferenceAssemblies))
        .ToImmutableArray(), cancellationToken);
    _ = await GetCompilerDiagnosticsAsync(project, verifier, cancellationToken).ConfigureAwait(false);

    ImmutableArray<Diagnostic> diagnostics;
    (project, diagnostics) =
      await getFixedProject(sourceGenerators, project, verifier, cancellationToken).ConfigureAwait(false);

    // After applying the source generator, compare the resulting string to the inputted one
    if (!TestBehaviors.HasFlag(TestBehaviors.SkipGeneratedSourcesCheck)) {
      var numOriginalSources = testState.Sources.Count;
      var updatedOriginalDocuments = project.Documents.Take(numOriginalSources).ToArray();
      var generatedDocuments = project.Documents.Skip(numOriginalSources).ToArray();

      // Verify no changes occurred to the original documents
      var updatedOriginalDocumentsWithTextBuilder =
        ImmutableArray.CreateBuilder<(Document document, SourceText content)>();
      foreach (var updatedOriginalDocument in updatedOriginalDocuments) {
        updatedOriginalDocumentsWithTextBuilder.Add((updatedOriginalDocument,
          await GetSourceTextFromDocumentAsync(updatedOriginalDocument, CancellationToken.None).ConfigureAwait(false)));
      }

      VerifyDocuments(
        verifier.PushContext("Original files after running source generators"),
        updatedOriginalDocumentsWithTextBuilder.ToImmutable(),
        testState.Sources.ToImmutableArray(),
        false,
        DefaultFilePathPrefix,
        GetNameAndFoldersFromPath,
        MatchDiagnosticsTimeout);

      // Verify the source generated documents match expectations
      var generatedDocumentsWithTextBuilder = ImmutableArray.CreateBuilder<(Document document, SourceText content)>();
      foreach (var generatedDocument in generatedDocuments) {
        generatedDocumentsWithTextBuilder.Add((generatedDocument,
          await GetSourceTextFromDocumentAsync(generatedDocument, CancellationToken.None).ConfigureAwait(false)));
      }

      VerifyDocuments(
        verifier.PushContext("Verifying source generated files"),
        generatedDocumentsWithTextBuilder.ToImmutable(),
        testState.GeneratedSources.ToImmutableArray(),
        true,
        DefaultFilePathPrefix,
        static (_, path) => GetNameAndFoldersFromSourceGeneratedFilePath(path),
        MatchDiagnosticsTimeout);
    }

    return diagnostics;

    static void VerifyDocuments(
      IVerifier verifier,
      ImmutableArray<(Document document, SourceText content)> actualDocuments,
      ImmutableArray<(string filename, SourceText content)> expectedDocuments,
      bool allowReordering,
      string defaultFilePathPrefix,
      Func<string, string, (string fileName, IEnumerable<string> folders)> getNameAndFolders,
      TimeSpan matchTimeout) {
      ImmutableArray<WeightedMatch.Result<(string filename, SourceText content), (Document document, SourceText content
        )>> matches;
      if (allowReordering) {
        matches = WeightedMatch.Match(
          expectedDocuments,
          actualDocuments,
          ImmutableArray
            .Create<Func<(string filename, SourceText content), (Document document, SourceText content), bool, double>>(
              static (expected, actual, exactOnly) => {
                if (actual.content.ToString() == expected.content.ToString()) {
                  return 0.0;
                }

                if (exactOnly) {
                  // Avoid expensive diff calculation when exact match was requested.
                  return 1.0;
                }

                var diffBuilder = new InlineDiffBuilder(new Differ());
                var diff = diffBuilder.BuildDiffModel(expected.content.ToString(), actual.content.ToString(), true,
                  false, new LineChunker());
                var changeCount =
                  diff.Lines.Count(static line => line.Type is ChangeType.Inserted or ChangeType.Deleted);
                if (changeCount == 0) {
                  // We have a failure caused only by line ending or whitespace differences. Make sure
                  // to use a non-zero value so it can be distinguished from exact matches.
                  changeCount = 1;
                }

                // Apply a multiplier to the content distance to account for its increased importance
                // over encoding and checksum algorithm changes.
                var priority = 3;

                return priority * changeCount / (double)diff.Lines.Count;
              },
              static (expected, actual, exactOnly) => {
                return actual.content.Encoding == expected.content.Encoding ? 0.0 : 1.0;
              },
              static (expected, actual, exactOnly) => {
                return actual.content.ChecksumAlgorithm == expected.content.ChecksumAlgorithm ? 0.0 : 1.0;
              },
              (expected, actual, exactOnly) => {
                var distance = 0.0;
                var (fileName, folders) = getNameAndFolders(defaultFilePathPrefix, expected.filename);
                if (fileName != actual.document.Name) {
                  distance += 1.0;
                }

                if (!folders.SequenceEqual(actual.document.Folders)) {
                  distance += 1.0;
                }

                return distance;
              }),
          matchTimeout);
      }
      else {
        // Matching with an empty set of matching functions always takes the 1:1 alignment without reordering
        matches = WeightedMatch.Match(
          expectedDocuments,
          actualDocuments,
          ImmutableArray<Func<(string filename, SourceText content), (Document document, SourceText content), bool,
            double>>.Empty,
          matchTimeout);
      }

      // Use EqualOrDiff to verify the actual and expected filenames (and total collection length) in a convenient manner
      verifier.EqualOrDiff(
        string.Join(Environment.NewLine,
          matches.Select(match => match.TryGetExpected(out var expected) ? expected.filename : string.Empty)),
        string.Join(Environment.NewLine,
          matches.Select(match => match.TryGetActual(out var actual) ? actual.document.FilePath : string.Empty)),
        "Expected source file list to match");

      // Follow by verifying each property of interest
      foreach (var result in matches) {
        if (!result.TryGetExpected(out var expected)
            || !result.TryGetActual(out var actual)) {
          throw new InvalidOperationException("Unexpected state: should have failed during the previous assertion.");
        }

        verifier.EqualOrDiff(expected.content.ToString(), actual.content.ToString(),
          $"content of '{expected.filename}' did not match. Diff shown with expected as baseline:");
        verifier.Equal(expected.content.Encoding, actual.content.Encoding,
          $"encoding of '{expected.filename}' was expected to be '{expected.content.Encoding?.WebName}' but was '{actual.content.Encoding?.WebName}'");
        verifier.Equal(expected.content.ChecksumAlgorithm, actual.content.ChecksumAlgorithm,
          $"checksum algorithm of '{expected.filename}' was expected to be '{expected.content.ChecksumAlgorithm}' but was '{actual.content.ChecksumAlgorithm}'");

        // Source-generated sources are implicitly in a subtree, so they have a different folders calculation.
        var (fileName, folders) = getNameAndFolders(defaultFilePathPrefix, expected.filename);
        verifier.Equal(fileName, actual.document.Name,
          $"file name was expected to be '{fileName}' but was '{actual.document.Name}'");
        verifier.SequenceEqual(folders, actual.document.Folders,
          message:
          $"folders was expected to be '{string.Join("/", folders)}' but was '{string.Join("/", actual.document.Folders)}'");
      }
    }
  }

  private static (string fileName, IEnumerable<string> folders) GetNameAndFoldersFromSourceGeneratedFilePath(
    string filePath) {
    // Source-generated files are always implicitly subpaths under the project root path.
    var folders = Path.GetDirectoryName(filePath)!.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var fileName = Path.GetFileName(filePath);
    return (fileName, folders);
  }

  private async Task<(Project project, ImmutableArray<Diagnostic> diagnostics)> ApplySourceGeneratorAsync(
    ImmutableArray<ISourceGenerator> sourceGenerators, Project project, IVerifier verifier,
    CancellationToken cancellationToken) {
    var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
    verifier.True(compilation is { });

    var driver = CreateGeneratorDriver(project, sourceGenerators).RunGenerators(compilation, cancellationToken);
    var result = driver.GetRunResult();

    var updatedProject = project;
    foreach (var tree in result.GeneratedTrees) {
      var (fileName, folders) = GetNameAndFoldersFromSourceGeneratedFilePath(tree.FilePath);
      updatedProject = updatedProject.AddDocument(fileName,
        await tree.GetTextAsync(cancellationToken).ConfigureAwait(false), folders, tree.FilePath).Project;
    }

    return (updatedProject, result.Diagnostics);
  }

  /// <summary>
  ///   Get the existing compiler diagnostics on the input document.
  /// </summary>
  /// <param name="project">The <see cref="Project" /> to run the compiler diagnostic analyzers on.</param>
  /// <param name="verifier">The verifier to use for test assertions.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> that the task will observe.</param>
  /// <returns>The compiler diagnostics that were found in the code.</returns>
  private static async Task<ImmutableArray<Diagnostic>> GetCompilerDiagnosticsAsync(Project project, IVerifier verifier,
    CancellationToken cancellationToken) {
    var allDiagnostics = ImmutableArray.Create<Diagnostic>();

    foreach (var document in project.Documents) {
      var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
      verifier.True(semanticModel is { });

      allDiagnostics = allDiagnostics.AddRange(semanticModel.GetDiagnostics(cancellationToken: cancellationToken));
    }

    return allDiagnostics;
  }

  /// <summary>
  ///   Given a document, turn it into a string based on the syntax root.
  /// </summary>
  /// <param name="document">The <see cref="Document" /> to be converted to a string.</param>
  /// <param name="cancellationToken">The <see cref="CancellationToken" /> that the task will observe.</param>
  /// <returns>A <see cref="SourceText" /> containing the syntax of the <see cref="Document" /> after formatting.</returns>
  private static async Task<SourceText> GetSourceTextFromDocumentAsync(Document document,
    CancellationToken cancellationToken) {
    var simplifiedDoc = await Simplifier
      .ReduceAsync(document, Simplifier.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);
    var formatted = await Formatter
      .FormatAsync(simplifiedDoc, Formatter.Annotation, cancellationToken: cancellationToken).ConfigureAwait(false);
    return await formatted.GetTextAsync(cancellationToken).ConfigureAwait(false);
  }
  
  protected (string fileName, IEnumerable<string> folders) GetNameAndFoldersFromPath(string projectPathPrefix, string path)
        {
            // Normalize to platform path separators for simplicity later on
            var normalizedPath = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            var normalizedDefaultPathPrefix = projectPathPrefix.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            if (!Path.IsPathRooted(normalizedDefaultPathPrefix))
            {
                // If our default path isn't rooted, then we assume that we don't have any rooted paths
                // and just use the file name
                return (Path.GetFileName(normalizedPath), folders: new string[0]);
            }

            // | Default path | Project root path |
            // |--------------|-------------------|
            // |   /0/Temp    |  /0/              |
            // |   /0/        |  /0/              |
            var projectRootPath = Path.GetFileName(normalizedDefaultPathPrefix) == string.Empty
                ? normalizedDefaultPathPrefix
                : (Path.GetDirectoryName(normalizedDefaultPathPrefix) + Path.DirectorySeparatorChar);

            // If the default path prefix is a directory name (ending with a directory separator)
            // then treat it as the project root.
            if (!normalizedPath.StartsWith(projectRootPath))
            {
                // If our path doesn't start with the default path prefix, then the file is out of tree.
                if (Path.IsPathRooted(normalizedPath))
                {
                    // If the user provides a rooted path as the file name, just use that as-is.
                    return (path, folders: new string[0]);
                }

                // Otherwise, to match VS behavior we will report no folders and only the file name.
                return (Path.GetFileName(normalizedPath), folders: new string[0]);
            }

            var subpath = normalizedPath.Substring(projectRootPath.Length);

            var fileName = Path.GetFileName(subpath);
            if (Path.GetDirectoryName(subpath) == string.Empty)
            {
                return (fileName, folders: new string[0]);
            }

            var folders = Path.GetDirectoryName(subpath)!.Split(Path.DirectorySeparatorChar);
            return (fileName, folders);
        }
}