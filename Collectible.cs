using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : Monobehaviour
{
  void OnTriggerEnter(Collider other)
  {
  
      if(other.tag == "Player")
      {

        str colName = this.Name;

        switch(colName)
          {
          case "HP":
            Console.WriteLine("Heal 50 point of health");
            break;
          case "SPD":
            Console.WriteLine("Increased Speed");
            break;
          case "Pwr":
            Console.WriteLine("atk pwr up for 10 seconds");
            break;
          case "Exp":
            Console.WriteLine("Exp point up");
            break;
          case "Weapon":
            Console.WriteLine("Select the Weapon want to upgrade");
            break;
          case "Coin":
            Console.WriteLine("Coin up");
            break;
          }
          
      Destroy(this.gameObject);
      }
  }
}


