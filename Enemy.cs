using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;   //NavMeshAgent 사용을 위해

[RequireComponent (typeof (NavMeshAgent))]

public class Enemy : LivingEntity //이미 LivingEntity가 MonoBehaviour와 IDmageable class를 상속받았으므로
{

    public enum State {Idle, Chasing, Attacking};   //현재 상태값을 지정하도록 하여, 상태 별로 메서드를 실행하여 충돌하지 않게
    State currentState;

    public ParticleSystem deathEffect;  //죽었을때 효과 파티클

    public static event System.Action OnDeathStatic;    //스코어 측정을 위함

    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;  //공격 시 색상 변하게 하기위한 변수
    Color originalColour;   //공격 후 원래 색상으로 돌아가기 위한 변수

    public float enemySpeed;
    public float enemyAtkPwr;

    float attackDistanceThreshold = .5f;   //적이 플레이어를 공격하는 거리 임계값
    float timeBetweenAttacks = 1;   //적의 공격 간격(초)
    float damage = 1;
    float nextAttackTime;

    float myCollisionRadius;    //적이 player에 겹치지 않게 반경을 확인해서 멈추게 하기 위한 변수
    float targetCollisionRadius;    //상동

    bool hasTarget;     //목표가 죽었는지 살았는지 담을 변수
    
    void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent> ();
        
        skinMaterial = GetComponent<Renderer> ().material;
        originalColour = skinMaterial.color;    //원 색상 지정
        
