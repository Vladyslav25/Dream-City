using Gameplay.StreetComponents;
using MeshGeneration;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public int MaxGeneration { get; } = 18;
        public float GridMaxSquareArea { get; } = 5f;
        public float GridMinSquareArea { get; } = 0.7f;

        static List<Task<Cell>> listTask = new List<Task<Cell>>();
        static List<Cell> output = new List<Cell>();
        public static List<Cell> m_AllCells = new List<Cell>();
        public static List<Cell> m_FirstGenCells = new List<Cell>();
        public Material CellDefault;

        public static List<Cell> m_AllLivingCells = new List<Cell>();
        public static List<Cell> m_AllBusinessCells = new List<Cell>();
        public static List<Cell> m_AllIndustryCells = new List<Cell>();

        public static IEnumerator CheckForFinish(Street _street)
        {
            List<Cell> cellsToDelete = new List<Cell>();
            while (listTask.Count > 0)
            {
                for (int i = listTask.Count - 1; i >= 0; i--)
                {
                    if (listTask[i].IsCompleted)
                    {
                        if (listTask[i].Result.IsValid)
                            output.Add(listTask[i].Result);
                        else
                            cellsToDelete.Add(listTask[i].Result);
                        listTask.RemoveAt(i);
                    }
                }
                yield return null;
            }

            //Create Normal Grid
            GameObject obj = new GameObject("Grid");
            MeshFilter mf = obj.AddComponent<MeshFilter>();
            MeshRenderer mr = obj.AddComponent<MeshRenderer>();
            obj.transform.position = new Vector3(0, 0.1f, 0);
            obj.transform.parent = _street.transform;

            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.material = Instance.CellDefault;

            foreach (Cell c in output)
            {
                _street.m_StreetCells.Add(c.Pos, c);
                if (c.Pos.x == 1 || c.Pos.x == -1)
                    m_FirstGenCells.Add(c);
            }

            foreach (Cell c in cellsToDelete)
            {
                c.Delete();
            }

            //Remove Cells if Generation before is missing
            for (int i = 0; i < output.Count; i++)
            {
                Cell c = output[i];
                if (_street.m_StreetCells.ContainsKey(c.Pos))
                {
                    if (c.m_isLeft && c.Pos.x - 1 > 0 && !_street.m_StreetCells.ContainsKey(new Vector2Int(c.Pos.x - 1, c.Pos.y)))
                    {
                        Vector2Int nextPose = c.Pos;
                        while (_street.m_StreetCells.ContainsKey(nextPose))
                        {
                            output.Remove(c);
                            c.Delete();
                            nextPose.x++;
                        }
                    }
                    if (!c.m_isLeft && c.Pos.x + 1 < 0 && !_street.m_StreetCells.ContainsKey(new Vector2Int(c.Pos.x + 1, c.Pos.y)))
                    {
                        Vector2Int nextPose = c.Pos;
                        while (_street.m_StreetCells.ContainsKey(nextPose))
                        {
                            c.Delete();
                            output.Remove(c);
                            nextPose.x--;
                        }
                    }
                }
            }

            _street.m_RowAmount = _street.m_StreetCells.Keys.Max(v => v.y) + 1;
            MeshGenerator.CreateGridMesh(_street, mf, mr);
            m_AllCells.AddRange(output);
            _street.m_GridObj = obj;
            _street.m_GridRenderer = mr;
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
