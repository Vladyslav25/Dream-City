using Splines;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Streets;
using UnityEngine;

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

        public static void Extrude(Street _street)
        {
            ExtrudeShapeBase Shape = _street.m_Shape;
            Spline Spline = _street.m_Spline;

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
                    verticies[id] = Spline.OPs[i].LocalToWorld(Shape.verts[j]) - _street.m_MeshOffset;
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

            Mesh mesh = _street.m_MeshFilterRef.mesh;
            mesh.Clear();
            mesh.vertices = verticies;
            mesh.triangles = triangelIndices;
            mesh.RecalculateNormals();
            mesh.uv = uvs;
            mesh.RecalculateBounds();
            mesh.Optimize();
            _street.m_MeshCollider.sharedMesh = null;
            _street.m_MeshCollider.sharedMesh = mesh;
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
}