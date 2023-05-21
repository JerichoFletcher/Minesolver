# Minesolver

A simple player as well as auto-solver for Minesweeper, a classic puzzle video game.

## Requirements

1. .NET Runtime 7.0 or greater

## Running

1. Download the latest build zip archive from [the latest release](https://github.com/JerichoFletcher/Minesolver/releases/latest).
2. Extract to a folder and run `Minesolver.exe`.

## Usage

### Player

1. Enter the board size (format: `<row> <col>`) and the number of mines. A game board will be displayed.
2. Enter a pair of numbers (format: `<row> <col>`) to click a square on the board.
3. Enter a pair of numbers followed by 'F' (format: `<row> <col> F`) to flag a covered square.
4. Clicking on an already revealed square performs a chord, which automatically reveals adjacent covered squares if the number of adjacent flags match the number displayed on the square. For example:<br />![image](https://github.com/JerichoFletcher/Minesolver/assets/62737325/7409ed86-502c-4333-80e4-cd11540a60e7)<br />Clicking on the square **(2, 2)**, which is numbered **2**, will reveal the squares **(2, 3)**, **(3, 1)**, **(3, 2)**, and **(3, 3)**. Note that if one of the flagged square is a safe square, this may result in a mined square being revealed by chording.
5. The game ends when all the remaining covered squares are mined, or a mined square is revealed.

### Auto-solver
1. Enter the board size (format: `<row> <col>`) and the number of mines.
2. The auto-solver will also prompt for either the interval (in seconds) between clicks for single solve mode, or the number of attempts for mass solve mode.
3. In single solve mode, each click performed by the auto-solver and its result will be displayed in a game board on the terminal.
4. In mass solve mode, the result of each attempt, and the ratio of success to the total number of attempts, will be displayed on the terminal.
5. Each auto-solver employs different strategies to solve the board, and thus may produce different results.
