using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Seoner
{
    public class InputUtils : MonoBehaviour
    {
        private Camera cam;

        void Awake()
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("Can't find main camera");
            }
        }
        public RaycastHit GetRayCastHit(Vector2 screenPos)
        {
            Ray ray = GetRay(screenPos);
            Debug.DrawRay(screenPos,ray.direction * 10f,Color.red,3f);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);
            return hit;
        }

        public Ray GetRay(Vector2 screenPos)
        {
            //MousePosition
            return cam.ScreenPointToRay(screenPos);
        }
    }
}

