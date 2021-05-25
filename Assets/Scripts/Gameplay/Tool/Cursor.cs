using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    public static GameObject m_Cursor;
    public GameObject CursorPrefab;
    private static MeshRenderer m_cursorRenderer;
    void Awake()
    {
        m_Cursor = Instantiate(CursorPrefab);
        m_cursorRenderer = m_Cursor.GetComponent<MeshRenderer>();
    }

    public static void SetPosition(Vector3 _pos)
    {
        m_Cursor.transform.position = _pos;
    }

    public static void SetColor(Color _c)
    {
        m_cursorRenderer.material.color = _c;
    }

    public static void SetActiv(bool _active)
    {
        m_cursorRenderer.enabled = _active;
    }
}
