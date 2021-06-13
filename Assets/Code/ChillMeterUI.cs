using System;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ChillMeterUI : MonoBehaviour
    {
        public Image barImage;

        private void Update()
        {
            barImage.fillAmount = Locator.Instance.ChillStat.Value;
        }
    }
}