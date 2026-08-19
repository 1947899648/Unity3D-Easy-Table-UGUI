using UnityEngine;
using UnityEngine.UI;

namespace WPZ0325.EasyTable
{
    /// <summary>
    /// UGUI 文本元素（同时承担表头项与内容项的文本显示）
    /// </summary>
    public class TextItem : TextItemBase
    {
        [SerializeField] Text m_Text;

        protected override void SetTextValue(string value)
        {
            m_Text.text = value;
        }
    }
}
