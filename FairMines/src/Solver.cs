namespace FairMines;

public class Solver
{
    private List<List<Cell>> previouslyCheckedBoundaries = new List<List<Cell>>();


    public void ResetBoundaries()
    {
        previouslyCheckedBoundaries.Clear();
    }


    private static List<List<Cell>> SplitBoundaries(List<Cell> fullBoundary)
    {
        List<List<Cell>> splitBoundaries = new List<List<Cell>>();

        if (fullBoundary.Count == 0)
        {
            return splitBoundaries;
        }


        splitBoundaries.Add(new List<Cell>());
        splitBoundaries[0].Add(fullBoundary[0]);
        for (int i = 1; i < fullBoundary.Count; i++)
        {
            Cell currentCell = fullBoundary[i];
            bool sameAsCurrentBoundary = false;
            foreach (List<Cell> boundary in splitBoundaries)
            {
                foreach (Cell previousCell in boundary)
                {
                    if (Math.Abs(currentCell.posRow - previousCell.posRow) <= 2 &&
                        Math.Abs(currentCell.posCol - previousCell.posCol) <= 2)
                    {
                        sameAsCurrentBoundary = true;
                        boundary.Add(currentCell);
                        break;
                    }
                }

                if (sameAsCurrentBoundary)
                {
                    break;
                }
            }

            if (!sameAsCurrentBoundary)
            {
                splitBoundaries.Add(new List<Cell>());
                splitBoundaries[splitBoundaries.Count - 1].Add(currentCell);
            }
        }

        bool shouldBreak = false;
        for (int i = 0; i < splitBoundaries.Count; i++)
        {
            List<Cell> boundary = splitBoundaries[i];
            foreach (Cell cell in boundary)
            {
                for (int j = 0; j < i; j++)
                {
                    List<Cell> prevBoundary = splitBoundaries[j];
                    for (int k = 0; k < prevBoundary.Count; k++)
                    {
                        Cell previousCell = prevBoundary[k];
                        if (Math.Abs(cell.posRow - previousCell.posRow) <= 2 &&
                            Math.Abs(cell.posCol - previousCell.posCol) <= 2)
                        {
                            prevBoundary.AddRange(boundary);
                            splitBoundaries.RemoveAt(i);
                            i--;
                            shouldBreak = true;
                            break;
                        }
                    }

                    if (shouldBreak)
                    {
                        break;
                    }
                }

                if (shouldBreak)
                {
                    break;
                }
            }

            if (shouldBreak)
            {
                shouldBreak = false;
            }
        }

        return splitBoundaries;
    }


