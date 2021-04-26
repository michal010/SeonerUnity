using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Seoner
{
    [RequireComponent(typeof(Collider2D))]
    public class Trigger2D : MonoBehaviour
    {
        public UnityEvent OnTriggerEnter2DEvent;
        private void Awake()
        {
            if (OnTriggerEnter2DEvent == null)
                OnTriggerEnter2DEvent = new UnityEvent();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            OnTriggerEnter2DEvent?.Invoke();
        }

    }
}

