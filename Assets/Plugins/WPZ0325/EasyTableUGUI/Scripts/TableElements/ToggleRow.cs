using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WPZ0325.EasyTableUGUI
{
    public class ToggleRow : MonoBehaviour
    {
        [SerializeField] Toggle m_Toggle;
        [SerializeField] int m_RowIndex;
        public void SetToggleRow(int index,bool value,UnityAction<bool> action = null)
        {
            m_RowIndex = index;
            m_Toggle.onValueChanged.RemoveAllListeners();
            if (!System.Object.ReferenceEquals(action,null))
            {
                m_Toggle.onValueChanged.AddListener(action);
            }
            m_Toggle.isOn = value;
        }
        public int GetRowIndex() => m_RowIndex;
    }
}

