namespace Sudoku.Core.Game;

public record UndoAction(
    int Row,
    int Col,
    int PreviousValue,
    bool[] PreviousCandidates,
    bool[] PreviousManualCandidates,
    bool[] PreviousExcludedCandidates);
