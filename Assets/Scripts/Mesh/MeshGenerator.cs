﻿using Splines;
using System.Collections.Generic;
using UnityEngine;
using Grid;
using Gameplay.StreetComponents;
using System.Linq;

namespace MeshGeneration
{
    public class MeshGenerator : MonoBehaviour
    {
        #region -SingeltonPattern-
        private static MeshGenerator _instance;
        public static MeshGenerator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<MeshGenerator>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("MeshGenerator");
                        _instance = container.AddComponent<MeshGenerator>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        public static MeshFilter CreateGridMesh(Street _s, MeshFilter _mf, MeshRenderer _mr)
        {
            List<Cell> cells = _s.m_StreetCells.Values.ToList();
            Vector3[] vertices = new Vector3[cells.Count * 4];
            Mesh m = new Mesh();
            int maxRow = _s.m_RowAmount;
            m.subMeshCount = maxRow * 2;
            for (int a = 0; a < cells.Count; a++)
            {
                Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.Inverse(cells[a].m_Orientation));
                for (int i = 0; i < 4; i++)
                {
                    vertices[i + a * 4] = cells[a].m_WorldCorner[i];
                }
            }
            m.SetVertices(vertices);

            Dictionary<int, List<int>> indiceDic = new Dictionary<int, List<int>>(); //subMeshIndex, indices

            for (int a = 0; a < cells.Count; a++)
            {
                int offset = 4 * a;
                int[] indices = new int[6];
                if (cells[a].m_isLeft)
                {
                    indices[0] = 0 + offset;
                    indices[1] = 3 + offset;
                    indices[2] = 1 + offset;

                    indices[3] = 0 + offset;
                    indices[4] = 2 + offset;
                    indices[5] = 3 + offset;

                    //031 023
                }
                else
                {
                    indices[0] = 0 + offset;
                    indices[1] = 3 + offset;
                    indices[2] = 2 + offset;

                    indices[3] = 0 + offset;
                    indices[4] = 1 + offset;
                    indices[5] = 3 + offset;

                    //032   013
                }

                int subMeshIndex = cells[a].Pos.y;
                if (cells[a].Pos.x < 0)
                    subMeshIndex += maxRow;

                if (!indiceDic.ContainsKey(subMeshIndex))
                    indiceDic[subMeshIndex] = new List<int>();

                indiceDic[subMeshIndex].AddRange(indices);
            }

            for (int i = 0; i < m.subMeshCount; i++) //foreach subMesh, set Indices
            {
                if (indiceDic.ContainsKey(i))
                    m.SetIndices(indiceDic[i], MeshTopology.Triangles, i);
            }
            _mf.sharedMesh = m;

            if (_mr.sharedMaterials.Length == 1)
            {
                //Create Material Default
                Material[] materials = new Material[m.subMeshCount];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = _mr.sharedMaterial;
                }
                _mr.materials = materials;
            }

