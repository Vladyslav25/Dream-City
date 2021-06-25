using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Gameplay.Productions;
using static Gameplay.Productions.Condition;
using System;

[CustomPropertyDrawer(typeof(Condition))]
public class ConditionProperyDrawer : PropertyDrawer
{
    private bool initialized = false;

    private SerializedProperty P_Condition;
    private SerializedProperty P_ChoosenType;
    private SerializedProperty P_EValue_Inflow;
    private SerializedProperty P_EValue_Cell;
    private SerializedProperty P_EValue_Money;
    private SerializedProperty P_EValue_IBalance;
    private SerializedProperty P_EValue_IAmount;
    private SerializedProperty P_EValue_PAmount;
    private SerializedProperty P_Sign;
    private SerializedProperty P_ValueToCompare;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedObject SO = new SerializedObject(property.serializedObject.targetObject as Production);

        if (!initialized)
            Init(SO);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("Condition", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(P_ChoosenType);
        EditorGUILayout.Space(15);
        if (P_ChoosenType.enumValueIndex != (int)EValueType.NONE)
        {
            switch ((EValueType)P_ChoosenType.enumValueIndex)
            {
                case EValueType.NONE:
                    break;
                case EValueType.INFLOW:
                    EditorGUILayout.PropertyField(P_EValue_Inflow);
                    break;
                case EValueType.CELL_AMOUNT:
                    EditorGUILayout.PropertyField(P_EValue_Cell);
                    break;
                case EValueType.MONEY:
                    EditorGUILayout.PropertyField(P_EValue_Money);
                    break;
                case EValueType.INVENTORY_BALANCE:
                    EditorGUILayout.PropertyField(P_EValue_IBalance);
                    break;
                case EValueType.INVENTORY_AMOUNT:
                    EditorGUILayout.PropertyField(P_EValue_IAmount);
                    break;
                case EValueType.PRODUCTIONBUILDING_AMOUNT:
                    EditorGUILayout.PropertyField(P_EValue_PAmount);
                    break;
            }

            EditorGUILayout.PropertyField(P_Sign);
            EditorGUILayout.PropertyField(P_ValueToCompare);
        }

        if (EditorGUI.EndChangeCheck())
        {
            SO.ApplyModifiedProperties();
        }
    }

    private void Init(SerializedObject _serializedObject)
    {
        P_Condition = _serializedObject.FindProperty("m_Condition");
        P_ChoosenType = P_Condition.FindPropertyRelative("m_EValueType");
        P_EValue_Inflow = P_Condition.FindPropertyRelative("m_EValue_Inflow");
        P_EValue_Cell = P_Condition.FindPropertyRelative("m_EValue_Cell");
        P_EValue_Money = P_Condition.FindPropertyRelative("m_EValue_Money");
        P_EValue_IBalance = P_Condition.FindPropertyRelative("m_EValue_IBalance");
        P_EValue_IAmount = P_Condition.FindPropertyRelative("m_EValue_IAmount");
        P_EValue_PAmount = P_Condition.FindPropertyRelative("m_EValue_PAmount");
        P_Sign = P_Condition.FindPropertyRelative("m_Compare");
        P_ValueToCompare = P_Condition.FindPropertyRelative("m_Value");
    }
}
