namespace Sudoku.Core;

public static class DifficultyExtensions
{
    public static string DisplayName(this Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => "Gentle",
        Difficulty.Medium => "Steady",
        Difficulty.Hard => "Challenging",
        Difficulty.Expert => "Deep",
        Difficulty.Evil => "Profound",
        _ => difficulty.ToString()
    };
}
