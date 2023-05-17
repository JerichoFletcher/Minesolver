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
            firstClicked = false;
            State = BoardState.NotStarted;
            BuildCells();
        }

        public bool WithinBounds(int row, int col) {
            return row >= 0 && col >= 0 && row < RowCount && col < ColCount;
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
                            if(cell.IsUncovered) {
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

        public int Click(int clickRow, int clickCol) {
            (int row, int col) = (clickRow - 1, clickCol - 1);
            if(!WithinBounds(row, col)) throw new ArgumentOutOfRangeException($"Position {clickRow}:{clickCol} -> {row}:{col} is out of bounds for mapsize {RowCount}x{ColCount}");
            if(cells[row, col].IsUncovered) return Peek(clickRow, clickCol) ?? int.MinValue;

            if(Finished) {
                throw new InvalidOperationException("The board is already finished");
            }
            
            State = BoardState.Started;
            if(!firstClicked) {
                int tryCount = 0;
                bool check = cells[row, col].Uncover() != 0;
                while(check) {
                    BuildCells();
                    int checkVal = cells[row, col].Uncover();
                    check = (tryCount++ < RowCount * ColCount) ? (checkVal != 0) : (checkVal == -1);
                }
                firstClicked = true;
            }

            int value = cells[row, col].Uncover();
            RemainingCoveredCells--;

            if(value == 0) {
                for(int dx = -1; dx <= 1; dx++) {
                    for(int dy = -1; dy <= 1; dy++) {
                        if(dx == 0 && dy == 0) continue;
                        int neighborRow = row + dx;
                        int neighborCol = col + dy;
                        if(!WithinBounds(neighborRow, neighborCol)) continue;

                        Click(neighborRow + 1, neighborCol + 1);
                    }
                }
            } else if(value == -1) State = BoardState.Lose;

            if(!Finished && RemainingCoveredCells == MineCount) State = BoardState.Win;

            return value;
        }

        public int? Peek(int peekRow, int peekCol) {
            (int row, int col) = (peekRow - 1,  peekCol - 1);
            if(!WithinBounds(row, col)) throw new ArgumentOutOfRangeException($"Position {row}:{col} is out of bounds for mapsize {RowCount}x{ColCount}");
            return cells[row, col].Value;
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
                    for(int dx = -1; dx <= 1; dx++) {
                        for(int dy = -1; dy <= 1; dy++) {
                            if(dx == 0 && dy == 0) continue;
                            int neighborRow = row + dx;
                            int neighborCol = col + dy;
                            if(!WithinBounds(neighborRow, neighborCol)) continue;

                            if(numbers[neighborRow, neighborCol] != -1) {
                                numbers[neighborRow, neighborCol]++;
                            }
                        }
                    }
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
                    switch(value) {
                        case -1:
                            return ConsoleColor.Black;
                        case 0:
                            return ConsoleColor.White;
                        case 1:
                            return ConsoleColor.Blue;
                        case 2:
                            return ConsoleColor.Green;
                        case 3:
                            return ConsoleColor.Red;
                        case 4:
                            return ConsoleColor.DarkBlue;
                        case 5:
                            return ConsoleColor.DarkRed;
                        case 6:
                            return ConsoleColor.DarkCyan;
                        case 7:
                            return ConsoleColor.Magenta;
                        case 8:
                            return ConsoleColor.Gray;
                        default:
                            return null;
                    }
                } 
            }

            private int value;

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
