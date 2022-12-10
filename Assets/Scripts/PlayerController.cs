using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    private enum CharacterState { Idle, Running, Jumping, Attacking, Died };
    private CharacterState characterState;

    private bool lookingRight = true;

    private float speed = 6f;
    private bool canJump = true;
    private float jumpPower = 500f;
    private float sprHeight;
    private float atkDuration = 0.25f;

    [FormerlySerializedAs("giris")] public InputController input;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprRenderer;
    private Vector3 spawnPoint;
    private GameController gameController;
    private AudioSource jumpSound;
    private AudioSource swordSound;
    private AudioSource keySound;
    [FormerlySerializedAs("efektKilic")] public GameObject sfxSword;
    [FormerlySerializedAs("efektAnahtar")] public GameObject sfxKey;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprRenderer = GetComponent<SpriteRenderer>();
        gameController = FindObjectOfType<GameController>();
        jumpSound = transform.GetChild(0).GetComponent<AudioSource>();
        swordSound = transform.GetChild(1).GetComponent<AudioSource>();
        keySound = transform.GetChild(2).GetComponent<AudioSource>();
        
        sprHeight = GetComponent<SpriteRenderer>().bounds.size.y;
        spawnPoint = transform.position;

        characterState = CharacterState.Idle;
    }

    void Update()
    {
        Attack();

        if(Variables.playerDied)
            Die();
    }

    void FixedUpdate()
    {
        Jump();
        Run();
    }

    void Run()
    {
        if(characterState == CharacterState.Died) return;

        if (input.MovementX != 0)
        {
            transform.Translate(input.MovementX * speed * Time.deltaTime, 0f, 0f);
            FixDirection();

            if (characterState == CharacterState.Idle)
            {
                characterState = CharacterState.Running;
                anim.SetBool("Running", true);
            }
        }
        else
        {
            if (characterState == CharacterState.Running)
            {
                characterState = CharacterState.Idle;
                anim.SetBool("Running", false);
            }
        }
    }

    void FixDirection()
    {
        if(input.MovementX > 0)
        {
            if(!lookingRight)
            {
                lookingRight = true;
                transform.localScale = new Vector3(1f, transform.localScale.y, transform.localScale.z);
            }
        }
        else if(input.MovementX < 0)
        {
            if(lookingRight)
            {
                lookingRight = false;
                transform.localScale = new Vector3(-1f, transform.localScale.y, transform.localScale.z);
            }
        }
    }

    void Jump()
    {        
        if(characterState == CharacterState.Attacking || characterState == CharacterState.Died) return;

        if (rb.velocity.y < 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, sprHeight/1.9f, LayerMask.GetMask("Ground", "Enemy"));

            if (hit.collider)
            {
                if(hit.collider.gameObject.tag == "Blok")
                {
                    characterState = CharacterState.Idle;
                    anim.SetBool("Jumping", false);
                    anim.SetBool("Running", false);
                    StartCoroutine(Jumped());
                } else if ((hit.collider.gameObject.tag == "Enemy")&&(hit.collider.GetType() != typeof(EdgeCollider2D)))
                {
                    Die();
                }
            }
            else
            {
                characterState = CharacterState.Jumping;
                anim.SetBool("Jumping", true);
                anim.SetBool("Running", false);
                canJump = false;
            }
        }

        if(input.MovementY > 0)
        {
            if((characterState != CharacterState.Jumping) && (canJump))
            {
                rb.AddForce(new Vector2(0, jumpPower * Time.fixedDeltaTime), ForceMode2D.Impulse);
                jumpSound.Play();
                anim.SetBool("Jumping", true);
                anim.SetBool("Running", false);
                characterState = CharacterState.Jumping;
                canJump = false;
            }
        }        
    }

    IEnumerator Jumped()
    {
        yield return new WaitForSeconds(0.33f);
        
        canJump = true;
    }

    void Attack()
    {
        if(characterState == CharacterState.Attacking || characterState == CharacterState.Jumping || characterState == CharacterState.Died) return;
        if(Variables.levelFinished) return;

        if(Input.GetKeyDown(KeyCode.Space))
        {
            characterState = CharacterState.Attacking;
            swordSound.Play();
            anim.SetBool("Attacking", true);
            anim.SetBool("Running", false);
            StartCoroutine(Attacked());

            Instantiate(sfxSword, transform);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, lookingRight ? Vector2.right : Vector2.left, sprHeight*0.9f, LayerMask.GetMask("Enemy"));

            if(hit.collider)
            {
                
                hit.collider.gameObject.GetComponent<EnemyController>().Die();
            }
        }
    }

    void Die()
    {
        if(characterState == CharacterState.Died) return;

        characterState = CharacterState.Died;
        Variables.lives--;
        anim.SetBool("Died", true);
        anim.SetBool("Running", false);
        anim.SetBool("Attacking", false);

        gameController.UpdateUI();

        transform.DORotate(new Vector3(0, 0, lookingRight ? 80f : -80f), 0.5f);
        sprRenderer.DOColor(Color.clear, 1.2f).SetOptions(true).OnComplete(Sifirla);

        if(Variables.lives < 1)
            gameController.Failed();
    }

    void Sifirla()
    {
        DOTween.Kill(transform);
        DOTween.Kill(sprRenderer);
        Variables.playerDied = false;
        transform.position = spawnPoint;
        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        lookingRight = true;
        sprRenderer.color = Color.white;
        anim.SetBool("Died", false);
        gameController.ResetEnemies();
        characterState = CharacterState.Idle;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Pit")
        {
            Die();
        }

        if (col.gameObject.tag == "Key")
        {
            CollectKey(col.gameObject);
        }

        if (col.gameObject.tag == "FinishLine")
        {
            gameController.FinishLevel();
        }
    }

    void CollectKey(GameObject key)
    {
        keySound.Play();
        int keysLeft = GameObject.FindGameObjectsWithTag("Key").Length -1;
        Variables.keysCollected = Variables.keyCount - keysLeft;

        Instantiate(sfxKey, key.transform.position, Quaternion.identity);

        if (Variables.keysCollected > Variables.keyCount)
        {
            Variables.keysCollected = Variables.keyCount;
        }

        Destroy(key);
        gameController.UpdateUI();
    }

    IEnumerator Attacked()
    {
        yield return new WaitForSeconds(atkDuration);
        characterState = CharacterState.Idle;
        anim.SetBool("Attacking", false);
    }
}