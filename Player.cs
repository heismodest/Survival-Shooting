using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof (PlayerController))]
//PlayerController와 Player 스크립트가 같은 오브젝트에 붙어있게 하기 위해 속성 추가
//이렇게 해야 controller = GerComponent<PlayerController>(); 에러나지 않음.
[RequireComponent(typeof (GunController))]
//상 동
public class Player : LivingEntity //이미 LivingEntity가 MonoBehaviour와 IDmageable class를 상속받았으므로
{
    public float moveSpeed = 5;

    public Crosshairs crosshairs;
    Camera viewCamera;  //플레이어가 바라보는 지점을 가리켜줄 카메라변수
    PlayerController controller;
    GunController gunController;
    Projectile bullet;

    protected override void Start()
    //LivingEntity 클래스를 상속받는 player, enemy에 있는 start메소드와 겹치면 실행이 안되기 때문에 override를 붙여줌.
    {
        base.Start();   //LivingEntity 내의 start를 먼저 실행함으로써 두 start를 모두 실행
    }

    void Awake()
    {
        controller = GetComponent<PlayerController> ();
        //PlayerController와 Player 스크립트가 같은 오브젝트에 붙어있는 것을 전제(?)로(붙어있게 하려고?)
        gunController = GetComponent<GunController> ();
        viewCamera = Camera.main;   //플레이어가 바라보는 방향 변수에 메인카메라 지정 후 update에서 ray로 위치 지정
        FindObjectOfType<Spawner> ().OnNewWave += OnNewWave;
    }
    void OnNewWave(int waveNumber)
    {
        health = startingHealth;
        gunController.EquipGun(waveNumber - 1); //int 타입의 equipgun 메서드 호출
    }

    void Update()
    {
    //Movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move (moveVelocity); //Move 메소드 만들 예정(PlayerController에)

    //Look input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition); //ray 선을 쏘아서 그 닿는 위치를 반환
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);    //Vector3.zero);
        //ray가 닿는 위치를 만들기위함(vector3.up은 닿는 그 위의위치)
        //Vector3.zero);는 바닥에 ray를 조사, 총구 높이를 곱해줌

        float rayDistance;

        if (groundPlane.Raycast(ray,out rayDistance))   //,out은 변수를 참조로 전달한다는 의미
        {
            Vector3 point = ray.GetPoint(rayDistance);  //바닥에 닿은 좌표를 반환함
            //Debug.DrawLine(ray.origin,point,Color.red);   //카메라에서 출발한 ray가 바닥에 교차하는 지점까지 선을 그려줌
            controller.LookAt(point);   //point 위치를 상속받는 LookAt 메소드 호출
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);
            //일정 거리 이상에서만 작동하게
            if ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude > 1f)
            {
                gunController.Aim(point);   //크로스헤어 중앙 맞추기
            }
            //커서와 플레이어 간의 위치 출력
            // print ((new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).magnitude);
        }

    //Weapon input
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }

        if (transform.position.y < -10)
        {
            TakeDamage(health);
        }
    }

    void OnTriggerEnter(Collider collectibles)
    {
        
        string colName = collectibles.name;
        
        switch(colName)
          {
          case "HP(Clone)":
            Debug.Log("Heal 50 point of health");
            
            if(health + 50 >= startingHealth)
            {
                health = startingHealth;
            }
            else
            {
                health = health + 50;
            }
            
            break;
          case "SPD(Clone)":
            Debug.Log("Increased Speed");

            if(moveSpeed + 1 >= 10)
            {
                moveSpeed = 10;
            }
            else
            {
                moveSpeed = moveSpeed + 4;
            }
            break;

          case "PWRUP(Clone)":
            Debug.Log("atk pwr up for 10 seconds");
            if(bullet.damage + 1 >= 10)
            {
                bullet.damage = 10;
            }
            else
            {
                bullet.damage ++;
            }
            break;
          case "Exp":
            Debug.Log("Exp point up");
            break;
          case "GunSPD(Clone)":
            Debug.Log("Select the Weapon want to upgrade");
            if(bullet.speed + 1 >= 10)
            {
                bullet.speed = 10;
            }
            else
            {
                bullet.speed ++;
            }
            break;
          case "Diamond(Clone)":
            Debug.Log("Coin up");
            break;
          }
    }







    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
