using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SlotRogue.Unity.Common
{
    public class HPBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private Color _fullColor = Color.green;
        [SerializeField] private Color _lowColor = Color.red;
        [SerializeField] private float _animSpeed = 5f;

        private float _targetFill;
        private int _currentHP;
        private int _maxHP;

        public void Initialize(int maxHP)
        {
            _maxHP = maxHP;
            _currentHP = maxHP;
            _targetFill = 1f;
            if (_fillImage != null)
            {
                _fillImage.fillAmount = 1f;
                _fillImage.color = _fullColor;
            }
            UpdateText();
        }

        public void SetHP(int current, int max = -1)
        {
            if (max > 0) _maxHP = max;
            _currentHP = current;
            _targetFill = _maxHP > 0 ? (float)current / _maxHP : 0;
            UpdateText();
        }

        private void Update()
        {
            if (_fillImage == null) return;

            _fillImage.fillAmount = Mathf.Lerp(_fillImage.fillAmount, _targetFill, Time.deltaTime * _animSpeed);
            _fillImage.color = Color.Lerp(_lowColor, _fullColor, _fillImage.fillAmount);
        }

        private void UpdateText()
        {
            if (_hpText != null)
                _hpText.text = $"{_currentHP}/{_maxHP}";
        }
    }
}
