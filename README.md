# Fair minesweeper
Minesweeper implementation on a 15x15 grid with "fair" rules:
* if a player can deduce that at least one unopened tile is definitely a number, and the player chooses to open another tile whose value cannot be for sure known, then the opened tile will contain a mine.
* if there are no tiles whose value can be deduced, then the player may choose any unopened unknown tile and it will not contain a mine.  

## Controls
* `Space/MouseLeft` - reveal a tile
* `R` - restart the game
* `Q` - quit the game
* `F/MouseRight` - place/remove a flag on a tile
* `M` - toggle debug mode

## Difficulty
You may choose from 3 difficulties: `EASY`, `NORMAL` and `HARD`.
* `EASY` - 10% of the tiles are mines
* `NORMAL` - 20% of the tiles are mines
* `HARD` - 30% of the tiles are mines

## Debug mode
By pressing `M` on your keyboard, you can toggle the debug mode on and off. In the debug mode, unopened tiles that are definitely a mine are shown as red. Unopened tiles that are definitely a number are shown as green.
By default, the debug mode is disabled.

## Technicalities
This application was written in Rider IDE and uses Raylib library https://www.raylib.com/index.html. This library is supported across multiple platforms and has over 60 bindings to the library so be sure to check them out!
If you want to launch an executable file, you can go to the `/FairMines/bin/Release/net8.0` folder and choose Windows (x64 only), Linux (x64 only) or macOS (arm64 only) and inside those folders should be executable files named FairMines (I worked on this game on macOS, but also tested the executable file on Windows, but not on Linux :(   ).  

# Code sum-up

## Game.cs
This staic class contains information about the whole game and the whole UI code also written there. It's got a few variables needed for calculations such as:
* `Cell[,] grid` - 2d array that contains cells/tiles of the game
* `DateTime timeStart` - info about when user started current game
* `DateTime timeEnd` - info about when user ended current game
* `int tilesRevealed` - number of tiles that are revealed
* `Difficulty difficulty` - game difficulty (default value - `EASY`)
* `Mode mode` - game mode (default value - `NORMAL`)
* `int numOfAllMines` - number of all mines based on game difficulty
* `int numOfUnknownMines` - number of those mines that are unknown

## Cell.cs
This class contains information about each individual cell inside the grid. Each cell contains these variables and functions:

### Variables
* `int posRow`
* `int posCol`
* `int numOfMinesAround` - number of mines around
* `int numOfUnknownMinesAround` - number of unknown mines around
* `int numOfUnknownTilesAround` - number of unknown tiles around
* `bool flagged` - if a flag is put on top of the cell
* `bool revealed` - if a tile is revealed
* `bool shouldBeKnown` - if a played should be able to deduce that said tile is for sure a number or a mine
* `CellValue valueForAlgorithm` and `public CellValue finalValue` - variables used in the algorithm for computations 

### Functions
* `GetNeighbours` - returns a list of cells that are neighbours of current cell
* `GetNumOfNeighbouringKnownMines` - returns a list of neighbours that are a mine and should be known by the player
* `GetNumOfUnknownNeighbours` - returns a list of neighbours that are unknown
* `GetNumOfNeighbouringMines` - returns a list of all neighbouring mines for current grid state
* `GetNumOfNeighbouringMinesForAlgorithm` - returns a list of all neighbouring mines for the current grid computation (using the `valueForAlgorithm` value)
* `GetNumOfNeighbouringNumbersForAlgorithm` - returns a list of all neighbouring numbers for the current grid computation (using the `valueForAlgorithm` value)


## Generator.cs
This class takes care of the initial generation of the grid and it also takes care of the regeneration when user clicked a unknown mine, so the program has to change the value of that cell to a number and regenerate all other cells so that a correct position is found.

### Regeneration
If a user does not have a cell whose value should be known and it is a number, then if the user clicks on a unknown cell that contains a mine, we have to regenerate the current mine placement on the grid. Firstly we try to find correct mine placement for all the cells that are neighbours to the already opened cells. If such a mine placement is found, then we just have to place the remaining mines randomly throughout the unknown cells.

* `void Generate` - function that generates the initial game grid
* `bool Regenerate` - function that regenerates all the unknown cell after a unknown cell has been clicked.
* `void PlaceMines` - function that places all the remaining mines that are unknown, used either in generating the grid, or in regenerating the grid.
* `void CalculateNeighbouringMines` - sets the correct number of mines around to each number
* `int UpdateValuesAfterCorrectCombination` - this function updates values of the cells after every regeneration


## Solver.cs
This class handles all the algorithmic stuff of this program. Main functions:

* `void FindAnyCombination` - function used in regenerating the grid and finding the right combination for already opened cells that still have unknown, unopened neighbours
* `void TryAllGridCombinations` - function used in every update of the game, it's purpose is to try all the mine placement combinations around all opened cells, and stores the computation values inside `valueForAlgorithm` and `finalValue` variables
* `void UpdateGameAfterOpenedCell` - called after any cell is opened, it uses the `TryAllGridCombinations` function and based on the final values it determines whether the value of a cell should be known to the user or not

## Utils.cs
This file contains some constants and functions that I didn't want in other files

[//]: # ()
[//]: # (## Known issues)
[//]: # (As the algorithm takes care only about the neighbours of opened cells, it is possible that no NUMBERS &#40;!!!&#41; are left, and if a player clicks a unknown cell that contains a mine, the value of the cell should be changed to a number, but the value cannot be changed and the player loses.)