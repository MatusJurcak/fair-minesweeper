using Raylib_cs;


namespace FairMines;

public enum Difficulty
{
    EASY,
    NORMAL,
    HARD
}

public static class Utils
{
    public const int SCREEN_WIDTH = 600;
    public const int SCREEN_HEIGHT = 700;
    public const int GRID_HEIGHT = SCREEN_HEIGHT - 100;
    public const int GRID_WIDTH = SCREEN_WIDTH;
    public const int ROWS = 15;
    public const int COLUMNS = 15;
    public const int CELL_HEIGHT = GRID_HEIGHT / ROWS;
    public const int CELL_WIDTH = GRID_WIDTH / COLUMNS;
    public static readonly int[] allPositionsY = { 1, 1, 1, 0, 0, -1, -1, -1 };
    public static readonly int[] allPositionsX = { -1, 0, 1, -1, 1, -1, 0, 1 };
    
    public const int NUMBER = -1; // cell is a unknown number - used mainly in computations
    public const int MINE = -2;
    public const int UNKNOWN = -3;

    public const int NUM_OF_ALL_MINES_FOR_EASY_DIFF = (int)(ROWS * COLUMNS * 0.1f);
    public const int NUM_OF_ALL_MINES_FOR_NORMAL_DIFF = (int)(ROWS * COLUMNS * 0.2f);
    public const int NUM_OF_ALL_MINES_FOR_HARD_DIFF = (int)(ROWS * COLUMNS * 0.3f);
    
    public static bool IsIndexValid(int i, int j)
    {
        return i >= 0 && i < ROWS && j >= 0 && j < COLUMNS;
    }
    
    public static Color GetColorBasedOnText(string text)
    {
        switch (text)
        {
            case "1": return Color.DarkBlue;
            case "2": return Color.DarkGreen;
            case "3": return Color.Red;
            case "4": return Color.Violet;
            case "5": return Color.Orange;
            case "6": return Color.Beige;
            case "7": return Color.Pink;
            case "8": return Color.Maroon;
            default: return Color.DarkGray;
        }
    }
    public static int GetNumOfMinesBasedOnDifficulty(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.HARD:
            {
                return NUM_OF_ALL_MINES_FOR_HARD_DIFF;
            }
            case Difficulty.NORMAL:
            {
                return NUM_OF_ALL_MINES_FOR_NORMAL_DIFF;
            }
            default: return NUM_OF_ALL_MINES_FOR_EASY_DIFF;
        }
    }
}
