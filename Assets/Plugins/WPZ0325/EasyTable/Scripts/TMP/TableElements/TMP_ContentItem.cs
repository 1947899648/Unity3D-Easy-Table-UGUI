using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WPZ0325.EasyTable
{
    public class TMP_ContentItem : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_Text;

        public void SetContentItem(string value)
        {
            m_Text.text = value;
        }
    }
}

