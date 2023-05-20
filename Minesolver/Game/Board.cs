using Minesolver.CLI;

namespace Minesolver.Game {
    internal class Board {
        public int MineCount { get; private set; }
        public int RowCount { get; private set; }
        public int ColCount { get; private set; }

        public int RemainingMineCount { get; private set; }
        public int RemainingCoveredCells { get; private set; }
        public BoardState State { get; private set; }
        public bool Finished => State == BoardState.Win || State == BoardState.Lose;

        private readonly Cell[,] cells;
        private readonly HashSet<(int, int)> flags = new HashSet<(int, int)>();
        private bool firstClicked;

        public Board(int mineCount, int rowCount, int colCount) {
            if(rowCount <= 0) throw new ArgumentOutOfRangeException(nameof(rowCount), "Board row count must be positive");
            if(colCount <= 0) throw new ArgumentOutOfRangeException(nameof(colCount), "Board column count must be positive");
            if(mineCount < 0) throw new ArgumentOutOfRangeException(nameof(mineCount), "Mine count must be non-negative");
            if(mineCount >= rowCount * colCount) throw new ArgumentOutOfRangeException(nameof(mineCount), $"Mine count ({mineCount}) must be less than cell count ({rowCount * colCount})");

            RowCount = rowCount;
            ColCount = colCount;
            MineCount = mineCount;

            cells = new Cell[rowCount, colCount];
            Reset();
        }

        public void Reset() {
            flags.Clear();
            firstClicked = false;
            State = BoardState.NotStarted;
            BuildCells();
        }

        public void EachNeighbor(int peekRow, int peekCol, Action<int, int> action) {
            for(int dRow = -1; dRow <= 1; dRow++) {
                for(int dCol = -1; dCol <= 1; dCol++) {
                    if(dRow == 0 && dCol == 0) continue;

                    (int row, int col) = (peekRow + dRow, peekCol + dCol);
                    if(!WithinBounds(row, col)) continue;

                    action(row, col);
                }
            }
        }

        public bool WithinBounds(int peekRow, int peekCol) {
            return peekRow >= 1 && peekCol >= 1 && peekRow <= RowCount && peekCol <= ColCount;
        }

        public bool IsAdjacent(int row1, int col1, int row2, int col2) {
            (int dRow, int dCol) = (Math.Abs(row1 - row2), Math.Abs(col1 - col2));
            return Math.Abs(row1 - row2) <= 1 && Math.Abs(col1 - col2) <= 1;
        }

