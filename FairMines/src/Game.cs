namespace FairMines;

using Raylib_cs;
using System.Numerics;

public static class Game
{
    public enum GameState
    {
        SETTINGS,
        PLAYING,
        LOSE,
        WIN
    }

    public enum Mode
    {
        NORMAL,
        DEBUG
    }

    public static GameState gameState = GameState.SETTINGS;
    public static int tilesRevealed;
    public static DateTime timeStart;
    public static DateTime timeEnd;
    public static Cell[,] grid = new Cell[Utils.ROWS,Utils.COLUMNS];
    public static int numOfAllMines;
    public static int numOfUnknownMines;
    private static bool shouldPlaySound = false;
    public static Generator generator = new Generator();
    public static Mode mode = Mode.NORMAL;
    public static Difficulty difficulty = Difficulty.EASY;

    public static void GameLoop()
    {
        Raylib.InitWindow(Utils.SCREEN_WIDTH, Utils.SCREEN_HEIGHT, "Fair minesweeper");
        Raylib.InitAudioDevice();

        Sound loseSound = Raylib.LoadSound("../../../sounds/NAUR.wav");
        Sound winSound = Raylib.LoadSound("../../../sounds/YIPEEE.wav");

        Rectangle easyButton = new Rectangle((Utils.SCREEN_WIDTH / 2) - 100, 350, 200, 50);
        Rectangle normalButton = new Rectangle((Utils.SCREEN_WIDTH / 2) - 100, 420, 200, 50);
        Rectangle hardButton = new Rectangle((Utils.SCREEN_WIDTH / 2) - 100, 490, 200, 50);

        Raylib.SetTargetFPS(60);

        while (!Raylib.WindowShouldClose())
        {
            // HANDLING EVENTS
            if (gameState != GameState.SETTINGS)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left) || Raylib.IsKeyPressed(KeyboardKey.Space))
                {
                    Vector2 mousePosition = Raylib.GetMousePosition();
                    int indexI = (int)(mousePosition.Y - (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT)) / Utils.CELL_HEIGHT;
                    int indexJ = (int)mousePosition.X / Utils.CELL_WIDTH;

                    if (gameState == GameState.PLAYING && Utils.IsIndexValid(indexI, indexJ))
                    {
                        CellReveal(indexI, indexJ);
                    }
                }
                else if (Raylib.IsMouseButtonPressed(MouseButton.Right) || Raylib.IsKeyPressed(KeyboardKey.F))
                {
                    Vector2 mousePosition = Raylib.GetMousePosition();
                    int indexI = (int)(mousePosition.Y - (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT)) / Utils.CELL_HEIGHT;
                    int indexJ = (int)mousePosition.X / Utils.CELL_WIDTH;

                    if (gameState == GameState.PLAYING && Utils.IsIndexValid(indexI, indexJ))
                    {
                        CellFlag(indexI, indexJ);
                    }
                }

                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    gameState = GameState.SETTINGS;
                }

                if (Raylib.IsKeyPressed(KeyboardKey.M))
                {
                    if (mode == Mode.DEBUG)
                    {
                        mode = Mode.NORMAL;
                    }
                    else
                    {
                        mode = Mode.DEBUG;
                    }
                }
            }

            if (gameState == GameState.SETTINGS)
            {
                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    Vector2 mousePosition = Raylib.GetMousePosition();
                    if (Raylib.CheckCollisionPointRec(mousePosition, easyButton))
                    {
                        difficulty = Difficulty.EASY;
                        GameInit();
                    }
                    else if (Raylib.CheckCollisionPointRec(mousePosition, normalButton))
                    {
                        difficulty = Difficulty.NORMAL;
                        GameInit();
                    }
                    else if (Raylib.CheckCollisionPointRec(mousePosition, hardButton))
                    {
                        difficulty = Difficulty.HARD;
                        GameInit();
                    }
                }
            }

            if (Raylib.IsKeyPressed(KeyboardKey.Q))
            {
                break;
            }

            // DRAWING
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.LightGray);

            if (gameState != GameState.SETTINGS)
            {
                DrawTopSection();
                DrawGrid();

                if (gameState == GameState.LOSE)
                {
                    if (shouldPlaySound)
                    {
                        Raylib.PlaySound(loseSound);
                        shouldPlaySound = false;
                    }

                    Raylib.DrawRectangle(0, 0, Utils.SCREEN_WIDTH, Utils.SCREEN_HEIGHT, Raylib.Fade(Color.White, 0.8f));
                    Raylib.DrawText("You lost!", Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("You lost!", 50) / 2,
                        (Utils.SCREEN_HEIGHT / 2) - 25, 50, Color.DarkGray);
                    Raylib.DrawText("Press 'R' to restart the game",
                        Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Press 'R' to restart the game", 20) / 2,
                        (int)(Utils.SCREEN_HEIGHT * 0.75f - 20), 20, Color.DarkGray);
                    Raylib.DrawText("Press 'Q' to quit the game",
                        Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Press 'Q' to quit the game", 20) / 2,
                        (int)(Utils.SCREEN_HEIGHT * 0.75f + 20), 20, Color.DarkGray);

                    TimeSpan timePlaying = timeEnd - timeStart;
                    int minutes = (int)timePlaying.Minutes;
                    int seconds = (int)timePlaying.Seconds % 60;
                    Raylib.DrawText($"Time: {minutes} minutes, {seconds} seconds", 20, Utils.SCREEN_HEIGHT - 40,
                        20, Color.DarkGray);
                }

                if (gameState == GameState.WIN)
                {
                    if (shouldPlaySound)
                    {
                        Raylib.PlaySound(winSound);
                        shouldPlaySound = false;
                    }

                    Raylib.DrawRectangle(0, 0, Utils.SCREEN_WIDTH, Utils.SCREEN_HEIGHT, Raylib.Fade(Color.White, 0.8f));
                    Raylib.DrawText("Congratulations,",
                        Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Congratulations,", 50) / 2,
                        (Utils.SCREEN_HEIGHT / 2) - 50,
                        50, Color.DarkGray);
                    Raylib.DrawText("you won!",
                        Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("you won!", 50) / 2, (Utils.SCREEN_HEIGHT / 2),
                        50, Color.DarkGray);
                    Raylib.DrawText("Press 'R' to restart the game",
                        Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Press 'R' to restart the game", 20) / 2,
                        (int)(Utils.SCREEN_HEIGHT * 0.75f - 20), 20, Color.DarkGray);
                    Raylib.DrawText("Press 'Q' to quit the game",
                        Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Press 'Q' to quit the game", 20) / 2,
                        (int)(Utils.SCREEN_HEIGHT * 0.75f + 20), 20, Color.DarkGray);

                    TimeSpan timePlaying = timeEnd - timeStart;
                    int minutes = (int)timePlaying.Minutes;
                    int seconds = (int)timePlaying.Seconds % 60;
                    Raylib.DrawText($"Time: {minutes} minutes, {seconds} seconds", 20, Utils.SCREEN_HEIGHT - 40,
                        20, Color.DarkGray);
                }
            }

            else if (gameState == GameState.SETTINGS)
            {
                Raylib.DrawText("Fair minesweeper",
                    Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Fair minesweeper", 50) / 2,
                    100, 50, Color.DarkGray);
                Raylib.DrawText("Choose difficulty:",
                    Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Choose difficulty:", 30) / 2,
                    300, 30, Color.DarkGray);
                Raylib.DrawRectangleRec(easyButton, Color.Green);
                Raylib.DrawText("Easy",
                    Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Easy", 30) / 2,
                    (int)easyButton.Y + 10, 30, Color.DarkGray);
                Raylib.DrawRectangleRec(normalButton, Color.Yellow);
                Raylib.DrawText("Normal",
                    Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Normal", 30) / 2,
                    (int)normalButton.Y + 10, 30, Color.DarkGray);
                Raylib.DrawRectangleRec(hardButton, Color.Orange);
                Raylib.DrawText("Hard",
                    Utils.SCREEN_WIDTH / 2 - Raylib.MeasureText("Hard", 30) / 2,
                    (int)hardButton.Y + 10, 30, Color.DarkGray);
            }

            Raylib.EndDrawing();
        }

        Raylib.UnloadSound(loseSound);
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }


    static void DrawGrid()
    {
        for (int i = 0; i < Utils.ROWS; i++)
        {
            for (int j = 0; j < Utils.COLUMNS; j++)
            {
                CellDraw(grid[i, j]);
            }
        }
    }

    static void DrawTopSection()
    {
        Raylib.DrawRectangle(0, 0, Utils.SCREEN_WIDTH, (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT), Color.White);
        Raylib.DrawText($"Difficulty: {Game.difficulty}", (Utils.SCREEN_WIDTH / 2) - (Raylib.MeasureText($"Difficulty: {Game.difficulty}", 40) / 2), 30, 40, Color.Black);
    }


    static void CellDraw(Cell cell)
    {
        if (cell.revealed || mode == Mode.DEBUG)
        {
            if (cell.numOfMinesAround == Utils.MINE)
            {
                if (cell.shouldBeKnown && mode == Mode.DEBUG || cell.revealed)
                {
                    Raylib.DrawRectangle(cell.posCol * Utils.CELL_WIDTH,
                        (cell.posRow * Utils.CELL_HEIGHT + (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT)),
                        Utils.CELL_WIDTH,
                        Utils.CELL_HEIGHT,
                        Color.Red);
                }

                if (cell.revealed)
                {
                    Raylib.DrawText("X", (cell.posCol * Utils.CELL_WIDTH) + (Utils.CELL_WIDTH / 2) -
                                         (Raylib.MeasureText(
                                             "X",
                                             (int)(Utils.CELL_HEIGHT * 0.75f)) / 2),
                        cell.posRow * Utils.CELL_HEIGHT + (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT) + 6,
                        (int)(Utils.CELL_HEIGHT * 0.75f),
                        Utils.GetColorBasedOnText("X"));
                }
            }
            else if (cell.IsANumber())
            {
                if (cell.revealed)
                {
                    Raylib.DrawRectangle(cell.posCol * Utils.CELL_WIDTH,
                        cell.posRow * Utils.CELL_HEIGHT + (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT),
                        Utils.CELL_WIDTH,
                        Utils.CELL_HEIGHT, Color.White);
                    if (cell.numOfMinesAround > 0 && cell.numOfMinesAround <= 8)
                    {
                        Raylib.DrawText(cell.numOfMinesAround.ToString(), (cell.posCol * Utils.CELL_WIDTH) +
                                                                          (Utils.CELL_WIDTH / 2) -
                                                                          (Raylib.MeasureText(
                                                                              cell.numOfMinesAround.ToString(),
                                                                              (int)(Utils.CELL_HEIGHT * 0.75f)) / 2),
                            cell.posRow * Utils.CELL_HEIGHT + (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT) + 6,
                            (int)(Utils.CELL_HEIGHT * 0.75f),
                            Utils.GetColorBasedOnText(cell.numOfMinesAround.ToString()));
                    }
                }
                else if (cell.shouldBeKnown && mode == Mode.DEBUG)
                {
                    Raylib.DrawRectangle(cell.posCol * Utils.CELL_WIDTH,
                        cell.posRow * Utils.CELL_HEIGHT + (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT),
                        Utils.CELL_WIDTH,
                        Utils.CELL_HEIGHT, Color.Green);
                }
            }
        }
        else if (cell.flagged)
        {
            Raylib.DrawText("F", (cell.posCol * Utils.CELL_WIDTH) + (Utils.CELL_WIDTH / 2) -
                                 (Raylib.MeasureText(
                                     "F",
                                     (int)(Utils.CELL_HEIGHT * 0.75f)) / 2),
                cell.posRow * Utils.CELL_HEIGHT + (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT) + 6,
                (int)(Utils.CELL_HEIGHT * 0.75f), Color.DarkGray);
        }

        Raylib.DrawRectangleLines(cell.posCol * Utils.CELL_WIDTH,
            cell.posRow * Utils.CELL_HEIGHT + (Utils.SCREEN_HEIGHT - Utils.GRID_HEIGHT), Utils.CELL_WIDTH,
            Utils.CELL_HEIGHT,
            Color.Black);
    }

    static void HandleCellMineReveal(int rowNumber, int columnNumber)
    {
        gameState = GameState.LOSE;
        timeEnd = DateTime.Now;
        grid[rowNumber, columnNumber].revealed = true;
    }

    static void CheckWin()
    {
        if (tilesRevealed >= Utils.ROWS * Utils.COLUMNS - numOfAllMines)
        {
            gameState = GameState.WIN;
            timeEnd = DateTime.Now;
        }
    }

    static void HandleCellNumberReveal(int rowNumber, int columnNumber)
    {
        grid[rowNumber, columnNumber].revealed = true;
        grid[rowNumber, columnNumber].shouldBeKnown = true;
        generator.solver.UpdateCellStats(grid[rowNumber, columnNumber]);
        if (grid[rowNumber, columnNumber].numOfMinesAround == 0)
        {
            GridClearMultiple(rowNumber, columnNumber);
        }

        tilesRevealed++;
        CheckWin();
    }

    static void HandleCellReveal(int rowNumber, int columnNumber)
    {
        if (grid[rowNumber, columnNumber].numOfMinesAround == Utils.MINE)
        {
            HandleCellMineReveal(rowNumber, columnNumber);
        }
        else
        {
            HandleCellNumberReveal(rowNumber, columnNumber);
            generator.solver.UpdateGameAfterOpenedCell();
        }
    }


    static void CellReveal(int rowNumber, int columnNumber)
    {
        if (grid[rowNumber, columnNumber].flagged || grid[rowNumber, columnNumber].revealed)
        {
            return;
        }

        if (!AnyTileRevealed())
        {
            generator.Generate(rowNumber, columnNumber);
            grid[rowNumber, columnNumber].revealed = true;
            tilesRevealed++;
            if (grid[rowNumber, columnNumber].numOfMinesAround == 0)
            {
                GridClearMultiple(rowNumber, columnNumber);
            }

            CheckWin();
            generator.solver.UpdateGameAfterOpenedCell();
            return;
        }

        if (!grid[rowNumber, columnNumber].shouldBeKnown)
        {
            if (!generator.solver.HasAnyKnownUnopenedNumberCells())
            {
                if (grid[rowNumber, columnNumber].numOfMinesAround == Utils.MINE)
                {
                    bool didRegenerate = generator.ReGenerate(grid[rowNumber, columnNumber]);
                    if (didRegenerate)
                    {
                        HandleCellNumberReveal(rowNumber, columnNumber);
                        generator.solver.UpdateGameAfterOpenedCell();
                    }
                    else
                    {
                        grid[rowNumber, columnNumber].numOfMinesAround = Utils.MINE;
                        HandleCellMineReveal(rowNumber, columnNumber);
                    }
                }
                else
                {
                    HandleCellNumberReveal(rowNumber, columnNumber);
                    generator.solver.UpdateGameAfterOpenedCell();
                }
            }
            else
            {
                grid[rowNumber, columnNumber].numOfMinesAround = Utils.MINE;
                HandleCellMineReveal(rowNumber, columnNumber);
            }
        }
        else
        {
            HandleCellReveal(rowNumber, columnNumber);
        }
    }

    static void CellFlag(int rowIndex, int columnIndex)
    {
        if (!grid[rowIndex, columnIndex].revealed)
        {
            grid[rowIndex, columnIndex].flagged = !grid[rowIndex, columnIndex].flagged;
        }
    }

    static void GridClearMultiple(int rowNumber, int columnNumber)
    {
        foreach (Cell neighbour in Game.grid[rowNumber, columnNumber].GetNeighbours())
        {
            if (!neighbour.revealed && !neighbour.flagged)
            {
                HandleCellNumberReveal(neighbour.posRow, neighbour.posCol);
            }
        }
    }

    static private bool AnyTileRevealed()
    {
        for (int rowIndex = 0; rowIndex < Utils.ROWS; rowIndex++)
        {
            for (int columnIndex = 0; columnIndex < Utils.COLUMNS; columnIndex++)
            {
                if (grid[rowIndex, columnIndex].revealed)
                {
                    return true;
                }
            }
        }

        return false;
    }

    static void GridInit()
    {
        grid = new Cell[Utils.ROWS, Utils.COLUMNS];
        for (int i = 0; i < Utils.ROWS; i++)
        {
            for (int j = 0; j < Utils.COLUMNS; j++)
            {
                grid[i, j] = new Cell(i, j);
            }
        }
    }


    static void GameInit()
    {
        shouldPlaySound = true;
        GridInit();
        numOfAllMines = Utils.GetNumOfMinesBasedOnDifficulty(difficulty);
        numOfUnknownMines = numOfAllMines;
        gameState = GameState.PLAYING;
        tilesRevealed = 0;
        timeStart = DateTime.Now;
    }
}