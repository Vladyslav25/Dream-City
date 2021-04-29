using MeshGeneration;
using Streets;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public int MaxGeneration { get; } = 5;
        public float GridMaxSquareArea { get; } = 5f;
        public float GridMinSquareArea { get; } = 0.7f;

        public static List<Cell> m_AllCells = new List<Cell>();
        public Material White;

        public static List<Cell> CreateGrid(Street _street)
        {
            List<Cell> output = new List<Cell>();

            GameObject obj = new GameObject("Grid");
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            obj.transform.position = Vector3.zero;
            obj.transform.parent = _street.transform;

            NativeList<Cell> cellList = new NativeList<Cell>();

            NativeList<JobHandle> jobHandelList = new NativeList<JobHandle>(Allocator.Temp);

            //Left Side
            for (int y = 0; y < _street.m_Spline.GridOPs.Length; y++)
            {
                for (int x = 0; x < Instance.MaxGeneration; x++)
                {
                    //Cell c = CreateCell(_street, out bool isValid, true, x, y);
                    CreateCellJob job = new CreateCellJob();
                    job.s = _street.m_Spline;
                    job.id = _street.ID;
                    job.x = x + 1;
                    job.y = y;
                    job.isLeft = true;
                    job.cellList = cellList;
                    JobHandle handle = job.Schedule();
                    jobHandelList.Add(handle);
                }
            }
            JobHandle.CompleteAll(jobHandelList);

            //if (!isValid)
            //{
            //    break;
            //}
            output.AddRange(cellList);

            jobHandelList.Dispose();

            //Right Side
            for (int y = 0; y < _street.m_Spline.GridOPs.Length; y++)
            {
                for (int x = 1; x < Instance.MaxGeneration + 1; x++)
                {
                    Cell c = CreateCell(_street, out bool isValid, false, x, y);

                    if (!isValid)
                    {
                        break;
                    }
                    output.Add(c);
                }
            }

            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.material = Instance.White;

            MeshGenerator.CreateGridMesh(_street.m_StreetCells, mf);
            m_AllCells.AddRange(output);
            return output;
        }

        /// <summary>
        /// Create a Cell with all needed Components
        /// </summary>
        /// <param name="_obj">The Gameobject of the Cell</param>
        /// <param name="isValid">out if the size and position(collision) is valid</param>
        /// <param name="_isLeft"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Return the Cell</returns>
        private static Cell CreateCell(Street _street, out bool isValid, bool _isLeft, int x, int y)
        {
            Cell c = new Cell();
            if (y + 1 >= _street.m_Spline.GridOPs.Length)
                isValid = c.Init(_street, _street.m_Spline.GridOPs[y].t, _street.m_Spline.GetLastOrientedPoint().t, x, _isLeft);
            else
                isValid = c.Init(_street, _street.m_Spline.GridOPs[y].t, _street.m_Spline.GridOPs[y + 1].t, x, _isLeft);

            if (isValid)
                _street.m_StreetCells.Add(c);

            return c;
        }


    }
}
