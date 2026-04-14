namespace Sudoku.Core.Generators;

public interface IGenerator
{
    SudokuBoard Generate(Difficulty difficulty);
}
