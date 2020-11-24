using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindTest : MonoBehaviour
{
    List<Vector2> path;
    Vector3 start;
    Vector3 goal;
    List<NavTri> tripath;

    float timerStep = 0.5f;
    float timer = 0;
    int counter = 0;

    float navPathTimer = 0;
    int navPathCounter = 0;

    List<Vector2> path2;

    // Start is called before the first frame update
    void Start()
    {
        start = transform.Find("start").position;
        goal = transform.Find("goal").position;
        GameManager gm = GameManager.main;
        Vector2 start2D = Utils.Vector3ToVector2XZ(start);
        Vector2 goal2D = Utils.Vector3ToVector2XZ(goal);
        path = Pathfinding.getPath(start2D,goal2D,0.5f,gm.gameData.map.TriKDMap,gm.gameData.map.VertEdgeMap);

        NavTri startTri = NavCalc.TriFromPoint(start2D,gm.gameData.map.TriKDMap,gm.gameData.map.VertEdgeMap);
        NavTri goalTri = NavCalc.TriFromPoint(goal2D,gm.gameData.map.TriKDMap,gm.gameData.map.VertEdgeMap);

        tripath = Pathfinding.pathfind(start2D,goal2D,startTri,goalTri,Movement.WALKING);
        //path = Pathfinding.funnel(start2D,goal2D,tripath);
        ;
    }

    // Update is called once per frame
    void Update()
    {
        Utils.debugStarPoint(start,0.5f,CustomColors.niceBlue);
        Utils.debugStarPoint(goal,0.5f,Color.red);

        Debug.DrawLine(Utils.Vector2XZToVector3(path[0]),Utils.Vector2XZToVector3(path[1]),CustomColors.niceBlue);
        Debug.DrawLine(Utils.Vector2XZToVector3(path[path.Count - 2]),Utils.Vector2XZToVector3(path[path.Count - 1]),Color.red);

        for(int i = 1; i < path.Count - 2; i++) {
            Debug.DrawLine(Utils.Vector2XZToVector3(path[i]),Utils.Vector2XZToVector3(path[i + 1]),Color.yellow);

        }

        for (int i = 0; i < tripath.Count; i++) {
            //NavUtils.DrawLines(tripath[i],Color.grey);
            NavTri t = tripath[i];
            HalfEdge e1 = t.e1;
            Vector3 a = Utils.Vector2XZToVector3(e1.start) + new Vector3(0,0.1f,0);
            Vector3 b = Utils.Vector2XZToVector3(e1.next.start) + new Vector3(0,0.1f,0);
            Vector3 c = Utils.Vector2XZToVector3(e1.next.next.start) + new Vector3(0,0.1f,0);
            Debug.DrawLine(a,b,Color.grey);
            Debug.DrawLine(b,c,Color.grey);
            Debug.DrawLine(c,a,Color.grey);
        }

        if(timer < Time.time) {
            counter = (counter + 1) % path.Count;
            timer = Time.time + timerStep;
        }

        if(navPathTimer < Time.time) {
            navPathCounter = (navPathCounter + 1) % tripath.Count;
            navPathTimer = Time.time + timerStep;
        }

        Utils.debugStarPoint(Utils.Vector2XZToVector3(path[counter]),1f,Color.red);
        Utils.debugStarPoint(Utils.Vector2XZToVector3(tripath[navPathCounter].centroid()),1f,Color.yellow);
    }
}
