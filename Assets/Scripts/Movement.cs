using System;
using System.Collections.Generic;
using System.Numerics;
using Mono.Cecil;
using Unity.VisualScripting;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Sprites;

public class Movement : MonoBehaviour
{   
    [SerializeField] Transform obj;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] InputActionReference move;
    [SerializeField] float scaled;
    private UnityEngine.Vector2 moveDir;

    [SerializeField] SpriteRenderer renderer;
    [SerializeField] int idleFrame;
    [SerializeField] string frameFolder;
    private Sprite[] frames;
    private float rot = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        frames = Resources.LoadAll<Sprite>(frameFolder);
        //Debug.Log(frames.Length);

        renderer.sprite = frames[idleFrame];
        rot = FindAnyObjectByType<GameManager>().getRotation();
    }

    // Update is called once per frame
    void Update()
    {   
        if (gameObject.tag.Equals("Player"))
        {
            moveDir = move.action.ReadValue<UnityEngine.Vector2>();
            rb.linearVelocity = new UnityEngine.Vector2(moveDir.x*rot, moveDir.y);
        }
        renderer.sprite = frames[idleFrame+((int)obj.position.x-FindAnyObjectByType<RoadManager>().getMiddle())];

        obj.localScale = new UnityEngine.Vector3(Math.Abs(obj.position.y)/2*scaled,Math.Abs(obj.position.y)/2*scaled);
    }   
}
