using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UI_Panel : UI_Element
{
    /*
     * GetElement by Michael Jordan
     * Description:
     *  A Generic function used to find a UI_Element within this container.
     *
     * Generic: 
     *   T - "typeof the UI_Element you are trying to find". Must me a child of the UI_Element class.
     * Param:
     *  name - "name of the UI_Element within the heirarchy". By default it is blank.
     *
     * Return: 
     *  T - the element or null if it can not be found.
     */
    public T GetElement<T>(string name = "") where T : UI_Element
    {
        foreach (var element in GetComponentsInChildren<UI_Element>())
        {
            if (element == this)
                continue;

            T item = element as T;
            if (item != null && (item.name == name || name == ""))
            {
                return item;
            }

            //Check inside panel
            UI_Panel panel = element as UI_Panel;
            if (panel != null)
            {
                T subItem = panel.GetElement<T>(name);
                if (subItem != null)
                    return subItem;
            }
        }
        return null;
    }

    #region Parent override functions
    public override bool IsContainingVector(Vector2 _pos)
    {
        foreach (var element in GetComponentsInChildren<UI_Element>())
        {
            if (element == this)
                continue;

            if(element.IsContainingVector(_pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void OnMouseDownEvent()
    {
        foreach (var element in GetComponentsInChildren<UI_Element>())
        {
            if (element == this)
                continue;

            element.OnMouseDownEvent();
        }
    }

    public override void OnMouseUpEvent()
    {
        foreach (var element in GetComponentsInChildren<UI_Element>())
        {
            if (element == this)
                continue;

            element.OnMouseUpEvent();
        }
    }
    #endregion
}
