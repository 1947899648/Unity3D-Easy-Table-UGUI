using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WPZ0325.EasyTable
{
    public class TableDemo : MonoBehaviour
    {
        [SerializeField] KeyCode m_ClearTable;
        [SerializeField] KeyCode m_UpdateTable;
        [SerializeField] TableController m_TableController;
        [SerializeField] Text m_TableInformation;

        private void Awake()
        {
            if (m_TableController != null)
            {
                m_TableController.ToggleChanged += OnToggleChanged;
                m_TableController.ButtonClicked += OnButtonClicked;
            }
        }

        private void OnDestroy()
        {
            if (m_TableController != null)
            {
                m_TableController.ToggleChanged -= OnToggleChanged;
                m_TableController.ButtonClicked -= OnButtonClicked;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_ClearTable))
            {
                m_TableController.CleanTable();
                SetInformation("Table cleared");
            }

            if (Input.GetKeyDown(m_UpdateTable))
            {
                m_TableController.UpdateTableRawData("");
                SetInformation($"Table updated: {m_TableController.RowCount} rows x {m_TableController.ColumnCount} columns");
            }
        }

        void OnToggleChanged(int rowIndex, bool value)
        {
            SetInformation($"Row {rowIndex} toggle: {(value ? "ON" : "OFF")}");
        }

        void OnButtonClicked(int rowIndex)
        {
            SetInformation($"Row {rowIndex} button clicked");
        }

        void SetInformation(string text)
        {
            if (m_TableInformation != null)
            {
                m_TableInformation.text = text;
            }
        }
    }
}
