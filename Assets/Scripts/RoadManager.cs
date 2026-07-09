using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private Transform pos;
    [SerializeField] public InputActionReference move;
    public int segmentIndex;
    private float rot = 0f;
    private GameManager gm;
    private float _currentDamping = 1f;
    private float _currentOffset = 0f;
    private SpriteRenderer _sr;
    private Transform playerTransform;

    private void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
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

        if (move == null || move.action == null || gm == null)
        {
            return;
        }

        Vector2 moveDir = move.action.ReadValue<Vector2>();

        float distanceFromCenter = Mathf.Abs(pos != null ? pos.position.y : transform.position.y);
        float baseCurveStrength = gm.getCurve() * Mathf.Exp(-distanceFromCenter);

        float playerDepthFactor = 1f;
        if (playerTransform != null)
        {
            playerDepthFactor = Mathf.Clamp01(Mathf.Abs(playerTransform.position.y) / 4f);
        }

        float turnSpeed = Time.deltaTime * Mathf.Clamp01(gm.speed);

        if (Mathf.Abs(moveDir.x) < 0.01f)
        {
            _currentDamping = Mathf.Lerp(_currentDamping, 1f, turnSpeed);
        }
        else if (gm.getCurve() * moveDir.x > 0f)
        {
            _currentDamping = Mathf.Lerp(_currentDamping, 0.5f, turnSpeed);
        }
        else
        {
            _currentDamping = Mathf.Lerp(_currentDamping, 1.5f, turnSpeed);
        }

        _currentOffset = Mathf.Lerp(_currentOffset, -moveDir.x + gm.getCurve(), turnSpeed);
        float targetX = baseCurveStrength * _currentDamping + _currentOffset;
        transform.position = new UnityEngine.Vector3(targetX, transform.position.y, transform.position.z);

        if (_sr != null)
        {
            int patternIndex = segmentIndex + Mathf.FloorToInt(gm.scrollDistance * 15f);
            
            if (gameObject.CompareTag("Sand"))
            {
                // Invert the strobe logic so it appears lighter when the road is darker
                _sr.color = (patternIndex % 2 != 0) ? new Color(0.95f, 0.95f, 0.95f) : Color.white;
                
                // Pseudo-randomly flip the sprite using the pattern index to give the illusion of scrolling sand noise
                _sr.flipX = ((patternIndex * 1337) % 2 == 0);
                _sr.flipY = ((patternIndex * 9973) % 2 == 0);
            }
            else
            {
                _sr.color = (patternIndex % 2 == 0) ? new Color(0.95f, 0.95f, 0.95f) : Color.white;
            }
        }
    }

    public int getMiddle() {
        return (int)transform.position.x;
    }
}
