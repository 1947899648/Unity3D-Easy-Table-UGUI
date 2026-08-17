using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace WPZ0325.EasyTableUGUI
{
    public class TableController : MonoBehaviour
    {
        /// <summary>
        /// Table Raw Data
        /// </summary>
        List<string> m_TableHeader = new List<string>();
        List<List<string>> m_TableData = new List<List<string>>();
        List<ToggleRow> m_ToggleRows = new List<ToggleRow>();
        List<ButtonRow> m_ButtonRows = new List<ButtonRow>();

        [Header("-----------------Table Key Element----------------------------------")]
        [SerializeField] ScrollRect m_ToggleRowsHolder;
        [SerializeField] ScrollRect m_ButtonRowsHolder;
        [SerializeField] ScrollRect m_HeaderArea;
        [SerializeField] RectTransform m_HeaderRowsHolder; //不是ScrollRect的content
        [SerializeField] ScrollRect m_ContentRowsHolder;

        [Header("-----------------Table Prefabs----------------------------------")]
        [SerializeField] ToggleRow m_ToggleRow;
        [SerializeField] ButtonRow m_ButtonRow;
        [SerializeField] HeaderItem m_HeaderItem;
        [SerializeField] RectTransform m_ContentRow;
        [SerializeField] ContentItem m_ContentItem;

        [Header("-----------------Table Style Tool----------------------------------")]
        [SerializeField] TableStyleTool m_TableStyleTool;

        private void Awake()
        {
            CleanTable();
        }

        private void Update()
        {
            TableScrollRectSync();
        }

        /// <summary>
        /// 清空表格
        /// </summary>
        public void CleanTable()
        {
            m_TableHeader.Clear();
            m_TableData.Clear();
            m_ToggleRows.Clear();
            m_ButtonRows.Clear();
            RemoveAllChildren(m_ToggleRowsHolder.content);
            RemoveAllChildren(m_ButtonRowsHolder.content);
            RemoveAllChildren(m_HeaderRowsHolder);
            RemoveAllChildren(m_ContentRowsHolder.content);
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
        /// 加载原始数据至表格并呈现
        /// </summary>
        private void LoadRawDataToTable()
        {
            //Update Toggle Area
            if (m_TableStyleTool.IsShowToggleColumn)
            {
                for (int i = 0; i < m_TableData.Count; i++)
                {
                    ToggleRow newToggle = Instantiate(m_ToggleRow);
                    newToggle.transform.SetParent(m_ToggleRowsHolder.content);
                    newToggle.GetComponent<RectTransform>().localScale = Vector3.one;
                    //Set Toggle Value and Event....
                    newToggle.SetToggleRow(i,false,(bool b)=> 
                    {
                        print($"Toggle:{b},{newToggle.GetRowIndex()},{m_TableData[newToggle.GetRowIndex()]}");
                    });;
                    m_ToggleRows.Add(newToggle);
                }
            }

            //Update Button Area
            if (m_TableStyleTool.IsShowButtonColumn)
            {
                for (int i = 0; i < m_TableData.Count; i++)
                {
                    ButtonRow newButton = Instantiate(m_ButtonRow);
                    newButton.transform.SetParent(m_ButtonRowsHolder.content);
                    newButton.GetComponent<RectTransform>().localScale = Vector3.one;
                    //Set Button Value and Event....
                    newButton.SetButtonRow(i,"Click me", ()=> 
                    {
                        print($"Button:{newButton.GetRowIndex()},{m_TableData[newButton.GetRowIndex()]}");
                    });
                    m_ButtonRows.Add(newButton);
                }
            }

            //Update Header Area
            for (int i = 0; i < m_TableHeader.Count; i++)
            {
                HeaderItem newHeaderItem = Instantiate(m_HeaderItem);
                newHeaderItem.transform.SetParent(m_HeaderRowsHolder);
                newHeaderItem.GetComponent<RectTransform>().localScale = Vector3.one;
                //Set HeaderItem Value and Width
                newHeaderItem.SetHeaderItem(m_TableHeader[i]);
            }

            //Update Content Area
            for (int i = 0; i < m_TableData.Count; i++)
            {
                RectTransform newContentRow = GameObject.Instantiate(m_ContentRow);
                newContentRow.SetParent(m_ContentRowsHolder.content);
                newContentRow.GetComponent<RectTransform>().localScale = Vector3.one;
                for (int j = 0; j < m_TableHeader.Count; j++)
                {
                    ContentItem newContentItem = Instantiate(m_ContentItem);
                    newContentItem.transform.SetParent(newContentRow);
                    newContentItem.GetComponent<RectTransform>().localScale = Vector3.one;
                    //Set ContentItem Value and Width
                    newContentItem.SetContentItem(m_TableData[i][j]);
                }
            }
            m_TableStyleTool.SetTableType();
            m_TableStyleTool.SetTableSize();
            m_TableStyleTool.SetTableColor();
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

