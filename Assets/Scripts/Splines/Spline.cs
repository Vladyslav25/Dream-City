using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Splines
{
    public class Spline : MonoBehaviour
    {
        [Header("Gizmo Settings")]
        [SerializeField] [Tooltip("The Amount of Segments to Draw")] private int segments = 15;
        [SerializeField] [Tooltip("Draw the Curve?")] private bool drawLine = true;
        [SerializeField] [Tooltip("Draw the Tangent?")] private bool drawTangent;
        [SerializeField] [Tooltip("Draw the Normal Up?")] private bool drawNormalUp;
        [SerializeField] [Tooltip("Draw the Normal?")] private bool drawNormal;
        [SerializeField] [Tooltip("Tangent Lenght")] private float tangentLenght = 1;
        [SerializeField] [Tooltip("Normal Up Lenght")] private float normalLenghtUp = 1;
        [SerializeField] [Tooltip("Normal Lenght")] private float normalLenght = 1;
        [Header("Spline Settings")]
        [SerializeField] private Vector3[] points;

        [HideInInspector]
        public Vector3 StartPos { get { return points[0]; } }

        [HideInInspector]
        public Vector3 TangentPos { get { return points[1]; } }

        [HideInInspector]
        public Vector3 EndPos { get { return points[2]; } }

        [MyReadOnly] [SerializeField] private int id = -1;

        public int ID
        {
            get
            {
                if (id > 0) return id;
                else return -1;
            }
        }

        public void Init(Vector3 _startPos, Vector3 _tangent, Vector3 _endPos)
        {
            points = new Vector3[3];
            points[0] = _startPos;
            points[1] = _tangent;
            points[2] = _endPos;
            id = SplineManager.GetNewSplineID();
        }

        public Vector3 GetPositionAt(float _t)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetPosition in Spline ID: {ID}");
                return Vector3.zero;
            }
            return
                StartPos * (1 - 2 * _t + (float)Math.Pow(_t, 2)) +
                TangentPos * (2 * _t - 2 * (float)Math.Pow(_t, 2)) +
                EndPos * (float)Math.Pow(_t, 2);
        }

        public Vector3 GetTangentAt(float _t)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetTanget in Spline ID: {ID}");
                return Vector3.zero;
            }
            Vector3 tagent =
                StartPos * (_t - 1) +
                TangentPos * (1 - (2 * _t)) +
                EndPos * _t;

            return tagent.normalized;

        }

        public Vector3 GetNormalUpAt(float _t, Vector3 _up)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetNormalUp in Spline ID: {ID}");
                return Vector3.zero;
            }

            if (_up == Vector3.zero)
            {
                Debug.LogError($"Wrong UpVector in GetNormalUp in Spline ID: {ID}");
                return Vector3.zero;
            }

            Vector3 tng = GetTangentAt(_t);
            Vector3 binormal = Vector3.Cross(_up, tng).normalized;
            return Vector3.Cross(tng, binormal);
        }

        public Vector3 GetNormalAt(float _t)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetNormal in Spline ID: {ID}");
                return Vector3.zero;
            }

            Vector3 tng = GetTangentAt(_t);
            return new Vector3(-tng.z, 0f, tng.x);
        }

        private void OnDrawGizmos()
        {
            for (int i = 0; i <= segments; i++)
            {
                float t = 1.0f / segments * i;
                float tnext = 1.0f / segments * (i + 1);

                if (!(tnext > 1))
                {
                    if (drawLine)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawLine(GetPositionAt(t), GetPositionAt(tnext));
                    }
                }
                if (drawTangent)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(GetPositionAt(t), GetPositionAt(t) + GetTangentAt(t) * tangentLenght);
                }
                if (drawNormalUp)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(GetPositionAt(t), GetPositionAt(t) + GetNormalUpAt(t, Vector3.up) * normalLenghtUp);
                }
                if (drawNormal)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(GetPositionAt(t), GetPositionAt(t) + GetNormalAt(t) * normalLenght);
                }
            }
        }
    }
}
