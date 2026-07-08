using UnityEngine;

public class RoadManager : MonoBehaviour
{
    [SerializeField] private Transform pos;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float gravity;
    [SerializeField] private float xScale;
    [SerializeField] private float yScale;

    private void Update()
    {
        if (Camera.main == null)
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
        }
    }
}
