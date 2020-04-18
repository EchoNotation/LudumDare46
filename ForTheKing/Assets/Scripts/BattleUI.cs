using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUI : MonoBehaviour
{
    public GameObject currentlySelected;

    bool hasSelection = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setSelected(GameObject select)
    {
        currentlySelected = select;
        hasSelection = true;
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
