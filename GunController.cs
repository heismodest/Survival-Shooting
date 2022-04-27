using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//적들도 총을 들 수 있기 때문에 playercontroller에서 처리하기 보다 별도의 스크립트로 처리

public class GunController : MonoBehaviour
{
    public Transform weaponHold;    //변수
    public Gun[] allGuns;     //시작할때 들고 있어야 할 총
    Gun equippedGun;    //변수

    void Start()
    {
        // if (startingGun !=null)
        // {
        //     EquipGun(startingGun);
        // }
    }

    public void EquipGun(Gun gunToEquip)    //총을 장착하는 메서드
    {
        if (equippedGun != null)    //현재 총이 없지 않으면(있으면)
        {
            Destroy(equippedGun.gameObject);    //그걸 먼저 파괴하고
        }
        
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation) as Gun;  //장착해라 총잡는 손 위치에
        equippedGun.transform.parent = weaponHold;  //무기가 총잡는 손 위치에 붙어서 따라다니도록
    }

    public void EquipGun(int weaponIndex)
    {
        EquipGun(allGuns [weaponIndex]);
    }

    public void OnTriggerHold()
    {
        if (equippedGun !=null)
        {
            equippedGun.OnTriggerHold();
        }

    }
    public void OnTriggerRelease()
    {
        if (equippedGun !=null)
        {
            equippedGun.OnTriggerRelease();
        }
    }

    public float GunHeight    //crosshair의 높이가 총구의 높이와 같게 해서 화면 표시가 어색하지 않게 만들기
    {
        get
        {
            return weaponHold.position.y;
        }
    }

    public void Aim(Vector3 aimPoint)   //크로스헤어 중앙 맞추기
    {
        if (equippedGun !=null)
        {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload()
    {
        if (equippedGun !=null)
        {
            equippedGun.Reload();
        }
    }
}
