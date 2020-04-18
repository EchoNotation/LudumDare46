using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum Direction
{
    UP,
    LEFT,
    DOWN,
    RIGHT,
    UP_LEFT,
    UP_RIGHT,
    DOWN_LEFT,
    DOWN_RIGHT,
    NONE
}

public class BattleManager : MonoBehaviour
{
    GameObject[] controllableUnits, assassins, civilians;
    public Tilemap tiles;
    int[][] board;
    int empty = -1;
    int passable = 0;
    int wall = 1;
    int king = 2;
    int knight = 3;
    int jester = 4;
    int noble = 5;
    int civilian = 6;
    int assassin = 7;
    int gridSize = 10;

    public Text cavalryText;
    public int turnsToSurvive;

    // Start is called before the first frame update
    void Awake()
    {
        if (gridSize % 2 != 0)
        {
            Debug.LogError("Grid size is not divisible by 2!");
            return;
        }

        board = new int[gridSize][];
        readTiles();
        controllableUnits = GameObject.FindGameObjectsWithTag("Unit");
        assassins = GameObject.FindGameObjectsWithTag("Assassin");
        civilians = GameObject.FindGameObjectsWithTag("Civillian");
        readUnits();
        //printBoard();
        cavalryText.text = "Cavalry arrive in " + turnsToSurvive + " turns!";

        for(int i = 0; i < controllableUnits.Length; i++)
        {
            controllableUnits[i].GetComponent<ControllableUnit>().unitID = i;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void predictAssassinMove()
    {

    }

    public void moveToPosition(GameObject moving, Vector2 newPos)
    {
        //Debug.Log("moveToPosition");
        //TODO have battle UI deal with it
        if (moving.GetComponent<ControllableUnit>().hasMoved) return;

        //update board (old and new spaces)
        int oldX = moving.GetComponent<ControllableUnit>().gridX;
        int oldY = moving.GetComponent<ControllableUnit>().gridY;

        //Debug.Log("oldX " + oldX + " oldY " + oldY);

        board[oldX][oldY] = passable;

        int controllableType = 0;
        switch(moving.GetComponent<ControllableUnit>().getUnitType())
        {
            case ControllableUnit.UnitType.KNIGHT:
                controllableType = knight;
                break;
            case ControllableUnit.UnitType.NOBLE:
                controllableType = noble;
                break;
            case ControllableUnit.UnitType.JESTER:
                controllableType = jester;
                break;
        }

        //if you have bug, look here
        int newX = (int)(newPos.x) + gridSize / 2;
        int newY = (int)(newPos.y) + gridSize / 2;
        board[newX][newY] = controllableType;
        moving.GetComponent<ControllableUnit>().gridX = newX;
        moving.GetComponent<ControllableUnit>().gridY = newY;

        //move prefab to correct location
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, 0);
        moving.transform.position = tiles.CellToWorld(new Vector3Int((int)newPos.x, (int)newPos.y, 0)) + offset;
    }

    public void shove(GameObject shoved, Direction dir)
    {
        int startX = 0;
        int startY = 0;

        if(shoved.CompareTag("Unit"))
        {
            startX = shoved.GetComponent<ControllableUnit>().gridX;
            startY = shoved.GetComponent<ControllableUnit>().gridY;
        }
        else if(shoved.CompareTag("Civilian"))
        {

        }

        int shovedType = board[startX][startY];

        int endX = startX;
        int endY = startY;

        switch(dir)
        {
            case Direction.UP:
                endY++;
                break;
            case Direction.LEFT:
                endX--;
                break;
            case Direction.DOWN:
                endY--;
                break;
            case Direction.RIGHT:
                endX++;
                break;
            case Direction.UP_LEFT:
                endX--;
                endY++;
                break;
            case Direction.UP_RIGHT:
                endX++;
                endY++;
                break;
            case Direction.DOWN_LEFT:
                endX--;
                endY--;
                break;
            case Direction.DOWN_RIGHT:
                endX++;
                endY--;
                break;
            default:
                Debug.Log("Invalid direction in shove! Dir: " + dir.ToString());
                break;
        }

        board[startX][startY] = passable;
        board[endX][endY] = shovedType;

        if (shoved.CompareTag("Unit"))
        {
            shoved.GetComponent<ControllableUnit>().gridX = endX;
            shoved.GetComponent<ControllableUnit>().gridY = endY;
        }
        else if (shoved.CompareTag("Civilian"))
        {

        }

        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, 0);
        shoved.transform.position = tiles.CellToWorld(new Vector3Int((int)endX - (gridSize / 2), (int)endY - (gridSize / 2), 0)) + offset;
    }

