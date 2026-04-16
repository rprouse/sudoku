namespace Sudoku.Core.Game;

public record RelatedCellChange(
    int Row,
    int Col,
    bool[] PreviousCandidates,
    bool[] PreviousManualCandidates,
    bool[] PreviousExcludedCandidates);

public record UndoAction(
    int Row,
    int Col,
    int PreviousValue,
    bool[] PreviousCandidates,
    bool[] PreviousManualCandidates,
    bool[] PreviousExcludedCandidates,
    List<RelatedCellChange>? RelatedCellChanges = null);
