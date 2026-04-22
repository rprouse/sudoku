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

    // Like Place, but also removes the placed digit from every peer cell's
    // CellCandidates (same row, column, or box). Used by TechniqueSolver so
    // that CellCandidates stays accurate across technique firings. Do NOT
    // use from BitmaskSolver's backtracking — that path does not read
    // CellCandidates and the peer updates are not undone by Remove.
    public void PlacePropagating(int r, int c, int digit)
    {
        Place(r, c, digit);
        var bit = 1 << (digit - 1);
        // Remove the digit from every peer cell's persistent candidate mask.
        // Peers include every cell in the same row, column, or box as (r,c).
        for (var k = 0; k < 9; k++)
        {
            if (k != c) CellCandidates[r, k] &= ~bit;
            if (k != r) CellCandidates[k, c] &= ~bit;
        }
        var boxStartR = (r / 3) * 3;
        var boxStartC = (c / 3) * 3;
        for (var br = boxStartR; br < boxStartR + 3; br++)
        {
            for (var bc = boxStartC; bc < boxStartC + 3; bc++)
            {
                if (br == r || bc == c) continue; // already handled above
                CellCandidates[br, bc] &= ~bit;
            }
        }
    }

    // Removes a single candidate digit from a cell's persistent CellCandidates
    // mask. Returns true if the mask actually changed, false if the digit was
    // already not a candidate. Used by techniques that identify eliminations
    // without immediate placements (e.g., a NakedPair reducing peers' options).
    public bool EliminateCandidate(int r, int c, int digit)
    {
        var bit = 1 << (digit - 1);
        var old = CellCandidates[r, c];
        if ((old & bit) == 0) return false;
        CellCandidates[r, c] = old & ~bit;
        return true;
    }

    public void Remove(int r, int c, int digit)
    {
        var bit = 1 << (digit - 1);
        Grid[r, c] = 0;
        RowMask[r] &= ~bit;
        ColMask[c] &= ~bit;
        BoxMask[BoxIndex(r, c)] &= ~bit;
        EmptyCount++;
        // CellCandidates is left stale intentionally: undoing a placement
        // re-opens 'digit' as a candidate for every empty cell in the same
        // row, column, and box. Callers that read per-cell candidates after
        // Remove must either recompute via UnitCandidates on demand or
        // refresh CellCandidates for all peers — do not trust CellCandidates.
    }

    public static int BoxIndex(int r, int c) => (r / 3) * 3 + (c / 3);

    // Pre-computed (r,c) coordinate arrays for each of the 27 units. Techniques
    // that iterate unit cells share these to avoid re-allocating 9-element
    // arrays inside the solver's hot loop.
    internal static readonly (int r, int c)[][] RowCells = BuildRowCells();
    internal static readonly (int r, int c)[][] ColCells = BuildColCells();
    internal static readonly (int r, int c)[][] BoxCells = BuildBoxCells();

    private static (int r, int c)[][] BuildRowCells()
    {
        var rows = new (int, int)[9][];
        for (var r = 0; r < 9; r++)
        {
            rows[r] = new (int, int)[9];
            for (var c = 0; c < 9; c++) rows[r][c] = (r, c);
        }
        return rows;
    }

    private static (int r, int c)[][] BuildColCells()
    {
        var cols = new (int, int)[9][];
        for (var c = 0; c < 9; c++)
        {
            cols[c] = new (int, int)[9];
            for (var r = 0; r < 9; r++) cols[c][r] = (r, c);
        }
        return cols;
    }

    private static (int r, int c)[][] BuildBoxCells()
    {
        var boxes = new (int, int)[9][];
        for (var box = 0; box < 9; box++)
        {
            boxes[box] = new (int, int)[9];
            var startR = (box / 3) * 3;
            var startC = (box % 3) * 3;
            var i = 0;
            for (var r = startR; r < startR + 3; r++)
                for (var c = startC; c < startC + 3; c++)
                    boxes[box][i++] = (r, c);
        }
        return boxes;
    }

    public static int PopCount(int mask) =>
        System.Numerics.BitOperations.PopCount((uint)mask);

    public static int LowestBitIndex(int mask) =>
        System.Numerics.BitOperations.TrailingZeroCount(mask);
}
