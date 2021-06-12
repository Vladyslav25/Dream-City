﻿using Gameplay.Streets;
using Gameplay.Tools;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

namespace Gameplay.Tools
{
    /// <summary>
    /// State Mashine for the Player Tools
    /// </summary>
    public class ToolManager : MonoBehaviour
    {
        #region -SingeltonPattern-
        private static ToolManager _instance;
        public static ToolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<ToolManager>();
                    if (_instance == null)
                    {
                        GameObject container = new GameObject("MeshGenerator");
                        _instance = container.AddComponent<ToolManager>();
                    }
                }
                return _instance;
            }
        }
        #endregion

        [MyReadOnly]
        public Tool m_CurrendTool;

        public GameObject m_spherePrefab;

        private Dictionary<TOOLTYPE, Tool> m_dic_TypeTool = new Dictionary<TOOLTYPE, Tool>();

        public void Awake()
        {
            Tool[] tools = GetComponents<Tool>();
            foreach (Tool t in tools)
            {
                t.enabled = false;
                m_dic_TypeTool.Add(t.m_Type, t);
            }
            ChangeTool(TOOLTYPE.BUILDINGCLICK);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                ChangeTool(TOOLTYPE.STREET);
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                ChangeTool(TOOLTYPE.CROSS);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                ChangeTool(TOOLTYPE.ASSIGNMENT);
            }
            if(Input.GetKeyDown(KeyCode.T))
            {
                ChangeTool(TOOLTYPE.TEARDOWN);
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                ChangeTool(TOOLTYPE.BUILDINGCLICK);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.Instance.ResetHighlightButton();
                UIManager.Instance.SetActivToolChoose();
                ChangeTool(TOOLTYPE.BUILDINGCLICK);
            }
        }

        public StreetTool GetStreetTool()
        {
            return m_dic_TypeTool[TOOLTYPE.STREET] as StreetTool;
        }

        public CellAssignmentTool GetCellAssignmentTool()
        {
            return m_dic_TypeTool[TOOLTYPE.ASSIGNMENT] as CellAssignmentTool;
        }

        public CrossTool GetCrossTool()
        {
            return m_dic_TypeTool[TOOLTYPE.CROSS] as CrossTool;

        }

        public void ChangeTool(TOOLTYPE _tool)
        {


            if (m_CurrendTool != null)
            {
                m_CurrendTool.ToolEnd();
                m_CurrendTool.enabled = false;
            }
            if (m_dic_TypeTool.ContainsKey(_tool))
                m_CurrendTool = m_dic_TypeTool[_tool];
            else
            {
                m_CurrendTool = null;
                return;
            }
            m_CurrendTool.enabled = true;
            m_CurrendTool.ToolStart();

        }
    }
}
