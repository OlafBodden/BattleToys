using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponSystem", menuName = "WeaponSystem", order = 53)]
public class WeaponSystemSO : ScriptableObject
{
    public string WeaponSystemName;
    public WeapontTypeSO weaponType;

    [Header("Reload")]
    public bool needToReload;
    public float reloadAfterSeconds;
    public bool reloadAfterEachShot;
    public float reloadTime;

}
