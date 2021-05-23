using Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public abstract class StreetComponent : MonoBehaviour
    {
        [SerializeField]
        private Connection m_StartConnection; //every StreetComponent have at least one Connection 

        [SerializeField]
        [MyReadOnly]
        private int id;
        public int ID
        {
            get
            {
                return id;
            }
            protected set
            {
                id = value;
            }
        }

        public MeshRenderer m_MeshRenderer;

        private bool m_isInvalid = true;

        //Can be Invalid if Street collide with something or have an invalid Form
        public bool m_IsInvalid
        {
            get
            {
                return m_isInvalid;
            }
            set
            {
                m_isInvalid = value;
                //Set the Color of the Preview Street
                if (value)
                {
                    StreetComponentManager.SetStreetColor(this, Color.green);
                }
                else
                {
                    StreetComponentManager.SetStreetColor(this, Color.red);
                }
            }
        }

        /// <summary>
        /// Give the Component a new ID if needed (Attention: Call it only for fineshed Streets, not Preview)
        /// </summary>
        /// <param name="_needID">Need an new ID?</param>
        public virtual void Init(bool _needID)
        {
            if (_needID)
                ID = StreetComponentManager.GetNewStreetComponentID();
        }

        public virtual Connection GetStartConnection()
        {
            return m_StartConnection;
        }

        public virtual void SetStartConnection(Connection _conn)
        {
            m_StartConnection = _conn;
        }
    }

    public class OrientedPoint
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float t;

        /// <summary>
        /// Create a new Oriented Point (Location and Rotation)
        /// </summary>
        /// <param name="_position">The Location of the Oriented Point</param>
        /// <param name="_rotation">The Rotation of the Oriented Point</param>
        /// <param name="_t">The t-Ratio on the Spline(0 <= t <= 1) | -1 if dont needed</param>
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
