using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{

    Projector markingProjector;

    BTPlayer player;

    // Start is called before the first frame update
    public void Init(BTPlayer player)
    {
        this.player=player;

        markingProjector=GetComponentInChildren<Projector>();
        if (markingProjector==null) Debug.LogError("No Projector found");
        markingProjector.enabled=false;
        markingProjector.material.color=Color.green;
    }

    public void Select()
    {
        if (markingProjector) markingProjector.enabled=true;
    }

    public void DeSelect()
    {
        if (markingProjector) markingProjector.enabled=false;
    }

    public bool IsMe(BTPlayer player)
    {
        if (player==null) return false;
        return player==this.player;
    }
}
