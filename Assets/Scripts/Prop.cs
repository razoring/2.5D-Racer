using UnityEngine;

public class Prop : MonoBehaviour
{
    public float sideOffset = 0f;
    private GameManager gm;
    private float bottomEdge = -5f;
    private Vector3 originalScale;

    void Start()
    {
        gm = FindAnyObjectByType<GameManager>();
        if (Camera.main != null)
        {
            bottomEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).y - 1f;
        }
        originalScale = transform.localScale;
        
        if (sideOffset == 0f)
        {
            sideOffset = (Random.value > 0.5f ? 1f : -1f) * Random.Range(4f, 8f);
        }
    }

    void LateUpdate()
    {
        if (gm == null) return;

        // Move down the screen based on game speed
        // The road scrollDistance increments by speed * Time.fixedDeltaTime
        // We move Y linearly from 0 to bottomEdge.
        // Wait, if road visually scrolls at a certain speed, what speed should the prop move?
        // Let's use speed * 3f to match the visual flow
        // Move down the screen slower
        transform.position += Vector3.down * gm.speed * 1.5f * Time.deltaTime;

        if (transform.position.y < bottomEdge)
        {
            Destroy(gameObject);
            return;
        }

        // Calculate perspective scale. Start exactly at 0 so it emerges cleanly from the vanishing point.
        float t = transform.position.y / bottomEdge; // 0 at horizon, 1 at bottom
        float visualScale = Mathf.Lerp(0f, 0.7f, t);
        float positionScale = Mathf.Lerp(0.1f, 0.7f, t); // Matches road width perspective
        
        transform.localScale = originalScale * visualScale * 15f;

        // Apply alpha fade for the very beginning to smooth the pop-in
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = Mathf.Clamp01(t * 10f); // Fully opaque once it reaches 10% of the screen down
            sr.color = c;
        }

        // Lock to road perspective
        RoadManager[] allRoads = FindObjectsByType<RoadManager>(FindObjectsSortMode.None);
        float minD = 9999f;
        float roadX = 0f;
        
        foreach (RoadManager r in allRoads)
        {
            if (r.gameObject.CompareTag("Sand")) continue;

            float d = Mathf.Abs(r.transform.position.y - transform.position.y);
            if (d < minD)
            {
                minD = d;
                roadX = r.transform.position.x;
            }
        }

        // Position the prop alongside the road, scaled by perspective so it fans out
        transform.position = new Vector3(roadX + (sideOffset * positionScale * 10f), transform.position.y, transform.position.z);
    }
}
