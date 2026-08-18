using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace WPZ0325.EasyTable
{
    /// <summary>
    /// 表格行循环虚拟化器（快照式）：只实例化覆盖视口+缓冲数量的行，
    /// content 高度写死为 总行数x行高，滚动时按快照整体重排行位置与数据
    /// </summary>
    public class TableVirtualizer
    {
        const int BUFFER_ROWS = 6;
        const float FALLBACK_VIEWPORT_HEIGHT = 600.0f;

        readonly ToggleRow m_ToggleRowPrefab;
        readonly ButtonRow m_ButtonRowPrefab;
        readonly RectTransform m_ContentRowPrefab;
        readonly ContentItem m_ContentItemPrefab;

        readonly ScrollRect m_ToggleRowsHolder;
        readonly ScrollRect m_ButtonRowsHolder;
        readonly ScrollRect m_ContentRowsHolder;

        readonly TableStyleTool m_StyleTool;
        readonly Action<int, bool> m_OnToggleChanged;
        readonly Action<int> m_OnButtonClicked;

        List<List<string>> m_Data;
        int m_ColumnCount;
        int m_DataRowCount;
        float m_ColumnWidth = 150.0f;

        float m_RowHeight;
        float m_LastRowHeight;

        int m_RowCapacity = -1;
        int m_CurrentFirst = -1;

        bool m_ToggleEnabled;
        bool m_ButtonEnabled;

        bool[] m_ToggleStates;

        readonly Color[] m_LastColors = new Color[8];

        ContentRowView[] m_ContentRows;
        ToggleRow[] m_ToggleRows;
        ButtonRow[] m_ButtonRows;

        readonly RectTransform m_ContentPlaceholder;
        readonly RectTransform m_TogglePlaceholder;
        readonly RectTransform m_ButtonPlaceholder;

        sealed class ContentRowView
        {
            public RectTransform rect;
            public ContentItem[] items;
        }

        /// <summary>
        /// 构造：持有依赖引用，禁用行容器的布局组件（行定位改由代码控制），清理示例行并创建占位对象
        /// </summary>
        public TableVirtualizer(
            ToggleRow toggleRowPrefab, ButtonRow buttonRowPrefab, RectTransform contentRowPrefab, ContentItem contentItemPrefab,
            ScrollRect toggleRowsHolder, ScrollRect buttonRowsHolder, ScrollRect contentRowsHolder,
            TableStyleTool styleTool,
            Action<int, bool> onToggleChanged, Action<int> onButtonClicked)
        {
            m_ToggleRowPrefab = toggleRowPrefab;
            m_ButtonRowPrefab = buttonRowPrefab;
            m_ContentRowPrefab = contentRowPrefab;
            m_ContentItemPrefab = contentItemPrefab;
            m_ToggleRowsHolder = toggleRowsHolder;
            m_ButtonRowsHolder = buttonRowsHolder;
            m_ContentRowsHolder = contentRowsHolder;
            m_StyleTool = styleTool;
            m_OnToggleChanged = onToggleChanged;
            m_OnButtonClicked = onButtonClicked;

            //禁用行容器及其父级viewport上的布局组件（行定位/尺寸改由代码与ScrollRect控制）
            DisableRowLayout(m_ToggleRowsHolder.viewport);
            DisableRowLayout(m_ToggleRowsHolder.content);
            DisableRowLayout(m_ButtonRowsHolder.viewport);
            DisableRowLayout(m_ButtonRowsHolder.content);
            DisableRowLayout(m_ContentRowsHolder.viewport);
            DisableRowLayout(m_ContentRowsHolder.content);

            //content锚点改为顶部(0,1)，与ScrollRect滚动公式对齐（原(0,0)锚点会导致内容定位在视口底部之下）
            SetupContentAnchor(m_ToggleRowsHolder.content);
            SetupContentAnchor(m_ButtonRowsHolder.content);
            SetupContentAnchor(m_ContentRowsHolder.content);

            //清理 prefab 中预置的示例行
            ClearContentChildren(m_ToggleRowsHolder.content);
            ClearContentChildren(m_ButtonRowsHolder.content);
            ClearContentChildren(m_ContentRowsHolder.content);

            //占位对象撑起 ScrollRect 的滚动范围（bounds 由 active 子对象包围盒计算）
            m_TogglePlaceholder = CreatePlaceholder(m_ToggleRowsHolder.content);
            m_ButtonPlaceholder = CreatePlaceholder(m_ButtonRowsHolder.content);
            m_ContentPlaceholder = CreatePlaceholder(m_ContentRowsHolder.content);

            LayoutElement itemElement = m_ContentItemPrefab.GetComponent<LayoutElement>();
            if (itemElement != null && itemElement.minWidth > 0)
            {
                m_ColumnWidth = itemElement.minWidth;
            }
            CaptureColors();
        }

        /// <summary>
        /// 设置表格数据：写死 content 高度，实例化固定数量行并渲染首屏
        /// </summary>
        public void SetData(List<List<string>> data, int columnCount)
        {
            ClearAll();

            m_Data = data;
            m_DataRowCount = data.Count;
            m_ColumnCount = columnCount;
            m_RowHeight = m_StyleTool.ContentRowHeight;
            m_LastRowHeight = m_RowHeight;
            m_ToggleStates = new bool[m_DataRowCount];

            ResizeContents();
            m_RowCapacity = CalcRowCapacity(GetViewportHeight());
            EnsureRows();
            ResetScrollPosition();
            RebuildSnapshot(CalcFirstDataRow());
        }

        /// <summary>
        /// 清空表格：销毁全部行并清除数据
        /// </summary>
        public void ClearAll()
        {
            m_Data = null;
            m_DataRowCount = 0;
            m_ToggleStates = null;
            m_CurrentFirst = -1;
            DestroyAllRows();
        }

        /// <summary>
        /// 每帧驱动：行高/视口/列显示开关变化时重建，滚动跨行时快照重排
        /// </summary>
        public void OnUpdate()
        {
            if (m_Data == null || m_DataRowCount == 0)
            {
                return;
            }

            if (HasColorsChanged())
            {
                CaptureColors();
                RebuildSnapshot(m_CurrentFirst);
                return;
            }

            float rowHeight = m_StyleTool.ContentRowHeight;
            if (rowHeight != m_LastRowHeight)
            {
                m_RowHeight = rowHeight;
                m_LastRowHeight = rowHeight;
                ResizeContents();
                RebuildSnapshot(m_CurrentFirst);
                return;
            }

            int capacity = CalcRowCapacity(GetViewportHeight());
            if (capacity != m_RowCapacity)
            {
                m_RowCapacity = capacity;
                EnsureRows();
                RebuildSnapshot(CalcFirstDataRow());
                return;
            }

            int first = CalcFirstDataRow();
            if (first != m_CurrentFirst)
            {
                RebuildSnapshot(first);
            }
        }

        void RebuildSnapshot(int first)
        {
            m_CurrentFirst = first;
            bool showToggle = m_StyleTool.IsShowToggleColumn;
            bool showButton = m_StyleTool.IsShowButtonColumn;

            for (int k = 0; k < m_RowCapacity; k++)
            {
                int dataRow = first + k;
                bool valid = dataRow < m_DataRowCount;
                List<string> rowData = valid ? m_Data[dataRow] : null;

                ContentRowView row = m_ContentRows[k];
                PositionRow(row.rect, dataRow);
                for (int j = 0; j < m_ColumnCount; j++)
                {
                    row.items[j].SetContentItem(valid && j < rowData.Count ? rowData[j] : "");
                }
                ApplyContentRowColor(row, dataRow);

                if (showToggle && m_ToggleRows[k] != null)
                {
                    ToggleRow toggleRow = m_ToggleRows[k];
                    PositionRow(toggleRow.GetComponent<RectTransform>(), dataRow);
                    ApplyToggleRowColor(toggleRow, dataRow);
                    int captured = dataRow;
                    UnityAction<bool> toggleAction = null;
                    if (valid)
                    {
                        toggleAction = b =>
                        {
                            m_ToggleStates[captured] = b;
                            if (m_OnToggleChanged != null)
                            {
                                m_OnToggleChanged(captured, b);
                            }
                        };
                    }
                    toggleRow.SetToggleRow(captured, valid && m_ToggleStates[captured], toggleAction);
                }

                if (showButton && m_ButtonRows[k] != null)
                {
                    ButtonRow buttonRow = m_ButtonRows[k];
                    PositionRow(buttonRow.GetComponent<RectTransform>(), dataRow);
                    ApplyButtonRowColor(buttonRow, dataRow);
                    int captured = dataRow;
                    UnityAction buttonAction = null;
                    if (valid)
                    {
                        buttonAction = () =>
                        {
                            if (m_OnButtonClicked != null)
                            {
                                m_OnButtonClicked(captured);
                            }
                        };
                    }
                    buttonRow.SetButtonRow(captured, "Click me", buttonAction);
                }
            }
        }

        void EnsureRows()
        {
            if (m_ContentRows != null && m_ContentRows.Length == m_RowCapacity &&
                m_ToggleEnabled == m_StyleTool.IsShowToggleColumn &&
                m_ButtonEnabled == m_StyleTool.IsShowButtonColumn)
            {
                return;
            }

            m_ToggleEnabled = m_StyleTool.IsShowToggleColumn;
            m_ButtonEnabled = m_StyleTool.IsShowButtonColumn;
            DestroyAllRows();

            m_ContentRows = new ContentRowView[m_RowCapacity];
            m_ToggleRows = new ToggleRow[m_RowCapacity];
            m_ButtonRows = new ButtonRow[m_RowCapacity];

            for (int k = 0; k < m_RowCapacity; k++)
            {
                ContentRowView view = new ContentRowView();
                RectTransform row = UnityEngine.Object.Instantiate(m_ContentRowPrefab, m_ContentRowsHolder.content);
                row.localScale = Vector3.one;
                view.rect = row;
                view.items = new ContentItem[m_ColumnCount];
                for (int j = 0; j < m_ColumnCount; j++)
                {
                    ContentItem item = UnityEngine.Object.Instantiate(m_ContentItemPrefab, row);
                    item.transform.localScale = Vector3.one;
                    view.items[j] = item;
                }
                m_ContentRows[k] = view;

                if (m_ToggleEnabled)
                {
                    ToggleRow toggleRow = UnityEngine.Object.Instantiate(m_ToggleRowPrefab, m_ToggleRowsHolder.content);
                    toggleRow.transform.localScale = Vector3.one;
                    m_ToggleRows[k] = toggleRow;
                }

                if (m_ButtonEnabled)
                {
                    ButtonRow buttonRow = UnityEngine.Object.Instantiate(m_ButtonRowPrefab, m_ButtonRowsHolder.content);
                    buttonRow.transform.localScale = Vector3.one;
                    m_ButtonRows[k] = buttonRow;
                }
            }
            m_CurrentFirst = -1;
        }

        void DestroyAllRows()
        {
            if (m_ContentRows != null)
            {
                for (int k = 0; k < m_ContentRows.Length; k++)
                {
                    if (m_ContentRows[k] != null && m_ContentRows[k].rect != null)
                    {
                        UnityEngine.Object.DestroyImmediate(m_ContentRows[k].rect.gameObject);
                    }
                }
            }
            if (m_ToggleRows != null)
            {
                for (int k = 0; k < m_ToggleRows.Length; k++)
                {
                    if (m_ToggleRows[k] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(m_ToggleRows[k].gameObject);
                    }
                }
            }
            if (m_ButtonRows != null)
            {
                for (int k = 0; k < m_ButtonRows.Length; k++)
                {
                    if (m_ButtonRows[k] != null)
                    {
                        UnityEngine.Object.DestroyImmediate(m_ButtonRows[k].gameObject);
                    }
                }
            }
            m_ContentRows = null;
            m_ToggleRows = null;
            m_ButtonRows = null;
        }

        void PositionRow(RectTransform rect, int dataRow)
        {
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, m_RowHeight);
            rect.anchoredPosition = new Vector2(0, -dataRow * m_RowHeight);
        }

        void ApplyContentRowColor(ContentRowView row, int dataRow)
        {
            Image rowImage = row.rect.GetComponent<Image>();
            if (rowImage != null)
            {
                rowImage.color = m_StyleTool.GetContentRowColor(dataRow);
            }
            for (int j = 0; j < row.items.Length; j++)
            {
                Image itemImage = row.items[j].GetComponent<Image>();
                if (itemImage != null)
                {
                    itemImage.color = m_StyleTool.GetContentItemColor(j);
                }
            }
        }

        void ApplyToggleRowColor(ToggleRow row, int dataRow)
        {
            Image image = row.GetComponent<Image>();
            if (image != null)
            {
                image.color = m_StyleTool.GetToggleRowColor(dataRow);
            }
        }

        void ApplyButtonRowColor(ButtonRow row, int dataRow)
        {
            Image image = row.GetComponent<Image>();
            if (image != null)
            {
                image.color = m_StyleTool.GetButtonRowColor(dataRow);
            }
        }

        void ResizeContents()
        {
            float totalHeight = m_DataRowCount * m_RowHeight;
            m_ContentRowsHolder.content.sizeDelta = new Vector2(m_ColumnCount * m_ColumnWidth, totalHeight);
            ResizePlaceholder(m_ContentPlaceholder, totalHeight);
            if (m_StyleTool.IsShowToggleColumn)
            {
                m_ToggleRowsHolder.content.sizeDelta = new Vector2(m_StyleTool.ToggleColumnWidth, totalHeight);
                ResizePlaceholder(m_TogglePlaceholder, totalHeight);
            }
            if (m_StyleTool.IsShowButtonColumn)
            {
                m_ButtonRowsHolder.content.sizeDelta = new Vector2(m_StyleTool.ButtonColumnWitdh, totalHeight);
                ResizePlaceholder(m_ButtonPlaceholder, totalHeight);
            }
        }

        void ResetScrollPosition()
        {
            Canvas.ForceUpdateCanvases();
            m_ContentRowsHolder.verticalNormalizedPosition = 1.0f;
            m_ContentRowsHolder.horizontalNormalizedPosition = 0.0f;
            if (m_StyleTool.IsShowToggleColumn)
            {
                m_ToggleRowsHolder.verticalNormalizedPosition = 1.0f;
            }
            if (m_StyleTool.IsShowButtonColumn)
            {
                m_ButtonRowsHolder.verticalNormalizedPosition = 1.0f;
            }
        }

        float GetViewportHeight()
        {
            float height = m_ContentRowsHolder.viewport != null ? m_ContentRowsHolder.viewport.rect.height : 0.0f;
            return height > 0 ? height : FALLBACK_VIEWPORT_HEIGHT;
        }

        void CaptureColors()
        {
            m_LastColors[0] = m_StyleTool.GetContentRowColor(0);
            m_LastColors[1] = m_StyleTool.GetContentRowColor(1);
            m_LastColors[2] = m_StyleTool.GetContentItemColor(0);
            m_LastColors[3] = m_StyleTool.GetContentItemColor(1);
            m_LastColors[4] = m_StyleTool.GetToggleRowColor(0);
            m_LastColors[5] = m_StyleTool.GetToggleRowColor(1);
            m_LastColors[6] = m_StyleTool.GetButtonRowColor(0);
            m_LastColors[7] = m_StyleTool.GetButtonRowColor(1);
        }

        bool HasColorsChanged()
        {
            return m_LastColors[0] != m_StyleTool.GetContentRowColor(0) ||
                   m_LastColors[1] != m_StyleTool.GetContentRowColor(1) ||
                   m_LastColors[2] != m_StyleTool.GetContentItemColor(0) ||
                   m_LastColors[3] != m_StyleTool.GetContentItemColor(1) ||
                   m_LastColors[4] != m_StyleTool.GetToggleRowColor(0) ||
                   m_LastColors[5] != m_StyleTool.GetToggleRowColor(1) ||
                   m_LastColors[6] != m_StyleTool.GetButtonRowColor(0) ||
                   m_LastColors[7] != m_StyleTool.GetButtonRowColor(1);
        }

        int CalcRowCapacity(float viewportHeight)
        {
            float rowHeight = m_RowHeight > 0 ? m_RowHeight : 1.0f;
            return Mathf.Max(1, Mathf.CeilToInt(viewportHeight / rowHeight) + BUFFER_ROWS);
        }

        int CalcFirstDataRow()
        {
            float rowHeight = m_RowHeight > 0 ? m_RowHeight : 1.0f;
            RectTransform content = m_ContentRowsHolder.content;
            //content向上滚动时 anchoredPosition.y 为正（pivot在左上角），可视区在content本地y∈[-D-viewH, -D]
            float scrollY = content != null ? content.anchoredPosition.y : 0.0f;
            int first = Mathf.CeilToInt(scrollY / rowHeight) - BUFFER_ROWS;
            int maxFirst = Mathf.Max(0, m_DataRowCount - m_RowCapacity);
            return Mathf.Clamp(first, 0, maxFirst);
        }

        static void DisableRowLayout(RectTransform content)
        {
            if (content == null)
            {
                return;
            }
            HorizontalOrVerticalLayoutGroup layout = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layout != null)
            {
                layout.enabled = false;
            }
        }

        static void SetupContentAnchor(RectTransform content)
        {
            if (content == null)
            {
                return;
            }
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(0, 1);
        }

        static void ClearContentChildren(RectTransform content)
        {
            if (content == null)
            {
                return;
            }
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(content.GetChild(i).gameObject);
            }
        }

        static RectTransform CreatePlaceholder(RectTransform content)
        {
            if (content == null)
            {
                return null;
            }
            GameObject go = new GameObject("VirtualPlaceholder");
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.SetParent(content, false);
            rt.localScale = Vector3.one;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.SetSiblingIndex(0);
            return rt;
        }

        static void ResizePlaceholder(RectTransform placeholder, float height)
        {
            if (placeholder == null)
            {
                return;
            }
            placeholder.anchoredPosition = new Vector2(0, -height * 0.5f);
            placeholder.sizeDelta = new Vector2(0, height);
        }
    }
}
