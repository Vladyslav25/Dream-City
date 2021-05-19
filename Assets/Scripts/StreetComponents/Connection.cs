using System;

namespace Gameplay.StreetComponents
{
    [Serializable]
    public class Connection
    {
        public int id = -777; //ID for Debuging
        public Connection m_OtherConnection { get; private set; } //The other Connection to this

        public StreetComponent m_OtherComponent //The other Component with the Connection to this
        {
            get
            {
                if (m_OtherConnection != null)
                    return m_OtherConnection.m_Owner;
                return null;
            }
        }
        public bool m_OtherStart //Are we ref to the Start of the others Component? 
        {
            get
            {
                if (m_OtherConnection != null)
                    return m_OtherConnection.m_OwnerStart;
                return false;
            }
        }

        public StreetComponent m_Owner { get; private set; } //The StreetComponent of this Connection
        public bool m_OwnerStart { get; private set; } //Are we self at the Start of our Component

        public Connection(Connection _otherConn, StreetComponent _self, bool _ownStart)
        {
            if (_otherConn != null)
                Combine(this, _otherConn);
            m_Owner = _self;
            m_OwnerStart = _ownStart;
            if (m_OtherConnection != null)
                id = m_OtherComponent.ID;
        }

        /// <summary>
        /// Combine two Connecetions together
        /// </summary>
        /// <param name="_conn1"></param>
        /// <param name="_conn2"></param>
        public static void Combine(Connection _conn1, Connection _conn2)
        {
            _conn1.m_OtherConnection = _conn2;
            _conn2.m_OtherConnection = _conn1;
            
            if (_conn1.m_Owner != null)
                _conn1.id = _conn2.m_Owner.ID;
            else
                _conn1.id = -999;
            
            if (_conn2.m_Owner != null)
                _conn2.id = _conn1.m_Owner.ID;
            else
                _conn2.id = -999;

        }

        /// <summary>
        /// Decombine two Connections
        /// </summary>
        /// <param name="_conn1"></param>
        /// <param name="_conn2"></param>
        public static void DeCombine(Connection _conn1, Connection _conn2)
        {
            _conn2.m_OtherConnection = null;
            _conn2.id = -500;
            _conn1.m_OtherConnection = null;
            _conn1.id = -500;
        }
    }
}
