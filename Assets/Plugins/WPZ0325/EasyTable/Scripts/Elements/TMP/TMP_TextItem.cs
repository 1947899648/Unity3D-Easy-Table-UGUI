using UnityEngine;
using TMPro;

namespace WPZ0325.EasyTable
{
    /// <summary>
    /// TMP 文本元素（同时承担表头项与内容项的文本显示）
    /// </summary>
    public class TMP_TextItem : TextItemBase
    {
        [SerializeField] TextMeshProUGUI m_Text;

        protected override void SetTextValue(string value)
        {
            m_Text.text = value;
        }
    }
}
