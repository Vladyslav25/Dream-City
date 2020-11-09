using MeshGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Splines
{
    public class Spline
    {
        [Header("Spline Settings")]
        [MyReadOnly]
        [SerializeField]
        private GameObject[] pointsObj;

        [HideInInspector]
        public Vector3 StartPos { get { return pointsObj[0].transform.position; } private set { pointsObj[0].transform.position = value; } }

        [HideInInspector]
        public Vector3 TangentPos { get { return pointsObj[1].transform.position; } private set { pointsObj[1].transform.position = value; } }

        [HideInInspector]
        public Vector3 EndPos { get { return pointsObj[2].transform.position; } private set { pointsObj[2].transform.position = value; } }

        public OrientedPoint[] OPs;

        public int segments;

        public Spline(GameObject _startPos, GameObject _tangent, GameObject _endPos, int _segments)
        {
            pointsObj = new GameObject[3];
            pointsObj[0] = _startPos;
            pointsObj[1] = _tangent;
            pointsObj[2] = _endPos;
            segments = _segments;
            UpdateOPs();
        }

        public void SetStartPos(Vector3 _newPos)
        {
            if (_newPos == Vector3.zero) return;

            StartPos = _newPos;
            UpdateOPs();
        }

        public void SetTangentPos(Vector3 _newPos)
        {
            if (_newPos == Vector3.zero) return;

            TangentPos = _newPos;
            UpdateOPs();
        }

        public void SetEndPos(Vector3 _newPos)
        {
            if (_newPos == Vector3.zero) return;

            EndPos = _newPos;
            UpdateOPs();
        }

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
            return
                StartPos * (1 - 2 * _t + (float)Math.Pow(_t, 2)) +
                TangentPos * (2 * _t - 2 * (float)Math.Pow(_t, 2)) +
                EndPos * (float)Math.Pow(_t, 2);
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
            Vector3 tagent =
                StartPos * (_t - 1) +
                TangentPos * (1 - (2 * _t)) +
                EndPos * _t;

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

        private void UpdateOPs()
        {
            OPs = new OrientedPoint[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float t = 1.0f / segments * i;
                OPs[i] = new OrientedPoint(GetPositionAt(t), GetOrientationUp(t), t);
            }
        }
    }

    public class OrientedPoint
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float t;

        public OrientedPoint(Vector3 _position, Quaternion _rotation, float _t)
        {
            Position = _position;
            Rotation = _rotation;
            t = _t;
        }

        public Vector3 LocalToWorld(Vector3 _point)
        {
            return Position + Rotation * _point;
        }

        public Vector3 WorldToLocal(Vector3 _point)
        {
            return Quaternion.Inverse(Rotation) * (_point - Position);
        }

        public Vector3 LocalToWorldDirection(Vector3 _dir)
        {
            return Rotation * _dir;
        }
    }
}
