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
    bool[][] unitStatuses, assassinStatuses, civilianStatuses;
    int maxNumberOfTurns = 30;
    public int turnNumber = 1;
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
    public GameObject gold;

    private Vector3 goldRestingPlace = new Vector3(-15, 0, 0);

    BattleUI battleUI;

    public Text cavalryText;
    public int turnsToSurvive;

    public bool[] goldExists;
    private Vector2Int[] goldTilePos;
    private bool[] goldWasPlacedThisTurn;

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
        unitStatuses = new bool[maxNumberOfTurns][];
        assassinStatuses = new bool[maxNumberOfTurns][];
        civilianStatuses = new bool[maxNumberOfTurns][];

        goldExists = new bool[maxNumberOfTurns];
        goldTilePos = new Vector2Int[maxNumberOfTurns];
        goldWasPlacedThisTurn = new bool[maxNumberOfTurns];

        for(int i = 0; i < maxNumberOfTurns; i++)
        {
            unitPositions[i] = new Vector3[controllableUnits.Length];
            assassinPositions[i] = new Vector3[assassins.Length];
            civilianPositions[i] = new Vector3[civilians.Length];
            unitGridSpaces[i] = new Vector2Int[controllableUnits.Length];
            assassinGridSpaces[i] = new Vector2Int[assassins.Length];
            civilianGridSpaces[i] = new Vector2Int[civilians.Length];
            unitStatuses[i] = new bool[controllableUnits.Length];
            assassinStatuses[i] = new bool[assassins.Length];
            civilianStatuses[i] = new bool[civilians.Length];

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
        battleUI = FindObjectOfType<BattleUI>();
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

        if (assassins == null) return;

        for(int i = 0; i < assassins.Length; i++)
        {
            Assassin script = assassins[i].GetComponent<Assassin>();
            if (script.endAction == Assassin.EndAction.AIM)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(assassins[i].transform.position, script.target.transform.position);
            }
            else if (script.endAction == Assassin.EndAction.AIM)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(assassins[i].transform.position, script.target.transform.position);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(turnNumber);
        if (Input.GetKeyDown(KeyCode.P)) printBoardIndicies();

        //Debug.Log("GoldX: " + goldTilePos[turnNumber].x + " GoldY: " + goldTilePos[turnNumber].y);
        //Debug.Log("TurnNumber: " + turnNumber);
    }

    //plans assassin moves
    void predictAssassinMove()
    {
        battleUI.removeMoveMarkers("AssassinMoveMarker");

        Vector2Int kingPos = new Vector2Int(FindObjectOfType<King>().gridX - gridSize/2, FindObjectOfType<King>().gridY - gridSize/2 + 1);
        for (int i = 0; i < assassins.Length; i++)
        {
            Assassin script = assassins[i].GetComponent<Assassin>();
            Vector2Int pos = new Vector2Int(script.gridX - gridSize/2, script.gridY - gridSize/2);

            if(script.taunter != null)
            {

            }
            else
            {
                //go after king or stab people on way to king
                
            }

            Vector2Int[] fullPath = findPathClosest(2, pos, kingPos);

            script.target = null;
            script.endAction = Assassin.EndAction.IDLE;

            if (fullPath != null)
            {
                script.desiredPath = fullPath;

                List<Vector2Int> currentPath = new List<Vector2Int>();
                
                //step along path for the next turn
                for(int j = 0; j < script.speed + 1 && j < fullPath.Length; j++)
                {
                    currentPath.Add(fullPath[j]);

                    //check that along the path isn't someone to stab
                    //check stab up
                    if(isStabbable(currentPath[j] + new Vector2Int(0,1)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(0, 1));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //check stab up left
                    else if(isStabbable(currentPath[j] + new Vector2Int(-1,0)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(-1, 0));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //check stab up right
                    else if(isStabbable(currentPath[j] + new Vector2Int(1,1)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(1, 1));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //check stab right
                    else if(isStabbable(currentPath[j] + new Vector2Int(1,0)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(1, 0));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //check stab left
                    else if(isStabbable(currentPath[j] + new Vector2Int(-1,0)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(-1, 0));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //check stab down left
                    else if(isStabbable(currentPath[j] + new Vector2Int(-1,-1)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(-1, -1));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //check stab down right
                    else if(isStabbable(currentPath[j] + new Vector2Int(1,-1)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(1, -1));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //check stab down
                    else if(isStabbable(currentPath[j] + new Vector2Int(0,-1)))
                    {
                        script.endAction = Assassin.EndAction.STAB;
                        script.target = gameObjectAtTile(currentPath[j] + new Vector2Int(0, -1));
                        //excange for "stab marker"
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                    }

                    //if no one to stab, keep walking towards king
                    else if(script.endAction != Assassin.EndAction.STAB && Vector2Int.Distance(currentPath[j], kingPos) <= script.range)
                    {
                        Debug.Log("not stabbing and in shooting range");
                        //if good distance, check sightline for wall
                        RaycastHit2D hit = lineOfSight(assassins[i], kingObj);
                        if(hit.collider.tag == "King")
                        {
                            //aim
                            script.endAction = Assassin.EndAction.AIM;
                            script.target = kingObj;
                        }
                        else if(hit.collider.tag == "Unit")
                        {

                            script.endAction = Assassin.EndAction.AIM;
                            script.target = hit.collider.gameObject;
                        }
                        else if(hit.collider.tag == "Civilian")
                        {

                            script.endAction = Assassin.EndAction.AIM;
                            script.target = hit.collider.gameObject;
                        }

                        //if wall, continue walking
                    }

                    Debug.Log("assassin is going to " + script.endAction.ToString());

                    if(j != 0)
                        battleUI.createMarkerAtTile(currentPath[j], assassinMarker);
                }

                script.nextTurnPath = currentPath.ToArray();
            }
            else Debug.LogWarning("TODO implement when assassin has no path to king");

        }
    }

    /*
     * pos is in tilespace
     */
    private bool isStabbable(Vector2Int pos)
    {
        int tile = getTileAtSpace(pos);
        return tile == knight || tile == king || tile == civilian || tile == jester || tile == noble;
    }

    /*
     * pos is in tilespace
     */
    public GameObject gameObjectAtTile(Vector2Int pos)
    {
        Debug.Log("looking for game object at " + pos);

        int gridX = pos.x + 5;
        int gridY = pos.y + 5;

        //check if position of the king matches
        King kingSc = kingObj.GetComponent<King>();
        if (kingSc.gridX == gridX && kingSc.gridY == gridY) return kingObj;

        //loop through units list and see if any match position
        for(int i = 0; i < controllableUnits.Length; i++)
        {
            ControllableUnit script = controllableUnits[i].GetComponent<ControllableUnit>();
            if (!script.isAlive) continue;
            //are they standing in that position 
            if(script.gridX == gridX && script.gridY == gridY)
            {
                //Debug.Log("found game object with unit ID: " + script.unitID);
                return script.gameObject;
            }
        }

        //loop through civilians list and see if any match position
        for(int i = 0; i < civilians.Length; i++)
        {
            Civilian script = civilians[i].GetComponent<Civilian>();
            if (!script.isAlive) continue;
            //are they standing in that position 
            if(script.gridX == gridX && script.gridY == gridY)
            {
                //Debug.Log("found game object civilian");
                return script.gameObject;
            }
        }

        Debug.Log("found no game objects at that tile");
        return null;
    }

    public void moveToPosition(GameObject moving, Vector2 newPos)
    {
        ControllableUnit script = moving.GetComponent<ControllableUnit>();

        //Debug.Log("moveToPosition");
        if (script.hasMoved) return;

        //update board (old and new spaces)
        int oldX = script.gridX;
        int oldY = script.gridY;

        //Debug.Log("oldX " + oldX + " oldY " + oldY);

        board[(gridSize - 1) - oldY][oldX] = passable;

        int controllableType = 0;
        switch(script.getUnitType())
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
        script.gridX = newX;
        script.gridY = newY;

        script.updateMoveablePositions();

        //move prefab to correct location
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        moving.transform.position = tiles.CellToWorld(new Vector3Int((int)newPos.x, (int)newPos.y, 0)) + offset;

        //update assassin plans
        predictAssassinMove();
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

        predictAssassinMove();
    }

    public void tossCoin(Vector2Int pos)
    {
        //Enable/instantiate Gold prefab
        //Debug.Log("SpawningGold!");
        if (goldExists[turnNumber]) return;

        goldExists[turnNumber] = true;
        goldTilePos[turnNumber] = pos;
        goldWasPlacedThisTurn[turnNumber] = true;
        moveGoldPrefab(pos);
    }
    
    public void taunt(GameObject target, GameObject taunter)
    {
        target.GetComponent<Assassin>().taunter = taunter;
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

    public RaycastHit2D lineOfSightPos(Vector2 origin, Vector2 destination)
    {
        return Physics2D.Raycast(origin, destination - origin);
    }

    //PATHFINDING
    class Node
    {
        public int x;
        public int y;

        public double f = 0.0;
        public double g = 0.0;
        public double h = 100.0;

        public Node parent = null;

        public Node(Node parent, int x, int y)
        {
            this.x = x;
            this.y = y;

            this.parent = parent;
        }

        public override bool Equals(object obj)
        {
            Node o = (Node)obj;
            return o.x == x && o.y == y;
        }
    }

    /*
     * src is in tilespace, radius represents number of tiles away from src
     */
    public Vector2Int[] passableInRadius(Vector2Int src, int radius)
    {
        //perform depth first "search" away from src

        Node start = new Node(null, src.x, src.y);

        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        open.Add(start);

        while(open.Count > 0)
        {
            Node current_node = open[0];
            open.RemoveAt(0);

            closed.Add(current_node);

            if (current_node.g > radius) continue;

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

            for(int i = 0; i < children.Count; i++)
            {
                bool skip = false;

                children[i].g = current_node.g + 1;

                //check if child in open, if so check if g can be updated
                for(int j = 0; j < open.Count; j++)
                {
                    if(children[i].Equals(open[j]))
                    {
                        if(children[i].g < open[j].g)
                        {
                            open[j].g = children[i].g;
                        }
                        skip = true;
                    }
                }

                if (skip) continue;

                //check if child is in closed
                for(int j = 0; j < closed.Count; j++)
                {
                    if(children[i].Equals(closed[j]))
                    {
                        if(children[i].g < closed[j].g)
                        {
                            closed[j].g = children[i].g;
                        }
                        skip = true;
                    }
                }

                if (skip) continue;

                //if neither of those, add to open
                open.Add(children[i]);
            }

        }

        Vector2Int[] pos = new Vector2Int[closed.Count];
        for(int i = 0; i < pos.Length; i++)
        {
            if (closed[i].g > radius) continue;
            pos[i] = new Vector2Int(closed[i].x, closed[i].y);
        }

        return pos;
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

    public Vector2Int[] findPathClosest(int range, Vector2Int src, Vector2Int dest)
    {
        Node start = new Node(null, src.x, src.y);
        Node end = new Node(null, dest.x, dest.y);

        return aStar(range, start, end);
    }

    private Vector2Int[] aStar(int range, Node start, Node end)
    {
        List<Node> open = new List<Node>();
        List<Node> closed = new List<Node>();

        open.Add(start);

        //loop until reach the end
        while(open.Count > 0)
        {
            Node current_node = open[0];
            int currentIdx = 0;

            //search for node closest to destination
            for(int i = 0; i < open.Count; i++)
            {
                if(open[i].f < current_node.f)
                {
                    current_node = open[i];
                    currentIdx = i;
                }
            }

            //remove that
            open.RemoveAt(currentIdx);
            closed.Add(current_node);

            if(current_node.x == start.x && current_node.y == end.y)
            {
                return constructPath(start, current_node);
            }

            //generate children for all 8 directions
            List<Node> children = new List<Node>();

            Node upLeft = createChildIfValid(current_node, -1, 1);
            if (upLeft != null)
            {
                children.Add(upLeft);
                upLeft.g += 0.5;
            }
            
            Node upMid = createChildIfValid(current_node, 0, 1);
            if (upMid != null) children.Add(upMid);

            Node upRight = createChildIfValid(current_node, 1, 1);
            if (upRight != null)
            {
                upRight.g += 0.5;
                children.Add(upRight);
            }
            
            Node left = createChildIfValid(current_node, -1, 0);
            if (left != null) children.Add(left);

            Node right = createChildIfValid(current_node, 1, 0);
            if (right != null) children.Add(right);

            Node downLeft = createChildIfValid(current_node, -1, -1);
            if (downLeft != null)
            {
                downLeft.g += 0.5;
                children.Add(downLeft);
            }

            Node downMid = createChildIfValid(current_node, 0, -1);
            if (downMid != null) children.Add(downMid);

            Node downRight = createChildIfValid(current_node, 1, -1);
            if (downRight != null)
            {
                downRight.g += 0.5;
                children.Add(downRight);
            }

            //loop through children
            for(int i = 0; i < children.Count; i++)
            {
                if(closed.Contains(children[i]))
                {
                    continue;
                }

                //update values
                children[i].g = current_node.g + 1;
                //children[i].h = Mathf.CeilToInt(Vector2Int.Distance(new Vector2Int(children[i].x, children[i].y), new Vector2Int(end.x,end.y)));
                children[i].h = Mathf.CeilToInt(Mathf.Abs(children[i].x - end.x) + Mathf.Abs(children[i].y - end.y));
                children[i].f = children[i].g + children[i].h;

                if(children[i].h < range && range != -1)
                {
                    return constructPath(start, children[i]);
                }

                bool wasOpen = false;

                //add child to open list if not on the list already with more efficient path
                for(int j = 0; j < open.Count; j++)
                {
                    if(children[i].Equals(open[j]))
                    {
                        if(children[i].g < open[j].g)
                        {
                            open[j].g = children[i].g;
                        }
                        wasOpen = true;
                        break;
                    }
                }

                if (wasOpen) continue;

                open.Add(children[i]);
            }
        }

        //no path found
        if (range != -1)
        {
            //find the child that got the closest
            double closestDist = 100;
            Node closest = null;

            for(int i = 0; i < closed.Count; i++)
            {
                if(closed[i].h < closestDist)
                {
                    closestDist = closed[i].h;
                    closest = closed[i];
                }
            }

            if (closest != null)
            {
                Debug.Log("couldn't get within range, closest was " + closest.h + " tiles away at " + closest.x + ", " + closest.y);
                return constructPath(start, closest);
            }
            else return null;
        }
        else return null;
    }

    private Vector2Int[] constructPath(Node start, Node end)
    {
        //generate path going backwards
        List<Vector2Int> path = new List<Vector2Int>();

        Node current = end;
        while (current.parent != null)
        {
            path.Add(new Vector2Int(current.x, current.y));
            current = current.parent;
        }
        path.Add(new Vector2Int(start.x, start.y));

        path.Reverse();
        return path.ToArray();
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

        //int boardSquareID = board[Mathf.Abs(dest.y - 4)][dest.x + gridSize/2];
        int boardSquareID = board[gridSize - 1 - (dest.y + 5)][dest.x + 5];
        if(boardSquareID != passable)
        {
            //Debug.Log("Can't Pathfind: end is not passable");
            return null;
        }

        return aStar(-1, start, end);

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
        int boardSquareID = board[gridSize - 1 - (child.y + 5)][child.x + 5];
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
        goldExists[turnNumber + 1] = goldExists[turnNumber];
        goldTilePos[turnNumber + 1] = goldTilePos[turnNumber];
        goldWasPlacedThisTurn[turnNumber + 1] = false;

        assassinTurn();
        civilianTurn();
        turnsToSurvive--;

        if(!kingObj.GetComponent<King>().isAlive)
        {

        }
        else if(turnsToSurvive <= 0)
        {
            //Victory!
            GameObject.Find("SceneManager").GetComponent<SceneController>().LoadScene("Victory");
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

        predictAssassinMove();
    }

    public void rewind()
    {
        if (turnNumber < 2)
        {
            restartTurn();
            return;
        }

        goldExists[turnNumber] = false;
        goldTilePos[turnNumber] = new Vector2Int();
        goldWasPlacedThisTurn[turnNumber] = false;
        goldExists[turnNumber - 1] = goldExists[turnNumber - 2];
        goldTilePos[turnNumber - 1] = goldTilePos[turnNumber - 2];

        if(goldExists[turnNumber - 1])
        {
            moveGoldPrefab(goldTilePos[turnNumber - 1]);
        }
        else
        {
            gold.transform.position = goldRestingPlace;
        }
        
        turnNumber--;
        loadBoard(turnNumber);
        turnsToSurvive++;
        cavalryText.text = "Cavalry arrive in " + turnsToSurvive + " turns!";

        battleUI.nextTurnButton.interactable = true;

        kingObj.GetComponent<King>().isAlive = true;
        kingObj.GetComponent<BoxCollider2D>().enabled = true;
        kingObj.GetComponent<SpriteRenderer>().enabled = true;

        for(int i = 0; i < assassins.Length; i++)
        {
            assassins[i].GetComponent<Assassin>().desiredPath = null;
            assassins[i].GetComponent<Assassin>().nextTurnPath = null;
            battleUI.removeMoveMarkers("AssassinMoveMarker");

        }

        predictAssassinMove();

    }

    public void restartTurn()
    {
        resetActions();
        loadBoard(turnNumber);

        if(goldExists[turnNumber])
        {
            if(goldWasPlacedThisTurn[turnNumber])
            {
                //Remove
                goldExists[turnNumber] = false;
                goldTilePos[turnNumber] = new Vector2Int();
                goldWasPlacedThisTurn[turnNumber] = false;
                gold.transform.position = goldRestingPlace;
            }
            else
            {
                //Create
                goldExists[turnNumber] = true;
                goldTilePos[turnNumber] = goldTilePos[turnNumber - 1];
                moveGoldPrefab(goldTilePos[turnNumber]);
            }
        }

        battleUI.nextTurnButton.interactable = true;

        kingObj.GetComponent<King>().isAlive = true;
        kingObj.GetComponent<BoxCollider2D>().enabled = true;
        kingObj.GetComponent<SpriteRenderer>().enabled = true;

        for(int i = 0; i < assassins.Length; i++)
        {
            assassins[i].GetComponent<Assassin>().desiredPath = null;
            assassins[i].GetComponent<Assassin>().nextTurnPath = null;
            battleUI.removeMoveMarkers("AssassinMoveMarker");

        }
        predictAssassinMove();

    }


    public void saveBoard(int id)
    {
        if (id < 0 || id >= 30) return;

        int gridSize = tiles.size.y;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);

        for (int i = 0; i < controllableUnits.Length; i++)
        {
            bool isAlive = controllableUnits[i].GetComponent<ControllableUnit>().isAlive;
            unitStatuses[id][i] = isAlive;

            int tempX = controllableUnits[i].GetComponent<ControllableUnit>().gridX;
            int tempY = controllableUnits[i].GetComponent<ControllableUnit>().gridY;
            unitPositions[id][i] = tiles.CellToWorld(new Vector3Int(tempX - gridSize / 2, tempY - gridSize / 2, 0)) + offset;
            unitGridSpaces[id][i] = new Vector2Int(tempX, tempY);
        }

        for (int i = 0; i < assassins.Length; i++)
        {
            bool isAlive = assassins[i].GetComponent<Assassin>().isAlive;
            assassinStatuses[id][i] = isAlive;

            int tempX = assassins[i].GetComponent<Assassin>().gridX;
            int tempY = assassins[i].GetComponent<Assassin>().gridY;
            assassinPositions[id][i] = tiles.CellToWorld(new Vector3Int(tempX - gridSize / 2, tempY - gridSize / 2, 0)) + offset;
            assassinGridSpaces[id][i] = new Vector2Int(tempX, tempY);
        }

        for(int i = 0; i < civilians.Length; i++)
        {
            bool isAlive = civilians[i].GetComponent<Civilian>().isAlive;
            civilianStatuses[id][i] = isAlive;

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
            controllableUnits[i].GetComponent<ControllableUnit>().isAlive = unitStatuses[id][i];
            controllableUnits[i].GetComponent<SpriteRenderer>().enabled = unitStatuses[id][i];
            controllableUnits[i].GetComponent<BoxCollider2D>().enabled = unitStatuses[id][i];
            controllableUnits[i].GetComponent<ControllableUnit>().gridX = temp.x;
            controllableUnits[i].GetComponent<ControllableUnit>().gridY = temp.y;
        }

        for(int i = 0; i < assassins.Length; i++)
        {
            assassins[i].transform.position = assassinPositions[id][i];
            Vector2Int temp2 = assassinGridSpaces[id][i];
            assassins[i].GetComponent<Assassin>().isAlive = assassinStatuses[id][i];
            assassins[i].GetComponent<SpriteRenderer>().enabled = assassinStatuses[id][i];
            assassins[i].GetComponent<BoxCollider2D>().enabled = assassinStatuses[id][i];
            assassins[i].GetComponent<Assassin>().gridX = temp2.x;
            assassins[i].GetComponent<Assassin>().gridY = temp2.y;
        }

        for(int i = 0; i < civilians.Length; i++)
        {
            civilians[i].transform.position = civilianPositions[id][i];
            Vector2Int temp3 = civilianGridSpaces[id][i];
            civilians[i].GetComponent<Civilian>().isAlive = civilianStatuses[id][i];
            civilians[i].GetComponent<SpriteRenderer>().enabled = civilianStatuses[id][i];
            civilians[i].GetComponent<BoxCollider2D>().enabled = civilianStatuses[id][i];
            civilians[i].GetComponent<Civilian>().gridX = temp3.x;
            civilians[i].GetComponent<Civilian>().gridY = temp3.y;
        }

        
    }

    void moveGoldPrefab(Vector2Int newLocation)
    {
        newLocation.x += 5;
        newLocation.y += 5;
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -0.5f);
        Vector3 goldWorldPos = tiles.CellToWorld(new Vector3Int(newLocation.x - gridSize / 2, newLocation.y - gridSize / 2, 0)) + offset;
        gold.transform.position = goldWorldPos;
    }

    void civilianTurn()
    {
        for(int i = 0; i < civilians.Length; i++)
        {
            if (!civilians[i].GetComponent<Civilian>().isAlive) continue;

            if(goldExists[turnNumber])
            {
                //Path to gold-- Doesn't matter if you can't see it.
                int origX = civilians[i].GetComponent<Civilian>().gridX;
                int origY = civilians[i].GetComponent<Civilian>().gridY;

                //Debug.Log("CivPos: " + origX + " " + origY);

                Vector2Int[] pathToGold = findPathClosest(1, new Vector2Int(origX - 5, origY - 5), new Vector2Int(goldTilePos[turnNumber].x, goldTilePos[turnNumber].y));

                //Debug.Log("PathLength: " + pathToGold.Length);

                if (pathToGold == null)
                {
                    //Debug.Log("No possible path!");
                    continue;
                }

                //Debug.Log("Distance to gold: " + pathToGold.Length);

                if(pathToGold.Length > 1)
                {
                    //Need to try to move closer to gold.
                    moveCivilian(civilians[i], new Vector2Int(pathToGold[1].x + 5, pathToGold[1].y + 5));
                }
                else
                {
                    //Already adjacent to gold, no need to move.
                }
            }
            else
            {
                bool foundAssassin = false;
                int assassinID = -1;

                for(int j = 0; j < assassins.Length; j++)
                {
                    if (!assassins[j].GetComponent<Assassin>().isAlive) continue;
                    RaycastHit2D hit = lineOfSight(civilians[i], assassins[j]);
                    
                    if(hit.collider.CompareTag("Assassin"))
                    {
                        foundAssassin = true;
                        assassinID = j;
                        break;
                    }
                }
                
                if(foundAssassin)
                {
                    //Found an assassin! Try to break line of sight!

                    List<Vector2Int> possibleSpaces = new List<Vector2Int>();
                    int origX = civilians[i].GetComponent<Civilian>().gridX;
                    int origY = civilians[i].GetComponent<Civilian>().gridY;

                    possibleSpaces.Add(new Vector2Int(origX+1, origY));
                    possibleSpaces.Add(new Vector2Int(origX, origY+1));
                    possibleSpaces.Add(new Vector2Int(origX+1, origY+1));
                    possibleSpaces.Add(new Vector2Int(origX-1, origY));
                    possibleSpaces.Add(new Vector2Int(origX, origY-1));
                    possibleSpaces.Add(new Vector2Int(origX-1, origY-1));
                    possibleSpaces.Add(new Vector2Int(origX-1, origY+1));
                    possibleSpaces.Add(new Vector2Int(origX+1, origY-1));
                    Vector2Int[] possibleArray = possibleSpaces.ToArray();

                    Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
                    Vector2Int assassinGridPos = new Vector2Int(assassins[assassinID].GetComponent<Assassin>().gridX, assassins[assassinID].GetComponent<Assassin>().gridY);
                    Vector3 assassinWorldPos3D = tiles.CellToWorld(new Vector3Int(assassinGridPos.x - gridSize / 2, assassinGridPos.y - gridSize / 2, 0)) + offset;
                    Vector2 assassinWorldPos = new Vector2(assassinWorldPos3D.x, assassinWorldPos3D.y);

                    for (int k = 0; k < possibleArray.Length; k++)
                    {
                        if (board[(gridSize - 1) - possibleArray[k].y][possibleArray[k].x] != passable) continue;

                        Vector2Int temp = possibleArray[k];
                        Vector3 tempWorldPos3D = tiles.CellToWorld(new Vector3Int(temp.x - gridSize / 2, temp.y - gridSize / 2, 0)) + offset;
                        Vector2 rayOrigin = new Vector2(tempWorldPos3D.x, tempWorldPos3D.y);

                        RaycastHit2D hit2 = lineOfSightPos(rayOrigin, assassinWorldPos);

                        if(!hit2.collider.CompareTag("Assassin"))
                        {
                            //Broke line of sight! Move to new space!
                            moveCivilian(civilians[i], possibleArray[k]);
                        }
                    }
                }
                else
                {
                    //Do nothing...
                }
            }
        }

        //If any civilian is adjacent to the gold, remove the gold.
        for(int i = 0; i < civilians.Length; i++)
        {
            if (!civilians[i].GetComponent<Civilian>().isAlive) continue;
            int civX = civilians[i].GetComponent<Civilian>().gridX;
            int civY = civilians[i].GetComponent<Civilian>().gridY;

            if(Mathf.Abs(civX - (goldTilePos[turnNumber].x + 5)) <= 1 && Mathf.Abs(civY - (goldTilePos[turnNumber].y + 5)) <= 1)
            {
                //Civilian is adjacent to the gold!
                goldExists[turnNumber + 1] = false;
                goldTilePos[turnNumber + 1] = new Vector2Int();
                gold.transform.position = goldRestingPlace;
                break;
            }
        }
    }

    void moveCivilian(GameObject civ, Vector2Int newPos)
    {
        int oldGridX = civ.GetComponent<Civilian>().gridX;
        int oldGridY = civ.GetComponent<Civilian>().gridY;

        board[(gridSize - 1) - oldGridY][oldGridX] = passable;
        board[(gridSize - 1) - newPos.y][newPos.x] = civilian;

        civ.GetComponent<Civilian>().gridX = newPos.x;
        civ.GetComponent<Civilian>().gridY = newPos.y;

        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        civ.transform.position = tiles.CellToWorld(new Vector3Int(newPos.x - gridSize / 2, newPos.y - gridSize / 2, 0)) + offset;
    }

    void assassinTurn()
    {
        //have assassin execute their turn
        for(int i = 0; i < assassins.Length; i++)
        {
            Assassin script = assassins[i].GetComponent<Assassin>();

            Debug.Log("assassin " + i + " taking turn");
            //if stabbing
            if (script.endAction == Assassin.EndAction.STAB)
            {
                Debug.Log("assassin stabbing from gridpos " + script.gridX + ", " + script.gridY);
                //"walk" to end of path
                moveAssassin(assassins[i], script.nextTurnPath[script.nextTurnPath.Length - 1]);

                //stab the person
                killGameObject(script.target);
            }
            else if (script.endAction == Assassin.EndAction.AIM)
            {
                //"walk" to end of path
                moveAssassin(assassins[i], script.nextTurnPath[script.nextTurnPath.Length - 1]);

                //aim bow
                Debug.Log("fire!");

                //stab the person
                killGameObject(script.target);
                
            }
            else if (script.endAction == Assassin.EndAction.IDLE)
            {
                //just "walk" to end of path
                moveAssassin(assassins[i], script.nextTurnPath[script.nextTurnPath.Length - 1]);

                Debug.Log("to place wanted to go");
            }
        }
    }

    /*
     * will be erors if not passing in a killable character
     */
    void killGameObject(GameObject toKill)
    {
        toKill.GetComponent<SpriteRenderer>().enabled = false;

        toKill.GetComponent<BoxCollider2D>().enabled = false;
        //remove them from board
        if (toKill.tag == "Unit")
        {
            ControllableUnit control = toKill.GetComponent<ControllableUnit>();

            control.isAlive = false;

            board[gridSize - 1 - control.gridY][control.gridX] = passable;
        }
        else if (toKill.tag == "Civilian")
        {
            Civilian civ = toKill.GetComponent<Civilian>();

            civ.isAlive = false;

            board[gridSize - 1 - civ.gridY][civ.gridX] = passable;
        }
        else if (toKill.tag == "King")
        {
            King kingScript = toKill.GetComponent<King>();

            // kill king
            kingScript.isAlive = false;

            onKingKilled();

            //TODO implement game over
            Debug.Log("The king is dead");
        }
        else Debug.LogError("assassin has unidentified target! can't kill!");

    }

    public void onKingKilled()
    {
        battleUI.nextTurnButton.interactable = false;
    }

    /*
     * pos is in tilespace
     */
    public void moveAssassin(GameObject assassin, Vector2Int pos)
    {
        //assassin script
        Assassin script = assassin.GetComponent<Assassin>();

        //get old position
        int oldGridX = script.gridX;
        int oldGridY = script.gridY;

        //update gridX, gridY
        script.gridX = pos.x + gridSize/2;
        script.gridY = pos.y + gridSize/2;

        //move prefab to correct location
        Vector3 offset = new Vector3(tiles.cellSize.x / 2, tiles.cellSize.x / 2, -1);
        assassin.transform.position = tiles.CellToWorld(new Vector3Int(pos.x, pos.y, 0)) + offset;

        //remove old space on  board
        board[gridSize - 1 - (pos.y + gridSize/2)][pos.x + gridSize/2] = passable;


        //put new space on board
        board[gridSize - 1 - (pos.y + gridSize/2)][pos.x + gridSize/2] = this.assassin;
    }

    /*
     * pos is in tilespace
     */
    public bool isPassableAtTileSpace(Vector2Int pos)
    {
        int tile = getTileAtSpace(pos);
        return tile == passable;

    }

    /*
     * pos is in tilespace
     */
    public int getTileAtSpace(Vector2Int pos)
    {
        int tile = board[gridSize - 1 - (pos.y + gridSize/2)][pos.x + gridSize/2];
        return tile;
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

        predictAssassinMove();

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

