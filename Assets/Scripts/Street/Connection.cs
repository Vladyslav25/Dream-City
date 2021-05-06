using Streets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connection
{
    public Street m_Street;
    public Vector3 m_Direction;
    public Transform m_Trans;

    public Connection() 
    {
    }

    public Connection(Vector3 _dir, Transform _trans, Street _street = null)
    {
        m_Street = _street;
        m_Direction = _dir;
        m_Trans = _trans;
    }

}
