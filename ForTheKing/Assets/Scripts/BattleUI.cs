using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public GameObject currentlySelected;
    public Canvas optionCanvas;

    public GameObject movementMarker;
    public GameObject selectionMarker;

    bool hasSelection = false;

    bool awaitingInput = false;
    int gridSize;

    BattleManager battleManager;

    Tilemap tiles;

    public Button nextTurnButton;
    public Button rewindButton;
    public Button specialButton;

    [SerializeField]
    private bool debugUnlimitedMovement = false;

    enum Action {
        NONE,
        MOVE,
        SHOVE,
        COIN,
        TAUNT,
        BLOCK
    }

    Action currentAction = Action.NONE;

    private void Awake()
    {
        tiles = FindObjectOfType<Tilemap>();
    }

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        optionCanvas.enabled = false;
        gridSize = BattleManager.gridSize;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            unSelect();
        }

        updateRewindButton(battleManager.turnNumber);
    }

    public void onRewind()
    {
        unSelect();

        battleManager.resetActions();
        battleManager.rewind();
    }

    public void onNextTurn()
    {
        unSelect();

        battleManager.resetActions();
        battleManager.commit();
    }

    public void onMoveButton()
    {
        //highlight movable areas Knight: moves like chess King, Noble: moves in 5 diameter circle, Jester: moves in 7 diameter circle

        if (currentlySelected.GetComponent<ControllableUnit>().hasMoved) return;

        //if already moving, cancel moving
        if (currentAction == Action.MOVE)
        {
            currentAction = Action.NONE;
            awaitingInput = false;
            removeMoveMarkers("MoveMarker");

            return;
        }

        awaitingInput = true;
        currentAction = Action.MOVE;

        currentlySelected.GetComponent<ControllableUnit>().updateMoveablePositions();
        Vector2Int[] moves = currentlySelected.GetComponent<ControllableUnit>().moveablePositions.ToArray();

        for(int i = 0; i < moves.Length; i++)
        {
            createMarkerAtTile(moves[i], movementMarker);
        }
    }

    public void createMarkerAtTile(Vector2Int tilePos, GameObject marker)
    {
        float offset = tiles.cellSize.x / 2;
        Vector3 worldPos = new Vector3(tilePos.x + offset, tilePos.y + offset);
        Instantiate(marker, worldPos, Quaternion.identity);
    }

    public void removeMoveMarkers(string tag)
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag(tag);
        for(int i = 0; i < markers.Length; i++)
        {
            Destroy(markers[i]);
        }
        
    }

    public void onShoveButton()
    {
        //highlight adjacent units that can be shoved

        //get ready for next unit click to be receiver
        awaitingInput = true;
        currentAction = Action.SHOVE;

        int[][] tempBoard = GetComponent<BattleManager>().getBoard();

        int currentGridX = currentlySelected.GetComponent<ControllableUnit>().gridX;
        int currentGridY = (gridSize - 1) - currentlySelected.GetComponent<ControllableUnit>().gridY;
        /*Debug.Log("GridX: " + currentGridX + " GridY: " + currentGridY);
        Debug.Log("TL:" + tempBoard[currentGridY - 1][currentGridX - 1]);
        Debug.Log("U:" + tempBoard[currentGridY - 1][currentGridX]);
        Debug.Log("TR:" + tempBoard[currentGridY - 1][currentGridX + 1]);
        Debug.Log("R:" + tempBoard[currentGridY][currentGridX + 1]);
        Debug.Log("DR:" + tempBoard[currentGridY + 1][currentGridX + 1]);
        Debug.Log("D:" + tempBoard[currentGridY + 1][currentGridX]);
        Debug.Log("DL:" + tempBoard[currentGridY + 1][currentGridX - 1]);
        Debug.Log("L:" + tempBoard[currentGridY][currentGridX - 1]);*/

    }

    public void onSpecial()
    {
        //do whatever the special for the selected unit is
        awaitingInput = true;
        
        switch(currentlySelected.GetComponent<ControllableUnit>().getUnitType())
        {
            case ControllableUnit.UnitType.KNIGHT:
                currentAction = Action.BLOCK;
                break;
            case ControllableUnit.UnitType.JESTER:
                currentAction = Action.TAUNT;
                break;
            case ControllableUnit.UnitType.NOBLE:
                currentAction = Action.COIN;
                break;
            default:
                Debug.Log("Invalid UnitType in OnSpecial! UnitType: " + currentlySelected.GetComponent<ControllableUnit>().getUnitType().ToString());
                break;
        }
    }

    public void onTileClick(int x, int y)
    {
        //Debug.Log(x + " " + y);

        if(awaitingInput)
        {
            if(currentAction == Action.MOVE)
            {
                //TODO check if valid move
                currentlySelected.GetComponent<ControllableUnit>().updateMoveablePositions();
                Vector2Int[] moves = currentlySelected.GetComponent<ControllableUnit>().moveablePositions.ToArray();

                bool validMovement = false;
                for(int i = 0; i < moves.Length; i++)
                {
                    if(moves[i].x == x && moves[i].y == y)
                    {
                        validMovement = true;
                        break;
                    }
                }

                if (!validMovement) return;
                if (currentlySelected.GetComponent<ControllableUnit>().hasMoved) return;

                //forward to battle manager
                battleManager.moveToPosition(currentlySelected, new Vector2(x, y));

                if(!debugUnlimitedMovement)
                    currentlySelected.GetComponent<ControllableUnit>().hasMoved = true;

                removeMoveMarkers("MoveMarker");

                unSelect();
            }            
            else if(currentAction == Action.COIN)
            {
                if (currentlySelected.GetComponent<ControllableUnit>().hasTakenAction) return;

                //Check if valid move
                int[][] tempBoard = battleManager.getBoard();

                Debug.Log("X: " + x + " Y: " + y);

                if (tempBoard[(gridSize - 1) - (y + 5)][(x + 5)] != battleManager.passable) return;

                battleManager.tossCoin(new Vector2Int(x, y));

                if(!debugUnlimitedMovement)
                    currentlySelected.GetComponent<ControllableUnit>().hasTakenAction = true;

                unSelect();              
            }
        }
    }

    public void onAssassinClick(GameObject clicked)
    {
        if(awaitingInput && currentAction == Action.TAUNT)
        {
            //send to battle manager
            battleManager.taunt(clicked, currentlySelected);

            //clear button
            currentlySelected.GetComponent<ControllableUnit>().hasTakenAction = true;

            unSelect();
        }
    }

    public void onUnitClick(GameObject clicked)
    {
        //Debug.Log("Unit click");

        if(awaitingInput)
        {
            if(currentAction == Action.MOVE)
            {
                removeMoveMarkers("MoveMarker");
                unSelect();

            }

            //do the thing
            if(currentAction == Action.SHOVE)
            {
                if (currentlySelected.GetComponent<ControllableUnit>().hasTakenAction) return;

                //Check if selection is valid: Adjacent to current unit
                int shoverX = currentlySelected.GetComponent<ControllableUnit>().gridX;
                int shoverY = currentlySelected.GetComponent<ControllableUnit>().gridY;

                int shovedX = 0;
                int shovedY = 0;

                if(clicked.CompareTag("Unit"))
                {
                    shovedX = clicked.GetComponent<ControllableUnit>().gridX;
                    shovedY = clicked.GetComponent<ControllableUnit>().gridY;
                }
                else if(clicked.CompareTag("Civilian"))
                {
                    shovedX = clicked.GetComponent<Civilian>().gridX;
                    shovedY = clicked.GetComponent<Civilian>().gridY;
                }

                /*Debug.Log("ShoverX: " + shoverX + " ShoverY: " + shoverY);
                Debug.Log("ShovedX: " + shovedX + " ShovedY: " + shovedY);*/

                if (Mathf.Abs(shoverX - shovedX) <= 1 && Mathf.Abs(shoverY - shovedY) <= 1)
                {

                    string shoveString = (shoverX - shovedX) + "" + (shoverY - shovedY);
                    Direction shoveDir = Direction.NONE;

                    int iCoord = (gridSize - 1) - shovedY;
                    int jCoord = shovedX;

                    switch(shoveString)
                    {
                        case "-1-1":
                            shoveDir = Direction.UP_RIGHT;
                            iCoord--;
                            jCoord++;
                            break;
                        case "-10":
                            shoveDir = Direction.RIGHT;
                            jCoord++;
                            break;
                        case "-11":
                            shoveDir = Direction.DOWN_RIGHT;
                            jCoord++;
                            iCoord++;
                            break;
                        case "0-1":
                            shoveDir = Direction.UP;
                            iCoord--;
                            break;
                        case "01":
                            shoveDir = Direction.DOWN;
                            iCoord++;
                            break;
                        case "1-1":
                            shoveDir = Direction.UP_LEFT;
                            iCoord--;
                            jCoord--;
                            break;
                        case "10":
                            shoveDir = Direction.LEFT;
                            jCoord--;
                            break;
                        case "11":
                            shoveDir = Direction.DOWN_LEFT;
                            jCoord--;
                            iCoord++;
                            break;
                        case "00":
                        default:
                            Debug.Log("Invalid shove string in onUnitClick! ShoveString: " + shoveString);
                            break;
                    }

                    int[][] tempBoard = battleManager.getBoard();

                    //Debug.Log("TargetI: " + (iCoord) + " TargetJ: " + jCoord);

                    if(tempBoard[iCoord][jCoord] == battleManager.passable)
                    {
                        //Debug.Log("BoardSpace: " + tempBoard[iCoord][jCoord]);
                        battleManager.shove(clicked, shoveDir);
                        
                        if(!debugUnlimitedMovement)
                            currentlySelected.GetComponent<ControllableUnit>().hasTakenAction = true;

                        unSelect();
                    }                  
                }
            }
            else if(currentAction == Action.TAUNT)
            {
                if (currentlySelected.GetComponent<ControllableUnit>().hasTakenAction) return;

                if(clicked.CompareTag("Assassin"))
                {
                    //Taunt the assassin

                    if(!debugUnlimitedMovement)
                        currentlySelected.GetComponent<ControllableUnit>().hasTakenAction = true;

                    unSelect();
                }
            }
        }
        else
        {
            if(clicked.CompareTag("Unit")) 
                setSelected(clicked);
        }

    }

    public void setSelected(GameObject select)
    {
        removeMoveMarkers("MoveMarker");
        currentlySelected = select;
        hasSelection = true;
        awaitingInput = false;
        //show option menu
        optionCanvas.enabled = true;

        GameObject.Find("MoveButton").GetComponent<Button>().interactable = !select.GetComponent<ControllableUnit>().hasMoved;
        bool canTakeAction = !select.GetComponent<ControllableUnit>().hasTakenAction;
        GameObject.Find("ShoveButton").GetComponent<Button>().interactable = canTakeAction;
        GameObject.Find("SpecialButton").GetComponent<Button>().interactable = canTakeAction;

        string specialText = "Special";

        switch (select.GetComponent<ControllableUnit>().getUnitType())
        {
            case ControllableUnit.UnitType.JESTER:
                specialText = "Taunt";
                break;
            case ControllableUnit.UnitType.KNIGHT:
                specialText = "Block";
                break;
            case ControllableUnit.UnitType.NOBLE:
                specialText = "Toss Coin";
                break;
            default:
                Debug.Log("Invalid UnitType! UnitType: " + select.GetComponent<ControllableUnit>().getUnitType().ToString());
                break;
        }

        GameObject.Find("SpecialButton").GetComponentInChildren<Text>().text = specialText;

        updateSpecialButton(battleManager.goldExists[battleManager.turnNumber]);
        selectionMarker.transform.position = currentlySelected.transform.position;
    }

    public void unSelect()
    {
        if(currentlySelected != null) currentlySelected.GetComponent<ControllableUnit>().moveablePositions.Clear();
        removeMoveMarkers("MoveMarker");
        currentlySelected = null;
        hasSelection = false;
        currentAction = Action.NONE;
        awaitingInput = false;
        optionCanvas.enabled = false;

        selectionMarker.transform.position = new Vector3(-15, 3, 0);
    }

    private void OnDrawGizmos()
    {
        if(hasSelection && currentlySelected != null)
            Gizmos.DrawWireSphere(currentlySelected.transform.position, 1);
    }

    public void updateRewindButton(int turnNumber)
    {
        if(turnNumber <= 1)
        {
            rewindButton.interactable = false;
        }
        else
        {
            rewindButton.interactable = true;
        }
    }

    public void updateSpecialButton(bool goldExists)
    {
        if(currentlySelected.GetComponent<ControllableUnit>().getUnitType() == ControllableUnit.UnitType.NOBLE)
        {
            if (goldExists)
            {
                specialButton.interactable = false;
            }
            else
            {
                specialButton.interactable = true;
            }
        }       
    }
}
