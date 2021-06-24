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
    public EValueType choosenType;

    public EValue_Inflow EValue_Inflow;
    public EValue_Cell EValue_Cell;
    public EValue_Money EValue_Money;
    public EValue_IBalance EValue_IBalance;
    public EValue_IAmount EValue_IAmount;
    public EValue_PAmount EValue_PAmount;
    public EComparisonSign sign;

    public float valueToCompare;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.serializedObject.Update();

        choosenType = (EValueType)EditorGUILayout.EnumPopup("Value Type:", choosenType);
        if (choosenType != EValueType.NONE)
        {
            EditorGUILayout.Space(20);
            switch (choosenType)
            {
                case EValueType.NONE:
                    break;
                case EValueType.INFLOW:
                    EValue_Inflow = (EValue_Inflow)EditorGUILayout.EnumPopup(new GUIContent("Value"), EValue_Inflow);
                    break;
                case EValueType.CELL_AMOUNT:
                    EValue_Cell = (EValue_Cell)EditorGUILayout.EnumPopup(new GUIContent("Value"), EValue_Cell);
                    break;
                case EValueType.MONEY:
                    EValue_Money = (EValue_Money)EditorGUILayout.EnumPopup(new GUIContent("Value"), EValue_Money);
                    break;
                case EValueType.INVENTORY_BALANCE:
                    EValue_IBalance = (EValue_IBalance)EditorGUILayout.EnumPopup(new GUIContent("Value"), EValue_IBalance);
                    break;
                case EValueType.INVENTORY_AMOUNT:
                    EValue_IAmount = (EValue_IAmount)EditorGUILayout.EnumPopup(new GUIContent("Value"), EValue_IAmount);
                    break;
                case EValueType.PRODUCTIONBUILDING_AMOUNT:
                    EValue_PAmount = (EValue_PAmount)EditorGUILayout.EnumPopup(new GUIContent("Value"), EValue_PAmount);
                    break;
            }

            sign = (EComparisonSign)EditorGUILayout.EnumPopup("Sign", sign);
            valueToCompare = EditorGUILayout.FloatField("Compare to", valueToCompare);
        }
    }
}
