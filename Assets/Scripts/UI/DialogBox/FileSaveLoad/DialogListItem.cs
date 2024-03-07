using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MyNamespace
{
    public class DialogListItem : MonoBehaviour
    {
        public InputField inputField;

        public void OnClickSave()
        {
            inputField.text = transform.GetComponentInChildren<Text>().text;
        }
    }

}
