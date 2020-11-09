using MeshGeneration;
using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Street : MonoBehaviour
{
    [Header("Gizmo Settings")]
    [SerializeField]
    [Tooltip("The Amount of Segments to Draw")]
    [Range(1, 256)]
    private int segments = 15;
    [SerializeField]
    private bool drawMesh = true;
    [SerializeField]
    [Tooltip("Draw the Curve?")]
    private bool drawLine = false;
    [SerializeField]
    [Tooltip("Draw the Tangent?")]
    private bool drawTangent = false;
    [SerializeField]
    [Tooltip("Draw the Normal Up?")]
    private bool drawNormalUp = false;
    [SerializeField]
    [Tooltip("Draw the Normal?")]
    private bool drawNormal = false;
    [SerializeField]
    [Tooltip("Tangent Lenght")]
    private float tangentLenght = 1;
    [SerializeField]
    [Tooltip("Normal Up Lenght")]
    private float normalLenghtUp = 1;
    [SerializeField]
    [Tooltip("Normal Lenght")]
    private float normalLenght = 1;

    public Spline m_Spline;
    public MeshFilter m_MeshFilterRef;
    public ExtrudeShapeBase[] m_Shape;
    public Vector3 m_MeshOffset
    {
        get
        {
            return transform.position;
        }
    }

    [MyReadOnly]
    [SerializeField]
    private int id = -1;

    public int ID
    {
        get
        {
            if (id > 0) return id;
            else return -1;
        }
    }

    public Street Init(GameObject _startPos, GameObject _tangent, GameObject _endPos, int _segments, MeshFilter _meshFilter, params ExtrudeShapeBase[] _shape)
    {
        id = StreetManager.GetNewSplineID();
        m_Spline = new Spline(_startPos, _tangent, _endPos, _segments);
        m_MeshFilterRef = _meshFilter;
        m_Shape = _shape;
        MeshGenerator.Extrude(this);
        return this;
    }

    private void Update()
    {
        if (m_Spline.OPs.Length != segments + 1)
            m_Spline.OPs = new OrientedPoint[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float t = 1.0f / segments * i;
            m_Spline.OPs[i] = new OrientedPoint(m_Spline.GetPositionAt(t), m_Spline.GetOrientationUp(t), t);
        }
        if (drawMesh)
            MeshGenerator.Extrude(this);
        else
            m_MeshFilterRef.mesh.Clear();

        Debug.Log("TangentPos: " + m_Spline.TangentPos);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < m_Spline.OPs.Length; i++)
        {
            float t = 1.0f / segments * i;
            float tnext = 1.0f / segments * (i + 1);

            if (!(i + 1 >= m_Spline.OPs.Length))
            {
                if (drawLine)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i + 1].Position);
                }
            }
            if (drawTangent)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i].Position + m_Spline.GetTangentAt(t) * tangentLenght);
            }
            if (drawNormalUp)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i].Position + m_Spline.GetNormalUpAt(t) * normalLenghtUp);
            }
            if (drawNormal)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(m_Spline.OPs[i].Position, m_Spline.OPs[i].Position + m_Spline.GetNormalAt(t) * normalLenght);
            }
        }
    }
}
