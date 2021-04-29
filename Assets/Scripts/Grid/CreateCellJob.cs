using Grid;
using MeshGeneration;
using Splines;
using Streets;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct CreateCellJob : IJob
{
    [ReadOnly]
    public Spline s;
    [ReadOnly]
    public int id;
    public bool isLeft;
    public NativeList<Cell> cellList;
    public int x;
    public int y;

    public void Execute()
    {
        Cell c = CreateCell();
        if (c.isValid)
            cellList.Add(c);
    }

    private Cell CreateCell()
    {
        Cell c = new Cell();
        if (y + 1 >= s.GridOPs.Length)
            c.Init(s, id, s.GridOPs[y].t, s.GetLastOrientedPoint().t, x, isLeft);
        else
            c.Init(s, id, s.GridOPs[y].t, s.GridOPs[y + 1].t, x, isLeft);

        return c;
    }
}
