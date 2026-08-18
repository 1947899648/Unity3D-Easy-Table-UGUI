using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WPZ0325.EasyTableUGUI
{
    public class ButtonRow : MonoBehaviour
    {
        [SerializeField] Button m_Button;
        [SerializeField] Text m_ButtonText;
        [SerializeField] int m_RowIndex;
        public void SetButtonRow(int index,string buttonName, UnityAction action = null)
        {
            m_RowIndex = index;
            m_ButtonText.text = buttonName;
            m_Button.onClick.RemoveAllListeners();
            if (!System.Object.ReferenceEquals(action, null))
            {
                m_Button.onClick.AddListener(action);
            }
        }

        public int GetRowIndex() => m_RowIndex;
    }
}

