using MeshGeneration;
using Streets;
using System;
using System.Collections;
using System.Collections.Generic;
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
        public float GridMaxSquareArea { get; } = 2f;
        public float GridMinSquareArea { get; } = 0.7f;

        public Material White;

        public static List<Cell> CreateGrid(Street _street)
        {
            List<Cell> output = new List<Cell>();

            //Left Side
            for (int x = 1; x < Instance.MaxGeneration + 1; x++)
            {
                for (int y = 0; y < _street.m_Spline.GridOPs.Length; y++)
                {
                    GameObject obj = new GameObject(_street.ID + "_" + x + "_" + y + "_Left");

                    Cell c = CreateCell(obj, _street, out bool hasValidSize, true, x, y);

                    if (!hasValidSize)
                    {
                        Destroy(obj);
                        continue;
                    }

                    output.Add(c);
                }
            }

            //Right Side
            for (int x = 1; x < Instance.MaxGeneration + 1; x++)
            {
                for (int y = 0; y < _street.m_Spline.GridOPs.Length; y++)
                {
                    GameObject obj = new GameObject(_street.ID + "_" + x + "_" + y + "_Right");

                    Cell c = CreateCell(obj, _street, out bool hasValidSize, false, x, y);

                    if (!hasValidSize)
                    {
                        Destroy(obj);
                        continue;
                    }

                    output.Add(c);
                }
            }

            return output;
        }

        /// <summary>
        /// Create a Cell with all needed Components
        /// </summary>
        /// <param name="_obj">The Gameobject of the Cell</param>
        /// <param name="hasValidSize">out if the size is valid</param>
        /// <param name="_isLeft"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Return the Cell</returns>
        private static Cell CreateCell(GameObject _obj, Street _street, out bool hasValidSize, bool _isLeft, int x, int y)
        {
            Cell c = _obj.AddComponent<Cell>();
            if (y + 1 >= _street.m_Spline.GridOPs.Length)
                hasValidSize = c.Init(_street, _street.m_Spline.GridOPs[y].t, _street.m_Spline.GetLastOrientedPoint().t, x, _isLeft);
            else
                hasValidSize = c.Init(_street, _street.m_Spline.GridOPs[y].t, _street.m_Spline.GridOPs[y + 1].t, x, _isLeft);

            _obj.transform.rotation = c.m_Orientation;
            _obj.transform.position = c.m_WorldPosCenter;

            MeshFilter mf = _obj.AddComponent<MeshFilter>();
            MeshRenderer mr = _obj.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.material = Instance.White;

            mf.mesh = MeshGenerator.CreateCellMesh(c, _isLeft);

            _obj.AddComponent<BoxCollider>();

            _obj.transform.SetParent(Instance.transform);
            c.CheckForCollision();

            return c;
        }
    }
}
