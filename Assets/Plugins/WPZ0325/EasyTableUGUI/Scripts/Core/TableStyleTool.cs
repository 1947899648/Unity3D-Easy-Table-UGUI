using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WPZ0325.EasyTableUGUI
{
    /// <summary>
    /// 分“类型+尺寸+颜色”控制
    /// 对应的是TableType，TableSize、TableColor
    /// </summary>
    public class TableStyleTool : MonoBehaviour
    {
        [Header("----------------------TableType-----------------------------------------------")]
        [Tooltip("Toggle列 Button列 显示状态控制")]
        public bool IsShowToggleColumn = false;
        public bool IsShowButtonColumn = false;

        [Header("----------------------TableSize-----------------------------------------------")]
        [Tooltip("Toggle列宽度")][Range(0,150.0f)]
        [SerializeField] float ToggleColumnWidth = 100.0f;
        [Tooltip("Button列宽度")][Range(0,150.0f)]
        [SerializeField] float ButtonColumnWitdh = 150.0f;
        [Tooltip("表格头Header高度")][Range(0, 100.0f)]
        [SerializeField] float HeaderHeight = 80.0f;
        [Tooltip("表格行Row高度")][Range(0,100.0f)]
        [SerializeField] float ContentRowHeight = 60.0f;

        [Header("----------------------TableColor-----------------------------------------------")]
        [Tooltip("Toggle列头部颜色")] [SerializeField] Color ToggleColumnHeaderColor = Color.white;
        [Tooltip("Toggle列奇数行颜色")] [SerializeField] Color ToggleColumnOddRowColor = Color.white;
        [Tooltip("Toggle列偶数行颜色")] [SerializeField] Color ToggleColumnEvenRowColor = Color.white;
        [Tooltip("Button列头部颜色")] [SerializeField] Color ButtonColumnHeaderColor = Color.white;
        [Tooltip("Button列奇数行颜色")] [SerializeField] Color ButtonColumnOddRowColor = Color.white;
        [Tooltip("Button列偶数行颜色")] [SerializeField] Color ButtonColumnEvenRowColor = Color.white;
        [Tooltip("Header背景颜色")] [SerializeField] Color HeaderBackgroundColor = Color.white;
        [Tooltip("HeaderItem奇数列颜色")] [SerializeField] Color HeaderItemOddColumnColor = Color.white;
        [Tooltip("HeaderItem偶数列颜色")] [SerializeField] Color HeaderItemEvenColumnColor = Color.white;
        [Tooltip("Content奇数行颜色")] [SerializeField] Color ContentOddRowColor = Color.white;
        [Tooltip("Content偶数行颜色")] [SerializeField] Color ContentEvenRowColor = Color.white;
        [Tooltip("ContentItem奇数列颜色")] [SerializeField] Color ContentItemOddColumnColor = Color.white;
        [Tooltip("ContentItem偶数列颜色")] [SerializeField] Color ContentItemEvenColumnColor = Color.white;

        [Header("------------------Table UI Element---------------------------------------------")]
        [SerializeField] LayoutElement m_ToggleColumn;
        [SerializeField] LayoutElement m_ButtonColumn;
        [SerializeField] List<LayoutElement> m_Headers = new List<LayoutElement>();
        [SerializeField] List<ScrollRect> m_RowsHolderArea = new List<ScrollRect>();
        [SerializeField] Image m_ToggleColumnHeaderImage;
        [SerializeField] Image m_ButtonColumnHeaderImage;
        [SerializeField] Image m_HeaderBackground;
        [SerializeField] RectTransform m_HeaderItemHolder;//用于获取Header中所有的HeaderItem以控制颜色，不是ScrollRect的content

        private void OnValidate()
        {
            SetTableType();
            SetTableSize();
            SetTableColor();
        }

        /// <summary>
        /// 设置表格复合类型
        /// </summary>
        public void SetTableType()
        {
            m_ToggleColumn.gameObject.SetActive(IsShowToggleColumn);
            m_ButtonColumn.gameObject.SetActive(IsShowButtonColumn);
        }

        /// <summary>
        /// 设置表格尺寸方面
        /// </summary>
        public void SetTableSize()
        {
            //Toggle列宽度
            m_ToggleColumn.minWidth = ToggleColumnWidth;
            //Button列宽度
            m_ButtonColumn.minWidth = ButtonColumnWitdh;
            //表格头Header高度
            if (m_Headers.Count > 0)
            {
                foreach (LayoutElement item in m_Headers)
                {
                    item.minHeight = HeaderHeight;
                }
            }
            //表格行Row高度
            foreach (ScrollRect area in m_RowsHolderArea)
            {
                for (int i = 0; i < area.content.childCount; i++)
                {
                    area.content.GetChild(i).GetComponent<LayoutElement>().minHeight = ContentRowHeight;
                }
            }
        }

        /// <summary>
        /// 设置表格颜色方面
        /// </summary>
        public void SetTableColor()
        {
            //Set Toggle Column
            m_ToggleColumnHeaderImage.color = ToggleColumnHeaderColor;
            for (int i = 0; i < m_RowsHolderArea[0].content.transform.childCount; i++)
            {
                Image toggleRow = m_RowsHolderArea[0].content.transform.GetChild(i).GetComponent<Image>();
                toggleRow.color = IsOdd(i) ? ToggleColumnOddRowColor : ToggleColumnEvenRowColor;
            }
            //Set Button Column
            m_ButtonColumnHeaderImage.color = ButtonColumnHeaderColor;
            for (int i = 0; i < m_RowsHolderArea[1].content.transform.childCount; i++)
            {
                Image buttonRow = m_RowsHolderArea[1].content.transform.GetChild(i).GetComponent<Image>();
                buttonRow.color = IsOdd(i) ? ButtonColumnOddRowColor : ButtonColumnEvenRowColor;
            }
            //Set Header
            m_HeaderBackground.color = HeaderBackgroundColor;
            for (int i = 0; i < m_HeaderItemHolder.childCount; i++)
            {
                Image headerItem = m_HeaderItemHolder.GetChild(i).GetComponent<Image>();
                headerItem.color = IsOdd(i) ? HeaderItemOddColumnColor : HeaderItemEvenColumnColor;
            }
            //Set Content
            for (int i = 0; i < m_RowsHolderArea[2].content.transform.childCount; i++)
            {
                Image contentRowImage = m_RowsHolderArea[2].content.transform.GetChild(i).GetComponent<Image>();
                contentRowImage.color = IsOdd(i) ? ContentOddRowColor : ContentEvenRowColor;
                for (int j = 0; j < m_RowsHolderArea[2].content.transform.GetChild(i).childCount; j++)
                {
                    Image contentItemImage = m_RowsHolderArea[2].content.transform.GetChild(i).GetChild(j).GetComponent<Image>();
                    contentItemImage.color = IsOdd(j) ? ContentItemOddColumnColor : ContentItemEvenColumnColor;
                }
            }
        }

        /// <summary>
        /// 判断数字奇偶性
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsOdd(int value) => value % 2 == 1;
    }
}

