namespace Sudoku.Core.Game;

public class CellState
{
    public int Value { get; set; }
    public bool IsGiven { get; set; }
    public bool IsError { get; set; }
    public bool[] Candidates { get; set; } = new bool[9];
    public bool[] ManualCandidates { get; set; } = new bool[9];
    public bool[] ExcludedCandidates { get; set; } = new bool[9];

    public bool HasValue => Value != 0;

    public bool HasCandidate(int number) => Candidates[number - 1];

    public void ToggleCandidate(int number)
    {
        Candidates[number - 1] = !Candidates[number - 1];
    }

    public void ClearCandidates()
    {
        Array.Clear(Candidates);
    }

    public bool[] CloneCandidates()
    {
        return (bool[])Candidates.Clone();
    }

    public bool[] CloneManualCandidates()
    {
        return (bool[])ManualCandidates.Clone();
    }

    public bool[] CloneExcludedCandidates()
    {
        return (bool[])ExcludedCandidates.Clone();
    }
}
