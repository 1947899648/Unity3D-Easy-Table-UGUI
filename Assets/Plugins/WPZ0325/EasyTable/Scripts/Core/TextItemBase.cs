using UnityEngine;

namespace WPZ0325.EasyTable
{
    /// <summary>
    /// 文本元素基类：UGUI/TMP 版本共用，版本差异仅表现在文本组件上
    /// </summary>
    public abstract class TextItemBase : MonoBehaviour
    {
        public void SetText(string value)
        {
            SetTextValue(value);
        }

        protected abstract void SetTextValue(string value);
    }
}
