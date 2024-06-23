using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public SpriteRenderer sr;

    private Vector3 spawnPos;
    private float range;
    private float speed;
    private float dir;

    public Vector3 SpawnPos
    {
        set => spawnPos = value;
        get => spawnPos;
    }
    public float Range
    {
        set => range = Mathf.Max(0, value);
        get => range;
    }
    public float Speed
    {
        set => speed = Mathf.Max(0, value);
        get => speed;
    }
    public float Dir
    {
        set => dir = Mathf.Max(0, value);
        get => dir;
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public abstract void SetProjectile(Vector2 spawnPos, float dir, float speed, float range);
}