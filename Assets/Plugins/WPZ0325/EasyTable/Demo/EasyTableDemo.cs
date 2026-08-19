using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace WPZ0325.EasyTable
{
    /// <summary>
    /// 双表格Demo：同时控制UGUI版与TMP版表格，信息分别显示到对应Text
    /// </summary>
    public class EasyTableDemo : MonoBehaviour
    {
        [SerializeField] KeyCode m_ClearTable;
        [SerializeField] KeyCode m_UpdateTable;
        [SerializeField] TableController m_UguiTableController;
        [SerializeField] TableController m_TmpTableController;
        [SerializeField] TextMeshProUGUI m_UguiTableInformation;
        [SerializeField] TextMeshProUGUI m_TmpTableInformation;

        private void Awake()
        {
            if (m_UguiTableController != null)
            {
                m_UguiTableController.ToggleChanged += OnUguiToggleChanged;
                m_UguiTableController.ButtonClicked += OnUguiButtonClicked;
            }
            if (m_TmpTableController != null)
            {
                m_TmpTableController.ToggleChanged += OnTmpToggleChanged;
                m_TmpTableController.ButtonClicked += OnTmpButtonClicked;
            }
        }

        private void OnDestroy()
        {
            if (m_UguiTableController != null)
            {
                m_UguiTableController.ToggleChanged -= OnUguiToggleChanged;
                m_UguiTableController.ButtonClicked -= OnUguiButtonClicked;
            }
            if (m_TmpTableController != null)
            {
                m_TmpTableController.ToggleChanged -= OnTmpToggleChanged;
                m_TmpTableController.ButtonClicked -= OnTmpButtonClicked;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_ClearTable))
            {
                if (m_UguiTableController != null)
                {
                    m_UguiTableController.CleanTable();
                }
                if (m_TmpTableController != null)
                {
                    m_TmpTableController.CleanTable();
                }
                SetInformation(m_UguiTableInformation, "UGUI Table cleared");
                SetInformation(m_TmpTableInformation, "TMP Table cleared");
            }

            if (Input.GetKeyDown(m_UpdateTable))
            {
                if (m_UguiTableController != null)
                {
                    m_UguiTableController.UpdateTableRawData("");
                    SetInformation(m_UguiTableInformation, $"UGUI Table updated: {m_UguiTableController.RowCount} rows x {m_UguiTableController.ColumnCount} columns");
                }
                if (m_TmpTableController != null)
                {
                    m_TmpTableController.UpdateTableRawData("");
                    SetInformation(m_TmpTableInformation, $"TMP Table updated: {m_TmpTableController.RowCount} rows x {m_TmpTableController.ColumnCount} columns");
                }
            }
        }

        void OnUguiToggleChanged(int rowIndex, bool value)
        {
            SetInformation(m_UguiTableInformation, $"UGUI Row {rowIndex} toggle: {(value ? "ON" : "OFF")}");
        }

        void OnUguiButtonClicked(int rowIndex)
        {
            SetInformation(m_UguiTableInformation, $"UGUI Row {rowIndex} button clicked");
        }

        void OnTmpToggleChanged(int rowIndex, bool value)
        {
            SetInformation(m_TmpTableInformation, $"TMP Row {rowIndex} toggle: {(value ? "ON" : "OFF")}");
        }

        void OnTmpButtonClicked(int rowIndex)
        {
            SetInformation(m_TmpTableInformation, $"TMP Row {rowIndex} button clicked");
        }

        void SetInformation(TextMeshProUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
