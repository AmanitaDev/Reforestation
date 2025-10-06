using System.Collections;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerInteraction : MonoBehaviour
    {
        public int energyCost;
        public ResourceBarTracker energyBar;

        public TextMeshProUGUI feedbackText;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                bool canAttack = energyBar.ChangeResourceByAmount(energyCost);
                feedbackText.text = canAttack ? "Attack!!" : "Not enough energy";
                feedbackText.color = canAttack ? Color.green : Color.red;
                StartCoroutine(FadeTextOut());
            }
        }

        IEnumerator FadeTextOut()
        {
            yield return new WaitForSeconds(.5f);
            feedbackText.color = Color.clear;
        }
    }
}