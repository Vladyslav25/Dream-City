﻿using Gameplay.StreetComponents;
using Grid;
using Unity.Collections;
using UnityEngine;

public struct CreateCellJob
{
    [ReadOnly]
    public Street s;
    [ReadOnly]
    public int id;
    public bool isLeft;
    public int x;
    public int y;

    public Cell CreateCell()
    {
        Cell c = new Cell();
        if (y + 1 >= s.m_Spline.GridOPs.Length)
            c.Init(s, s.m_Spline.GridOPs[y].t, s.m_Spline.GetLastOrientedPoint().t, x, isLeft, new Vector2Int(x, y));
        else
            c.Init(s, s.m_Spline.GridOPs[y].t, s.m_Spline.GridOPs[y + 1].t, x, isLeft, new Vector2Int(x, y));

        return c;
    }
}
