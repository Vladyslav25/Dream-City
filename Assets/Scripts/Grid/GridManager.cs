using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    #region -SingeltonPattern-
    private static GridManager _instance;
    public static GridManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GridManager>();
                if (_instance == null)
                {
                    GameObject container = new GameObject("GridManager");
                    _instance = container.AddComponent<GridManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    private float gridSize = 1f;
    public float GridSize
    {
        get
        {
            return gridSize;
        }
    }
}
