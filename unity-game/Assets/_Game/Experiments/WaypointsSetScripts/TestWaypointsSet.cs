using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EnemyController))]
public class TestWaypointsSet : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private EnemyController enemyController;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        enemyController = GetComponent<EnemyController>();
    }

    private void OnEnable()
    {
        enemyController.PathChanged += DisplayPath;
    }

    private void OnDisable()
    {
        if (enemyController != null)
        {
            enemyController.PathChanged -= DisplayPath;
        }

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
        }
    }

    private void DisplayPath(
        object sender,
        EnemyController.Waypoint newWaypoint)
    {
        if (enemyController.CurrentState != EnemyState.Chase
            || newWaypoint == null
            || newWaypoint.Path == null
            || newWaypoint.CurrentIndex < 0
            || newWaypoint.CurrentIndex >= newWaypoint.Path.Count)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        int remainingPointCount =
            newWaypoint.Path.Count - newWaypoint.CurrentIndex;

        Vector3[] positions = new Vector3[remainingPointCount];

        for (int i = 0; i < remainingPointCount; i++)
        {
            positions[i] =
                newWaypoint.Path[newWaypoint.CurrentIndex + i];
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
}
