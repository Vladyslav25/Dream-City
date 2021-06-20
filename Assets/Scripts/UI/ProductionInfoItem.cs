using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ProductionInfoItem : MonoBehaviour
    {
        public Text m_TextAmount;
        public Image m_ImageIcon;

        public void SetAmount(float _amount)
        {
            m_TextAmount.text = _amount.ToString();
        }

        public void SetAmount(string _text)
        {
            m_TextAmount.fontSize = 30;
            m_TextAmount.text = _text;
        }

        public void SetIcon(Sprite _s)
        {
            m_ImageIcon.sprite = _s;
        }
    }
}
