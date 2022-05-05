using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{

  public ParticleSystem collectedEffect;

  void OnTriggerEnter(Collider other)
  {
  
  

      if(other.tag == "Player")
      {
      // collectedEffect.GetComponent<Renderer>().material.color = new Color (this.GetComponent<Renderer>().material.color.r, this.GetComponent<Renderer>().material.color.g, this.GetComponent<Renderer>().material.color.b, 1);
      Destroy(Instantiate(collectedEffect.gameObject, this.transform.position, Quaternion.identity), collectedEffect.main.startLifetime.constant);
      Destroy(this.gameObject);
      }
  }
}


