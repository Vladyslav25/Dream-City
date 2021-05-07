using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.StreetComponents
{
    public class Connection
    {
        public StreetComponent[] m_Component = new StreetComponent[2];
        public bool[] m_isStarts = new bool[2];
        public OrientedPoint m_OP;

        public Connection(StreetComponent _comp1, bool _isStart1, StreetComponent _comp2, bool _isStart2, OrientedPoint _op)
        {
            m_Component[0] = _comp1;
            m_Component[1] = _comp2;
            m_isStarts[0] = _isStart1;
            m_isStarts[1] = _isStart2;
            m_OP = _op;
        }

        public Connection(StreetComponent _comp1, bool _isStart1, StreetComponent _comp2, bool _isStart2, Vector3 _pos, Quaternion _rot, float _t = -1)
        {
            m_Component[0] = _comp1;
            m_Component[1] = _comp2;
            m_isStarts[0] = _isStart1;
            m_isStarts[1] = _isStart2;
            m_OP = new OrientedPoint(_pos, _rot, _t);
        }

        public StreetComponent GetOtherComponent(StreetComponent _comp)
        {
            if (m_Component[0] == _comp) return m_Component[1];
            else if (m_Component[1] == _comp) return m_Component[0];
            else return null;
        }

        public bool GetOtheIsStart(StreetComponent _comp)
        {
            if (m_Component[0] == _comp) return m_isStarts[1];
            else if (m_Component[1] == _comp) return m_isStarts[0];
            
            return false;
        }
    }
}
