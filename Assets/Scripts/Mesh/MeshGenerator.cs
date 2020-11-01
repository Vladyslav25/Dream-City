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

            for (int i = 0; i < _spline.OPs.Length; i++)
            {
                int offset = i * vertsInShape;
                for (int j = 0; j < vertsInShape; j++)
                {
                    int id = offset + j;
                    verticies[id] = _spline.OPs[i].LocalToWorld(_shape.verts[j]) - _meshOffset;
                    //normals[id] = _spline.OPs[i].LocalToWorldDirection(_shape.normals[j]);
                    //uvs[id] = new Vector2(_shape.us[j], i / ((float)edgeLoops));
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
            //_mesh.normals = normals;
            _mesh.RecalculateNormals();
            _mesh.uv = uvs;
            return _mesh;
        }
    }

    public class ExtrudeShape
    {
        public Vector2[] verts = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(.3f,0),
            new Vector2(.3f,0),
            new Vector2(.3f,0.5f)
        };

        public Vector2[] normals = new Vector2[]
        {
            new Vector2(0,1),
            new Vector2(.7f, .7f),
            new Vector2(0,2)
        };
        public float[] us = new float[]
        {
            0,
            .5f,
            1f
        };
        public int[] lines = new int[]
        {
            0, 1,
            2, 3
        };
    }
}
