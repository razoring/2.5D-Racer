using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private Transform pos;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InputActionReference move;
    private float rot = 0f;
    private GameManager gm;

    private void Start()
    {
        gm = FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            rot = gm.getRotation();
        }
        else
        {
            Debug.LogWarning("GameManager not found in RoadManager.Start().");
        }
    }

    private void Update()
    {
        /*if (Camera.main == null)
        {
            return;
        }

        float edge = Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, 0f)).y - 1f;
        float rate = FindAnyObjectByType<GameManager>().getRate();

        rb.gravityScale += rate * gravity * Time.deltaTime;
        pos.localScale += new Vector3(pos.localScale.x * rate * xScale * Time.deltaTime, pos.localScale.y * rate * yScale * Time.deltaTime, 1f);

        if (transform.position.y < edge)
        {
            Destroy(gameObject); 
        }*/

        if (rb == null || move == null || move.action == null || gm == null)
        {
            return;
        }

        Vector2 moveDir = move.action.ReadValue<Vector2>();

        float distanceFromCenter = Mathf.Abs(pos != null ? pos.position.y : transform.position.y);
        float baseCurveStrength = gm.getCurve() * Mathf.Exp(-distanceFromCenter);

        float inputStrength = Mathf.Abs(moveDir.x);
        float damping = Mathf.Lerp(1f, 0.25f, inputStrength);

        float targetX = baseCurveStrength * damping;
        rb.MovePosition(new Vector2(targetX, rb.position.y));
    }

    public int getMiddle() {
        return (int)transform.position.x;
    }
}
