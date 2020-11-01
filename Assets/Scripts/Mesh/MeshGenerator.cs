using Splines;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

        public static Mesh Extrude(Mesh _mesh, ExtrudeShape _shape, Spline _spline, Vector3 _meshOffset)
        {
            int vertsInShape = _shape.verts.Length;
            int segments = _spline.OPs.Length - 1;
            int edgeLoops = _spline.OPs.Length;
            int vertCount = vertsInShape * edgeLoops;
            int triCount = _shape.lines.Length * segments;
            int triIndexCount = triCount * 3;

            int[] triangelIndices = new int[triIndexCount];
            Vector3[] verticies = new Vector3[vertCount];
            Vector3[] normals = new Vector3[vertCount];
            Vector2[] uvs = new Vector2[vertCount];

            float[] arr = new float[segments];
            CalcLengthTableInto(arr, _spline);
            float shapeLength = _shape.GetLineLength();

            for (int i = 0; i < _spline.OPs.Length; i++)
            {
                int offset = i * vertsInShape;
                for (int j = 0; j < vertsInShape; j++)
                {
                    int id = offset + j;
                    verticies[id] = _spline.OPs[i].LocalToWorld(_shape.verts[j]) - _meshOffset;
                    uvs[id] = new Vector2(arr.Sample(i / ((float)edgeLoops)) / shapeLength, _shape.us[j]);
                }
            }

            int ti = 0;
            for (int i = 0; i < segments; i++)
            {
                int offset = i * vertsInShape;
                for (int j = 0; j < _shape.lines.Length; j += 2)
                {
                    int a = offset + _shape.lines[j] + vertsInShape;
                    int b = offset + _shape.lines[j];
                    int c = offset + _shape.lines[j + 1];
                    int d = offset + _shape.lines[j + 1] + vertsInShape;
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

            _mesh.Clear();
            _mesh.vertices = verticies;
            _mesh.triangles = triangelIndices;
            _mesh.RecalculateNormals();
            _mesh.uv = uvs;
            return _mesh;
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

    public class ExtrudeShape
    {
        public Vector2[] verts = new Vector2[]
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

        public float[] us = new float[]
        {
            0,
            0,
            0,
            0.25f,
            0.25f,
            0.25f,
            0.25f,

            0.75f,
            0.75f,
            0.75f,
            0.75f,
            1,
            1,
            1
        };

        public int[] lines = new int[]
        {
            0, 1,
            2, 3,
            4, 5,
            6, 7,
            8, 9,
            10, 11,
            12, 13,
        };

        public float GetLineLength()
        {
            float output = 0;
            for (int i = 0; i < verts.Length - 1; i++)
            {
                output += Vector3.Distance(verts[i], verts[i + 1]);
            }
            return output;
        }
    }
}
