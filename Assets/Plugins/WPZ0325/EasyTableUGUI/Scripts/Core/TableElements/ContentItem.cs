using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WPZ0325.EasyTableUGUI
{
    public class ContentItem : MonoBehaviour
    {
        [SerializeField] Text m_Text;

        public void SetContentItem(string value)
        {
            m_Text.text = value;
        }
    }
}

