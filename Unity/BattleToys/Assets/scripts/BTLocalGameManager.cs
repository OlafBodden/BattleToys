using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTLocalGameManager : MonoBehaviour
{
    static private BTLocalGameManager _instance;

    public BTPlayer localPlayer;


    static public BTLocalGameManager Instance { get { return _instance;}}

    private void Awake() {
        _instance=this;
    }
}
