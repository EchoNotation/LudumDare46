using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
       //TODO the night
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

    public void onMoveButton()
    {
        //highlight movable areas

        if (currentlySelected.GetComponent<ControllableUnit>().hasMoved) return;

        awaitingInput = true;
        currentAction = Action.MOVE;
    }

    public void onShoveButton()
    {
        //highlight adjacent units that can be shoved

        //get ready for next unit click to be receiver
        awaitingInput = true;

    }

    public void onSpecial()
    {
        //do whatever the special for the selected unit is
        awaitingInput = true;
    }

    public void onTileClick(int x, int y)
    {
        Debug.Log(x + " " + y);

        if(awaitingInput)
        {
            if(currentAction == Action.MOVE)
            {
                //TODO check if valid move
                if (currentlySelected.GetComponent<ControllableUnit>().hasMoved) return;

                //forward to battle manager
                battleManager.moveToPosition(currentlySelected, new Vector2(x, y));

                currentlySelected.GetComponent<ControllableUnit>().hasMoved = true;

                unSelect();
            }            
            else if(currentAction == Action.COIN)
            {

            }
        }
    }

    public void onUnitClick(GameObject clicked)
    {
        if(awaitingInput)
        {
            //do the thing
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
