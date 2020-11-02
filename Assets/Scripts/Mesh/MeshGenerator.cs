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
            int shapeAmount = _street.m_Shape.Length;
            _street.m_MeshFilterRef.mesh.subMeshCount = shapeAmount;
            Mesh mesh = _street.m_MeshFilterRef.mesh;
            Spline spline = _street.m_Spline;
            int segments = spline.OPs.Length - 1;

            int vertexAmount = 0; //Die Anzahl an Verts
            int indicesAmount = 0;  //Die Anzahl an Indices

            for (int k = 0; k < shapeAmount; k++)
            {
                vertexAmount += _street.m_Shape[k].verts.Length;
                indicesAmount += _street.m_Shape[k].lines.Length * segments * 3;
            }

            int[] vertCountMesh = new int[shapeAmount]; //Die Anzahl an Verts pro Mesh

            List<Vector3> verticies = new List<Vector3>(); //Alle Verts
            List<Vector2> uvs = new List<Vector2>(); //Alle UVs
            List<int>[] triangelIndices = new List<int>[shapeAmount]; //Alle Indices | Array -> MeshCount | List -> Triangel Data

            for (int k = 0; k < shapeAmount; k++)
            {
                ExtrudeShapeBase shape = _street.m_Shape[k];

                int vertsInShape = shape.verts.Length; //Anzahl der Verts im Shape
                int edgeLoops = spline.OPs.Length;
                int vertCountInShape = vertsInShape * edgeLoops; //Anzahl der Verts, welche im fertigen Mesh sein werden

                vertCountMesh[k] = vertCountInShape;    // Speichere die Anzahl im Array

                Vector3[] verticiesInShape = new Vector3[vertCountInShape]; //Die Verts im Shape
                Vector2[] uvsInShape = new Vector2[vertCountInShape];  //Die UVs im Shape

                float[] arr = new float[segments];
                CalcLengthTableInto(arr, spline);
                float shapeLength = shape.GetLineLength();

                for (int i = 0; i < spline.OPs.Length; i++)
                {
                    int offset = i * shape.verts.Length;
                    for (int j = 0; j < shape.verts.Length; j++)
                    {
                        int id = offset + j;
                        verticiesInShape[id] = spline.OPs[i].LocalToWorld(shape.verts[j]) - _street.m_MeshOffset;
                        uvsInShape[id] = new Vector2(arr.Sample(i / ((float)edgeLoops)) / shapeLength, shape.us[j]);
                    }
                }
                verticies.AddRange(verticiesInShape);
                uvs.AddRange(uvsInShape);
            }

            mesh.vertices = verticies.ToArray();
            mesh.uv = uvs.ToArray();
            //mesh.SetVertices(verticies);
            //mesh.SetUVs(0, uvs);

            for (int k = 0; k < shapeAmount; k++)
            {
                ExtrudeShapeBase shape = _street.m_Shape[k];

                int triCount = shape.lines.Length * segments * 3; // Amount of Indices in Mesh
                int[] triangelIndicesInShape = new int[triCount]; // Indices in Mesh

                int vertsInShape = shape.verts.Length; // Amount of Verts in Shape

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
                        triangelIndicesInShape[ti] = a;
                        ti++;
                        triangelIndicesInShape[ti] = b;
                        ti++;
                        triangelIndicesInShape[ti] = c;
                        ti++;
                        triangelIndicesInShape[ti] = c;
                        ti++;
                        triangelIndicesInShape[ti] = d;
                        ti++;
                        triangelIndicesInShape[ti] = a;
                        ti++;
                    }
                }
                triangelIndices[k] = new List<int>(triangelIndicesInShape);
            }
            for (int k = 0; k < shapeAmount; k++)
            {
                mesh.SetTriangles(triangelIndices[k], k);
            }
            mesh.RecalculateNormals();
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

    public class TestShapeOne : ExtrudeShapeBase
    {
        public TestShapeOne(Vector2[] _verts = null, float[] _us = null, int[] _lines = null)
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
            new Vector2(1,1),
            new Vector2(-1,1)
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

    public class TestShapeTwo : ExtrudeShapeBase
    {
        public TestShapeTwo(Vector2[] _verts = null, float[] _us = null, int[] _lines = null)
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
            new Vector2(1,5),
            new Vector2(-1,5)
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
