using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BattleManager : MonoBehaviour
{
    GameObject[] controllableUnits, assassins, civillians;
    public Tilemap tiles;
    int[][] board;
    int empty = -1;
    int passable = 0;
    int wall = 1;
    int king = 2;
    int knight = 3;
    int jester = 4;
    int noble = 5;
    int civillian = 6;
    int assassin = 7;
    int gridSize = 10;

    // Start is called before the first frame update
    void Start()
    {
        board = new int[gridSize][];
        readTiles();
        printBoard();
        controllableUnits = GameObject.FindGameObjectsWithTag("Unit");
        assassins = GameObject.FindGameObjectsWithTag("Assassin");
        civillians = GameObject.FindGameObjectsWithTag("Civillian");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void predictAssassinMove()
    {

    }

    void commit()
    {
        //Finalizes everything, lets Assassins take their turn.
        civillianTurn();
        assassinTurn();
    }

    void civillianTurn()
    {

    }
    
    void assassinTurn()
    {

    }

    void readTiles()
    {
        if (gridSize % 2 != 0)
        {
            Debug.Log("Grid size is not divisible by 2!");
            return;
        }
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

    void printBoard()
    {
        for(int i = 0; i < gridSize; i++)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for(int j = 0; j < gridSize; j++)
            {
                sb.Append("" + board[i][j] + " ");
            }
            Debug.Log(sb.ToString());
        }
    }
}
