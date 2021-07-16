using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToolTip : MonoBehaviour
{
    #region -SingeltonPattern-
    private static ToolTip _instance;
    public static ToolTip Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ToolTip>();
                if (_instance == null)
                {
                    GameObject container = new GameObject("SplineManager");
                    _instance = container.AddComponent<ToolTip>();
                }
            }
            return _instance;
        }
    }
    #endregion

    [Header("Ref")]
    [SerializeField]
    private TextMeshProUGUI m_text;
    [SerializeField]
    private RectTransform m_backgroundTransform;

    [Header("Setting")]
    [SerializeField]
    Vector2 m_padSize;

    private RectTransform m_ParentRectTransform;

    private void Awake()
    {
        m_text.text = "";
        m_ParentRectTransform = transform.parent.GetComponent<RectTransform>();
    }
    private void Update()
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ParentRectTransform, Input.mousePosition, null, out localPoint);

        //right
        if (localPoint.x + m_backgroundTransform.rect.width > Screen.width * 0.5f)
            localPoint.x = Screen.width * 0.5f - m_backgroundTransform.rect.width;

        //top
        if (localPoint.y + m_backgroundTransform.rect.height > Screen.width * 0.5f)
            localPoint.y = Screen.width * 0.5f - m_backgroundTransform.rect.height;

        transform.localPosition = localPoint;
    }

    private void ShowToolTip_private(string _tooltipText)
    {
        if (string.IsNullOrEmpty(_tooltipText)) return;

        gameObject.SetActive(true);
        m_text.SetText(_tooltipText);
        m_text.ForceMeshUpdate();

        m_backgroundTransform.sizeDelta = m_text.GetRenderedValues() + m_padSize * 2;
    }

    private void HideToolTip_private()
    {
        gameObject.SetActive(false);

    }

    public static void ShowToolTip(string _inputText)
    {
        Instance.ShowToolTip_private(_inputText);
    }

    public static void HideToolTip()
    {
        Instance.HideToolTip_private();
    }
}
