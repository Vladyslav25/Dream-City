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

        public static void Extrude(Street _street)
        {
            _street.m_MeshFilterRef.mesh.subMeshCount = _street.m_Shape.Length;
            Mesh mesh = _street.m_MeshFilterRef.mesh;
            Spline spline = _street.m_Spline;
            for (int k = 0; k < _street.m_Shape.Length; k++)
            {
                int vertsInShape = 0;
                int triCount = 0;
                int triIndexCount = 0;
                int segments = spline.OPs.Length - 1;
                ExtrudeShapeBase shape = _street.m_Shape[k];

                vertsInShape = shape.verts.Length;
                triCount = shape.lines.Length * segments;
                triIndexCount = triCount * 3;

                int edgeLoops = spline.OPs.Length;
                int vertCount = vertsInShape * edgeLoops;

                Vector3[] verticies = new Vector3[vertCount];
                Vector3[] normals = new Vector3[vertCount];
                Vector2[] uvs = new Vector2[vertCount];


                int[] triangelIndices = new int[triIndexCount];
                float[] arr = new float[segments];
                CalcLengthTableInto(arr, spline);
                float shapeLength = shape.GetLineLength();

                for (int i = 0; i < spline.OPs.Length; i++)
                {
                    int offset = i * shape.verts.Length;
                    for (int j = 0; j < shape.verts.Length; j++)
                    {
                        int id = offset + j;
                        verticies[id] = spline.OPs[i].LocalToWorld(shape.verts[j]) - _street.m_MeshOffset;
                        uvs[id] = new Vector2(arr.Sample(i / ((float)edgeLoops)) / shapeLength, shape.us[j]);
                    }
                }

                int ti = 0;
                for (int i = 0; i < segments; i++)
                {
                    int offset = i * vertsInShape;
                    for (int j = 0; j < shape.lines.Length; j += 2)
                    {
                        int a = offset + shape.lines[j] + vertsInShape;
                        int b = offset + shape.lines[j];
                        int c = offset + shape.lines[j + 1];
                        int d = offset + shape.lines[j + 1] + vertsInShape;
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

                mesh.SetVertices(verticies);
                mesh.SetTriangles(triangelIndices, k);
                mesh.RecalculateNormals();
                mesh.uv = uvs;
            }
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
        public Vector2[] verts;
        public float[] us;
        public int[] lines;

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

    public class StreetShapeComplete : ExtrudeShapeBase
    {
        public StreetShapeComplete(Vector2[] _verts = null, float[] _us = null, int[] _lines = null)
        {
            if (_verts != null)
                verts = _verts;
            else
                verts = vertsDefault;

            if (_us != null)
                us = _us;
            else
                us = usDefault;

            if (_lines != null)
                lines = _lines;
            else
                lines = linesDefault;
        }

        public Vector2[] vertsDefault = new Vector2[]
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

        public float[] usDefault = new float[]
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

        public int[] linesDefault = new int[]
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

    public class StreetShape : ExtrudeShapeBase
    {
        public StreetShape(Vector2[] _verts = null, float[] _us = null, int[] _lines = null)
        {
            if (_verts != null)
                verts = _verts;
            else
                verts = vertsDefault;

            if (_us != null)
                us = _us;
            else
                us = usDefault;

            if (_lines != null)
                lines = _lines;
            else
                lines = linesDefault;
        }

        public Vector2[] vertsDefault = new Vector2[]
        {
            new Vector2(0.75f,0.05f),
            new Vector2(-0.75f,0.05f)
        };

        public float[] usDefault = new float[]
        {
            0.25f,
            0.75f,
        };

        public int[] linesDefault = new int[]
        {
            0, 1,
        };
    }

    public class Sidewalk : ExtrudeShapeBase
    {
        public Sidewalk(Vector2[] _verts = null, float[] _us = null, int[] _lines = null)
        {
            if (_verts != null)
                verts = _verts;
            else
                verts = vertsDefault;

            if (_us != null)
                us = _us;
            else
                us = usDefault;

            if (_lines != null)
                lines = _lines;
            else
                lines = linesDefault;
        }

        public Vector2[] vertsDefault = new Vector2[]
        {
            new Vector2(1,0), //0
            new Vector2(1,0.1f),
            new Vector2(1,0.1f),
            new Vector2(0.75f,0.1f),
            new Vector2(0.75f,0.1f),
            new Vector2(0.75f,0.05f),

            new Vector2(-0.75f,0.05f), //6
            new Vector2(-0.75f,0.1f),
            new Vector2(-0.75f,0.1f),
            new Vector2(-1,0.1f),
            new Vector2(-1,0.1f),
            new Vector2(-1,0)
        };

        public float[] usDefault = new float[]
        {
            0,
            0,
            0,
            0.25f,
            0.25f,
            0.25f,

            0.75f,
            0.75f,
            0.75f,
            1,
            1,
            1
        };

        public int[] linesDefault = new int[]
        {
            0, 1,
            2, 3,
            4, 5,
            6, 7,
            8, 9,
            10, 11,
        };
    }
}
