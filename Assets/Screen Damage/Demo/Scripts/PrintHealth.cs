using UnityEngine;
using TMPro;

namespace ScreenDamageSpace
{
    public class PrintHealth : MonoBehaviour
    {
        public ScreenDamage script;
        public TextMeshProUGUI healthUIText;

        void Update()
        {
            healthUIText.text = $"Health: {Mathf.Floor(script.CurrentHealth)}";
        }
    }
}