    public bool HasAnyKnownUnopenedNumberCells()
    {
        for (int i = 0; i < Utils.ROWS; i++)
        {
            for (int j = 0; j < Utils.COLUMNS; j++)
            {
                if (Game.grid[i, j].shouldBeKnown && !Game.grid[i, j].revealed && Game.grid[i, j].numOfMinesAround >= 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool CheckMinePlacementForAllCells()
    {
        for (int i = 0; i < Utils.ROWS; i++)
        {
            for (int j = 0; j < Utils.COLUMNS; j++)
            {
                Cell cell = Game.grid[i, j];
                if (cell.IsANumber() && cell.numOfMinesAround != cell.GetNumOfNeighbouringMines())
                {
                    return false;
                }
            }
        }

        return true;
    }

    private List<Cell> GetUnknownNeighboursOfBoundaryCells(List<Cell> boundary)
    {
        List<Cell> unknownNeighboursOfBoundary = new List<Cell>();
        foreach (Cell cell in boundary)
        {
            foreach (Cell neighbour in cell.GetNeighbours())
            {
                if (!neighbour.revealed && !neighbour.shouldBeKnown &&
                    !unknownNeighboursOfBoundary.Contains(neighbour) && neighbour.numOfMinesAround != Utils.NUMBER)
                {
                    neighbour.valueForAlgorithm = CellValue.NONE;
                    unknownNeighboursOfBoundary.Add(neighbour);
                }
            }
        }

        return unknownNeighboursOfBoundary;
    }


    private void RemovePreviouslyCheckedBoundaries(List<List<Cell>> boundariesToCheck)
    {
        if (boundariesToCheck.Count == 0 || previouslyCheckedBoundaries.Count == 0)
        {
            return;
        }

        foreach (List<Cell> b in previouslyCheckedBoundaries)
        {
            for (int i = 0; i < boundariesToCheck.Count; i++)
            {
                List<Cell> boundary = boundariesToCheck[i];
                bool same = true;
                foreach (Cell cell in boundary)
                {
                    if (!b.Contains(cell))
                    {
                        same = false;
                        break;
                    }
                }

                if (same)
                {
                    List<Cell> neighbours = GetUnknownNeighboursOfBoundaryCells(boundariesToCheck[i]);
                    foreach (Cell cell in neighbours)
                    {
                        cell.finalValue = cell.valueForAlgorithm;
                    }

                    boundariesToCheck.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public bool UpdateCellStats(Cell cell)
    {
        bool wasUpdated =
            cell.numOfUnknownMinesAround != cell.numOfMinesAround - cell.GetNumOfNeighbouringKnownMines() ||
            cell.numOfUnknownTilesAround != cell.GetNumOfUnknownNeighbours();

        cell.numOfUnknownMinesAround =
            cell.numOfMinesAround - cell.GetNumOfNeighbouringKnownMines();
        cell.numOfUnknownTilesAround = cell.GetNumOfUnknownNeighbours();

        if (cell.numOfUnknownMinesAround == cell.numOfUnknownTilesAround || cell.numOfUnknownMinesAround == 0)
        {
            foreach (Cell neighbour in cell.GetNeighbours())
            {
                if (neighbour.shouldBeKnown) continue;

                neighbour.shouldBeKnown = true;
            }

            Game.numOfUnknownMines -= cell.numOfUnknownMinesAround;
            cell.numOfUnknownMinesAround = 0;
            cell.numOfUnknownTilesAround = 0;
        }

        return wasUpdated;
    }


    // regeneration functions
    public void FindAnyCombination(Cell cell)
    {
        List<Cell> cellsToCheck = new List<Cell>();
        foreach (Cell c in Game.grid)
        {
            if (c.revealed && c.IsANumber()  && c.numOfUnknownTilesAround > 0)
            {
                cellsToCheck.Add(c);
            }
        }

        cell.finalValue = CellValue.NUMBER;
        cell.valueForAlgorithm = CellValue.NUMBER;
        if (cellsToCheck.Contains(cell))
        {
            cellsToCheck.Remove(cell);
        }

        List<List<Cell>> boundairesToCheck = SplitBoundaries(cellsToCheck);
        if (boundairesToCheck.Count == 0)
        {
            return;
        }

        foreach (List<Cell> b in boundairesToCheck)
        foreach (Cell c in b)
            c.finalValue = CellValue.NONE;

        for (int i = 0; i < boundairesToCheck.Count; i++)
        {
            List<Cell> neighboursOfBoundary = GetUnknownNeighboursOfBoundaryCells(boundairesToCheck[i]);
            bool stop = false;
            GetCombinationForOneBoundary(boundairesToCheck[i], neighboursOfBoundary, 0, Game.numOfUnknownMines,
                ref stop);
        }
    }


    private void GetCombinationForOneBoundary(List<Cell> boundary, List<Cell> neighbourArea, int index,
        int minesToPlace,
        ref bool found)
    {
        if (index == neighbourArea.Count)
        {
            bool correctMinePlacement = true;
            foreach (Cell c in boundary)
            {
                if (c.numOfMinesAround >= 0 && c.numOfMinesAround != c.GetNumOfNeighbouringMinesForAlgorithm())
                {
                    correctMinePlacement = false;
                    break;
                }
            }

            if (correctMinePlacement)
            {
                found = true;
                foreach (Cell neighbour in neighbourArea)
                {
                    neighbour.finalValue = neighbour.valueForAlgorithm;
                }
            }

            return;
        }

        Cell cell = neighbourArea[index];

        bool possibleMine = true;
        List<Cell> cellNeighbours = cell.GetNeighbours();
        foreach (Cell neighbour in cellNeighbours)
        {
            if (neighbour.revealed && neighbour.GetNumOfNeighbouringMinesForAlgorithm() >= neighbour.numOfMinesAround)
            {
                possibleMine = false;
                break;
            }
        }

        if (!found)
        {
            if (possibleMine && minesToPlace > 0)
            {
                neighbourArea[index].valueForAlgorithm = CellValue.MINE;
                GetCombinationForOneBoundary(boundary, neighbourArea, index + 1, minesToPlace - 1, ref found);
                if (!found)
                {
                    neighbourArea[index].valueForAlgorithm = CellValue.NONE;
                }
            }
        }


        if (!found)
        {
            bool possibleNumber = true;
            foreach (Cell neighbour in cell.GetNeighbours())
            {
                if (boundary.Contains(neighbour) &&
                    neighbour.numOfUnknownMinesAround + neighbour.GetNumOfNeighbouringNumbersForAlgorithm() >=
                    neighbour.numOfUnknownTilesAround)
                {
                    possibleNumber = false;
                }
            }

            if (possibleNumber)
            {
                neighbourArea[index].valueForAlgorithm = CellValue.NUMBER;
                GetCombinationForOneBoundary(boundary, neighbourArea, index + 1, minesToPlace, ref found);
                if (!found)
                {
                    neighbourArea[index].valueForAlgorithm = CellValue.NONE;
                }
            }
        }
    }

    // functions for updating the game after every cell open

    private void TryAllGridCombinations()
    {
        List<Cell> cellsToCheck = new List<Cell>();
        foreach (Cell c in Game.grid)
        {
            if (c.revealed && c.numOfMinesAround > 0 && c.numOfUnknownTilesAround > 0)
            {
                cellsToCheck.Add(c);
            }
        }

        List<List<Cell>> boundariesToCheck = SplitBoundaries(cellsToCheck);
        if (boundariesToCheck.Count == 0)
        {
            return;
        }

        RemovePreviouslyCheckedBoundaries(boundariesToCheck);
        for (int i = 0; i < boundariesToCheck.Count; i++)
        {
            List<Cell> neighbours = GetUnknownNeighboursOfBoundaryCells(boundariesToCheck[i]);
            TryAllCombinationsForBoundary(boundariesToCheck[i],
                neighbours, 0,
                Game.numOfUnknownMines);
        }

        cellsToCheck.Clear();
        foreach (Cell c in Game.grid)
        {
            if (c.revealed && c.valueForAlgorithm > 0 && c.numOfUnknownTilesAround > 0)
            {
                cellsToCheck.Add(c);
            }
        }

        previouslyCheckedBoundaries = SplitBoundaries(cellsToCheck);
    }

    private void TryAllCombinationsForBoundary(List<Cell> boundary, List<Cell> neighbours, int index,
        int minesLeft)
    {
        if (index == neighbours.Count)
        {
            bool correctMinePlacement = true;
            foreach (Cell c in boundary)
            {
                if (c.numOfMinesAround >= 0 && c.numOfMinesAround != c.GetNumOfNeighbouringMinesForAlgorithm())
                {
                    correctMinePlacement = false;
                    break;
                }
            }

            if (correctMinePlacement)
            {
                foreach (Cell c in neighbours)
                {
                    if (c.finalValue == CellValue.NONE)
                        c.finalValue = c.valueForAlgorithm;
                    else if (c.finalValue != c.valueForAlgorithm)
                        c.finalValue = CellValue.UNKNOWN;
                }
            }

            return;
        }

        Cell cell = neighbours[index];

        bool possibleMine = true;
        foreach (Cell neighbour in cell.GetNeighbours())
        {
            if (neighbour.revealed &&
                neighbour.GetNumOfNeighbouringMinesForAlgorithm() >= neighbour.numOfMinesAround)
            {
                possibleMine = false;
                break;
            }
        }

        if (possibleMine && minesLeft > 0)
        {
            cell.valueForAlgorithm = CellValue.MINE;
            TryAllCombinationsForBoundary(boundary, neighbours, index + 1, minesLeft - 1);
            cell.valueForAlgorithm = CellValue.NONE;
        }


        bool possibleNumber = true;
        foreach (Cell neighbour in cell.GetNeighbours())
        {
            if (boundary.Contains(neighbour) &&
                neighbour.numOfUnknownMinesAround + neighbour.GetNumOfNeighbouringNumbersForAlgorithm() >=
                neighbour.numOfUnknownTilesAround)
                possibleNumber = false;
        }

        if (possibleNumber)
        {
            cell.valueForAlgorithm = CellValue.NUMBER;
            TryAllCombinationsForBoundary(boundary, neighbours, index + 1, minesLeft);
            cell.valueForAlgorithm = CellValue.NONE;
        }
    }


    public void UpdateGameAfterOpenedCell()
    {
        Queue<Cell> queue = new Queue<Cell>();
        foreach (Cell c in Game.grid)
        {
            if (c.revealed && c.IsANumber() && c.numOfUnknownMinesAround != 0)
            {
                queue.Enqueue(c);
            }
        }

        while (queue.Count > 0)
        {
            Cell next = queue.Dequeue();
            bool wasUpdated = UpdateCellStats(next);
            if (!wasUpdated) continue;

            foreach (Cell neighbour in next.GetNeighbours())
            {
                if (neighbour.revealed && neighbour.numOfUnknownTilesAround != 0)
                {
                    queue.Enqueue(neighbour);
                }
            }
        }

        TryAllGridCombinations();

        foreach (Cell cell in Game.grid)
        {
            if (cell.finalValue == CellValue.MINE)
            {
                cell.shouldBeKnown = true;
                Game.numOfUnknownMines--;
                cell.finalValue = CellValue.NONE;
            }
            else if (cell.finalValue == CellValue.NUMBER)
            {
                cell.shouldBeKnown = true;
                cell.finalValue = CellValue.NONE;
            }
            else if (cell.finalValue == CellValue.UNKNOWN)
            {
                cell.finalValue = CellValue.NONE;
                cell.valueForAlgorithm = CellValue.UNKNOWN;
            }
        }
    }
}