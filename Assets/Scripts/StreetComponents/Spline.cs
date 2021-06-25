using System.Collections.Generic;
using UnityEngine;
using Grid;
using Gameplay.StreetComponents;

namespace Splines
{
    public class Spline
    {
        [HideInInspector]
        public Vector3 StartPos { get; private set; }

        [HideInInspector]
        public Vector3 Tangent1Pos { get; private set; }

        [HideInInspector]
        public Vector3 Tangent2Pos { get; private set; }

        [HideInInspector]
        public Vector3 EndPos { get; private set; }

        public OrientedPoint[] OPs;

        public OrientedPoint[] GridOPs;

        public int segments;

        public StreetComponent m_Component;

        public Spline(Vector3 _startPos, Vector3 _tangent1Pos, Vector3 _tangent2Pos, Vector3 _endPos, int _segments, StreetComponent _comp)
        {
            StartPos = _startPos;
            Tangent1Pos = _tangent1Pos;
            Tangent2Pos = _tangent2Pos;
            EndPos = _endPos;
            segments = _segments;
            m_Component = _comp;
            UpdateOPs(_comp);
        }

        #region -Set Tangents, Start and End-
        public void SetStartPos(Vector3 _newPos)
        {
            if (_newPos == Vector3.zero) return;

            StartPos = _newPos;
        }

        public void SetTangent1Pos(Vector3 _newPos)
        {
            if (_newPos == Vector3.zero) return;

            Tangent1Pos = _newPos;
        }

        public void SetTangent2Pos(Vector3 _newPos)
        {
            if (_newPos == Vector3.zero) return;

            Tangent2Pos = _newPos;
        }

        public void SetEndPos(Vector3 _newPos)
        {
            if (_newPos == Vector3.zero) return;

            EndPos = _newPos;
        }
        #endregion

