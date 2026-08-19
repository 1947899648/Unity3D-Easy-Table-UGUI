using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WPZ0325.EasyTable
{
    /// <summary>
    /// 按钮行（通用组件，UGUI/TMP 版本共用，按钮文本引用 TextItemBase 适配组件）
    /// </summary>
    public class ButtonRow : MonoBehaviour
    {
        [SerializeField] Button m_Button;
        [SerializeField] TextItemBase m_ButtonText;
        [SerializeField] int m_RowIndex;
        public void SetButtonRow(int index,string buttonName, UnityAction action = null)
        {
            m_RowIndex = index;
            m_ButtonText.SetText(buttonName);
            m_Button.onClick.RemoveAllListeners();
            if (!System.Object.ReferenceEquals(action, null))
            {
                m_Button.onClick.AddListener(action);
            }
        }

        public int GetRowIndex() => m_RowIndex;
    }
}

