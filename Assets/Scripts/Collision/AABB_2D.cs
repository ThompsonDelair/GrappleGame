using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AABB_2D
{
    float x;
    float y;
    float width;
    float height;

    public AABB_2D(Vector2 pointA, Vector4 pointB) {
        x = (pointA.x < pointB.x) ? pointA.x : pointB.x;
        y = (pointA.y < pointB.y) ? pointA.y : pointB.y;
        width = Mathf.Abs(pointA.x - pointB.x);
        height = Mathf.Abs(pointA.y - pointB.y);
    }

    public AABB_2D(Actor a) {
        x = a.position2D.x - a.collider.Radius;
        y = a.position2D.y - a.collider.Radius;
        width = a.collider.Radius * 2;
        height = width;
    }

    public bool OverlapCheck(AABB_2D other) {
        return x < other.x + other.width &&
            x + width > other.x &&
            y < other.y + other.height &&
            y + height > other.y;
    }

    public void UpdateActorPos(Actor a) {
        x = a.position2D.x - a.collider.Radius;
        y = a.position2D.y - a.collider.Radius;
    }

    public bool ContainsPoint(Vector2 point) {
        return point.x >= x && point.x <= x + width && point.y >= y && point.y <= y + height;
    }

    public void DrawBox(Color c) {
        Vector3 start = new Vector3(x,0,y);
        Vector3 w = new Vector3(width,0,0);
        Vector3 h = new Vector3(0,0,height);
        Debug.DrawLine(start,start + w,c);
        Debug.DrawLine(start,start + h,c);
        Debug.DrawLine(start + w,start + w + h,c);
        Debug.DrawLine(start + h,start + w + h,c);
    }
}
