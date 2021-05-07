using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public abstract class StreetComponent : MonoBehaviour
    {
        public Connection m_StartConnection;

        public int ID { get; protected set; }

        public virtual void Init(bool _needID)
        {
            if (_needID)
                ID = StreetComponentManager.GetNewStreetComponentID();
        }
    }

    public struct OrientedPoint
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float t;

        public OrientedPoint(Vector3 _position, Quaternion _rotation, float _t = -1)
        {
            Position = _position;
            Rotation = _rotation;
            t = _t;
        }

        /// <summary>
        /// Calculate from Local Position to World Position
        /// </summary>
        /// <param name="_point">Local Position</param>
        /// <returns></returns>
        public Vector3 LocalToWorld(Vector3 _point)
        {
            return Position + Rotation * _point;
        }

        /// <summary>
        /// Calculate from World Position to Local Position
        /// </summary>
        /// <param name="_point">World Position</param>
        /// <returns></returns>
        public Vector3 WorldToLocal(Vector3 _point)
        {
            return Quaternion.Inverse(Rotation) * (_point - Position);
        }

        /// <summary>
        /// Calculate from Local Direction to World Direction
        /// </summary>
        /// <param name="_dir">Local Direction</param>
        /// <returns></returns>
        public Vector3 LocalToWorldDirection(Vector3 _dir)
        {
            return Rotation * _dir;
        }
    }
}
