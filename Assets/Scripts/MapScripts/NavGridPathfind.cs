using System.Collections.Generic;
using UnityEngine;

// Pathfinding for navgrid
//      Uses A* pathfinding
public static class NavGridPathfind
{
    public static List<Vector2> Pathfind(Vector2 start, Vector2 goal, Movement m, NavGrid grid, int rangeFind = -1) {
        Vector2Int startC = grid.CellPosFromPos2D(start);
        Vector2Int goalC = grid.CellPosFromPos2D(goal);
        return Pathfind(startC,goalC,m,grid,rangeFind);
    }


    public static List<Vector2> Pathfind(Vector2Int start,Vector2Int goal,Movement m,NavGrid grid,int rangeFind = -1) {

        if(grid.CellFromCellPos(start.x,start.y).m == 0 || grid.CellFromCellPos(goal.x,goal.y).m == 0) {
            Debug.Log("bad start or end for pathfinding");
        }

        float rangeFindCellScale = rangeFind / grid.cellSize;

        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        HashSet<Vector2Int> frontier = new HashSet<Vector2Int>();
        frontier.Add(start);
        Vector2Int lastNode = new Vector2Int();

        Dictionary<Vector2Int,Vector2Int> cameFrom = new Dictionary<Vector2Int,Vector2Int>();

        // actual score + raw distance to goal
        Dictionary<Vector2Int,float> heuristicDist = new Dictionary<Vector2Int,float>();
        heuristicDist.Add(start,Vector2.Distance(start,goal));

        // movement cost to get from start to a node
        Dictionary<Vector2Int,float> distToStart = new Dictionary<Vector2Int,float>();
        distToStart.Add(start,0);

        int counter = 0;
        bool pathfound = false;
        Vector2Int[] ns = { Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left };
        Vector2Int[] nd = {
            Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.right,
            Vector2Int.down + Vector2Int.left,
            Vector2Int.up + Vector2Int.left
        };

        while (frontier.Count > 0 && !pathfound) {
            float lowScore = Mathf.Infinity;
            Vector2Int curr = Vector2Int.zero;

            // find the node with the lowest heuristic score
            foreach (Vector2Int v in frontier) {
                float contenderScore = heuristicDist[v];
                if (contenderScore < lowScore) {
                    curr = v;
                    lowScore = contenderScore;
                }
            }

            // process current node
            frontier.Remove(curr);
            visited.Add(curr);
            float currMoveCost = grid.WalkCostFromCellPos(curr.x,curr.y);

            // process square neighbors
            for (int i = 0; i < ns.Length; i++) {
                Vector2Int n = ns[i] + curr;
                NewNodeCheck(n,curr,currMoveCost,1f);
            }

            // process diagonal neighbors
            for (int i = 0; i < nd.Length; i++) {


                Vector2Int n = nd[i] + curr;
                NewNodeCheck(n,curr,currMoveCost,1.4142f);
            }

            if (Utils.WhileCounterIncrementAndCheck(ref counter)) {
                break;
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();

        
        if (pathfound){
            path.Add(lastNode);
            int counter2 = 0;
            while (cameFrom.ContainsKey(lastNode)) {
                path.Add(cameFrom[lastNode]);
                lastNode = cameFrom[lastNode];
                if(Utils.WhileCounterIncrementAndCheck(ref counter2)) {
                    break;
                }
            }
            path.Reverse();
            CleanPath(path);
            return ConvertCellPathToRealPath(path,grid);
        } else {
            return null;
        }

        void NewNodeCheck(Vector2Int n,Vector2Int curr, float currMoveCost, float dist) {
            int cellType = grid.CellValueFromCellPos(n.x,n.y);
            if (cellType == 0 || (cellType == (int)Movement.FLYING && m == Movement.WALKING)) {
                return;
            }
            if (visited.Contains(n)) {
                return;
            }
            float nMoveCost = grid.WalkCostFromCellPos(n.x,n.y);
            float combinedCost = (nMoveCost + currMoveCost) / 2;

            // if this neighbor is not discovered yet, give it heuristic score and add to frontier
            if (!frontier.Contains(n)) {
                float dts = distToStart[curr] + dist * combinedCost;
                distToStart.Add(n,dts);
                heuristicDist.Add(n,dts + Vector2.Distance(n,goal));
                frontier.Add(n);
                cameFrom.Add(n,curr);
            } else {
                // check if we have a better distance coming from this cell
                float contenderA_Score = distToStart[curr] + dist * combinedCost;
                if (contenderA_Score < distToStart[n]) {
                    distToStart[n] = contenderA_Score;
                    heuristicDist[n] = distToStart[n] + Vector2.Distance(n,goal);
                    cameFrom[n] = curr;
                }
            }

            if(rangeFind != -1) {
                float distSqr = Vector2.SqrMagnitude(new Vector2(n.x,n.y) - goal);
                //float d = Vector2.Distance(grid.PosFromCell(n.x,n.y),goal);
                float rangeSqr = rangeFindCellScale * rangeFindCellScale;

                bool neighborsCanSee = true;

                for(int i = 0; i < ns.Length; i++) {
                    if(!grid.CellFromCellPos(n.x + ns[i].x, n.y + ns[i].y).canSeePlayer) {
                        neighborsCanSee = false;
                        break;
                    }
                }

                if (grid.CellFromCellPos(n.x,n.y).canSeePlayer && distSqr <= rangeSqr && neighborsCanSee) {
                    pathfound = true;
                    lastNode = n;                    
                }
            }

            if (n == goal) {
                lastNode = n;
                pathfound = true;
            }
        }
    }

    // removes redundant nodes from a path
    static void CleanPath(List<Vector2Int> path) {
        
        if(path.Count == 2) {
            return;
        }

        for(int i = path.Count -2; i > 0; ) {

            if (path.Count == 2) {
                return;
            }

            Vector2 next = path[i - 1];
            Vector2 curr = path[i];
            Vector2 prev = path[i + 1];
            Vector2 prevCurr = (prev - curr).normalized;
            Vector2 currNext = (curr - next).normalized;
            if (prevCurr.Equals(currNext)) {
                path.RemoveAt(i);
            }
            i--;
        }
    }

    // path starts as nav grid cell coordinates
    // this converts it to real world 2D space coordinates
    static List<Vector2> ConvertCellPathToRealPath(List<Vector2Int> cellPath, NavGrid grid) {
        List<Vector2> path = new List<Vector2>(cellPath.Count);
        for(int i = 0; i < cellPath.Count; i++) {
            path.Add(grid.PosFromCell(cellPath[i].x,cellPath[i].y));
        }
        return path;
    }
}
