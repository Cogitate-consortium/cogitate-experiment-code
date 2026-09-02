using UnityEngine;
using UnityEngine.UI;

namespace Helpers.UI.Core
{
    public class LeaderboardItem : MonoBehaviour
    {
        [SerializeField] private Image image = null;
        [SerializeField] private Color activeColor = Color.yellow;
        [SerializeField] private TMPro.TextMeshProUGUI rankText = null;
        [SerializeField] private TMPro.TextMeshProUGUI nameText = null;
        [SerializeField] private TMPro.TextMeshProUGUI scoreText = null;

        public void SetEntry(int rank, string name, float score)
        {
            rankText.text = string.Format("#{0}", rank);
            nameText.text = name;
            scoreText.text = score.ToString("0");
        }

        public void SetActive()
        {
            image.color = activeColor;
        }
    }
}