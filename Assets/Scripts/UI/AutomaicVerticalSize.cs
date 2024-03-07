using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public class AutomaicVerticalSize : MonoBehaviour
    {
        [SerializeField] private float childHeight = 64f;
        [SerializeField] private float padding = 8f;

        void Start()
        {
            AdJustSize();
        }

        public void AdJustSize()
        {
            Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
            size.y = this.transform.childCount * childHeight + ((this.transform.childCount-1)*padding);
            this.GetComponent<RectTransform>().sizeDelta = size;
        }
    }
}
