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
    private float _currentDamping = 1f;
    private float _currentOffset = 0f;

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

        if (Mathf.Abs(moveDir.x) < 0.01f)
        {
            _currentDamping = Mathf.Lerp(_currentDamping, 1f, Time.deltaTime);
        }
        else if (gm.getCurve() * moveDir.x > 0f)
        {
            _currentDamping = Mathf.Lerp(_currentDamping, -1f, Time.deltaTime);
        }
        else
        {
            _currentDamping = Mathf.Lerp(_currentDamping, 2f, Time.deltaTime);
        }

        _currentOffset = Mathf.Lerp(_currentOffset, -moveDir.x * gm.speed, Time.deltaTime * 5f);
        float targetX = baseCurveStrength * _currentDamping + _currentOffset;
        rb.MovePosition(new Vector2(targetX, rb.position.y));
    }

    public int getMiddle() {
        return (int)transform.position.x;
    }
}
