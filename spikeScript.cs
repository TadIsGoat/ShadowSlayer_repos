using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spikeScript : MonoBehaviour
{
    float defaultSpikeDamage = 200f;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthScript>().TakeHit(defaultSpikeDamage, 1); //has to deal at least 1 knockback to work even tho it does nothing
        }
    }
}