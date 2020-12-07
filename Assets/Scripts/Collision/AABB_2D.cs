using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 2D axis aligned bounding box
// used for trimming collision detection
// if two AABB's aren't overlapping, then there's no chance their more complicated object inside are colliding, either
public struct AABB_2D
{
    public float X { get { return x; } }
    float x;
    public float Y { get { return y; } }
    float y;
    public float Width { get { return width; } }
    float width;
    public float Height { get { return height; } }
    float height;

    public Vector2 bottomLeft { get { return new Vector2(x,y); } }
    public Vector2 topRight { get { return new Vector2(x+width,y+height); } }

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

    public AABB_2D(OurCircleCollider c) {
        x = c.Position2D.x - c.Radius;
        y = c.Position2D.y - c.Radius;
        width = c.Radius * 2;
        height = width;
    }

    public AABB_2D(Vector2[] points) {
        Vector2 bottomLeft = Vector2.positiveInfinity;
        Vector2 topRight = Vector2.negativeInfinity;
        for(int i = 0; i < points.Length; i++) {
            Vector2 p = points[i];
            if (p.x < bottomLeft.x)
                bottomLeft.x = p.x;
            if (p.x > topRight.x)
                topRight.x = p.x;
            if (p.y < bottomLeft.y)
                bottomLeft.y = p.y;
            if (p.y > topRight.y)
                topRight.y = p.y;
        }
        x = bottomLeft.x;
        y = bottomLeft.y;
        width = Mathf.Abs(topRight.x - bottomLeft.x);
        height = Mathf.Abs(topRight.y - bottomLeft.y);
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

    public void Merge(AABB_2D other) {
        float minX = (x < other.x) ? x : other.x;
        float minY = (y < other.y) ? y : other.y;

        float maxX = (x + width > other.x + other.width) ? x + width : other.x + other.width;
        float maxY = (y + height > other.y + other.height) ? y + height : other.y + other.height;

        x = minX;
        y = minY;
        width = Mathf.Abs(maxX - minX);
        height = Mathf.Abs(maxY - minY);
    }
}
