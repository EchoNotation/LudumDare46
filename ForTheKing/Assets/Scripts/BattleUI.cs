using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    public GameObject currentlySelected;
    public Canvas optionCanvas;

    bool hasSelection = false;

    bool awaitingInput = false;

    BattleManager battleManager;

    enum Action {
        NONE,
        MOVE,
        SHOVE,
        COIN,
        TAUNT,
       //TODO the knight
    }

    Action currentAction = Action.NONE;

    // Start is called before the first frame update
    void Start()
    {
        battleManager = FindObjectOfType<BattleManager>();
        optionCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            unSelect();
        }
        
    }

    public void onRewind()
    {
        unSelect();

        battleManager.rewind();
    }

    public void onNextTurn()
    {
        unSelect();

        battleManager.commit();
    }

    public void onMoveButton()
    {
        //highlight movable areas Knight: moves like chess King, Noble: moves in 5 diameter circle, Jester: moves in 7 diameter circle

        if (currentlySelected.GetComponent<ControllableUnit>().hasMoved) return;

        awaitingInput = true;
        currentAction = Action.MOVE;
    }

    public void onShoveButton()
    {
        //highlight adjacent units that can be shoved

        //get ready for next unit click to be receiver
        awaitingInput = true;
        currentAction = Action.SHOVE;

    }

    public void onSpecial()
    {
        //do whatever the special for the selected unit is
        awaitingInput = true;
        
        switch(currentlySelected.GetComponent<ControllableUnit>().getUnitType())
        {
            case ControllableUnit.UnitType.KNIGHT:
                //TODO Add Knight special.
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
                if (currentlySelected.GetComponent<ControllableUnit>().hasMoved) return;

                //forward to battle manager
                battleManager.moveToPosition(currentlySelected, new Vector2(x, y));

                //currentlySelected.GetComponent<ControllableUnit>().hasMoved = true;

                unSelect();
            }            
            else if(currentAction == Action.COIN)
            {
                if (currentlySelected.GetComponent<ControllableUnit>().hasTakenAction) return;

                //Check if valid move

                battleManager.tossCoin(new Vector2Int(x, y));
                currentlySelected.GetComponent<ControllableUnit>().hasTakenAction = true;
                unSelect();              
            }
        }
    }

    public void onUnitClick(GameObject clicked)
    {
        Debug.Log("Unit click");

        if(awaitingInput)
        {
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

                }

                if (Mathf.Abs(shoverX - shovedX) <= 1 && Mathf.Abs(shoverY - shovedY) <= 1)
                {
                    string shoveString = (shoverX - shovedX) + "" + (shoverY - shovedY);
                    Direction shoveDir = Direction.NONE;

                    switch(shoveString)
                    {
                        case "-1-1":
                            shoveDir = Direction.UP_RIGHT;
                            break;
                        case "-10":
                            shoveDir = Direction.RIGHT;
                            break;
                        case "-11":
                            shoveDir = Direction.DOWN_RIGHT;
                            break;
                        case "0-1":
                            shoveDir = Direction.UP;
                            break;
                        case "01":
                            shoveDir = Direction.DOWN;
                            break;
                        case "1-1":
                            shoveDir = Direction.UP_LEFT;
                            break;
                        case "10":
                            shoveDir = Direction.LEFT;
                            break;
                        case "11":
                            shoveDir = Direction.DOWN_LEFT;
                            break;
                        case "00":
                        default:
                            Debug.Log("Invalid shove string in onUnitClick! ShoveString: " + shoveString);
                            break;
                    }

                    battleManager.shove(clicked, shoveDir);
                    //currentlySelected.GetComponent<ControllableUnit>().hasTakenAction = true;
                    unSelect();
                }
            }
            else if(currentAction == Action.TAUNT)
            {
                if (currentlySelected.GetComponent<ControllableUnit>().hasTakenAction) return;

                if(clicked.CompareTag("Assassin"))
                {
                    //Taunt the assassin

                    currentlySelected.GetComponent<ControllableUnit>().hasTakenAction = true;
                    unSelect();
                }
            }
        }
        else
        {
            setSelected(clicked);
        }

    }

    public void setSelected(GameObject select)
    {
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

        switch(select.GetComponent<ControllableUnit>().getUnitType())
        {
            case ControllableUnit.UnitType.JESTER:
                specialText = "Taunt";
                break;
            case ControllableUnit.UnitType.KNIGHT:
                //TODO add knight special.
                specialText = "Knight Special";
                break;
            case ControllableUnit.UnitType.NOBLE:
                specialText = "Toss Coin";
                break;
            default:
                Debug.Log("Invalid UnitType! UnitType: " + select.GetComponent<ControllableUnit>().getUnitType().ToString());
                break;
        }

        GameObject.Find("SpecialButton").GetComponentInChildren<Text>().text = specialText;
    }

    public void unSelect()
    {
        currentlySelected = null;
        hasSelection = false;
        currentAction = Action.NONE;
        awaitingInput = false;
        optionCanvas.enabled = false;
    }

    private void OnDrawGizmos()
    {
        if(hasSelection)
            Gizmos.DrawWireSphere(currentlySelected.transform.position, 1);
    }
}
