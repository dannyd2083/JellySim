using UnityEngine;

public class WindSource : MonoBehaviour
{
    [Header("Wind Properties")]
    public float windStrength = 10f;
    public float windRadius = 3f;
    public float falloffPower = 2f;

    [Header("Controls")]
    public float moveSpeed = 5f;
    public float radiusChangeSpeed = 1f;
    public bool allowControl = false; 

    [Header("Visualization")]
    public bool showGizmos = true;
    public Color windColor = new Color(0, 1, 1, 0.3f);

    void Update()
    {
        if (allowControl)  
        {
            HandleInput();
        }
    }

    void HandleInput()
    {
        // WASD movement (horizontal)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.position += movement;

        // QE movement (vertical)
        if (Input.GetKey(KeyCode.Q))
        {
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }

        // Mouse scroll - adjust radius
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            windRadius = Mathf.Max(0.5f, windRadius + scroll * radiusChangeSpeed);
        }

        // Left Shift - boost wind strength
        if (Input.GetKey(KeyCode.LeftShift))
        {
            windStrength += Time.deltaTime * 5f;
        }

        else if (Input.GetKey(KeyCode.RightShift))
        {
            windStrength -= Time.deltaTime * 5f;
        }
        windStrength = Mathf.Clamp(windStrength, 1f, windStrength);
    }

    public Vector3 GetWindForceAt(Vector3 position)
    {
        Vector3 delta = position - transform.position;
        float distance = delta.magnitude;

        if (distance > windRadius)
        {
            return Vector3.zero;
        }

        Vector3 direction = delta.normalized;
        float falloff = 1f - Mathf.Pow(distance / windRadius, falloffPower);
        falloff = Mathf.Clamp01(falloff);

        Vector3 force = direction * windStrength * falloff;

        return force;
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = windColor;
        Gizmos.DrawWireSphere(transform.position, windRadius);

        Gizmos.color = Color.cyan;
        int arrowCount = 8;
        for (int i = 0; i < arrowCount; i++)
        {
            float angle = (360f / arrowCount) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.right;
            Vector3 start = transform.position;
            Vector3 end = start + dir * windRadius * 0.8f;

            Gizmos.DrawLine(start, end);
            Gizmos.DrawLine(end, end - dir * 0.3f + Vector3.up * 0.1f);
            Gizmos.DrawLine(end, end - dir * 0.3f - Vector3.up * 0.1f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}