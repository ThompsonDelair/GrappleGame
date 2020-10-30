using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KDNode2 {
    public bool xAxis;
    public Vector2 val;
    public float val2;
    List<Vector2> list;
    public KDNode2 left;
    public KDNode2 right;

    private KDNode2() { }

    public static KDNode2 BalancedConstructor(List<Vector2> points,int level, List<List<Vector2>> list, bool axis = true) {

        KDNode2 node = new KDNode2();
        node.xAxis = axis;

        if(level == 0) {
            list.Add(points);
            //Color c = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f));
            //foreach (Vector2 p in points) {
            //    Utilities.debugStarPoint(p,0.2f,c);
            //}
            //Debug.Log(points.Count);
            return node;
        }
        level--;

        if (axis) {
            points.Sort(node.CompareXAxis);
        } else {
            points.Sort(node.CompareYAxis);
        }

        int index = points.Count / 2;

        node.val2 = axis ? points[index].x : points[index].y;

        node.left = BalancedConstructor(points.GetRange(0,index),level,list,!axis);
        node.right = BalancedConstructor(points.GetRange(index,points.Count - index),level,list,!axis);

        return node;
    }

    public KDNode2(List<Vector2> points,int level,bool axis = true) {
        xAxis = axis;
        if (level == 0) {
            list = points;
            Color c = new Color(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f));
            foreach(Vector2 p in points) {
                Utils.debugStarPoint(p,0.2f,c);
            }
            Debug.Log(points.Count);
            return;
        }
        level--;

        int medianIndex = Calc.MyMedianFind(points,0,points.Count - 1,Compare);
        //val = points[medianIndex];
        try {
            val2 = axis ? points[medianIndex].x : points[medianIndex].y;
        } catch {
            ;
        }
        
        left = new KDNode2(points.GetRange(0,medianIndex+1),level,!axis);
        right = new KDNode2(points.GetRange(medianIndex+1,points.Count - medianIndex - 1),level,!axis);
    }

    class SearchInfo {
        public Vector2 bestPos;
        public float bestDist;
    }

    public Vector2 ClosestPoint(Vector2 pos) {
        SearchInfo info = new SearchInfo();
        info.bestDist = Mathf.Infinity;
        ClosestPointSearch(this,pos,ref info);
        return info.bestPos;
    }

    static void ClosestPointSearch(KDNode2 root,Vector2 pos,ref SearchInfo info) {

        if (root == null) {
            return;
        }

        float dist = Vector2.Distance(pos,root.val);
        if (dist <= info.bestDist) {
            info.bestDist = dist;
            info.bestPos = root.val;
        }

        int compare = root.Compare(pos,root.val);

        if (compare == 0)
            return;

        ClosestPointSearch((compare < 0) ? root.left : root.right,pos,ref info);
        if (root.ThresholdCheck(info.bestDist,pos))
            return;
        ClosestPointSearch((compare > 0) ? root.left : root.right,pos,ref info);
    }

    public bool ThresholdCheck(float bestDist,Vector2 pos) {
        if (xAxis) {
            return Mathf.Abs(pos.x - val.x) >= bestDist;
        } else {
            return Mathf.Abs(pos.y - val.y) >= bestDist;
        }
    }

    public int Compare(Vector2 a,Vector2 b) {
        if (xAxis)
            return CompareXAxis(a,b);
        else
            return CompareYAxis(a,b);
    }

    public int CompareXAxis(Vector2 a,Vector2 b) {
        if (a.x != b.x) {
            return a.x.CompareTo(b.x);
        }
        return a.y.CompareTo(b.y);

    }

    public int CompareYAxis(Vector2 a,Vector2 b) {
        if (a.y != b.y) {
            return a.y.CompareTo(b.y);
        }
        return a.x.CompareTo(b.x);
    }

    public int Pivot(List<Vector2> list,int left,int right) {
        if (right - left < 5) {
            return Partition5(list,left,right);
        }
        for (int i = left; i < right; i += 5) {
            int subRight = i + 4;
            if (subRight > right) {
                subRight = right;
            }
            int median5 = Partition5(list,i,subRight);
            Utils.ListSwap(list,median5,left + ((i - left) / 5));
            //Vector2 temp = list[median5];
            //list[median5] = list[left + ((i - left) / 5)];
        }
        int mid = (right - left) / 10 + left;
        //int pivotIndex = Pivot(list,left,right);
        //pivotIndex = Partition(list,left,right,pivotIndex,n);
        return Select(list,left,left + ((right - left) / 5),mid);
    }

    public int Partition5(List<Vector2> list,int left,int right) {
        int i = left + 1;
        int counter = 0;
        while (i <= right) {
            int j = i;
            int counter2 = 0;
            while (j > left && CompareVector2(list[j - 1],list[j])) {
                Vector2 temp = list[j - 1];
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

    public bool CompareVector2(Vector2 a,Vector2 b) {
        if (a.x != b.x) {
            return a.x > b.x;
        } else if (a.y != b.y) {
            return a.y > b.y;
        }
        return false;
    }

    int Select(List<Vector2> list,int left,int right,int n) {
        if (left == right)
            return left;
        int pivotIndex = Pivot(list,left,right);
        pivotIndex = Partition(list,left,right,pivotIndex,n);
        if (n == pivotIndex) {
            return n;
        } else if (n < pivotIndex) {
            right = pivotIndex - 1;
        } else {
            left = pivotIndex + 1;
        }
        return Select(list,left,right,n);
    }

    int Partition(List<Vector2> list,int left,int right,int pivotIndex,int n) {
        Vector2 pivotValue = list[pivotIndex];
        Utils.ListSwap(list,pivotIndex,right); // move the pivot to the end of the list
        int storeIndex = left;

        // move all elements smaller than the pivot to the left of the pivot
        for (int i = left; i < right; i++) {
            if (Compare(list[i],pivotValue) < 0) {
                Utils.ListSwap(list,storeIndex,i);
                storeIndex++;
            }
        }

        // move all elements equal to the pivot right after the smaller elements
        // shouldn't really need to do this??
        //int storeIndexEq = storeIndex;
        //for(int i = storeIndex; i < right; i++) {
        //    if (Compare(list[i],pivotValue) == 0) {
        //        Utilities.ListSwap(list,storeIndexEq,i);
        //        storeIndexEq++;
        //    }
        //}

        Utils.ListSwap(list,right,storeIndex);    // move the pivot back to its spot
        return storeIndex;
    }


}
