using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof (Rigidbody))]  //start 안에 있는 명령어와 연관
public class PlayerController : MonoBehaviour
{

    Rigidbody myRigidbody;  //변수선언 방법 : 타입 변수명
    Vector3 velocity;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody>();    //위에 클래스를 미리 선언(?)해줘야함
    }
    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void LookAt(Vector3 lookPoint)   //player 스크립트에서 LookAt 메소드에 point인수를 상속하게 함. point 속성은 Vector3임.
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        //y값을 고정시켜서 오브젝트가 기울어지지 않게
        transform.LookAt(heightCorrectedPoint);
    }

    void FixedUpdate()   // 프레임 저하 시 프레임에 시간의 가중치를 곱해서 실행되도록 하여 이동속도 유지토록
    {
        myRigidbody.MovePosition(myRigidbody.position + velocity * Time.fixedDeltaTime);    //fixedDeltaTime은 FixedUpdate가 호출된 시간간격
    }

}
// public / protected / private의 차이?