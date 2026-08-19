using UnityEditor;
using UnityEngine;

namespace WPZ0325.EasyTable.Editor
{
    /// <summary>
    /// TableController 面板：默认绘制，仅内部接线引用只读
    /// </summary>
    [CustomEditor(typeof(TableController))]
    public class TableControllerEditor : UnityEditor.Editor
    {
        private static readonly string[] ReadOnlyFields =
        {
            "m_ToggleRowsHolder", "m_ButtonRowsHolder", "m_HeaderArea",
            "m_HeaderRowsHolder", "m_ContentRowsHolder", "m_TableStyleTool"
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.name == "m_Script")
                {
                    continue;
                }
                bool isReadOnly = System.Array.IndexOf(ReadOnlyFields, property.name) >= 0;
                if (isReadOnly)
                {
                    EditorGUI.BeginDisabledGroup(true);
                }
                EditorGUILayout.PropertyField(property, true);
                if (isReadOnly)
                {
                    EditorGUI.EndDisabledGroup();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
