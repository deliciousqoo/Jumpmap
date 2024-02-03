using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private GameManager GM;

    public float maxSpeed;
    public float jumpPower;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    BoxCollider2D collider;
    Animator anim;

    private Coroutine damageCoroutine;

    private bool checkControl = true;
    private int jumpCount, dirc;

    [SerializeField]
    private GameObject damagedPrefab;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<BoxCollider2D>();
        anim = GetComponent<Animator>();
    }

    //Setter
    public void SetPlayerJumpCount(int count) { this.jumpCount = count; }

    //Getter
    public int GetPlayerJumpCount() { return jumpCount; }

    //Function
    private void Update()
    {
        if (checkControl)
        {
            //Jump
            if (Input.GetButtonDown("Jump") && anim.GetInteger("jumpCount") < 2)
            {
                //Debug.Log("jump");
                rigid.velocity = Vector2.zero;
                rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                anim.SetBool("isJumping", true);

                jumpCount++;
                anim.SetInteger("jumpCount", jumpCount);
            }

            //Stop Speed
            if (Input.GetButtonUp("Horizontal")) { rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.2f, rigid.velocity.y); }
            else if (Input.GetButton("Right") && Input.GetButton("Left")) { rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0f, rigid.velocity.y); }

            //Direction Sprite
            if (Input.GetButtonDown("Left")) { spriteRenderer.flipX = true; }
            else if (Input.GetButtonDown("Right")) { spriteRenderer.flipX = false; }
        }

        //Animation
        if (rigid.velocity.normalized.x == 0) { anim.SetBool("isRunning", false); }
        else { anim.SetBool("isRunning", true); }

        //Falling
        if (anim.GetBool("isJumping"))
        {
            if (rigid.velocity.y < 0)
            {
                anim.SetBool("isFalling", true);
                if (jumpCount == 2)
                {
                    jumpCount++;
                    anim.SetInteger("jumpCount", jumpCount);
                }
            }
            else { anim.SetBool("isFalling", false); }
        }
    }

    private void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (checkControl)
        {
            //Move Speed
            rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

            //Max Speed
            if (rigid.velocity.x > maxSpeed) { rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y); }
            else if (rigid.velocity.x < maxSpeed * (-1)) { rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y); }
        }

        //Landing Platform
        if (rigid.velocity.y == 0)
        {
            anim.SetBool("isJumping", false);
            RaycastHit2D rayHit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, 0.2f, LayerMask.GetMask("Platform"));
            if ((rayHit.collider != null && rayHit.distance < 0.015f) || jumpCount == 3) { 
                jumpCount = 0;
            }
            anim.SetInteger("jumpCount", jumpCount);
        }
        else { anim.SetBool("isJumping", true); }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Debug.Log("Attacked");
            if (damageCoroutine != null) { StopCoroutine(damageCoroutine); }
            damageCoroutine = StartCoroutine(OnDamaged(collision.transform.position));

            GameObject damagedEffect = Instantiate(damagedPrefab);
            damagedEffect.GetComponent<Transform>().position = gameObject.transform.position;
        }
    }

    public IEnumerator OnDamaged(Vector2 targetPos)
    {
        //Animation Control
        anim.Play("Attacked");

        //Block Input
        checkControl = false;

        //Attacked Change Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        //Player Pushing
        dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 0.5f) * 2f, ForceMode2D.Impulse);

        //Change Flip Direction
        if (dirc == 1) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;

        yield return new WaitUntil(() => rigid.velocity.y == 0);
        anim.Play("Collapse");
        yield return new WaitForSecondsRealtime(1f);

        //Return Origin State
        anim.Play("Idle");
        spriteRenderer.color = new Color(1, 1, 1, 1);

        checkControl = true;

        damageCoroutine = null;
    }


}