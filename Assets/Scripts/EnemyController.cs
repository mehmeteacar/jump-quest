using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Idle, Walking, Attacking, Died };
    [FormerlySerializedAs("durum")] public EnemyState state;

    private bool idle = true;
    private Animator anim;

    [FormerlySerializedAs("gezinmeMesafesi")] public float patrolDistance = 2f;
    [FormerlySerializedAs("hiz")] public float speed = 2f;
    [FormerlySerializedAs("gecikme")] public float delay = 4f;

    private float atkCheckDistance = 6f;
    private float atkCheckTimer = 0.2f;
    private float atkDistance = 1.5f;
    private float atkDuration = 0.4f;

    private float targetX;
    private bool goRight = true;

    private Transform player;
    private Vector3 spawnPoint;
    private Vector3 spawnScale;
    private SpriteRenderer sprRenderer;
    private AudioSource atkSound;

    void Awake()
    {
        sprRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnPoint = transform.position;
        spawnScale = transform.localScale;
        atkSound = GetComponent<AudioSource>();
    }

    void Start()
    {
        anim = GetComponent<Animator>();

        state = EnemyState.Idle;
        StartCoroutine(Patrol());
        StartCoroutine(CheckAtk());
    }

    void Update()
    {
        Walk();
    }

    IEnumerator Patrol()
    {
        while(true)
        {
            yield return new WaitForSeconds(delay);

            if ((idle) && (!Variables.playerDied))
            {
                switch (state)
                {
                    case EnemyState.Idle:
                        goRight = !goRight;
                        targetX = goRight ? transform.position.x + patrolDistance : transform.position.x - patrolDistance;
                        state = EnemyState.Walking;
                        FixDirection();
                        break;
                    case EnemyState.Walking:
                        state = EnemyState.Idle;
                        break;
                }
                FixAnim();
            }
        }
    }

    IEnumerator CheckAtk()
    {
        while (true)
        {
            yield return new WaitForSeconds(atkCheckTimer);

            if(state == EnemyState.Died) yield break;

            Vector3 playerPos = new Vector3(player.position.x, transform.position.y);
            
            if(Vector3.Distance(playerPos, transform.position) < atkCheckDistance)
            {
                if((Vector3.Distance(playerPos, transform.position) >= atkDistance)&&(!Variables.playerDied))
                {
                    idle = false;
                    state = EnemyState.Walking;
                    targetX = player.position.x;
                    goRight = targetX > transform.position.x;
                    FixAnim();
                    FixDirection();
                }
            }
            else if(!idle)
            {
                idle = true;
                state = EnemyState.Idle;
                FixAnim();
            }
        }
    }

    void Walk()
    {
        if(state != EnemyState.Walking) return;

        if(!idle)
        {
            targetX = player.position.x;
        }
        
        Vector3 targetPos = new Vector3(targetX, transform.position.y);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < atkDistance)
        {
            if(idle)
                state = EnemyState.Idle;
            else
            {
                state = EnemyState.Attacking;
                StartCoroutine(Attack());
            }

            FixAnim();
        }
    }

    IEnumerator Attack()
    {
        if(state != EnemyState.Attacking) yield break;

        yield return new WaitForSeconds(atkDuration);

        Vector3 playerPosX = new Vector3(player.position.x, transform.position.y);
        Vector3 playerPosY = new Vector3(transform.position.x, player.position.y);
        if((Vector3.Distance(playerPosX, transform.position) <= atkDistance*1.1f)
        && (Vector3.Distance(playerPosY, transform.position) <= atkDistance*0.7f))
        {
            if(state != EnemyState.Died)
            {
                Variables.playerDied = true;
                idle = true;
                state = EnemyState.Idle;
                atkSound.Play();
                FixAnim();
            }
        }
    }

    public void Die()
    {
        if((state == EnemyState.Died)||Variables.playerDied) return;

        state = EnemyState.Died;
        FixAnim();

        RemoveCollider();

        transform.DORotate(new Vector3(0, 0, goRight ? 80f : -80f), 0.5f);
        sprRenderer.DOColor(Color.clear, 1f).SetOptions(true).OnComplete(()=>Destroy(gameObject));
    }

    void RemoveCollider()
    {
        foreach (BoxCollider2D c in GetComponents<BoxCollider2D>())
        {
            c.enabled = false;
        }

        foreach (CircleCollider2D c in GetComponents<CircleCollider2D>())
        {
            c.enabled = false;
        }
    }

    public void Reset()
    {
        transform.position = spawnPoint;
        transform.rotation = Quaternion.identity;
        transform.localScale = spawnScale;
        state = EnemyState.Idle;
        idle = true;
        goRight = true;
        FixAnim();
    }

    void FixAnim()
    {
        anim.SetBool("Walking", state == EnemyState.Walking);
        anim.SetBool("Attacking", state == EnemyState.Attacking);
    }

    void FixDirection()
    {
        float xScale = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(goRight ? xScale : -xScale, transform.localScale.y, transform.localScale.z);
    }
}
