using Grid;
using MeshGeneration;
using Splines;
using Streets;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct CreateCellJob
{
    [ReadOnly]
    public Street s;
    [ReadOnly]
    public int id;
    public bool isLeft;
    public NativeList<Cell> cellList;
    public int x;
    public int y;

    public Cell CreateCell()
    {
        Cell c = new Cell();
        if (y + 1 >= s.m_Spline.GridOPs.Length)
            c.Init(s, s.m_Spline.GridOPs[y].t, s.m_Spline.GetLastOrientedPoint().t, x, isLeft);
        else
            c.Init(s, s.m_Spline.GridOPs[y].t, s.m_Spline.GridOPs[y + 1].t, x, isLeft);

        return c;
    }
}
