using MeshGeneration;
using Streets;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public int MaxGeneration { get; } = 5;
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

            _street.m_StreetCells.AddRange(output);
            MeshGenerator.CreateGridMesh(_street.m_StreetCells, mf);
            m_AllCells.AddRange(output);
        }

        public static List<Cell> CreateGrid(Street _street)
        {
            output.Clear();

            //Left Side
            for (int y = 0; y < _street.m_Spline.GridOPs.Length; y++)
            {
                for (int x = 0; x < Instance.MaxGeneration; x++)
                {
                    CreateCellJob jobLeft = new CreateCellJob();
                    jobLeft.s = _street;
                    jobLeft.id = _street.ID;
                    jobLeft.x = x + 1;
                    jobLeft.y = y;
                    jobLeft.isLeft = true;

                    Task<Cell> tL = new Task<Cell>(jobLeft.CreateCell);
                    listTask.Add(tL);
                    tL.Start();

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
