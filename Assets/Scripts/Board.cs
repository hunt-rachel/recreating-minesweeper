using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap {  get; private set; }

    //non-number tiles
    public Tile tileEmpty;
    public Tile tileExploded;
    public Tile tileFlag;
    public Tile tileMine;
    public Tile tileUnclicked;

    //number tiles
    public Tile tile1;
    public Tile tile2;
    public Tile tile3;
    public Tile tile4;
    public Tile tile5;
    public Tile tile6;
    public Tile tile7;
    public Tile tile8;

    private void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void Draw(Cell[,] state)
    {
        int width = state.GetLength(0);
        int height = state.GetLength(1);

        for(int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = state[x, y];
                tilemap.SetTile(cell.pos, GetTile(cell));
            }
        }
    }

    private Tile GetTile(Cell cell)
    {
        if(cell.revealed)
        {
            return GetRevealedTile(cell);
        }

        else if(cell.flagged)
        {
            return tileFlag;
        }

        else
        {
            return tileUnclicked;
        }
    }

    private Tile GetRevealedTile(Cell cell)
    {
        switch(cell.type) 
        {
            case Cell.Type.Empty:
                return tileEmpty;

            case Cell.Type.Mine:
                return cell.exploded ? tileExploded : tileMine;

            case Cell.Type.Number: 
                return GetNumberTile(cell);

            default:
                return null;
        }
    }

    private Tile GetNumberTile(Cell cell)
    {
        switch(cell.number)
        {
            case 1: return tile1;
            case 2: return tile2;
            case 3: return tile3;
            case 4: return tile4;
            case 5: return tile5;
            case 6: return tile6;
            case 7: return tile7;
            case 8: return tile8;

            default: return null; 
        } 
    }  
}