    public void tossCoin(Vector2Int pos)
    {

    }

    public void commit()
    {
        //Finalizes everything, lets Assassins take their turn.
        civillianTurn();
        assassinTurn();
        turnsToSurvive--;

        if(turnsToSurvive <= 0)
        {
            //Victory!
        }
        else
        {
            if (turnsToSurvive == 1)
            {
                cavalryText.text = "Cavalry arrive in 1 turn!";
            }
            else
            {
                cavalryText.text = "Cavalry arrive in " + turnsToSurvive + " turns!";
            }
        }
    }

    public void rewind()
    {
        
    }

    void civillianTurn()
    {

    }
    
    void assassinTurn()
    {

    }

    void readTiles()
    {
        //Grid coordinate refers to bottom-left corner of tile Ex// Tile with bottom left corner at 0,0,0 is at Vector3Int(0,0,0)
        for(int i = -(gridSize / 2); i < gridSize -(gridSize / 2); i++)
        {
            board[gridSize - (i + (gridSize / 2)) - 1] = new int[gridSize];
            for(int j = -(gridSize / 2); j < gridSize -(gridSize / 2); j++)
            {
                TileBase currentTile = tiles.GetTile(new Vector3Int(j, i, 0));
                int current = -1;

                switch(currentTile.name)
                {
                    case "Wall":
                        current = wall;
                        break;
                    case "Floor":
                        current = passable;
                        break;
                    default:
                        Debug.Log("Invalid tile name! TileName: " + currentTile.name);
                        break;
                }

                board[gridSize - (i + (gridSize / 2)) - 1][j + (gridSize / 2)] = current;
            }
        }
    }

    void readUnits()
    {
        for(int i = 0; i < controllableUnits.Length; i++)
        {
            Vector3 pos = controllableUnits[i].transform.position;
            Vector3Int tilePos = tiles.WorldToCell(pos);
            int x = tilePos.x + gridSize/2;
            int y = tilePos.y + gridSize/2;

            controllableUnits[i].GetComponent<ControllableUnit>().gridX = x;
            controllableUnits[i].GetComponent<ControllableUnit>().gridY = y;

            //Debug.Log(x + " " + y);

            switch (controllableUnits[i].GetComponent<ControllableUnit>().getUnitType())
            {
                case ControllableUnit.UnitType.KNIGHT:
                    board[x][y] = knight;
                    break;
                case ControllableUnit.UnitType.JESTER:
                    board[x][y] = jester;
                    break;
                case ControllableUnit.UnitType.NOBLE:
                    board[x][y] = noble;
                    break;
            }
        }
        for (int i = 0; i < assassins.Length; i++)
        {
            Vector3 pos = assassins[i].transform.position;
            Vector3Int tilePos = tiles.WorldToCell(pos);
            int x = tilePos.x + gridSize / 2;
            int y = tilePos.y + gridSize / 2;

            //Debug.Log(x + " " + y);

            board[x][y] = assassin;
        }
        for (int i = 0; i < civilians.Length; i++)
        {
            Vector3 pos = civilians[i].transform.position;
            Vector3Int tilePos = tiles.WorldToCell(pos);
            int x = tilePos.x + gridSize / 2;
            int y = tilePos.y + gridSize / 2;

            //Debug.Log(x + " " + y);

            board[x][y] = civilian;
        }

        /*Vector3 kingPos = GameObject.FindGameObjectWithTag("King").transform.position;
        Vector3Int kingTile = tiles.WorldToCell(kingPos);
        int kingX = kingTile.x + (gridSize / 2);
        int kingY = kingTile.y + (gridSize / 2);

        board[kingX][kingY] = king;*/
    }

    void printBoard()
    {
        for(int i = gridSize - 1; i > 0; i--)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for(int j = 0; j < gridSize; j++)
            {
                sb.Append("" + board[i][j] + " ");
            }
            Debug.Log(sb.ToString());
        }
    }

    public int[][] getBoard()
    {
        return board;
    }
}

