using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Calc
{
    public static Vector2 CartesianToIso(Vector2 cartPos, float scale = 1f, float shift = 2) {
        Vector2 isoPos = new Vector2();
        isoPos.x = cartPos.x - cartPos.y;
        isoPos.x *= scale;
        isoPos.y = (cartPos.x + cartPos.y) / shift;
        isoPos.y *= scale;
        return isoPos;
    }

    public static Vector2 IsoToCartesian(Vector2 isoPos,float scale = 1f, float shift = 2) {
        Vector2 cartPos = new Vector2();
        cartPos.x = (isoPos.x + shift *  isoPos.y) / shift;
        cartPos.y = cartPos.x - isoPos.x;
        cartPos.x *= scale;
        cartPos.y *= scale;
        return cartPos;
    }
    
    // val == 0     colinear
    // val > 0      clockwise
    // val < 0      counter clockwise
    public static int ClockWiseCheck(Vector2 a,Vector2 b,Vector2 c) {

        float val = (b.y - a.y) * (c.x - b.x) -
                  (b.x - a.x) * (c.y - b.y);

        // points are colinear
        if (val == 0) {
            return 0;
        }
        // if val is positive, the order is clockwise
        return (val > 0) ? 1 : -1;
    }

    public static bool DoLinesIntersect(Vector2 p1,Vector2 q1,Vector2 p2,Vector2 q2) {
        int o1 = ClockWiseCheck(p1,q1,p2);
        int o2 = ClockWiseCheck(p1,q1,q2);
        int o3 = ClockWiseCheck(p2,q2,p1);
        int o4 = ClockWiseCheck(p2,q2,q1);

        if (o1 != o2 && o3 != o4)
            return true;

        // p1, q1, and p2 are colinear and p2 lies between p1q1
        if (o1 == 0 && onSegment(p1,q2,q1))
            return true;

        if (o2 == 0 && onSegment(p1,q2,q1))
            return true;

        if (o3 == 0 && onSegment(p2,p1,q2))
            return true;

        if (o4 == 0 && onSegment(p2,q1,q2))
            return true;

        return false;
    }

    public static bool LineIntersect(Vector2 a1,Vector2 a2,Vector2 b1,Vector2 b2,out Vector2 intersect) {

        Vector2 aa = a2 - a1;
        Vector2 bb = b2 - b1;

        if(aa == Vector2.zero || bb == Vector2.zero) {
            Debug.LogError("line intersect AA or BB is Vector2.zero?");
            intersect = Vector2.negativeInfinity;
            return false;
        }

        if (CrossProduct(aa,bb) == 0) {
            // lines are parallel
            if (CrossProduct(b1 - a1, aa) == 0) {
                // lines are colinear
                               
                float t0 = DotProduct((b1 - a1),aa / DotProduct(aa,aa));
                float t1 = t0 + DotProduct(bb,aa / DotProduct(aa,aa));

                if (DotProduct(bb,aa) < 0) {
                    if(t1 >= 0 && t0 <= 1) {
                        intersect = b2;
                        return true;
                    }
                } else {
                    if (t1 >= 0 && t0 <= 1) {
                        intersect = b1;
                        return true;
                    }
                }
            }
        } else {
            float t = CrossProduct(b1 - a1,aa / CrossProduct(aa,bb));
            float u = CrossProduct(a1 - b1,bb / CrossProduct(bb,aa));

            if (0 <= t && t <= 1 && 0 <= u && u <= 1) {
                intersect = b1 + bb * t;
                return true;
            }
        }

        intersect = Vector2.negativeInfinity;
        return false;
    }

    public static float CrossProduct(Vector2 a, Vector2 b) {
        return a.x * b.y - a.y * b.x;
    }

    public static bool DoesLineIntersect(Vector2 p1,Vector2 q1,Vector2 p2,Vector2 q2) {
        int o1 = ClockWiseCheck(p1,q1,p2);
        int o2 = ClockWiseCheck(p1,q1,q2);
        int o3 = ClockWiseCheck(p2,q2,p1);
        int o4 = ClockWiseCheck(p2,q2,q1);

        if (o1 != o2 && o3 != o4)
            return true;

        // p1, q1, and p2 are colinear and p2 lies between p1q1
        if (o1 == 0 && onSegment(p1,q2,q1))
            return true;

        if (o2 == 0 && onSegment(p1,q2,q1))
            return true;

        if (o3 == 0 && onSegment(p2,p1,q2))
            return true;

        if (o4 == 0 && onSegment(p2,q1,q2))
            return true;

        return false;
    }

    //public static bool LineInersectIgnoreVerts()

    public static bool onSegment(Vector2 p,Vector2 q,Vector2 r) {
        if (ClockWiseCheck(p,q,r) != 0) {
            return false;
        }

        if (q.x <= Mathf.Max(p.x,r.x) && q.x >= Mathf.Min(p.x,r.x) && q.y <= Mathf.Max(p.y,r.y) && q.y >= Mathf.Min(p.y,r.y)) {
            return true;
        }
        return false;
    }

    public static int MyMedianFind<T>(List<T> list,int left,int right,compare<T> cmp) {
        if (right - left < 5) {
            return Partition5(list,left,right,cmp);
        }
        for (int i = left; i < right; i += 5) {
            int subRight = i + 4;
            if (subRight > right) {
                subRight = right;
            }
            int median5 = Partition5(list,i,subRight,cmp);
            Utils.ListSwap(list,median5,left + ((i - left) / 5));
            //Vector2 temp = list[median5];
            //list[median5] = list[left + ((i - left) / 5)];
        }
        //int mid = (right - left) / 5 + left;
        int pivotIndex = MyMedianFind(list,left,(right - left) / 5,cmp);
        return pivotIndex = Partition(list,left,right,pivotIndex,cmp);
    }

    static int Partition5<T>(List<T> list,int left,int right,compare<T> cmp) {
        int i = left + 1;
        int counter = 0;
        while (i <= right) {
            int j = i;
            int counter2 = 0;
            while (j > left && cmp(list[j - 1],list[j]) > 0) {
                T temp = list[j - 1];
                list[j - 1] = list[j];
                list[j] = temp;
                j = j - 1;
                if (Utils.WhileCounterIncrementAndCheck(ref counter2))
                    break;
            }
            i = i + 1;
            if (Utils.WhileCounterIncrementAndCheck(ref counter))
                break;
        }

        return (left + right) / 2;
    }

    static int Partition<T>(List<T> list,int left,int right,int pivotIndex,compare<T> cmp) {
        T pivotValue = list[pivotIndex];
        Utils.ListSwap(list,pivotIndex,right); // move the pivot to the end of the list
        int storeIndex = left;

        // move all elements smaller than the pivot to the left of the pivot
        for (int i = left; i < right; i++) {
            if (cmp(list[i],pivotValue) < 0) {
                Utils.ListSwap(list,storeIndex,i);
                storeIndex++;
            }
        }

        // move all elements equal to the pivot right after the smaller elements
        // shouldn't really need to do this??
        //int storeIndexEq = storeIndex;
        //for(int i = storeIndex; i < right; i++) {
        //    if (Compare(list[i],pivotValue) == 0) {
        //        Utils.ListSwap(list,storeIndexEq,i);
        //        storeIndexEq++;
        //    }
        //}

        Utils.ListSwap(list,right,storeIndex);    // move the pivot back to its spot
        return storeIndex;
    }

    public delegate int compare<T>(T a,T b);

    public static float DotProduct(Vector2 a,Vector2 b) {
        return a.x * b.x + a.y * b.y;
    }

    public static float Float2MagSqrd(Vector2 f) {
        return f.x * f.x + f.y * f.y;
    }

    public static float Float2Magnitude(Vector2 f2) {
        return Mathf.Sqrt(f2.x * f2.x + f2.y * f2.y);
    }

    public static float DistancePointToLine(Vector2 circlePos,Vector2 lineA,Vector2 lineB) {

        return Vector2.Distance(circlePos,ClosestPointToLine(circlePos,lineA,lineB));
    }

    public static Vector2 ClosestPointToLine(Vector2 circlePos,Vector2 lineA,Vector2 lineB) {
        float ACx = circlePos.x - lineA.x;
        float ACy = circlePos.y - lineA.y;
        float ABx = lineB.x - lineA.x;
        float ABy = lineB.y - lineA.y;

        float dot = ACx * ABx + ACy * ABy;
        float AB_lenSqrd = ABx * ABx + ABy * ABy;
        float linePerpendicularPercent = -1;
        if (AB_lenSqrd != 0) //in case of 0 length line
            linePerpendicularPercent = dot / AB_lenSqrd;

        //float xx, yy;
        Vector2 closestPoint = new Vector2();

        // 0 means the perpendicular intersect is "before" point A on the line's slope 
        // 0-1 means the perp intersect is between point A and point B, on the line's slope
        // 1 means the perp intersect is "beyond" point B on the line's slope

        if (linePerpendicularPercent <= 0) {
            // the closest point to the line is point A
            closestPoint.x = lineA.x;
            closestPoint.y = lineA.y;
        } else if (linePerpendicularPercent >= 1) {
            // the closest point to the line is point B
            closestPoint.x = lineB.x;
            closestPoint.y = lineB.y;
        } else {
            // the closest point to the line is the perpendicular intersection point
            closestPoint.x = lineA.x + linePerpendicularPercent * ABx;
            closestPoint.y = lineA.y + linePerpendicularPercent * ABy;
        }

        return closestPoint;
    }

    public static bool PointInCountClockPolygon(Vector2 point, List<Vector2> poly) {
        for(int i = 0; i < poly.Count; i++) {
            if (ClockWiseCheck(poly[i],poly[(i + 1) % poly.Count],point) > 0){
                return false;
            }
        }
        return true;
    }

    public static bool PointInOrOnRect(Vector2 point,Vector2 min,Vector2 max) {
        return point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y;
    }

    public static Vector2[] CircularPointsAroundPosition(Vector2 pos,int numPoints,float radius) {
        Vector2[] points = new Vector2[numPoints];
        for (int i = 0; i < numPoints; i++) {
            float angle = i * Mathf.PI * 2f / numPoints;
            points[i] = new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)) * radius;
            //points[i].y *= yMod;
            points[i] += pos;
        }
        return points;
    }

    public static Vector2 Vector2FromTheta(float theta,ANGLE_TYPE type) {
        if (type == ANGLE_TYPE.DEGREES)
            theta *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(theta),Mathf.Sin(theta));
    }
}

public enum ANGLE_TYPE { DEGREES, RADIANS }
