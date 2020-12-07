using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [Header ("View Parameters")]
    [SerializeField] protected bool viewExpandsClockwise = false;
    public bool ViewExpandsClockwise { get { return viewExpandsClockwise; } }
    [SerializeField] protected float viewRadius;
    public float ViewRadius { get { return viewRadius; } set { viewRadius = value; } }

    [Range(0, 360)]
    [SerializeField] protected float viewAngle;
    public float ViewAngle { get { return viewAngle; } set { viewAngle = value; } }

    [Header("Mask Parameters")]
    [SerializeField] protected LayerMask targetMask;
    [SerializeField] protected LayerMask obstacleMask;

    [SerializeField] protected List<Transform> visibleTargets = new List<Transform>();
    public List<Transform> VisibleTargets { get { return visibleTargets; } }

    [Header("Rendering Parameters")]
    public MeshFilter viewMeshFilter;
    private Mesh viewMesh;
    [SerializeField] protected float meshResolution = 0.1f;

    void Start() {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }

    // Generate a mesh to visualize FoV by casting rays at intervals defined by the meshResolution
    public void DrawFieldOfView() {

        // Calculate the resolution and the size of each angle.
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;

        // Keep track of a list of view points
        List<Vector3> viewPoints = new List<Vector3>();

        // Loop through each step count, cast a ray outward in that direction.
        for (int i = 0; i <= stepCount; ++i) {

            // Calculate the current angle being cast forward.
            float curAngle;
            if (!viewExpandsClockwise) {
                curAngle = transform.eulerAngles.y - viewAngle/2 + (stepAngleSize * i);
            } else {
                curAngle = transform.eulerAngles.y + (stepAngleSize * i);
            }
            
            // Debug.DrawLine(transform.position, transform.position + DirFromAngle(curAngle, true) * viewRadius, Color.red);

            // Construct a new ViewCastInfo to store the data found at the current angle.
            ViewCastInfo newViewCast = ViewCast(curAngle);
            viewPoints.Add(newViewCast.point);

        }

        // Construct a mesh from the collected Vector3 points.
        int vertexCount = viewPoints.Count + 1;

         // Must have at least 3 vertices to draw a Triangle.
        if (vertexCount >= 3) {
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[(vertexCount-2) * 3];

            // Assign vertex positions. First vertex will be at Vector3.zero
            vertices[0] = Vector3.zero;
            for (int i = 0; i < vertexCount - 1; ++i) {
                vertices[i+1] = transform.InverseTransformPoint(viewPoints[i]);

                // Ensure the vertex counter remains in range.
                if (i < vertexCount - 2) {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
                
            }

            viewMesh.Clear();
            viewMesh.vertices = vertices;
            viewMesh.triangles = triangles;
            viewMesh.RecalculateNormals();
        }
    }

    public void ClearViewMesh() {
        viewMesh.Clear();
    }

    public void FindVisibleTargets() {
        // Clear the current list of visible targets. This is the simplest way to remove targets that leave the radius.
        visibleTargets.Clear();

        UnityEngine.Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

        foreach(UnityEngine.Collider collider in targetsInViewRadius) {
            Transform target = collider.transform;

            if (WithinAngle(target.position, transform.forward, viewAngle)) {
                visibleTargets.Add (target);
            }
        }
    }

    public bool WithinAngle(Vector3 targetPosition, Vector3 angleDirection, float angleInDegrees = 0f) {
        Vector3 dirToTarget = (targetPosition - transform.position).normalized;
        // If not angle is provided, default to the viewing angle.
        if (angleInDegrees <= 0) {
            angleInDegrees = viewAngle;
        }

        // If the angle between the provided direction and target direction is within the supplied angle...
        if (Vector3.Angle(angleDirection, dirToTarget) < angleInDegrees / 2) {

            // Get the distance between this position and the collider
            float distToTarget = Vector3.Distance ( transform.position, targetPosition );

            // Raycast towards the target and check for collisions.
            if (!Physics.Raycast (transform.position, dirToTarget, distToTarget, obstacleMask)) {
                return true;
            }
        }

        return false;
    }

    public bool WithinRadius(Vector3 targetPosition, float radius = 0f) {
        Vector3 dirToTarget = (targetPosition - transform.position).normalized;
        float distToTarget = Vector3.Distance ( transform.position, targetPosition );

        if (distToTarget <= radius && !Physics.Raycast (transform.position, dirToTarget, distToTarget, obstacleMask)) {
            return true;
        }

        return false;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }
        
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // The mesh cone will cast outward, and must return into regarding what it hit.
    public struct ViewCastInfo {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;
        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle) {
            hit = _hit;
            point = _point;
            distance = _dist;
            angle = _angle;
        }
    }

    private ViewCastInfo ViewCast(float targetAngle) {
        Vector3 dir = DirFromAngle(targetAngle, true);

        // RaycastHit hit;
        return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, targetAngle);

        // // If an obstacle was struck
        // if (Physics.Raycast(transform.position, dir, out hit, viewRadius)) {
        //     Debug.Log("Obstruction hit");
        //     return new ViewCastInfo(true, hit.point, hit.distance, targetAngle);
        // } else {
        //     Debug.Log("Straight Line");
            
        // }
    }

}
