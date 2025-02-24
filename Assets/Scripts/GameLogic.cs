using UnityEngine;

public class GameLogic : MonoBehaviour
{
    //default board size
    public int width = 16;
    public int height = 16;
    public int mineCount = 32;
    public int flagCount;

    private Board board;
    private Cell[,] state;

    public bool gameOver; //not lose state, but indicating game is finished
    public bool wonGame = false;
    public bool flagsLeft = true;

    public GameUI gameUI;

    private void OnValidate()
    {
        mineCount = Mathf.Clamp(mineCount, 0, width * height);
    }
    
    private void Awake()
    {
        board = GetComponentInChildren<Board>();
    }

    private void Start()
    {
        NewGame();
        flagCount = mineCount;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) //press r to restart game
        {
            NewGame();
        }
        
        else if(!gameOver)
        {
            //right click == flag
            if (Input.GetMouseButtonDown(1))
            {
                FlagCell();
            }

            //left click == reveal
            else if (Input.GetMouseButtonDown(0))
            {
                RevealCell();
            }
        } 
    }

    //creates data for new game
    private void NewGame()
    {
        gameUI.time = 0;
        gameUI.timerActive = true;
        gameUI.winLoseText.text = "";
        wonGame = false;
        
        state = new Cell[width, height]; 
        gameOver = false;

        GenerateCells();
        GenerateMines();
        GenerateNumbers();

        Camera.main.transform.position = new Vector3(width / 2f, height / 2f, -10f);
        
        board.Draw(state);
    }

    private void GenerateCells()
    {
        for(int x  = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell();
                cell.pos = new Vector3Int(x,y,0);
                cell.type = Cell.Type.Empty;

                state[x,y] = cell;
            }
        }
    }

    private void GenerateMines()
    {
        for(int i  = 0; i < mineCount; i++)
        {
            int x = Random.Range(0, width); 
            int y = Random.Range(0, height);

            while (state[x,y].type == Cell.Type.Mine)
            {
                x++;

                if (x >= width)
                {
                    x = 0;
                    y++;

                    if (y >= height)
                    {
                        y = 0;
                    }
                }
            }

            state[x, y].type = Cell.Type.Mine;
        }
    }

    private void GenerateNumbers()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x,y];

                if(cell.type == Cell.Type.Mine)
                {
                    continue;
                }

                cell.number = CountMines(x,y);

                if(cell.number > 0)
                {
                    cell.type = Cell.Type.Number;
                }

                state[x,y] = cell;
            }
        }
    }

    private int CountMines(int cellX, int cellY)
    {
        int count = 0;

        //checks all surrounding cells for mines
        for(int adjacentX = -1; adjacentX <= 1; adjacentX++)
        {
            for(int adjacentY = -1; adjacentY <= 1; adjacentY++)
            {
                if(adjacentX == 0 && adjacentY == 0)
                {
                    continue;
                }
                
                int x = cellX + adjacentX;
                int y = cellY + adjacentY;

                if (GetCell(x,y).type == Cell.Type.Mine)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private bool IsValidCell(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    //cell flagging
    private void FlagCell()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = board.tilemap.WorldToCell(worldPos);

        Cell cell = GetCell(cellPos.x, cellPos.y);

        if(cell.type == Cell.Type.Invalid || cell.revealed || (!flagsLeft && cell.flagged == false))
        {
            return;
        }

        if(cell.flagged == false)
        {
            cell.flagged = true;
            flagCount--;

            if (flagCount == 0)
            {
                flagsLeft = false;
            }
        }

        else if((cell.flagged == false && cell.flagged) || cell.flagged == true)
        {
            cell.flagged = false;
            flagCount++;

            if(flagsLeft == false)
            {
                flagsLeft = true;
            }
        }

        state[cellPos.x, cellPos.y] = cell;
        board.Draw(state);
    }

    //revealing clicked cell
    private void RevealCell()
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPos = board.tilemap.WorldToCell(worldPos);

        Cell cell = GetCell(cellPos.x, cellPos.y);

        if (cell.type == Cell.Type.Invalid || cell.revealed || cell.flagged)
        {
            return;
        }

        switch(cell.type)
        {
            case Cell.Type.Mine:
                Explode(cell);
                break;
            case Cell.Type.Empty:
                FloodBoard(cell);
                CheckWinCondition();
                break;

            default:
                cell.revealed = true;
                state[cellPos.x, cellPos.y] = cell;
                CheckWinCondition();
                break;
        }

        board.Draw(state);
    }

    //empty cell flooding
    //reveals adjacent empty cells
    private void FloodBoard(Cell cell)
    {
        if(cell.revealed) return;
        if(cell.type == Cell.Type.Mine || cell.type == Cell.Type.Invalid) return;

        cell.revealed = true;
        state[cell.pos.x, cell.pos.y] = cell;
        
        //recursively calls function if the cell revealed was empty, checks surrounding cells
        if(cell.type == Cell.Type.Empty)
        {
            FloodBoard(GetCell(cell.pos.x - 1, cell.pos.y));
            FloodBoard(GetCell(cell.pos.x + 1, cell.pos.y));
            FloodBoard(GetCell(cell.pos.x, cell.pos.y - 1));
            FloodBoard(GetCell(cell.pos.x, cell.pos.y + 1));

            //floods corner tiles as well
            FloodBoard(GetCell(cell.pos.x - 1, cell.pos.y - 1));
            FloodBoard(GetCell(cell.pos.x + 1, cell.pos.y - 1));
            FloodBoard(GetCell(cell.pos.x - 1, cell.pos.y + 1));
            FloodBoard(GetCell(cell.pos.x + 1, cell.pos.y + 1));
        }
    }
    
    private void Explode(Cell cell)
    {
        gameOver = true;
        Debug.Log("game over!");

        cell.revealed = true;
        cell.exploded = true;

        state[cell.pos.x, cell.pos.y] = cell;

        for(int x =  0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                cell = state[x, y];

                if(cell.type == Cell.Type.Mine)
                {
                    cell.revealed = true;
                    state[x,y] = cell;
                }
            }
        }
    }
    
    private void CheckWinCondition()
    {
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if(cell.type != Cell.Type.Mine && !cell.revealed)
                {
                    return;
                }
            }
        }

        //all non mine cells revealed
        Debug.Log("you win!");
        gameOver = true;
        wonGame = true;

        //flags mine locations if not done so already
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];

                if (cell.type == Cell.Type.Mine)
                {
                    cell.flagged = true;
                    state[x, y] = cell;
                }
            }
        }
    }
    
    private Cell GetCell(int x, int y)
    {
        if(IsValidCell(x,y))
        {
            return state[x,y];
        }

        else
        {
            return new Cell();
        }
    }
}
