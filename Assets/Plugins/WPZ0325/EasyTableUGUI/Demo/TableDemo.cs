using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WPZ0325.EasyTableUGUI
{
    public class TableDemo : MonoBehaviour
    {
        [SerializeField] KeyCode m_ClearTable;
        [SerializeField] KeyCode m_UpdateTable;
        [SerializeField] TableController m_TableController;


        private void Update()
        {
            if (Input.GetKeyDown(m_ClearTable))
            {
                m_TableController.CleanTable();
            }

            if (Input.GetKeyDown(m_UpdateTable))
            {
                m_TableController.UpdateTableRawData("");
            }
        }
    }
}

