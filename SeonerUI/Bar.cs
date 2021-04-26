using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Seoner.Events;

namespace Seoner.UI
{
    public class Bar : MonoBehaviour
    {
        SingleFloatEvent OnValueChanged;

        public RectTransform mask;
        public bool ClampValue = true;
        private float maskSize;

        private float value;

        private void Awake()
        {
            maskSize = mask.rect.width;
            Value = 1;
        }

        public float Value {
            get { return value; }
            set
            {
                if(ClampValue)
                {
                    this.value = Mathf.Clamp(value, 0, 1);
                    SetMaskSize();
                    OnValueChanged?.Invoke(this.value);
                }
                else
                {
                    if (value >= 0 && value <= 1)
                    {
                        this.value = value;
                        SetMaskSize();
                        OnValueChanged?.Invoke(this.value);
                    }
                    else
                    {
                        Debug.LogWarning("Value of the bar has to be in the (0,1) range");
                    }

                }


            }
        }

        private void SetMaskSize()
        {
            float calculatedWidth = maskSize * value;
            mask.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, calculatedWidth);
        }

    }
}

