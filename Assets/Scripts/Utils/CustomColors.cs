using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Purpose:
///     Colors! :)
/// Contributors:
///     Thompson
/// </summary>
public static class CustomColors 
{
    public static Color orange = new Color32(255,118,0,255);
    public static Color purple = new Color32(180,50,252,255);
    public static Color green = new Color32(15,150,8,255);
    public static Color niceBlue = new Color32(0,135,255,255);
    public static Color darkGray = new Color32(80,80,80,255);
    public static Color darkRed = new Color32(80,0,0,255);

    public static Color RandomColor() {
        return new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f));
    }
}