        if (GameObject.FindGameObjectWithTag ("Player") !=null)     //player(태그) 존재할때 아래 실행
        {
            // currentState = State.Chasing;
            hasTarget = true;

            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity> ();    //targetEntity = GetComponent<LivingEntity> ();
            // targetEntity.OnDeath += OnTargetDeath;

            myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
            targetCollisionRadius = GetComponent<CapsuleCollider> ().radius;

            // StartCoroutine(UpdatePath());   //반복
        }
    }

    protected override void Start()
    //LivingEntity 클래스를 상속받는 player, enemy에 있는 start메소드와 겹치면 실행이 안되기 때문에 override를 붙여줌.
    {
        base.Start ();  //LivingEntity 내의 start를 먼저 실행함으로써 두 start를 모두 실행

        // pathfinder = GetComponent<NavMeshAgent> ();
        
        // skinMaterial = GetComponent<Renderer> ().material;
        // originalColour = skinMaterial.color;    //원 색상 지정
        
        if (hasTarget)  //GameObject.FindGameObjectWithTag ("Player") !=null)     //player(태그) 존재할때 아래 실행
        {
            currentState = State.Chasing;
            // hasTarget = true;

            // target = GameObject.FindGameObjectWithTag("Player").transform;
            // targetEntity = target.GetComponent<LivingEntity> ();    //targetEntity = GetComponent<LivingEntity> ();
            targetEntity.OnDeath += OnTargetDeath;

            // myCollisionRadius = GetComponent<CapsuleCollider> ().radius;
            // targetCollisionRadius = GetComponent<CapsuleCollider> ().radius;

            StartCoroutine(UpdatePath());   //반복
        }
    }

    public void SetCharacteristics(float moveSpeed, float atkMultiplier, float enemyHealth, Color skinColour)
    {
        pathfinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil(enemyAtkPwr * atkMultiplier); //targetEntity.startingHealth / hitsToKillPlayer);
        }
        deathEffect.startColor = new Color (skinColour.r, skinColour.g, skinColour.b, 1);   //main.startcolor를 쓰라는데 어떻게 하는건지
        startingHealth = enemyHealth;
        skinMaterial = GetComponent<Renderer> ().material;
        skinMaterial.color = skinColour;
        originalColour = skinMaterial.color;    //원 색상 지정
    }
    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impacts", transform.position);
        if (damage >= health)
        {
            if (OnDeathStatic != null)
            {
                OnDeathStatic ();
            }
            AudioManager.instance.PlaySound("Enemy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath() {
        hasTarget = false;
        currentState = State.Idle;
    }

    void Update()
    {
        //pathfinder.SetDestination (target.position);    //매 프레임마다 경로를 update하므로 리소스 소모 많아
        if(hasTarget)   //목표가 있는 경우에만
        {
            if (Time.time > nextAttackTime)
            {   //공격 타이밍이 되면
            
                float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;
                //Vector3.Distance <-- 제곱근이어서 직접 사용하면 무거워
                //자신과 목표의 위치 차이의 제곱한 수 즉, 목표까지의 거리의 제곱

                if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    //공격가능 거리인지 체크 후(콜라이더 표면부터 거리를 측정하기 위해 radius값을 더해줌)
                    nextAttackTime = Time.time + timeBetweenAttacks;    //공격 딜레이
                    AudioManager.instance.PlaySound("Enemy Attack", transform.position);
                    StartCoroutine(Attack());   //공격 반복
                }
            }
        }
    }

    IEnumerator Attack()
    {

        currentState = State.Attacking;
        pathfinder.enabled = false;     //공격할때 이동하지 않도록

        Vector3 originalPosition = transform.position;
        Vector3 dirToTarget = (target.position - transform.position).normalized;    //정규화? 하여 방향벡터를 얻음
        Vector3 attackPosition = target.position - dirToTarget * (myCollisionRadius);
        //공격 위치를 대상 위치로 하면 적이 플레이어를 완전히 덮치기 때문에 조금만 덮치게 하기 위해 공격 위치를 조정

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

//아래 부분 잘 모르겠다. 이해 어려움.

        while (percent<=1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
            }

            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;   //-Mathf.Pow(percent, 2) = -percent * percent
//그래서 이것을 interpolation(보간) 값 이라 부를 건데,
//(* 보간 - 알려진 점들의 위치를 참조하여, 집합의 일정 범위의 점들(선)을 새롭게 그리는 방법을 말합니다.
//여기서는 원지점->공격지점으로 이동할때 참조할 위 그래프의 대칭 곡선을 만드는 참조점을 의미합니다.)
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            //interpolation이 0일때 original position에 있다가 1일때 attack, 다시 0이될때 original로 돌아가
//Vector3.Lerp로 originalPosition에서 출발하여
//(* Lerp 메소드는 두 벡터 사이에 비례 값(0에서 1 사이)으로 내분점 지점을 반환.
//고등학교 수학1 에서 나오는 내분점 말하는거 맞습니다)
            yield return null;
//코루틴이기 때문에, yield return null을 사용합니다. 이는 while루프의 각 처리 사이에서 프레임을 스킵합니다.
//(* yield return null 지점에서 작업이 멈추고, Update 메소드의 작업이 완전 수행된 이후,
//다음 프레임으로 넘어갔을 때 yield 키워드 아래에 있는 코드나 다음번 루프가 실행된다는 말입니다)
        }
//공격 후
        skinMaterial.color = originalColour;    //원색상으로 변경
        currentState = State.Chasing;
        pathfinder.enabled = true;      //공격이 끝나면 다시 이동
    }

    IEnumerator UpdatePath()
    {
        float refreshRate = 0.25f;  //매 1초마다 경로 찾기

        while (hasTarget) //target !=null)
        {
            if (currentState == State.Chasing)
            {
//목표 위치에서, 일종의 적과 목표 사이의 방향(방향벡터)에 적과 목표의 충돌 범위의 반지름을 곱하여 뺀 값을 할당합니다
                Vector3 dirToTarget = (target.position - transform.position).normalized;    //정규화? 하여 방향벡터를 얻음
                Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold/2);
                //모르겠다. 이해가 어렵다.
                //target의 실제 위치 = new Vector3(target.position.x, 0, target.position.z);
                if (!dead)  //죽은 적이 돌아다니다가 에러날 수 있으므로 살아 있을때만 실행하도록
                {
                    pathfinder.SetDestination (targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }

    }

}

//yield는 무엇인가