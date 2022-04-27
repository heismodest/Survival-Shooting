using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection); //RaycastHit hit);  //damage 받을 damage 양 입력할 변수, RaycastHit 충돌지점 확인

    void TakeDamage(float damage);
}



