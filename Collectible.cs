using System.Collections;
using System.Collections.Generic;
using UnityEngine;

Public class Collectible : Monobehaviour
{
  void OnTriggerEnter(Collider other)
  {
    if(other.tag == "Player")
    {
      Debug.Log("HP up");
      Destroy(this.gameObject);
    {
  {
{