        /// <summary>
        /// Get the world position of the point on the spline depending of t
        /// </summary>
        /// <param name="_t">The t amount of the Lerp. (0 <= t <= 1)</param>
        /// <returns>The world position of the Point on the spline</returns>
        public Vector3 GetPositionAt(float _t)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetPosition in Spline");
                return Vector3.zero;
            }
            float omt = 1f - _t;
            float omt2 = omt * omt;
            float t2 = _t * _t;
            return
                StartPos * (omt2 * omt) +
                Tangent1Pos * (3f * omt2 * _t) +
                Tangent2Pos * (3f * omt * t2) +
                EndPos * (t2 * _t);
        }

        /// <summary>
        /// Get the tangent of the point on the spline depending of t
        /// </summary>
        /// <param name="_t">The t amount of the Lerp. (0 <= t <= 1)</param>
        /// <returns>The tangent of the point on the spline</returns>
        public Vector3 GetTangentAt(float _t)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetTanget in Spline ID");
                return Vector3.zero;
            }
            float omt = 1f - _t;
            float omt2 = omt * omt;
            float t2 = _t * _t;
            Vector3 tagent =
                StartPos * (-omt2) +
                Tangent1Pos * (3 * omt2 - 2 * omt) +
                Tangent2Pos * (-3 * t2 + 2 * _t) +
                EndPos * t2;

            return tagent.normalized;
        }

        /// <summary>
        /// Get the normal of the point on the spline depending of t
        /// </summary>
        /// <param name="_t">The t amount of the Lerp. (0 <= t <= 1)</param>
        /// <param name="_up">The world up Vector</param>
        /// <returns>The normal of the point on the spline (showing in y)</returns>
        public Vector3 GetNormalUpAt(float _t, Vector3 _up)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetNormalUp in Spline ID");
                return Vector3.zero;
            }

            if (_up == Vector3.zero)
            {
                Debug.LogError($"Wrong UpVector in GetNormalUp in Spline ID");
                return Vector3.zero;
            }

            Vector3 tng = GetTangentAt(_t);
            Vector3 binormal = Vector3.Cross(_up, tng).normalized;
            return Vector3.Cross(tng, binormal);
        }

        /// <summary>
        /// Get the normal of the point on the spline depending of t
        /// </summary>
        /// <param name="_t">The t amount of the Lerp. (0 <= t <= 1)</param>
        /// <returns>The normal of the point on the spline (showing in y)</returns>
        public Vector3 GetNormalUpAt(float _t)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetNormalUp in Spline ID");
                return Vector3.zero;
            }

            Vector3 tng = GetTangentAt(_t);
            Vector3 binormal = Vector3.Cross(Vector3.up, tng).normalized;
            return Vector3.Cross(tng, binormal);
        }

        /// <summary>
        /// Get the normal of the point on the spline depending of t
        /// </summary>
        /// <param name="_t">The t amount of the Lerp. (0 <= t <= 1)</param>
        /// <returns>The normal of the point on the Spline (showing in x,z)</returns>
        public Vector3 GetNormalAt(float _t)
        {
            if (_t < 0 || _t > 1)
            {
                Debug.LogError($"Wrong t in GetNormal in Spline ID");
                return Vector3.zero;
            }

            Vector3 tng = GetTangentAt(_t);
            return new Vector3(-tng.z, 0f, tng.x);
        }

        /// <summary>
        /// Get the Orientationof the point in the spline depending of t
        /// </summary>
        /// <param name="_t">The t amount of the Lerp. (0 <= t <= 1)</param>
        /// <returns>The Quarternion of the point on the spline</returns>
        public Quaternion GetOrientation(float _t)
        {
            return Quaternion.LookRotation(GetTangentAt(_t), GetNormalAt(_t));
        }

        /// <summary>
        /// Get the Orientationof the point in the spline depending of t
        /// </summary>
        /// <param name="_t">The t amount of the Lerp. (0 <= t <= 1)</param>
        /// <returns>The Quarternion of the point on the spline</returns>
        public Quaternion GetOrientationUp(float _t)
        {
            Vector3 tangent = GetTangentAt(_t);
            if (tangent == Vector3.zero) return new Quaternion();
            Vector3 normalUp = GetNormalUpAt(_t);
            if (normalUp == Vector3.zero) return new Quaternion();

            return Quaternion.LookRotation(tangent, normalUp);
        }

        public float GetLength()
        {
            float output = 0;
            for (int i = 0; i < OPs.Length - 1; i++)
            {
                output += Vector3.Distance(OPs[i].Position, OPs[i + 1].Position);
            }
            return output;
        }

        public OrientedPoint GetLastOrientedPoint()
        {
            return OPs[OPs.Length - 1];
        }

        public OrientedPoint GetFirstOrientedPoint()
        {
            return OPs[0];
        }

        public OrientedPoint GetCentredOrientedPoint()
        {
            return OPs[OPs.Length / 2];
        }

        /// <summary>
        /// Update the Oriented Points
        /// </summary>
        public void UpdateOPs(StreetComponent _comp = null)
        {
            OPs = new OrientedPoint[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = 1.0f / segments * i;
                OPs[i] = new OrientedPoint(GetPositionAt(t), GetOrientationUp(t), t);
            }
            if (_comp != null && _comp is Street)
            {
                Street s = (Street)_comp;
                if (s != null && s.ID > 0)
                {
                    s.ClearSegmentsCorner();
                    for (int i = 0; i < OPs.Length; i++)
                    {
                        s.AddSegmentsCorner(OPs[i].Position + GetNormalAt(OPs[i].t));
                        s.AddSegmentsCorner(OPs[i].Position - GetNormalAt(OPs[i].t));
                    }
                }
            }
        }

        public void CreateGridOPs()
        {
            List<OrientedPoint> tmp = new List<OrientedPoint>();
            tmp.Add(GetFirstOrientedPoint());

            float distanceToEnd = Vector3.Distance(StartPos, EndPos);
            float currT = 0;
            int iterrations = 0;
            Vector3 lastPos = StartPos;
            while (distanceToEnd > GridManager.Instance.CellSize && iterrations <= 1000)
            {
                iterrations += 1;
                currT = iterrations * 0.001f;
                Vector3 tmPos = GetPositionAt(currT);
                distanceToEnd = Vector3.Distance(tmPos, EndPos);
                float distance = Vector3.Distance(lastPos, tmPos);

                if (distance >= GridManager.Instance.CellSize)
                {
                    tmp.Add(new OrientedPoint(tmPos, GetOrientationUp(currT), currT));
                    lastPos = tmPos;
                }
            }
            GridOPs = tmp.ToArray();
        }
    }
}