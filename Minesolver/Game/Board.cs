using Minesolver.CLI;

namespace Minesolver.Game {
    internal class Board {
        public int MineCount { get; private set; }
        public int RowCount { get; private set; }
        public int ColCount { get; private set; }

        public int RemainingMineCount { get; private set; }
        public bool Finished => RemainingMineCount == 0;

        private readonly Cell[,] cells;

        public Board(int mineCount, int rowCount, int colCount) {
            if(mineCount > rowCount * colCount) throw new ArgumentOutOfRangeException($"Mine count ({mineCount}) exceeds cell count ({rowCount * colCount})");

            RowCount = rowCount;
            ColCount = colCount;
            MineCount = mineCount;

            cells = new Cell[rowCount, colCount];
            BuildCells();
        }

        public bool WithinBounds(int row, int col) {
            return row >= 0 && col >= 0 && row < RowCount && col < ColCount;
        }

        public void PrintBoardToConsole() {
            ConsoleHelper.Write("Remaining mines: ", ConsoleColor.White);
            ConsoleHelper.WriteLine(RemainingMineCount.ToString(), ConsoleColor.Red);

            for(int row = -1; row < RowCount; row++) {
                for(int col = -1; col < ColCount; col++) {
                    if(row == -1) {
                        if(col == -1) {
                            Console.Write(" ");
                        } else {
                            ConsoleHelper.Write(((col + 1) % 10).ToString(), ConsoleColor.DarkGray);
                        }
                    } else {
                        if(col == -1) {
                            ConsoleHelper.Write(((row + 1) % 10).ToString(), ConsoleColor.DarkGray);
                        } else {
                            Cell cell = cells[row, col];
                            if(cell.IsUncovered) {
                                int cellValue = cell.Value ?? int.MinValue;
                                ConsoleColor cellColor = cell.Color ?? ConsoleColor.DarkGray;
                                ConsoleHelper.Write(cellValue == -1 ? "*" : cellValue.ToString(), cellColor);
                            } else {
                                ConsoleColor color = Console.BackgroundColor;
                                Console.BackgroundColor = ConsoleColor.DarkGray;
                                Console.Write(" ");
                                Console.BackgroundColor = color;
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        //public int Click(int row, int col) {

        //}

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
                            return ConsoleColor.Black;
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
        }
    }
}
