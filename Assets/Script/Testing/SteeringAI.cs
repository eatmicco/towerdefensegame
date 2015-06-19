using UnityEngine;
using System.Collections;

public class SteeringAI : MonoBehaviour {

    public Transform target;
    public float moveSpeed = 6.0f;
    public float rotationSpeed = 1.0f;

    private float minDistance = 0.5f;
    private float safeDistance = 6.0f;

    private float maxAvoidForce = 5.0f;
    private float maxVelocity = 5f;

    public enum AIState
    {
        Idle, Seek, Flee, Arrive, Pursuit, Evade, PathFollowing
    };

    public AIState currentState;

    public Transform[] obstacles;
    public float obstacleRadius = 0.5f;

    public Transform[] path;
    public float pathRadius = 1.0f;

    private int currentPathId = 0;
	
	void Update () {
        switch (currentState)
        {
            case AIState.Idle:
                break;
            case AIState.Seek:
                Seek();
                break;
            case AIState.Flee:
                Flee();
                break;
            case AIState.Arrive:
                Arrive();
                break;
            case AIState.Pursuit:
                Pursuit();
                break;
            case AIState.Evade:
                Evade();
                break;
            case AIState.PathFollowing:
                PathFollowing();
                break;
        }
	}

    private void Seek()
    {
        Vector3 direction = target.position - transform.position;

        Vector3 moveVector = direction;

        if (direction.magnitude > minDistance)
        {
            moveVector = direction.normalized * moveSpeed * Time.deltaTime;

            Truncate(moveVector, maxVelocity);

            Vector3 avoidance = CollisionAvoidance(moveVector);

            moveVector = (moveVector.normalized + avoidance) * moveSpeed * Time.deltaTime;

            transform.position += moveVector;
        }

        float angle = Mathf.Atan2(moveVector.y, moveVector.x) * (180 / Mathf.PI);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), rotationSpeed * Time.deltaTime);

    }

    private void Flee()
    {
        Vector3 direction = transform.position - target.position;

        if (direction.magnitude < safeDistance)
        {   
            Vector3 moveVector = direction.normalized * moveSpeed * Time.deltaTime;

            Truncate(moveVector, maxVelocity);

            Vector3 avoidance = CollisionAvoidance(moveVector);

            moveVector = (moveVector.normalized + avoidance) * moveSpeed * Time.deltaTime;

            transform.position += moveVector;

            float angle = Mathf.Atan2(moveVector.y, moveVector.x) * (180 / Mathf.PI);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), rotationSpeed * Time.deltaTime);
        }
    }

    private void Arrive()
    {
        Vector3 direction = transform.position - target.position;

        float distance = direction.magnitude;

        float decelerationFactor = distance / 5;

        float speed = moveSpeed * decelerationFactor;

        Vector3 moveVector = direction.normalized * Time.deltaTime * speed;

        Truncate(moveVector, maxVelocity);

        Vector3 avoidance = CollisionAvoidance(moveVector);

        moveVector = (moveVector.normalized + avoidance) * moveSpeed * Time.deltaTime;

        transform.position += moveVector;

        float angle = Mathf.Atan2(moveVector.y, moveVector.x) * (180 / Mathf.PI);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), rotationSpeed * Time.deltaTime);
    }

    private void Pursuit()
    {
        int iterationAhead = 30;

        Vector3 targetSpeed = target.gameObject.GetComponent<SteeringTarget>().instantVelocity;

        Vector3 targetFuturePosition = target.transform.position + (targetSpeed * iterationAhead);

        Vector3 direction = targetFuturePosition - transform.position;

        Vector3 moveVector = direction;

        if (direction.magnitude > minDistance)
        {
            moveVector = direction.normalized * moveSpeed * Time.deltaTime;

            Truncate(moveVector, maxVelocity);

            Vector3 avoidance = CollisionAvoidance(moveVector);

            moveVector = (moveVector.normalized + avoidance) * moveSpeed * Time.deltaTime;

            transform.position += moveVector;
        }

        float angle = Mathf.Atan2(moveVector.y, moveVector.x) * (180 / Mathf.PI);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), rotationSpeed * Time.deltaTime);

    }

    private void Evade()
    {
        int iterationAhead = 30;

        Vector3 targetSpeed = target.gameObject.GetComponent<SteeringTarget>().instantVelocity;

        Vector3 targetFuturePosition = target.position + (targetSpeed * iterationAhead);

        Vector3 direction = transform.position - targetFuturePosition;

        Vector3 moveVector = direction;

        if (direction.magnitude < safeDistance)
        {
            moveVector = direction.normalized * moveSpeed * Time.deltaTime;

            Truncate(moveVector, maxVelocity);

            Vector3 avoidance = CollisionAvoidance(moveVector);

            moveVector = (moveVector.normalized + avoidance) * moveSpeed * Time.deltaTime;

            transform.position += moveVector;
        }

        float angle = Mathf.Atan2(moveVector.y, moveVector.x) * (180 / Mathf.PI);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, angle - 90), rotationSpeed * Time.deltaTime);

    }

    private Vector3 Truncate(Vector3 v, float max)
    {
        float i = v.magnitude != 0 ? max / v.magnitude : 0;
        if (i < 1)
        {
            v *= i;
        }

        return v;
    }

    private Transform FindMostThreateningObstacle(Vector3 ahead, Vector3 ahead2)
    {
        Transform mostThreatening = null;

        for (int i = 0; i < obstacles.Length; ++i)
        {
            bool collision = Vector3.Distance(obstacles[i].position, ahead) <= obstacleRadius ||
                Vector3.Distance(obstacles[i].position, ahead2) <= obstacleRadius;
            
            if (collision && (mostThreatening == null || Vector3.Distance(transform.position, obstacles[i].position) < Vector3.Distance(transform.position, mostThreatening.position)))
            {
                mostThreatening = obstacles[i];
            }
        }

        return mostThreatening;
    }

    private Vector3 CollisionAvoidance(Vector3 moveVector)
    {
        float dynamic_length = moveVector.magnitude / maxVelocity;
        Vector3 ahead = transform.position + moveVector.normalized * dynamic_length;
        Vector3 ahead2 = ahead * 0.5f;

        Transform mostThreatening = FindMostThreateningObstacle(ahead, ahead2);
        Vector3 avoidance = new Vector3(0, 0, 0);

        if (mostThreatening != null)
        {
            avoidance.x = ahead.x - mostThreatening.position.x;
            avoidance.y = ahead.y - mostThreatening.position.y;

            avoidance.Normalize();
            avoidance *= maxAvoidForce;
            Debug.Log(avoidance);
        }
        else
        {
            avoidance *= 0;//nullify the avoidance force
        }

        return avoidance;
    }

    private void PathFollowing()
    {
        if (currentPathId >= path.Length) return;

        target = path[currentPathId];

        Seek();

        if (Vector3.Distance(target.position, transform.position) <= pathRadius)
        {
            currentPathId++;
        }
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < obstacles.Length; ++i)
        {
            Gizmos.DrawWireSphere(obstacles[i].position, obstacleRadius);
        }

        for (int i = 0; i < path.Length; ++i)
        {
            Gizmos.DrawSphere(path[i].position, 0.1f);
        }
    }
}
