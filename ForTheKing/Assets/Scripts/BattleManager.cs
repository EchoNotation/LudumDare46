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
    Vector3[][] unitPositions, assassinPositions, civilianPositions;
    int[][][] savedBoards;
    Vector2Int[][] unitGridSpaces, assassinGridSpaces, civilianGridSpaces;
    int maxNumberOfTurns = 30;
    int turnNumber = 1;
    int empty = -1;
    public int passable = 0;
    int wall = 1;
    int king = 2;
    int knight = 3;
    int jester = 4;
    int noble = 5;
    int civilian = 6;
    int assassin = 7;
    public int gridSize = 10;

    public GameObject assassinMarker;

    public Text cavalryText;
    public int turnsToSurvive;

    //Vector2Int[] debugPath;

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
        civilians = GameObject.FindGameObjectsWithTag("Civilian");
        unitPositions = new Vector3[maxNumberOfTurns][];
        assassinPositions = new Vector3[maxNumberOfTurns][];
        civilianPositions = new Vector3[maxNumberOfTurns][];
        unitGridSpaces = new Vector2Int[maxNumberOfTurns][];
        assassinGridSpaces = new Vector2Int[maxNumberOfTurns][];
        civilianGridSpaces = new Vector2Int[maxNumberOfTurns][];
        savedBoards = new int[maxNumberOfTurns][][];

        for(int i = 0; i < maxNumberOfTurns; i++)
        {
            unitPositions[i] = new Vector3[controllableUnits.Length];
            assassinPositions[i] = new Vector3[assassins.Length];
            civilianPositions[i] = new Vector3[civilians.Length];
            unitGridSpaces[i] = new Vector2Int[controllableUnits.Length];
            assassinGridSpaces[i] = new Vector2Int[assassins.Length];
            civilianGridSpaces[i] = new Vector2Int[civilians.Length];

            savedBoards[i] = new int[gridSize][];

            for(int j = 0; j < gridSize; j++)
            {
                savedBoards[i][j] = new int[gridSize];
            }
        }

        readUnits();
        //printBoardIndicies();
        cavalryText.text = "Cavalry arrive in " + turnsToSurvive + " turns!";

        for(int i = 0; i < controllableUnits.Length; i++)
        {
            controllableUnits[i].GetComponent<ControllableUnit>().unitID = i;
        }

        /*debugPath = findPathTo(new Vector2Int(-1, -1), new Vector2Int(0, 0));
        if (debugPath == null) Debug.Log("No path found");
        else
            for (int i = 0; i < debugPath.Length; i++)
                Debug.Log("Step "+ i + ": " + debugPath[i].x + ", " + debugPath[i].y);*/
    }

    private void Start()
    {
        predictAssassinMove();
    }

    private void OnDrawGizmos()
    {
        /*
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
        */
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(turnNumber);
        if (Input.GetKeyDown(KeyCode.P)) printBoardIndicies();
    }

    void predictAssassinMove()
    {
        Vector2Int kingPos = new Vector2Int(FindObjectOfType<King>().gridX - gridSize/2, FindObjectOfType<King>().gridY - gridSize/2 + 1);
        for (int i = 0; i < assassins.Length; i++)
        {
            Assassin script = assassins[i].GetComponent<Assassin>();
            Vector2Int pos = new Vector2Int(script.gridX - gridSize/2, script.gridY - gridSize/2);
            Vector2Int[] fullPath = findPathTo(pos, kingPos);

            if (fullPath != null)
            {
                script.desiredPath = fullPath;

                //get first script.speed elements of the path to the king
                Vector2Int[] currentPath = new Vector2Int[script.speed + 1];
                for(int j = 0; j < script.speed + 1 && j < fullPath.Length; j++)
                {
                    currentPath[j] = fullPath[j];
                }
                
                for (int j = 1; j < currentPath.Length; j++)
                {
                    FindObjectOfType<BattleUI>().createMarkerAtTile(currentPath[j], assassinMarker);
                }
            }
            else Debug.LogWarning("TODO implement when assassin has no path to king");

        }
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

        board[(gridSize - 1) - oldY][oldX] = passable;

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
        board[(gridSize - 1) - newY][newX] = controllableType;
        moving.GetComponent<ControllableUnit>().gridX = newX;
        moving.GetComponent<ControllableUnit>().gridY = newY;

        moving.GetComponent<ControllableUnit>().updateMoveablePositions();

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
            startX = shoved.GetComponent<Civilian>().gridX;
            startY = shoved.GetComponent<Civilian>().gridY;
        }

        int shovedType = board[(gridSize - 1) - startY][startX];

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

        board[(gridSize - 1) - startY][startX] = passable;
        board[(gridSize - 1) - endY][endX] = shovedType;

        if (shoved.CompareTag("Unit"))
        {
            shoved.GetComponent<ControllableUnit>().gridX = endX;
            shoved.GetComponent<ControllableUnit>().gridY = endY;
        }
        else if (shoved.CompareTag("Civilian"))
        {
            shoved.GetComponent<Civilian>().gridX = endX;
            shoved.GetComponent<Civilian>().gridY = endY;
        }

        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        shoved.transform.position = tiles.CellToWorld(new Vector3Int((int)endX - (gridSize / 2), (int)endY - (gridSize / 2), 0)) + offset;
    }

    public void tossCoin(Vector2Int pos)
    {

    }

    public void taunt()
    {

    }

    public void block()
    {

    }
    
    public RaycastHit2D lineOfSight(GameObject start, GameObject dest)
    {
        Vector2 origin = new Vector2(start.transform.position.x, start.transform.position.y);
        Vector2 destination = new Vector2(dest.transform.position.x, dest.transform.position.y);
        return Physics2D.Raycast(origin, destination - origin);
    }

    //PATHFINDING
    class Node
    {
        public int x;
        public int y;

        public double f = 0.0;
        public double g = 0.0;
        public double h = 0.0;

        public Node parent = null;

        public Node(Node parent, int x, int y)
        {
            this.x = x;
            this.y = y;

            this.parent = parent;
        }
    }

    /*
     * Calculates distance between two tile points on map
     * Returns -1 if no path possible
     */
    public int pathDistance(Vector2Int src, Vector2Int dest)
    {
        Vector2Int[] path = findPathTo(src, dest);
        if (path != null) return path.Length;
        else return -1;
    }

    public Vector2Int[] findPathTo(Vector2Int src, Vector2Int dest)
    {
        Node start = new Node(null, src.x, src.y);
        Node end = new Node(null, dest.x, dest.y);


        if(dest.x < -5 || dest.x > 4 || dest.y > 4 || dest.y < -5)
        {
            return null;
        }

        //int boardSquareID = board[Mathf.Abs(src.y - 4)][src.x + gridSize/2];
        /*
        if(boardSquareID != passable)
        {
            Debug.Log("Can't Pathfind: start is not passable");
            return null;
        }
        */

        //check bounds of destination

        int boardSquareID = board[Mathf.Abs(dest.y - 4)][dest.x + gridSize/2];
        if(boardSquareID != passable)
        {
            //Debug.Log("Can't Pathfind: end is not passable");
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
            if (upLeft != null)
            {
                children.Add(upLeft);
                upLeft.g += 1.5;
            }
            
            Node upMid = createChildIfValid(current_node, 0, 1);
            if (upMid != null) children.Add(upMid);

            Node upRight = createChildIfValid(current_node, 1, 1);
            if (upRight != null)
            {
                upRight.g += 1.5;
                children.Add(upRight);
            }
            
            Node left = createChildIfValid(current_node, -1, 0);
            if (left != null) children.Add(left);

            Node right = createChildIfValid(current_node, 1, 0);
            if (right != null) children.Add(right);

            Node downLeft = createChildIfValid(current_node, -1, -1);
            if (downLeft != null)
            {
                downLeft.g += 1.5;
                children.Add(downLeft);
            }

            Node downMid = createChildIfValid(current_node, 0, -1);
            if (downMid != null) children.Add(downMid);

            Node downRight = createChildIfValid(current_node, 1, -1);
            if (downRight != null)
            {
                downRight.g += 1.5;
                children.Add(downRight);
            }

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

        turnNumber++;
        saveBoard(turnNumber);
    }

    public void rewind()
    {
        if (turnNumber <= 1) return;
        turnNumber--;
        loadBoard(turnNumber);
        turnsToSurvive++;
        cavalryText.text = "Cavalry arrive in " + turnsToSurvive + " turns!";
    }

    public void restartTurn()
    {
        resetActions();
        loadBoard(turnNumber);
    }


    public void saveBoard(int id)
    {
        if (id < 0 || id >= 30) return;

        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);

        for (int i = 0; i < controllableUnits.Length; i++)
        {
            
            int tempX = controllableUnits[i].GetComponent<ControllableUnit>().gridX;
            int tempY = controllableUnits[i].GetComponent<ControllableUnit>().gridY;
            unitPositions[id][i] = tiles.CellToWorld(new Vector3Int(tempX - gridSize / 2, tempY - gridSize / 2, 0)) + offset;
            unitGridSpaces[id][i] = new Vector2Int(tempX, tempY);
        }

        for (int i = 0; i < assassins.Length; i++)
        {
            int tempX = assassins[i].GetComponent<Assassin>().gridX;
            int tempY = assassins[i].GetComponent<Assassin>().gridY;
            assassinPositions[id][i] = tiles.CellToWorld(new Vector3Int(tempX - gridSize / 2, tempY - gridSize / 2, 0)) + offset;
            assassinGridSpaces[id][i] = new Vector2Int(tempX, tempY);
        }

        for(int i = 0; i < civilians.Length; i++)
        {
            int tempX = civilians[i].GetComponent<Civilian>().gridX;
            int tempY = civilians[i].GetComponent<Civilian>().gridY;
            civilianPositions[id][i] = tiles.CellToWorld(new Vector3Int(tempX - gridSize / 2, tempY - gridSize / 2, 0)) + offset;
            civilianGridSpaces[id][i] = new Vector2Int(tempX, tempY);
        }

        for(int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                savedBoards[id][i][j] = board[i][j];
            }
        }
    }

    public void loadBoard(int id)
    {
        if (id < 0 || id >= 30) return;

        for(int i = 0; i < gridSize; i++)
        {
            for(int j = 0; j < gridSize; j++)
            {
                board[i][j] = savedBoards[id][i][j];
            }
        }

        for(int i = 0; i < controllableUnits.Length; i++)
        {
            controllableUnits[i].transform.position = unitPositions[id][i];
            Vector2Int temp = unitGridSpaces[id][i];
            controllableUnits[i].GetComponent<ControllableUnit>().gridX = temp.x;
            controllableUnits[i].GetComponent<ControllableUnit>().gridY = temp.y;
        }

        for(int i = 0; i < assassins.Length; i++)
        {
            assassins[i].transform.position = assassinPositions[id][i];
            Vector2Int temp2 = assassinGridSpaces[id][i];
            assassins[i].GetComponent<Assassin>().gridX = temp2.x;
            assassins[i].GetComponent<Assassin>().gridY = temp2.y;
        }

        for(int i = 0; i < civilians.Length; i++)
        {
            civilians[i].transform.position = civilianPositions[id][i];
            Vector2Int temp3 = civilianGridSpaces[id][i];
            civilians[i].GetComponent<Civilian>().gridX = temp3.x;
            civilians[i].GetComponent<Civilian>().gridY = temp3.y;
        }

        
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
                    board[(gridSize - 1) - y][x] = knight;
                    break;
                case ControllableUnit.UnitType.JESTER:
                    board[(gridSize - 1) - y][x] = jester;
                    break;
                case ControllableUnit.UnitType.NOBLE:
                    board[(gridSize - 1) - y][x] = noble;
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

            board[(gridSize - 1) - y][x] = assassin;
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

            board[(gridSize - 1) - y][x] = civilian;
        }

        kingObj = GameObject.Find("King");
        Vector3 kingPos = kingObj.transform.position;
        Vector3Int kingTile = tiles.WorldToCell(kingPos);
        int kingX = kingTile.x + (gridSize / 2);
        int kingY = kingTile.y + (gridSize / 2);

        kingObj.GetComponent<King>().gridX = kingX;
        kingObj.GetComponent<King>().gridY = kingY;
        kingObj.GetComponent<King>().snapToGrid();

        //Debug.Log(kingX + " " + kingY);
        board[(gridSize - 1) - kingY][kingX] = king;

        saveBoard(turnNumber);
    }

    public void resetActions()
    {
        for(int i = 0; i < controllableUnits.Length; i++)
        {
            controllableUnits[i].GetComponent<ControllableUnit>().hasMoved = false;
            controllableUnits[i].GetComponent<ControllableUnit>().hasTakenAction = false;
        }
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
        //Debug.Log("Length: " + board.Length);
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

