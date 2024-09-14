namespace FairMines;

public class Generator
{
    public Solver solver = new Solver();

    public void Generate(int initialCellRow, int initialCellCol)
    {
        solver.ResetBoundaries();
        Game.grid[initialCellRow, initialCellCol].numOfMinesAround = Utils.NUMBER;
        Game.grid[initialCellRow, initialCellCol].shouldBeKnown = true;
        PlaceMines(Game.numOfAllMines);
    }

    public bool ReGenerate(Cell cell)
    {
        cell.numOfMinesAround = Utils.NUMBER;
        solver.FindAnyCombination(cell);
        Game.numOfUnknownMines = Game.numOfAllMines;
        int minesAlreadyPlaced = UpdateValuesAfterCorrectCombination();
        PlaceMines(Game.numOfUnknownMines - minesAlreadyPlaced);
        return solver.CheckMinePlacementForAllCells();
    }


    private void PlaceMines(int numOfMinesToPlace)
    {
        Random random = new Random();
        while (numOfMinesToPlace > 0)
        {
            int i = random.Next() % Utils.COLUMNS;
            int j = random.Next() % Utils.ROWS;

            if ((Game.grid[i, j].numOfMinesAround == Utils.UNKNOWN) &&
                !Game.grid[i, j].revealed)
            {
                Game.grid[i, j].numOfMinesAround = Utils.MINE;
                numOfMinesToPlace--;
            }
        }

        CalculateNeighbouringMines();
    }

    private void CalculateNeighbouringMines()
    {
        for (int i = 0; i < Utils.ROWS; i++)
        {
            for (int j = 0; j < Utils.COLUMNS; j++)
            {
                if (!Game.grid[i, j].revealed && Game.grid[i, j].numOfMinesAround != Utils.MINE)
                {
                    Game.grid[i, j].numOfMinesAround = Game.grid[i, j].GetNumOfNeighbouringMines();
                }
            }
        }
    }

    private int UpdateValuesAfterCorrectCombination()
    {
        int unknownMinesPlaced = 0;
        foreach (Cell c in Game.grid)
        {
            if (c.numOfMinesAround == Utils.MINE && c.shouldBeKnown)
                Game.numOfUnknownMines--;
            else if (c.finalValue == CellValue.MINE)
            {
                c.numOfMinesAround = Utils.MINE;
                c.finalValue = CellValue.NONE;
                c.valueForAlgorithm = CellValue.NONE;
                unknownMinesPlaced++;
            }
            else if (c.finalValue == CellValue.NUMBER)
            {
                c.numOfMinesAround = Utils.NUMBER;
                c.finalValue = CellValue.NONE;
                c.valueForAlgorithm = CellValue.NONE;
            }
            else if (!c.shouldBeKnown)
            {
                c.numOfMinesAround = Utils.UNKNOWN;
                c.valueForAlgorithm = CellValue.NONE;
            }
        }

        return unknownMinesPlaced;
    }
}