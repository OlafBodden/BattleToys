﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Selectable : MonoBehaviour
{

    Projector markingProjector;

    BTPlayer player;

    bool isSelected=false;

    bool isSelectedAsHoverUnit=false;
    bool isSelectedAsHoverTarget=false;
    bool isSelectedAsUnit=false;
    bool isSelectedAsTarget=false;

    // Start is called before the first frame update
    public void Init(BTPlayer player)
    {
        this.player=player;

        markingProjector=GetComponentInChildren<Projector>();
        if (markingProjector==null) Debug.LogError("No Projector found");
        markingProjector.enabled=false;
        //markingProjector.material.color=Color.green;
    }

    private void Awake() 
    {
        markingProjector=GetComponentInChildren<Projector>();
        if (markingProjector==null) Debug.LogError("No Projector found");
        markingProjector.enabled=false;
        //markingProjector.material.color=Color.green;
    }

    public void Select(BTPlayer player)
    {
        if (this.player==player) Select();
        else SelectAsTarget();
    }

    void Select()
    {
        if (markingProjector) 
        {
            markingProjector.material=BTLocalGameManager.Instance.projectorMaterialSelectedUnit;
            markingProjector.enabled=true;
        }

        isSelected=true;
        isSelectedAsUnit=true;
    }

    public void DeSelect()
    {
        if (markingProjector) markingProjector.enabled=false;

        isSelected=false;
        isSelectedAsUnit=false;
    }

    public void SelectAsTarget()
    {
        if (markingProjector) 
        {
            markingProjector.material=BTLocalGameManager.Instance.projectorMaterialSelectedTarget;
            markingProjector.enabled=true;
        }

        isSelected=true;
        isSelectedAsTarget=true;
    }

    public void DeSelectAsTarget()
    {
        if (markingProjector) 
        {
            markingProjector.enabled=false;
        }

        isSelected=false;
        isSelectedAsTarget=false;
    }

    public void SelectAsHoveredUnit()
    {       
        if (isSelected==true) return; //Only mark as hovered, when not allready Selected

        Debug.Log($"I am hovered (unit): { this.gameObject.name }");

        if (markingProjector)
        {
            markingProjector.material=BTLocalGameManager.Instance.projectorMaterialHoveredUnit;
            markingProjector.enabled=true;
        }

        isSelectedAsHoverUnit=true;

    }

    public void DeSelectHovered()
    {
        if (isSelected) return; //If a unit is selected, it cannot "De-Hover"

        Debug.Log($"I am De-hovered: { this.gameObject.name }");

        if (markingProjector)
        {
            markingProjector.enabled=false;
        }

        isSelectedAsHoverUnit=false;
        isSelectedAsHoverTarget=false;
    }

    public void SelectAsHoveredTarget()
    {       
        if (isSelected==true) return; //Only mark as hovered, when not allready Selected

        Debug.Log($"I am hovered (target): { this.gameObject.name }");

        if (markingProjector)
        {
            markingProjector.material=BTLocalGameManager.Instance.projectorMaterialHoveredTarget;
            markingProjector.enabled=true;
        }

        isSelectedAsHoverTarget=true;

    }

    public void SelectAsHovered(BTPlayer player)
    {
        if (this.player==player)
        {
            SelectAsHoveredUnit();
        } else
        {
            SelectAsHoveredTarget();
        }
    }



    public bool IsMe(BTPlayer player)
    {
        if (player==null) return false;
        return player==this.player;
    }
}