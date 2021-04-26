using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seoner
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class WindZone2D : MonoBehaviour
    {
        public Seoner.Events.SingleVector2Event OnWindDirectionChanged = new Events.SingleVector2Event();
        public Seoner.Events.SingleFloatEvent OnWindStrengthChanged = new Events.SingleFloatEvent();
        public LayerMask LayerInteraction;

        Vector2 windDirection;
        public Vector2 WindDirection {
            get
            {
                return windDirection;
            }
            set
            {
                Vector2 normalized = value.normalized;
                windDirection = normalized;
                OnWindDirectionChanged?.Invoke(windDirection);
            }
        }

        float windStrength;
        public float WindStrength
        {
            get
            {
                return windStrength;
            }
            set
            {
                windStrength = value;
                OnWindStrengthChanged?.Invoke(windStrength);
            }
        }

        List<Rigidbody2D> RigidbodiesInWindZone = new List<Rigidbody2D>();

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & LayerInteraction) == 0)
                return;
            //try get component
            Rigidbody2D rb;
            rb = collision.GetComponent<Rigidbody2D>();
            if(rb)
            {
                RigidbodiesInWindZone.Add(rb);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            Rigidbody2D rb;
            rb = collision.GetComponent<Rigidbody2D>();
            if(rb)
            {
                RigidbodiesInWindZone.Remove(rb);
            }
        }

        private void FixedUpdate()
        {
            if(RigidbodiesInWindZone.Count > 0)
            {
                foreach (var rb in RigidbodiesInWindZone)
                {
                    rb.AddForce(windDirection * WindStrength);
                }
            }
        }

    }

}
