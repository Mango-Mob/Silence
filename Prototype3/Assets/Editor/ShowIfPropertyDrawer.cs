using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //get the attribute data
        ShowIfAttribute condHAtt = attribute as ShowIfAttribute;
        //check if the propery we want to draw should be enabled
        bool enabled = GetShowResult(condHAtt, property);

        //Enable/disable the property
        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;

        //Check if we should draw the property
        if (condHAtt.readOnlyInInspector || enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        //Ensure that the next property that is being drawn uses the correct settings
        GUI.enabled = wasEnabled;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute condHAtt = attribute as ShowIfAttribute;
        bool enabled = GetShowResult(condHAtt, property);

        if (condHAtt.readOnlyInInspector || enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetShowResult(ShowIfAttribute condHAtt, SerializedProperty property)
    {
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, condHAtt.comparedPropertyName);

        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue == null)
        {
            Debug.LogWarning("Attempting to use a ShowIfAttribute but no matching value found in object: " + condHAtt.comparedPropertyName);
            return false; //BREAK
        }
        object currentValue = GetPropertyAsObject(sourcePropertyValue);
        if (currentValue.GetType() != condHAtt.targetValue.GetType())
        {
            Debug.LogWarning($"Attempting to use a ShowIfAttribute but the value provided: {condHAtt.targetValue.GetType().Name} doesn't match the type value found in: " + condHAtt.comparedPropertyName);
            return false; //BREAK
        }

        return HandleConditionCheck(condHAtt.condition, condHAtt.targetValue.GetType(), currentValue, condHAtt.targetValue);
    }

    private object GetPropertyAsObject(SerializedProperty propertyField)
    {
        var targetObject = propertyField.serializedObject.targetObject;
        var targetType = targetObject.GetType();
        var field = targetType.GetField(propertyField.propertyPath);
        if (field != null)
        {
            var value = field.GetValue(targetObject);
            return value;
        }
        return null;
    }

    private bool HandleConditionCheck(ShowIfCond condition, System.Type type, object a, object b)
    {
        switch (type.Name)
        {
            case "Boolean":
                switch (condition)
                {
                    case ShowIfCond.Equal: return a.Equals(b);
                    case ShowIfCond.NotEqual: return !a.Equals(b);
                    default:
                        Debug.LogWarning($"Please use Equal or NotEqual for boolean checks.");
                        break;
                }
                break;
            case "Single":
                switch (condition)
                {
                    case ShowIfCond.Equal: return a.Equals(b);
                    case ShowIfCond.NotEqual: return !a.Equals(b);
                    case ShowIfCond.MoreThan: return (float)a > (float)b;
                    case ShowIfCond.LessThan: return (float)a < (float)b;
                    case ShowIfCond.MoreThanOrEqual: return (float)a >= (float)b;
                    case ShowIfCond.LessThanOrEqual: return (float)a <= (float)b;
                    default:
                        break;
                }
                break;
            case "Int16":
            case "Int32":
                switch (condition)
                {
                    case ShowIfCond.Equal: return a.Equals(b);
                    case ShowIfCond.NotEqual: return !a.Equals(b);
                    case ShowIfCond.MoreThan: return (int)a > (int)b;
                    case ShowIfCond.LessThan: return (int)a < (int)b;
                    case ShowIfCond.MoreThanOrEqual: return (int)a >= (int)b;
                    case ShowIfCond.LessThanOrEqual: return (int)a <= (int)b;
                    default:
                        break;
                }
                break;
            default:
                Debug.LogWarning($"Type not supported with ShowIfAttribute: {type.Name}.");
                return false;
        }
        return false;
    }
}