        public void PrintBoardToConsole() {
            ConsoleHelper.Write("Remaining mines: ", ConsoleColor.White);
            ConsoleHelper.WriteLine(RemainingMineCount.ToString(), ConsoleColor.Red);

            for(int row = -1; row <= RowCount; row++) {
                for(int col = -1; col <= ColCount; col++) {
                    if(row == -1 || row == RowCount) {
                        if(col == -1 || col == ColCount) {
                            Console.Write(" ");
                        } else {
                            ConsoleHelper.Write(((col + 1) % 10).ToString(), ConsoleColor.DarkGray);
                        }
                    } else {
                        if(col == -1 || col == ColCount) {
                            ConsoleHelper.Write(((row + 1) % 10).ToString(), ConsoleColor.DarkGray);
                        } else {
                            Cell cell = cells[row, col];
                            if(flags.Contains((row, col))) {
                                ConsoleHelper.Write("F", ConsoleColor.DarkRed, ConsoleColor.DarkGray);
                            } else if(cell.IsUncovered) {
                                int cellValue = cell.Value ?? int.MinValue;
                                ConsoleColor cellColor = cell.Color ?? ConsoleColor.DarkGray;
                                if(cellValue == -1) {
                                    ConsoleHelper.Write("*", cellColor, ConsoleColor.DarkRed);
                                } else {
                                    ConsoleHelper.Write(cellValue.ToString(), cellColor);
                                }
                            } else {
                                ConsoleHelper.Write(" ", ConsoleColor.White, ConsoleColor.DarkGray);
                            }
                        }
                    }
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        public void Click(int clickRow, int clickCol) {
            if(!WithinBounds(clickRow, clickCol)) throw new ArgumentOutOfRangeException($"Position {clickRow}:{clickCol} is out of bounds for mapsize {RowCount}x{ColCount}");
            if(GetFlag(clickRow, clickCol)) throw new InvalidOperationException($"Cannot click {clickRow}:{clickCol} as it is flagged");
            if(Finished) throw new InvalidOperationException("The board is already finished");

            (int row, int col) = (clickRow - 1, clickCol - 1);
            if(cells[row, col].IsUncovered) {
                // Chording: click on neighboring covered squares
                int peekValue = Peek(clickRow, clickCol) ?? int.MinValue;
                int neighborFlagCount = 0;
                EachNeighbor(clickRow, clickCol, (r, c) => {
                    if(GetFlag(r, c)) neighborFlagCount++;
                });
                if(peekValue == neighborFlagCount) {
                    EachNeighbor(clickRow, clickCol, (r, c) => {
                        if(!GetFlag(r, c) && Peek(r, c) == null) Click(r, c);
                    });
                }
            }
            
            // Uncover a single square
            State = BoardState.Started;
            if(!firstClicked) {
                bool check = cells[row, col].Uncover() != 0;
                while(check) {
                    BuildCells();
                    int checkVal = cells[row, col].Uncover();
                    check = checkVal == -1;
                }
                firstClicked = true;
            }

            int value = cells[row, col].Uncover();
            RemainingCoveredCells--;

            if(value == 0) {
                EachNeighbor(clickRow, clickCol, (r, c) => {
                    if(Peek(r, c) == null) Click(r, c);
                });
                //for(int dx = -1; dx <= 1; dx++) {
                //    for(int dy = -1; dy <= 1; dy++) {
                //        if(dx == 0 && dy == 0) continue;
                //        int neighborRow = row + dx;
                //        int neighborCol = col + dy;
                //        if(!WithinBounds(neighborRow, neighborCol)) continue;

                //        Click(neighborRow + 1, neighborCol + 1);
                //    }
                //}
            } else if(value == -1) State = BoardState.Lose;

            if(!Finished && RemainingCoveredCells == MineCount) State = BoardState.Win;
        }

        public int? Peek(int peekRow, int peekCol) {
            if(!WithinBounds(peekRow, peekCol)) throw new ArgumentOutOfRangeException($"Position {peekRow}:{peekCol} is out of bounds for mapsize {RowCount}x{ColCount}");
            (int row, int col) = (peekRow - 1, peekCol - 1);
            return cells[row, col].Value;
        }

        public void SetFlag(int peekRow, int peekCol, bool flag) {
            if(!WithinBounds(peekRow, peekCol)) throw new ArgumentOutOfRangeException($"Position {peekRow}:{peekCol} is out of bounds for mapsize {RowCount}x{ColCount}");
            (int row, int col) = (peekRow - 1, peekCol - 1);
            bool _ = flag ? flags.Add((row, col)) : flags.Remove((row, col));
            RemainingMineCount = Math.Max(MineCount - flags.Count, 0);
        }

        public bool GetFlag(int peekRow, int peekCol) {
            if(!WithinBounds(peekRow, peekCol)) throw new ArgumentOutOfRangeException($"Position {peekRow}:{peekCol} is out of bounds for mapsize {RowCount}x{ColCount}");
            (int row, int col) = (peekRow - 1, peekCol - 1);
            return flags.Contains((row, col));
        }

        private void BuildCells() {
            int[,] numbers = new int[RowCount, ColCount];

            Random rand = new Random();
            RemainingMineCount = 0;
            while(RemainingMineCount < MineCount) {
                int row = rand.Next(0, RowCount);
                int col = rand.Next(0, ColCount);

                if(numbers[row, col] != -1) {
                    numbers[row, col] = -1;
                    RemainingMineCount++;
                    EachNeighbor(row + 1, col + 1, (r, c) => {
                        if(numbers[r - 1, c - 1] != -1) numbers[r - 1, c - 1]++;
                    });
                    //for(int dx = -1; dx <= 1; dx++) {
                    //    for(int dy = -1; dy <= 1; dy++) {
                    //        if(dx == 0 && dy == 0) continue;
                    //        int neighborRow = row + dx;
                    //        int neighborCol = col + dy;
                    //        if(!WithinBounds(neighborRow, neighborCol)) continue;

                    //        if(numbers[neighborRow, neighborCol] != -1) {
                    //            numbers[neighborRow, neighborCol]++;
                    //        }
                    //    }
                    //}
                }
            }

            RemainingCoveredCells = RowCount * ColCount;
            for(int row = 0; row < RowCount; row++) {
                for(int col = 0; col < ColCount; col++) {
                    cells[row, col] = new Cell(numbers[row, col]);
                }
            }
        }

        public class Cell {
            public bool IsUncovered { get; private set; }
            public int? Value => IsUncovered ? value : null;
            public ConsoleColor? Color { get {
                    if(!IsUncovered) return null;
                    return value switch {
                        -1 => (ConsoleColor?)ConsoleColor.Black,
                        0 => (ConsoleColor?)ConsoleColor.White,
                        1 => (ConsoleColor?)ConsoleColor.Blue,
                        2 => (ConsoleColor?)ConsoleColor.Green,
                        3 => (ConsoleColor?)ConsoleColor.Red,
                        4 => (ConsoleColor?)ConsoleColor.DarkBlue,
                        5 => (ConsoleColor?)ConsoleColor.DarkRed,
                        6 => (ConsoleColor?)ConsoleColor.DarkCyan,
                        7 => (ConsoleColor?)ConsoleColor.Magenta,
                        8 => (ConsoleColor?)ConsoleColor.Gray,
                        _ => null,
                    };
                } 
            }

            private readonly int value;

            // A value of -1 denotes a mine
            internal Cell(int value) {
                this.value = value;
            }

            internal int Uncover() {
                IsUncovered = true;
                return Value ?? int.MinValue;
            }
        }
    }

    public enum BoardState {
        NotStarted, Started, Win, Lose
    }
}
