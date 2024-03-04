using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PoisonSplash: MonoBehaviour
{
    public int Damage { get; set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Monster")
        {
            other.GetComponent<Monster>().TakeDamage(Damage, Element.POISON);
            Destroy(gameObject);
        }
    }
}
