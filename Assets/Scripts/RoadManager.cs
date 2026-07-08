using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private Transform pos;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private InputActionReference move;
    private float rot = 0f;

    private void Start()
    {
        rot = FindAnyObjectByType<GameManager>().getRotation();
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

        Vector2 moveDir = move.action.ReadValue<Vector2>();
        rb.linearVelocity = new Vector2(moveDir.x*-rot,moveDir.y);
    }

    public int getMiddle() {
        return (int)transform.position.x;
    }
}
