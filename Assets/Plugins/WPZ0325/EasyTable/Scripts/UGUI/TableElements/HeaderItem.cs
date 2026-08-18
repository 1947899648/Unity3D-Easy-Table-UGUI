using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WPZ0325.EasyTable
{
    public class HeaderItem : MonoBehaviour
    {
        [SerializeField] Text m_Text;

        public void SetHeaderItem(string value)
        {
            m_Text.text = value;
        }
    }
}


