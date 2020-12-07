using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate float EaseDelegate(float t);

// A collection of ease in/out functions for animation
public static class Ease
{
    public static float InSine(float t) {
        return 1 - Mathf.Cos((t * Mathf.PI) / 2);
    }

    public static float OutSine(float t) {
        return Mathf.Sin((t * Mathf.PI) / 2);
    }

    public static float InCubic(float t) {
        return t * t * t;
    }

    public static float OutCubic(float t) {
        return 1 - Mathf.Pow(1 - t,3);
    }

    public static float InOutCubic(float t) {
        return t < 0.5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2,3) / 2;
    }

    public static float InOutQuart(float t) {
        if (t < 0.5f)
            return 8 * t * t * t * t;

        return 1 - Mathf.Pow(-2 * t + 2,4) / 2;
    }

    public static float OutExpo(float t) {
        return (t == 1) ? 1 : 1 - Mathf.Pow(2,-10 * t);
    }

    public static float InOutExpo(float t) {
        if (t == 0f)
            return 0f;

        if (t == 1f)
            return 1f;

        if (t < 0.5f)
            return Mathf.Pow(2,20 * t - 10) / 2;

        return (2 - Mathf.Pow(2,-20 * t + 10)) / 2;
    }
}