            return _mf;
        }

        //Die Extrude Funktion wurde nach Freya Holmér gemacht. Aber vom Author an seine Situation angepasst
        //vgl. Holmér 2019 4:41:35 - 6:07:30
        public static void Extrude(SplineStreetComonents _comp)
        {
            ExtrudeShapeBase Shape = _comp.m_Shape;
            if (Shape == null)
                return;

            Spline Spline = _comp.m_Spline;
            if (Spline == null)
                return;

            int vertsInShape = Shape.verts.Length;
            int segments = Spline.OPs.Length - 1;
            int edgeLoops = Spline.OPs.Length;
            int vertCount = vertsInShape * edgeLoops;
            int triCount = Shape.lines.Length * segments;
            int triIndexCount = triCount * 3;

            int[] triangelIndices = new int[triIndexCount];
            Vector3[] verticies = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            float[] arr = new float[segments];
            CalcLengthTableInto(arr, Spline);
            float shapeLength = Shape.GetLineLength();

            for (int i = 0; i < Spline.OPs.Length; i++)
            {
                int offset = i * vertsInShape;
                for (int j = 0; j < vertsInShape; j++)
                {
                    int id = offset + j;
                    verticies[id] = Spline.OPs[i].LocalToWorld(Shape.verts[j]) - _comp.m_MeshOffset;
                    uvs[id] = new Vector2(Shape.us[j], arr.Sample(i / ((float)edgeLoops)) / shapeLength);
                }
            }

            int ti = 0;
            for (int i = 0; i < segments; i++)
            {
                int offset = i * vertsInShape;
                for (int j = 0; j < Shape.lines.Length; j += 2)
                {
                    int a = offset + Shape.lines[j] + vertsInShape;
                    int b = offset + Shape.lines[j];
                    int c = offset + Shape.lines[j + 1];
                    int d = offset + Shape.lines[j + 1] + vertsInShape;
                    triangelIndices[ti] = a;
                    ti++;
                    triangelIndices[ti] = b;
                    ti++;
                    triangelIndices[ti] = c;
                    ti++;
                    triangelIndices[ti] = c;
                    ti++;
                    triangelIndices[ti] = d;
                    ti++;
                    triangelIndices[ti] = a;
                    ti++;
                }
            }

            Mesh mesh = _comp.m_MeshFilter.mesh;
            mesh.Clear();
            mesh.vertices = verticies;
            mesh.triangles = triangelIndices;
            mesh.RecalculateNormals();
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            mesh.Optimize();
        }

        private static void CalcLengthTableInto(float[] arr, Spline _spline)
        {
            arr[0] = 0f;
            float totalLength = 0f;
            Vector3 prev = _spline.StartPos;
            for (int i = 1; i < arr.Length; i++)
            {
                float t = ((float)i) / (arr.Length - 1);
                Vector3 pt = _spline.GetPositionAt(t);
                float diff = (prev - pt).magnitude;
                totalLength += diff;
                arr[i] = totalLength;
                prev = pt;
            }
        }
    }

    #region -Shape Defenitions-
    public abstract class ExtrudeShapeBase
    {
        public float GetLineLength()
        {
            float output = 0;
            for (int i = 0; i < verts.Length - 1; i++)
            {
                output += Vector3.Distance(verts[i], verts[i + 1]);
            }
            return output;
        }

        public Vector2[] verts;

        public float[] us;

        public int[] lines;
    }

    public class DeadEndShape : ExtrudeShapeBase
    {
        public DeadEndShape()
        {
            verts = new Vector2[]
            {
            new Vector2(0.5f,0), //0
            new Vector2(0.5f,0.1f),
            new Vector2(0.5f,0.1f),
            new Vector2(0.25f,0.1f),
            new Vector2(0.25f,0.1f),
            new Vector2(0.25f,0.05f),
            new Vector2(0.25f,0.05f),
            new Vector2(-0.5f, 0.05f)
            };

            us = new float[]
            {
            0f,
            0.0001f,
            0.0001f,
            162f/1024f,
            162f/1024f,
            175f/1024f,
            175f/1024f,
            512f/1024f
            };

            lines = new int[]
            {
            0, 1,
            2, 3,
            4, 5,
            6, 7
            };
        }
    }

    public class StreetShape : ExtrudeShapeBase
    {
        public StreetShape()
        {
            verts = new Vector2[]
            {
            new Vector2(1,0), //0
            new Vector2(1,0.1f),
            new Vector2(1,0.1f),
            new Vector2(0.75f,0.1f),
            new Vector2(0.75f,0.1f),
            new Vector2(0.75f,0.05f),
            new Vector2(0.75f,0.05f),

            new Vector2(-0.75f,0.05f), //7
            new Vector2(-0.75f,0.05f),
            new Vector2(-0.75f,0.1f),
            new Vector2(-0.75f,0.1f),
            new Vector2(-1,0.1f),
            new Vector2(-1,0.1f),
            new Vector2(-1,0)
            };

            us = new float[]
            {
            0f,
            0.0001f,
            0.0001f,
            162f/1024f,
            162f/1024f,
            175f/1024f,
            175f/1024f,

            849f/1024f,
            849f/1024f,
            862f/1024f,
            862f/1024f,
            0.9999f,
            0.9999f,
            1
            };

            lines = new int[]
            {
            0, 1,
            2, 3,
            4, 5,
            6, 7,
            8, 9,
            10, 11,
            12, 13,
            };
        }
    }
    #endregion
}