﻿using Gameplay.StreetComponents;
using MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Grid
{
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

        public float CellSize { get; } = 1f;

        public int MaxGeneration { get; } = 4;
        public float GridMaxSquareArea { get; } = 5f;
        public float GridMinSquareArea { get; } = 0.7f;

        static List<Task<Cell>> listTask = new List<Task<Cell>>();
        static List<Cell> output = new List<Cell>();
        public static List<Cell> m_AllCells = new List<Cell>();
        public Material White;

        public static IEnumerator CheckForFinish(Street _street)
        {
            while (listTask.Count > 0)
            {
                for (int i = listTask.Count - 1; i >= 0; i--)
                {
                    if (listTask[i].IsCompleted)
                    {
                        if (listTask[i].Result.isValid)
                            output.Add(listTask[i].Result);
                        listTask.RemoveAt(i);
                    }
                }
                yield return null;
            }
            GameObject obj = new GameObject("Grid");
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            obj.transform.position = Vector3.zero;
            obj.transform.parent = _street.transform;

            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.material = Instance.White;

            foreach (Cell c in output)
            {
                _street.m_StreetCells.Add(c.pos, c);
            }

            //Remove Cells if Generation before is missing
            for (int i = 0; i < output.Count; i++)
            {
                Cell c = output[i];
                if (_street.m_StreetCells.ContainsKey(c.pos))
                {
                    if (c.m_isLeft && c.pos.x - 1 > 0 && !_street.m_StreetCells.ContainsKey(new Vector2(c.pos.x - 1, c.pos.y)))
                    {
                        Vector2 nextPose = c.pos;
                        while (_street.m_StreetCells.ContainsKey(nextPose))
                        {
                            output.Remove(_street.m_StreetCells[nextPose]);
                            _street.m_StreetCells.Remove(nextPose);
                            nextPose = new Vector2(nextPose.x + 1, nextPose.y);
                        }
                    }
                    if (!c.m_isLeft && c.pos.x + 1 < 0 && !_street.m_StreetCells.ContainsKey(new Vector2(c.pos.x + 1, c.pos.y)))
                    {
                        Vector2 nextPose = c.pos;
                        while (_street.m_StreetCells.ContainsKey(nextPose))
                        {
                            output.Remove(_street.m_StreetCells[nextPose]);
                            _street.m_StreetCells.Remove(nextPose);
                            nextPose = new Vector2(nextPose.x - 1, nextPose.y);
                        }
                    }
                }
            }

            MeshGenerator.CreateGridMesh(_street.m_StreetCells.Values.ToList(), mf);
            m_AllCells.AddRange(output);
            _street.m_GridObj = obj;
        }

        public static List<Cell> CreateGrid(Street _street)
        {
            output.Clear();

            for (int y = 0; y < _street.m_Spline.GridOPs.Length; y++)
            {
                for (int x = 0; x < Instance.MaxGeneration; x++)
                {
                    //Left Side
                    CreateCellJob jobLeft = new CreateCellJob();
                    jobLeft.s = _street;
                    jobLeft.id = _street.ID;
                    jobLeft.x = x + 1;
                    jobLeft.y = y;
                    jobLeft.isLeft = true;

                    Task<Cell> tL = new Task<Cell>(jobLeft.CreateCell);
                    listTask.Add(tL);
                    tL.Start();

                    //Right Side
                    CreateCellJob jobRight = new CreateCellJob();
                    jobRight.s = _street;
                    jobRight.id = _street.ID;
                    jobRight.x = x + 1;
                    jobRight.y = y;
                    jobRight.isLeft = false;
                    Task<Cell> tR = new Task<Cell>(jobRight.CreateCell);
                    listTask.Add(tR);
                    tR.Start();
                }
            }

            Instance.StartCoroutine(CheckForFinish(_street));

            return output;
        }

        public static void RemoveGridMesh(Street _street)
        {
            _street.m_GridObj.GetComponent<MeshFilter>().mesh.Clear();
        }
    }
}
