namespace FairMines;

public enum CellValue
{
    MINE, 
    NUMBER, 
    UNKNOWN,
    NONE
}
public class Cell
{
   

    public int posRow;
    public int posCol;
    
    // 0-8 if the cell is a number (then the corresponding number is a number of mines around this cell)
    // for other values check Utils file
    public int numOfMinesAround;
    public bool flagged;
    public bool revealed;
    public int numOfUnknownMinesAround;
    public int numOfUnknownTilesAround;
    
    // whether the user should be able to deduct the value of the cell by current state of the game
    public bool shouldBeKnown;
    
    // variable that is used in computations
    public CellValue valueForAlgorithm;

    // variable that is used in computations
    public CellValue finalValue;

    public Cell(int row, int col)
    {
        posRow = row;
        posCol = col;
        numOfMinesAround = Utils.UNKNOWN;
        flagged = false;
        revealed = false;
        numOfUnknownMinesAround = 8;
        numOfUnknownTilesAround = 8;
        valueForAlgorithm = CellValue.NONE;
        finalValue = CellValue.NONE;
        shouldBeKnown = false;
    }

    public List<Cell> GetNeighbours()
    {
        List<Cell> neighbours = new List<Cell>();
        for (int x = 0; x < Utils.allPositionsX.Length; x++)
        {
            int newRow = posRow + Utils.allPositionsX[x];
            int newCol = posCol + Utils.allPositionsY[x];
            if (Utils.IsIndexValid(newRow, newCol))
            {
                neighbours.Add(Game.grid[newRow, newCol]);
            }
        }

        return neighbours;
    }

    public bool IsANumber()
    {
        return numOfMinesAround >= 0 && numOfMinesAround <= 8;
    }
    
    public int GetNumOfNeighbouringMinesForAlgorithm()
    {
        int num = 0;
        foreach (Cell cell in GetNeighbours())
        {
            if ((cell.numOfMinesAround == Utils.MINE && cell.shouldBeKnown) || cell.valueForAlgorithm == CellValue.MINE)
            {
                num++;
            }
        }

        return num;
    }
    
    public int GetNumOfNeighbouringNumbersForAlgorithm()
    {
        int num = 0;
        foreach (Cell cell in GetNeighbours())
        {
            if (cell.valueForAlgorithm == CellValue.NUMBER)
            {
                num++;
            }
        }

        return num;
    }

    public int GetNumOfNeighbouringMines()
    {
        int num = 0;
        foreach (Cell cell in GetNeighbours())
        {
            if (cell.numOfMinesAround == Utils.MINE)
            {
                num++;
            }
        }

        return num;
    }
    
    public int GetNumOfUnknownNeighbours()
    {
        int num = 0;
        foreach (Cell cell in GetNeighbours())
        {
            if (!cell.shouldBeKnown)
            {
                num++;
            }
        }

        return num;
    }
    
    public int GetNumOfNeighbouringKnownMines()
    {
        int num = 0;
        foreach (Cell cell in GetNeighbours())
        {
            if (cell.numOfMinesAround == Utils.MINE && cell.shouldBeKnown)
            {
                num++;
            }
        }

        return num;
    }
}