using UnityEditor;
using UnityEngine;

namespace WPZ0325.EasyTable.Editor
{
    /// <summary>
    /// TableStyleTool 面板：默认绘制，仅 UI 元素接线引用只读
    /// </summary>
    [CustomEditor(typeof(TableStyleTool))]
    public class TableStyleToolEditor : UnityEditor.Editor
    {
        private static readonly string[] ReadOnlyFields =
        {
            "m_ToggleColumn", "m_ButtonColumn", "m_Headers", "m_RowsHolderArea",
            "m_ToggleColumnHeaderImage", "m_ButtonColumnHeaderImage",
            "m_HeaderBackground", "m_HeaderItemHolder"
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
