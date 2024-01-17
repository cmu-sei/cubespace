using Managers;
using System.Collections;
using System.Collections.Generic;
using Systems.GameBrain;
using TMPro;
using UnityEngine;

namespace UI.HUD
{
    public class UIHudTotalScore : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            ShipStateManager.OnMissionDatasChange += SetTotalScore;
            InitializeTotalScore();
        }

        private void OnDisable()
        {
            ShipStateManager.OnMissionDatasChange -= SetTotalScore;
        }

        public void SetTotalScore(List<MissionData> missions)
        {
            int curScore = 0;
            foreach (MissionData m in missions)
            {
                curScore += m.currentScore;
            }
            scoreText.text = curScore.ToString("#,0");
        }

        private void InitializeTotalScore()
        {
            if (ShipStateManager.Instance == null)
            {
                scoreText.text = "N/A";
            }
            else
            {
                SetTotalScore(ShipStateManager.Instance.MissionDatas);
            }
        }
    }
}
