using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding {



    public static List<Vector2> getPath(Vector2 start, Vector2 goal, float radius, Movement movement, Map map, PathfindVars vars = new PathfindVars()) {
        return getPath(
            start,
            goal,
            NavCalc.TriFromPoint(start,map.triKDMap,map.VertEdgeMap),
            NavCalc.TriFromPoint(goal,map.triKDMap,map.VertEdgeMap),
            radius,
            movement
            );
    }

    public static List<Vector2> getPath(MapChunked map,Vector2 start,Vector2 goal,float radius) {
        return getPath(start,goal,map.triFromPoint(start),map.triFromPoint(goal),radius,Movement.WALKING);
    }

    public static List<Vector2> getPath(OldMap map, Vector2 start, Vector2 goal,float radius) {
        return getPath(start,goal,map.triFromPoint(start),map.triFromPoint(goal),radius,Movement.WALKING);
    }

    public static List<Vector2> getPath(Vector2 start, Vector2 goal, float radius, KDNode root,Dictionary<Vector2,HalfEdge> vertEdgeMap) {
        return getPath(start,goal,NavCalc.TriFromPoint(start,root,vertEdgeMap),NavCalc.TriFromPoint(goal,root,vertEdgeMap),radius,Movement.WALKING);
    }

    public static List<Vector2> getPath(Vector2 start,Vector2 goal,NavTri startTri,NavTri goalTri,float radius, Movement m, HashSet<NavTri> pathTris = null,PathfindVars vars = new PathfindVars()) {
        if(startTri == null || goalTri == null) {
            return null;
        }
        List<NavTri> triPath = pathfind(start,goal,startTri,goalTri,m);
        if (triPath == null) {
            return null;
        }
        pathTris = new HashSet<NavTri>(triPath);

        List<Vector2> path = funnel(start,goal,triPath);
        if (path == null) {
            return null;
        }
        path = FunnelWithRadius(path,radius);

        return path;
    }

    public static List<NavTri> pathfind(Vector2 start,Vector2 goal,NavTri startTri,NavTri goalTri, Movement movement,PathfindVars vars = new PathfindVars()) {
        
        List<NavTri> path = new List<NavTri>();

        if (startTri == goalTri) {
            path.Add(startTri);
            return path;
        }

        bool pathfound = false;

        // the set of nodes already evaluated
        HashSet<NavTri> visited = new HashSet<NavTri>();

        // the set of currently discovered nodes that are not evaluated yet
        // initially only the start node is known
        HashSet<NavTri> frontier = new HashSet<NavTri>();
        frontier.Add(startTri);

        // stores where a node is most efficiently reached from in a single step
        Dictionary<NavTri,NavTri> cameFrom = new Dictionary<NavTri,NavTri>();

        // the mapped movement cost to get from start to a node
        Dictionary<NavTri,A_ScoreObj> actualScore = new Dictionary<NavTri,A_ScoreObj>();
        // the cost of getting from start to start is zero
        actualScore.Add(startTri,new A_ScoreObj(start,0));

        // fScore = the gScore (known) + raw distance to target (heuristic) cost of a node
        Dictionary<NavTri,float> heuristicScore = new Dictionary<NavTri,float>();
        // the gScore cost for start is 0, so its fScore is completely heuristic
        heuristicScore.Add(startTri,Vector2.Distance(start,goal));

        int counter = 0;

        while (frontier.Count > 0 &&  !pathfound) {
            // find the the node in frontier with the lowest fSore
            float lowScore = Mathf.Infinity;
            NavTri curr = null;

            // find the node in the frontier with the lowest fScore
            foreach (NavTri t in frontier) {
                float contenderScore = heuristicScore[t];
                if (contenderScore < lowScore) {
                    curr = t;
                    lowScore = contenderScore;
                }
            }

            if(vars.rangeLimit != 0 && lowScore > vars.rangeLimit) {
                return null;
            }

            // this is now our current now and must be processed
            frontier.Remove(curr);
            visited.Add(curr);

            // analyze each of current node's neighbours
            foreach (HalfEdge e in MapProcessing.TriEdges(curr)) {
                if (Utils.BitwiseOverlapCheck((int)e.moveBlock,(int)movement) || e.pair == null) {
                    continue;
                }

                NavTri n = e.pair.face;
                if (visited.Contains(n)) {
                    continue;
                }

                // if this neighbor has not discovered yet, give it a default gScore and add to frontier
                if (!frontier.Contains(n)) {
                    //float d = Mathf.Infinity;
                    A_ScoreObj newA_Score = new A_ScoreObj();
                    float best_Hscore = Mathf.Infinity;
                    foreach (HalfEdge eB in MapProcessing.TriEdges(n)) {
                        // use fscore as the contender score

                        float contender = Vector2.Distance(eB.start,goal) + actualScore[curr].score + Vector2.Distance(eB.start,actualScore[curr].pos);
                        if (contender < best_Hscore) {
                            newA_Score.pos.x = eB.start.x;
                            newA_Score.pos.y = eB.start.y;
                            best_Hscore = contender;
                        }
                        //Vector2 midpoint = new Vector2();
                        //midpoint.x = (f.start.x + f.next.start.x) / 2;
                        //midpoint.y = (f.start.y + f.next.start.y) / 2;
                        //contender = Vector2.Distance(midpoint,goal);
                        //if (contender < distance) {
                        //    g.pos.x = midpoint.x;
                        //    g.pos.y = midpoint.y;
                        //    distance = contender;
                        //}
                    }

                    newA_Score.score = actualScore[curr].score + Vector2.Distance(newA_Score.pos,actualScore[curr].pos);
                    actualScore.Add(n,newA_Score);

                    heuristicScore.Add(n,best_Hscore);
                    frontier.Add(n);
                    cameFrom.Add(n,curr);

                } else {
                    float contenderA_Score = actualScore[curr].score + Vector2.Distance(actualScore[curr].pos,actualScore[n].pos);

                    // if the contender gScore is lower than the neighbor's current gScore
                    // then we have found a better path to the neighbor
                    if (contenderA_Score <= actualScore[n].score) {
                        actualScore[n].score = contenderA_Score;
                        heuristicScore[n] = actualScore[curr].score + Vector2.Distance(actualScore[n].pos,actualScore[curr].pos)
                            + Vector2.Distance(actualScore[n].pos,goal);
                        cameFrom[n] = curr;
                    }
                }

                if (n == goalTri) {
                    pathfound = true;
                }
            }
            if (Utils.WhileCounterIncrementAndCheck(ref counter))
                break;
        }

        if (pathfound) {
            path.Add(goalTri);
            NavTri lastNode = goalTri;
            int counter2 = 0;
            while (cameFrom.ContainsKey(lastNode)) {
                path.Add(lastNode = cameFrom[lastNode]);
                if (Utils.WhileCounterIncrementAndCheck(ref counter2))
                    break;
            }
            path.Reverse();
            return path;

        } else {
            //Debug.LogError("no path found, tracker: "+tracker+" frontier.count : "+frontier.Count);
            return null;
        }

        //float gscore(NavTri tri, float prevScore, ) {
        //    float distance = Mathf.Infinity;
        //    float contender;
        //    foreach (HalfEdge e in MapProcessing.TriEdges(tri)) {
        //        if ((contender = Vector2.Distance(e.start,goal)) < distance) {
        //            distance = contender;
        //        }
        //    }
        //}

        //float OldA_Score(NavTri curr,HalfEdge n) {

        //    A_ScoreObj newA_Score = new A_ScoreObj();
        //    float best_Hscore = Mathf.Infinity;
        //    foreach (HalfEdge eB in MapProcessing.TriEdges(n)) {
        //        // use fscore as the contender score

        //        float contender = Vector2.Distance(eB.start,goal) + actualScore[curr].score + Vector2.Distance(eB.start,actualScore[curr].pos);
        //        if (contender < best_Hscore) {
        //            newA_Score.pos.x = eB.start.x;
        //            newA_Score.pos.y = eB.start.y;
        //            best_Hscore = contender;
        //        }
                
        //    }

        //    return actualScore[curr].score + Vector2.Distance(newA_Score.pos,actualScore[curr].pos);
        //    throw new NotImplementedException();

        //}

        //float NewA_Score(NavTri curr,HalfEdge n) {
        //    List<NavTri> list = new List<NavTri>();
        //    list.Add(n.face);
        //    list.Add(curr);
        //    NavTri lastNode = curr;
        //    int counter2 = 0;
        //    while (cameFrom.ContainsKey(lastNode)) {
        //        path.Add(lastNode = cameFrom[lastNode]);
        //        if (Utils.WhileCounterIncrementAndCheck(ref counter2))
        //            break;
        //    }
        //    throw new NotImplementedException();
        //}
    }



    class A_ScoreObj {
        public Vector2 pos;
        public float score;

        public A_ScoreObj() {
            pos = new Vector2();
            score = Mathf.Infinity;
        }

        public A_ScoreObj(Vector2 _pos,float _score) {
            pos = _pos;
            score = _score;
        }
    }

    public static List<Vector2> funnel(Vector2 start,Vector2 end,List<NavTri> triPath) {        

        List<Vector2> path = new List<Vector2>();
        path.Add(start);

        if (triPath.Count == 1) {
            path.Add(end);
            return path;
        }

        if (triPath.Count == 0)
        {
            Debug.LogError("trying to funnel zero length path");
            return null;
        }

        Vector2 apex = start;

        // make lists
        List<Vector2> leftList = new List<Vector2>();
        List<Vector2> rightList = new List<Vector2>();
        int leftIndex = 0;
        int rightIndex = 0;
        //List<HalfEdge> do we even really need a portals lists?

        for (int i = 0; i < triPath.Count - 1; i++) {
            foreach (HalfEdge e in MapProcessing.TriEdges(triPath[i])) {
                if (e.pair == null) {
                    continue;
                }

                if (e.pair.face == triPath[i + 1]) {
                    leftList.Add(e.start);
                    rightList.Add(e.next.start);
                }
            }
        }

        leftList.Add(end);
        rightList.Add(end);        

        for (int i = 1; i < triPath.Count; i++) {

            // check left
            if (leftIndex < i && leftList[i] != leftList[leftIndex] && Calc.ClockWiseCheck(apex,leftList[leftIndex],leftList[i]) != 1) {

                // check that it doesn't overlap with the other side
                if (Calc.ClockWiseCheck(apex,rightList[rightIndex],leftList[i]) != -1) {
                    leftIndex = i;
                } else {
                    apex = rightList[rightIndex];
                    if (path.Contains(apex)) {

                    }
                    

                    // NEED TO FIX THIS
                    // PROBLEMS CAUSED BY CLOCKWISECHECK ALWAYS RETURNING FALSE IF TWO POINTS ARE OVERLAPPING
                    // MEANS THAT WHEN apex == rightList[rightIndex] DUPLICATES ARE ADDED

                    if (!path.Contains(apex)) {
                        path.Add(apex);
                    }

                    
                    rightIndex++;
                    i = rightIndex + 1;
                    leftIndex = rightIndex;
                }
            }

            try
            {
                // check right
                if (rightIndex < i && rightList[i] != rightList[rightIndex] && Calc.ClockWiseCheck(apex, rightList[rightIndex], rightList[i]) != -1)
                {

                    // check that it doesn't overlap with the other side
                    if (Calc.ClockWiseCheck(apex, leftList[leftIndex], rightList[i]) != 1)
                    {
                        rightIndex = i;
                    }
                    else
                    {
                        apex = leftList[leftIndex];
                        if (path.Contains(apex))
                        {

                        }
                        path.Add(apex);
                        leftIndex++;
                        i = leftIndex + 1;
                        rightIndex = leftIndex;
                    }
                }
            }
            catch
            {
                //Debug.LogError("problem with funnel?");
                //Debug.LogError(e.ToString());
                //Utils.debugStarPoint(start, 1f, Color.red);
            }


        }
        path.Add(end);
        return path;
    }

    static List<Vector2> FunnelWithRadius(List<Vector2> original, float radius) {
        if(original.Count <= 2) {
            return original;
        }

        List<Vector2> newList = new List<Vector2>();
        newList.Add(original[0]);

        for(int i = 1; i < original.Count - 1; i++) {
            Vector2 a = original[i] - original[i - 1];
            Vector2 b = original[i + 1] - original[i];
            //Vector2 avg = ((a + b) / 2).normalized;
            //newList.Add(avg * -1 * radius + original[i]);

            //float angle = Vector2.SignedAngle(a,b);
            Vector2 avg = avg = ((a + b) / 2).normalized * radius;

            if (Calc.ClockWiseCheck(original[i - 1],original[i],original[i + 1]) > 0) {
                float temp = -avg.y;
                avg.y = avg.x;
                avg.x = temp;                
            } else {
                float temp = -avg.x;
                avg.x = avg.y;
                avg.y = temp;
            }
            
            newList.Add(avg + original[i]);
        }

        newList.Add(original[original.Count -1]);

        return newList;
    }



}

public struct PathfindVars
{
    public float radius;
    public float rangeFind;
    public float rangeLimit;
}
