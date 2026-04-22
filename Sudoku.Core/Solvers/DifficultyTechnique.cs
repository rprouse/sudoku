namespace Sudoku.Core.Solvers;

public enum DifficultyTechnique
{
    // Easy
    NakedSingle,
    HiddenSingle,

    // Medium
    NakedPair,
    HiddenPair,

    // Hard
    NakedTriple,
    HiddenTriple,
    NakedQuad,
    HiddenQuad,
    PointingPair,
    BoxLineReduction,

    // Expert
    XWing,
    XYWing,
    SimpleColoring,

    // Evil
    Swordfish,
    XYZWing,
    XChain
}
