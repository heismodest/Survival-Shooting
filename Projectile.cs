using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{

    public LayerMask collisionMask;     //Layer를 이용해서 충돌 체크하기 위한 변수
    public Color trailColour;

    public float speed = 10;
    public float damage = 1;

    float lifeTime = 3;
    float skinWidth = .1f;  //적이 오는 속도에 따른 거리(?)를 raycast가 이동하는 거리에 더해주기 위함

    public void Start()
    {
        Destroy(gameObject, lifeTime);  //gameObject를 lifeTime이 지난 뒤에 destroy
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        //총구가 길어서 총알이 적의 몸통 안에서 출발할 경우 적중이 안되는 문제를 해결하기 위해
        //발사체와 겹쳐있는 모든 충돌체의(어떤 값의?) 배열을 가져오는 것
        if (initialCollisions.Length >0)     //배열에 값이 하나라도 있다면
        {
            OnHitObject(initialCollisions[0], transform.position);
        }

        GetComponent<TrailRenderer>().material.SetColor("_TintColor", trailColour);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    void Update()
    {
        float moveDistance = speed * Time.deltaTime;    //발사체가 날아간 거리
        CheckCollisions (moveDistance);     //매 프레임 별 충돌 체크 - 총알이 이동한 거리(속도*시간)
        transform.Translate(Vector3.forward * moveDistance);    //발사체를 발사(이동)시키는 기능
    }


    void CheckCollisions(float moveDistance)
    {
        Ray ray = new Ray (transform.position, transform.forward);      //발사체의 위치와 방향으로 법선 긋기
        RaycastHit hit;     //raycast로부터 충돌 정보를 받기 위한 변수

        if (Physics.Raycast(ray, out hit, moveDistance + skinWidth, collisionMask, QueryTriggerInteraction.Collide))    //이동거리 + skinWidth
        //모르겠다. 다만, 충돌 여부 및 충돌 시 raycasthit이 된 거리를 측정하는 기능?
        {
            OnHitObject(hit.collider, hit.point);   //충돌하면 onhitobject 실행(총알 파괴 등등 실행)
        }
    }

    // void OnHitObject(RaycastHit hit)
    // {
    //     //print(hit.collider.gameObject.name);    //점검용

    //     IDamageable damageableObject = hit.collider.GetComponent<IDamageable> ();

    //     if (damageableObject !=null)
    //     {
    //         damageableObject.TakeHit(damage, hit);

    //     }

    //     GameObject.Destroy (gameObject);    //충돌 시 발사체 파괴
    // }
    void OnHitObject(Collider c, Vector3 hitPoint)
    {
        IDamageable damageableObject = c.GetComponent<IDamageable> ();

        if (damageableObject !=null)
        {
            damageableObject.TakeHit(damage, hitPoint, transform.forward);
        }

        GameObject.Destroy (gameObject);    //충돌 시 발사체 파괴

    }

}
