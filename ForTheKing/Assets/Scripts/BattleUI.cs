using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    public GameObject currentlySelected;

    bool hasSelection = false;

    bool awaitingInput = false;

    // Start is called before the first frame update
    void Start()
    {
        
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

        //get ready for next tile click to be destination (how to do ?)

        //
        awaitingInput = true;
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
    }

    public void unSelect()
    {
        currentlySelected = null;
        hasSelection = false;
    }

    private void OnDrawGizmos()
    {
        if(hasSelection)
            Gizmos.DrawWireSphere(currentlySelected.transform.position, 1);
    }
}
