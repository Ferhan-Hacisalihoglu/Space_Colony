using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyNamespace
{
    public class DialogBox : MonoBehaviour
    {
        virtual public void ShowDialog()
        {
            WorldController.Instance.IsModal = true;
            gameObject.SetActive(true);
        }

        virtual public void CloseDialog()
        {
            WorldController.Instance.IsModal = false;
            gameObject.SetActive(false);
        }
    }
}