using UnityEngine;
using System.Collections.Generic;

public class EnemyMinimapTarget : MonoBehaviour
{
   
    public static HashSet<EnemyMinimapTarget> ActiveEnemies =
        new HashSet<EnemyMinimapTarget>();

    void OnEnable()
    {
        ActiveEnemies.Add(this);
    }

    void OnDisable()
    {
        ActiveEnemies.Remove(this);
    }
}


