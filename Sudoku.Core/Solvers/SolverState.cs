namespace Sudoku.Core.Solvers;

// 9-bit masks: bit (d-1) represents digit d.
internal struct SolverState
{
    public const int All = 0x1FF; // bits 0..8 set

    public int[,] Grid;           // 9x9, 0 = empty
    public int[] RowMask;         // bit (d-1) set if digit d is placed in row r
    public int[] ColMask;
    public int[] BoxMask;
    public int[,] CellCandidates; // per-cell candidate mask (valid when grid cell is 0)
    public int EmptyCount;

    public static SolverState FromBoard(SudokuBoard board)
    {
        var state = new SolverState
        {
            Grid = new int[9, 9],
            RowMask = new int[9],
            ColMask = new int[9],
            BoxMask = new int[9],
            CellCandidates = new int[9, 9],
            EmptyCount = 0
        };

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                var v = board[r, c];
                state.Grid[r, c] = v;
                if (v > 0)
                {
                    var bit = 1 << (v - 1);
                    state.RowMask[r] |= bit;
                    state.ColMask[c] |= bit;
                    state.BoxMask[BoxIndex(r, c)] |= bit;
                }
                else
                {
                    state.EmptyCount++;
                }
            }
        }

        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                state.CellCandidates[r, c] = state.Grid[r, c] == 0
                    ? (~(state.RowMask[r] | state.ColMask[c] | state.BoxMask[BoxIndex(r, c)])) & All
                    : 0;
            }
        }

        return state;
    }

    public SudokuBoard ToBoard()
    {
        var board = new SudokuBoard();
        for (var r = 0; r < 9; r++)
        {
            for (var c = 0; c < 9; c++)
            {
                board[r, c] = Grid[r, c];
            }
        }
        return board;
    }

    public readonly int UnitCandidates(int r, int c) =>
        (~(RowMask[r] | ColMask[c] | BoxMask[BoxIndex(r, c)])) & All;

    public void Place(int r, int c, int digit)
    {
        var bit = 1 << (digit - 1);
        Grid[r, c] = digit;
        RowMask[r] |= bit;
        ColMask[c] |= bit;
        BoxMask[BoxIndex(r, c)] |= bit;
        CellCandidates[r, c] = 0;
        EmptyCount--;
    }

    public void Remove(int r, int c, int digit)
    {
        var bit = 1 << (digit - 1);
        Grid[r, c] = 0;
        RowMask[r] &= ~bit;
        ColMask[c] &= ~bit;
        BoxMask[BoxIndex(r, c)] &= ~bit;
        EmptyCount++;
        // Caller is responsible for recomputing CellCandidates if needed.
    }

    public static int BoxIndex(int r, int c) => (r / 3) * 3 + (c / 3);

    public static int PopCount(int mask)
    {
        // Avoid adding System.Numerics dependency; 9-bit popcount.
        var n = 0;
        while (mask != 0) { mask &= mask - 1; n++; }
        return n;
    }

    public static int LowestBitIndex(int mask) =>
        System.Numerics.BitOperations.TrailingZeroCount(mask);
}
