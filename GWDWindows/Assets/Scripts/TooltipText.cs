using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipText : MonoBehaviour
{
    public string nameOfObject;
    public string descriptionOfObject;

    public void SetObjName(string name)
    {
        nameOfObject = name;
    }

    public void SetDescriptionName(string description)
    {
        descriptionOfObject = description;
    }

    public string GetObjName()
    {
        return nameOfObject;
    }

    public string GetObjDescription()
    {
        return descriptionOfObject;
    }
}
