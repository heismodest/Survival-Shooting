using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable      //상속한 클래스(?) 안에 있는 메소드는 정의해야?
{
    public float startingHealth;
    public float health {get; protected set; }
    protected bool dead;
    public event System.Action OnDeath;     //적이 죽을때 알림을 받기 위한 event 메서드

    protected virtual void Start() {
    //LivingEntity 클래스를 상속받는 player, enemy에 있는 start메소드와 겹치면 실행이 안되기 때문에 virtual을 붙여줌.
    //이렇게 했을때 어떤 효과가 있을지는 잘 모름.
        
        health = startingHealth;    //초기 체력 설정
    }

    public virtual void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)  //RaycastHit hit)
    {
        //hit variable(raycast) 인수를 갖고있으므로 의미가 있음.
        //예컨대, 발사체가 적을 맞춘 지점을 감지하여 해당 위치에 파티클 생성 등
        TakeDamage(damage);
    }

    public virtual void TakeDamage(float damage)
    {
        health -= damage;
        if (health <=0 && !dead)    //체력 다 닳거나 아직 죽지 안았다면
        {
            Die();      //죽어라
        }
    }

    [ContextMenu("Self Destruct")]  //

    public virtual void Die()    //virtuall이 뭔지? Player class에서 override하기 위해 protected void를 public virtual로 수정
    {
        dead = true;    //죽었다
        if (OnDeath != null) {
            OnDeath();
        }
        GameObject.Destroy(gameObject);     //개체 파괴
    }



}
