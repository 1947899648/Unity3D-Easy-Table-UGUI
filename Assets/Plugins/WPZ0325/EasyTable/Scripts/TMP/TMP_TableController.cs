using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace WPZ0325.EasyTable
{
    public class TMP_TableController : MonoBehaviour
    {
        /// <summary>
        /// Table Raw Data
        /// </summary>
        List<string> m_TableHeader = new List<string>();
        List<List<string>> m_TableData = new List<List<string>>();

        [Header("-----------------Table Key Element----------------------------------")]
        [SerializeField] ScrollRect m_ToggleRowsHolder;
        [SerializeField] ScrollRect m_ButtonRowsHolder;
        [SerializeField] ScrollRect m_HeaderArea;
        [SerializeField] RectTransform m_HeaderRowsHolder; //不是ScrollRect的content
        [SerializeField] ScrollRect m_ContentRowsHolder;

        [Header("-----------------Table Prefabs----------------------------------")]
        [SerializeField] TMP_ToggleRow m_ToggleRow;
        [SerializeField] TMP_ButtonRow m_ButtonRow;
        [SerializeField] TMP_HeaderItem m_HeaderItem;
        [SerializeField] RectTransform m_ContentRow;
        [SerializeField] TMP_ContentItem m_ContentItem;

        [Header("-----------------Table Style Tool----------------------------------")]
        [SerializeField] TMP_TableStyleTool m_TableStyleTool;

        TMP_TableVirtualizer m_Virtualizer;

        /// <summary>
        /// 数据行数
        /// </summary>
        public int RowCount => m_TableData.Count;

        /// <summary>
        /// 数据列数
        /// </summary>
        public int ColumnCount => m_TableHeader.Count;

        /// <summary>
        /// Toggle行勾选变化事件
        /// </summary>
        public event Action<int, bool> ToggleChanged;

        /// <summary>
        /// Button行点击事件
        /// </summary>
        public event Action<int> ButtonClicked;

        private void Awake()
        {
            m_Virtualizer = new TMP_TableVirtualizer(
                m_ToggleRow, m_ButtonRow, m_ContentRow, m_ContentItem,
                m_ToggleRowsHolder, m_ButtonRowsHolder, m_ContentRowsHolder,
                m_TableStyleTool,
                OnToggleChanged, OnButtonClicked);
            CleanTable();
        }

        private void Update()
        {
            TableScrollRectSync();
            if (m_Virtualizer != null)
            {
                m_Virtualizer.OnUpdate();
            }
        }

        /// <summary>
        /// 清空表格
        /// </summary>
        public void CleanTable()
        {
            m_TableHeader.Clear();
            m_TableData.Clear();
            if (m_Virtualizer != null)
            {
                m_Virtualizer.ClearAll();
            }
            RemoveAllChildren(m_HeaderRowsHolder);
        }

        /// <summary>
        /// 更新表格原始数据
        /// </summary>
        /// <param name="json"></param>
        public void UpdateTableRawData(string json = "")
        {
            CleanTable();
            if (json == "")
            {
                int rows = UnityEngine.Random.Range(80,120);
                int columns = UnityEngine.Random.Range(4, 15);
                for (int i = 0; i < columns; i++)
                {
                    m_TableHeader.Add($"Header-{i}");
                }
                for (int i = 0; i < rows; i++)
                {
                    //构造新行
                    List<string> newRow = new List<string>();
                    for (int j = 0; j < columns; j++)
                    {
                        newRow.Add($"Item-{i},{j}");
                    }
                    m_TableData.Add(newRow);
                }
            }
            else
            {
                //Json解析表格数据......
            }
            LoadRawDataToTable();
        }

        
        /// <summary>
        /// 加载原始数据至表格并呈现（行内容走虚拟化，仅实例化可视窗口+缓冲的行）
        /// </summary>
        private void LoadRawDataToTable()
        {
            //Update Header Area
            for (int i = 0; i < m_TableHeader.Count; i++)
            {
                TMP_HeaderItem newHeaderItem = Instantiate(m_HeaderItem);
                newHeaderItem.transform.SetParent(m_HeaderRowsHolder);
                newHeaderItem.GetComponent<RectTransform>().localScale = Vector3.one;
                //Set TMP_HeaderItem Value and Width
                newHeaderItem.SetHeaderItem(m_TableHeader[i]);
            }

            //Update Toggle/Button/Content Area (Virtualized)
            if (m_Virtualizer != null)
            {
                m_Virtualizer.SetData(m_TableData, m_TableHeader.Count);
            }
            m_TableStyleTool.SetTableType();
            m_TableStyleTool.SetTableSize();
            m_TableStyleTool.SetTableColor();
        }

        /// <summary>
        /// Toggle行点击回调
        /// </summary>
        void OnToggleChanged(int rowIndex, bool value)
        {
            print($"Toggle:{value},{rowIndex},{m_TableData[rowIndex]}");
            if (ToggleChanged != null)
            {
                ToggleChanged(rowIndex, value);
            }
        }

        /// <summary>
        /// Button行点击回调
        /// </summary>
        void OnButtonClicked(int rowIndex)
        {
            print($"Button:{rowIndex},{m_TableData[rowIndex]}");
            if (ButtonClicked != null)
            {
                ButtonClicked(rowIndex);
            }
        }

        /// <summary>
        /// 删除某对象的所有子对象方法
        /// </summary>
        /// <param name="parent"></param>
        void RemoveAllChildren(Transform parent)
        {
            Transform t;
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                t = parent.GetChild(i);
                Destroy(t.gameObject);
            }
        }

        /// <summary>
        /// 表格四区域同步跟踪
        /// </summary>
        void TableScrollRectSync()
        {
            if (m_TableStyleTool.IsShowToggleColumn)
            {
                m_ToggleRowsHolder.verticalScrollbar.value = m_ContentRowsHolder.verticalScrollbar.value;
            }
            if (m_TableStyleTool.IsShowButtonColumn)
            {
                m_ButtonRowsHolder.verticalScrollbar.value = m_ContentRowsHolder.verticalScrollbar.value;
            }
            m_HeaderArea.horizontalScrollbar.value = m_ContentRowsHolder.horizontalScrollbar.value;
        }
    }
}
