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
    GameObject kingObj;
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

    Vector2Int[] debugPath;

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
        printBoardIndicies();
        cavalryText.text = "Cavalry arrive in " + turnsToSurvive + " turns!";

        for(int i = 0; i < controllableUnits.Length; i++)
        {
            controllableUnits[i].GetComponent<ControllableUnit>().unitID = i;
        }

        debugPath = findPathTo(new Vector2Int(3, 3), new Vector2Int(-3, 2));
        if (debugPath == null) Debug.Log("No path found");
        else
            for (int i = 0; i < debugPath.Length; i++)
                Debug.Log("Step "+ i + ": " + debugPath[i].x + ", " + debugPath[i].y);
    }

    private void OnDrawGizmos()
    {
        if (debugPath == null) return;

        float offset = tiles.cellSize.x / 2;
        Vector3 offset3D = new Vector3(offset, offset, 0);

        Gizmos.DrawWireSphere(new Vector3(debugPath[0].x, debugPath[0].y, 0) + offset3D, 0.5f);
        Gizmos.DrawWireSphere(new Vector3(debugPath[debugPath.Length-1].x, debugPath[debugPath.Length-1].y, 0) + offset3D, 0.5f);

        for(int i = 0; i < debugPath.Length - 1; i++)
        {
            Vector3 lineStart = new Vector3(debugPath[i].x, debugPath[i].y, 0) + offset3D;
            Vector3 lineEnd = new Vector3(debugPath[i + 1].x, debugPath[i + 1].y, 0) + offset3D;
            Gizmos.DrawLine(lineStart, lineEnd);
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
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
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

        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        shoved.transform.position = tiles.CellToWorld(new Vector3Int((int)endX - (gridSize / 2), (int)endY - (gridSize / 2), 0)) + offset;
    }

    public void tossCoin(Vector2Int pos)
    {

    }

    
    //PATHFINDING
    class Node
    {
        public int x;
        public int y;

        public int f = 0;
        public int g = 0;
        public int h = 0;

        public Node parent = null;

        public Node(Node parent, int x, int y)
        {
            this.x = x;
            this.y = y;

            this.parent = parent;
        }
    }

    public Vector2Int[] findPathTo(Vector2Int src, Vector2Int dest)
    {
        Node start = new Node(null, src.x, src.y);
        Node end = new Node(null, dest.x, dest.y);

        int boardSquareID = board[Mathf.Abs(src.y - 4)][src.x + gridSize/2];
        if(boardSquareID != passable)
        {
            Debug.Log("Can't Pathfind: start is not passable");
            return null;
        }

        boardSquareID = board[Mathf.Abs(dest.y - 4)][dest.x + gridSize/2];
        if(boardSquareID != passable)
        {
            Debug.Log("Can't Pathfind: end is not passable");
            return null;
        }


        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        open.Add(start);

        //loop until reach the end
        while(open.Count > 0)
        {
            Node current_node = open[0];
            int currentIdx = 0;

            for(int i = 0; i < open.Count; i++)
            {
                if(open[i].f < current_node.f)
                {
                    current_node = open[i];
                    currentIdx = i;
                }
            }

            open.RemoveAt(currentIdx);
            closed.Add(current_node);

            if(current_node.x == dest.x && current_node.y == dest.y)
            {
                //generate path going backwards
                List<Vector2Int> path = new List<Vector2Int>();

                Node current = current_node;
                while(current.parent != null)
                {
                    path.Add(new Vector2Int(current.x, current.y));
                    current = current.parent;
                }
                path.Add(src);

                path.Reverse();
                return path.ToArray();
            }

            //generate children for all 8 directions
            List<Node> children = new List<Node>();

            Node upLeft = createChildIfValid(current_node, -1, 1);
            if (upLeft != null) children.Add(upLeft);
            
            Node upMid = createChildIfValid(current_node, 0, 1);
            if (upMid != null) children.Add(upMid);

            Node upRight = createChildIfValid(current_node, 1, 1);
            if (upRight != null) children.Add(upRight);
            
            Node left = createChildIfValid(current_node, -1, 0);
            if (left != null) children.Add(left);

            Node right = createChildIfValid(current_node, 1, 0);
            if (right != null) children.Add(right);

            Node downLeft = createChildIfValid(current_node, -1, -1);
            if (downLeft != null) children.Add(downLeft);

            Node downMid = createChildIfValid(current_node, 0, -1);
            if (downMid != null) children.Add(downMid);

            Node downRight = createChildIfValid(current_node, 1, -1);
            if (downRight != null) children.Add(downRight);

            //loop through children
            for(int i = 0; i < children.Count; i++)
            {
                //don't use if child is already closed
                for(int j = 0; j < closed.Count; j++)
                {
                    if(closed[j].x == children[i].x && closed[j].y == children[i].y)
                    {
                        continue;
                    }
                }

                //update values
                children[i].g = current_node.g + 1;
                children[i].h = Mathf.CeilToInt(Vector2Int.Distance(new Vector2Int(children[i].x, children[i].y), dest));
                children[i].f = children[i].g + children[i].h;

                //add child to open list if not on the list already with more efficient path
                for(int j = 0; j < open.Count; j++)
                {
                    if (children[i].x == open[j].x && children[i].y == open[j].y && children[i].g > open[j].g) continue;
                }

                open.Add(children[i]);
            }
        }

        //no path found
        return null;
    }

    private Node createChildIfValid(Node parent, int offsetx, int offsety)
    {
        Node child = new Node(parent, parent.x + offsetx, parent.y + offsety);

        //check bounds
        if(child.x < -5 || child.x > 4 || child.y > 4 || child.y < -5)
        {
            return null;
        }

        //check valid square to move to
        //TODO this is NOT CORRECT
        int boardSquareID = board[Mathf.Abs(child.y - 4)][child.x + gridSize/2];
        if(boardSquareID != passable)
        {
            return null;
        }

        //safety checks passed
        return child;
    }



    public void commit()
    {
        //Finalizes everything, lets Assassins take their turn.
        civilianTurn();
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

    void civilianTurn()
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
            assassins[i].GetComponent<Assassin>().gridX = x;
            assassins[i].GetComponent<Assassin>().gridY = y;
            assassins[i].GetComponent<Assassin>().snapToGrid();

            board[x][y] = assassin;
        }
        for (int i = 0; i < civilians.Length; i++)
        {
            Vector3 pos = civilians[i].transform.position;
            Vector3Int tilePos = tiles.WorldToCell(pos);
            int x = tilePos.x + gridSize / 2;
            int y = tilePos.y + gridSize / 2;

            //Debug.Log(x + " " + y);
            civilians[i].GetComponent<Civilian>().gridX = x;
            civilians[i].GetComponent<Civilian>().gridY = y;
            civilians[i].GetComponent<Civilian>().snapToGrid();

            board[x][y] = civilian;
        }

        kingObj = GameObject.Find("King");
        Vector3 kingPos = kingObj.transform.position;
        Vector3Int kingTile = tiles.WorldToCell(kingPos);
        int kingX = kingTile.x + (gridSize / 2);
        int kingY = kingTile.y + (gridSize / 2);

        kingObj.GetComponent<King>().gridX = kingX;
        kingObj.GetComponent<King>().gridY = kingX;
        kingObj.GetComponent<King>().snapToGrid();

        board[kingX][kingY] = king;
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
    
    void printBoardIndicies()
    {
        string fullRes = " 0123456789\n";
        Debug.Log("Length: " + board.Length);
        for(int i = 0; i < board.Length; i++)
        {
            string res = i + " ";
            for(int j = 0; j < board[i].Length; j++)
            {
                res += board[i][j];
            }
            fullRes += res + "\n";
        }

        Debug.Log(fullRes);
    }

    public int[][] getBoard()
    {
        return board;
    }
}

