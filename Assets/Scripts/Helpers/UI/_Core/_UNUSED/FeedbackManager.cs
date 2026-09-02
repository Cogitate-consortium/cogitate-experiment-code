using UnityEngine;

namespace Helpers.UI.Core
{
    public class FeedbackManager : MonoBehaviour
    {
#if false

        [Header("UI")]
        [SerializeField] private TMPro.TextMeshProUGUI feedbackText = null;
        private int wrongGuessesConsecutive = 0;
        private int correctGuessesConsecutive = 0;

        private FeedbackConfig config { get { return ApplicationLibrary.Config.Feedback; } }

        public void Initialize()
        {
            Clear();
            Reset();
        }

        private void Reset()
        {
            wrongGuessesConsecutive = 0;
            correctGuessesConsecutive = 0;
        }

        private void ActivateFeedback(FeedbackItem feedbackItem)
        {
            if (!config.useFeedback) return;

            Display(feedbackItem.textToDisplay);
            ApplicationLibrary.PlaySFX(feedbackItem.audioToPlay);
        }

        private void Display(string text, float duration = -1)
        {
            if (text.IsNullOrEmpty()) return;

            if (duration < 0)
                duration = text.GetAverageReadingTime();

            feedbackText.text = text;

            CancelInvoke("Clear");
            Invoke("Clear", duration);
        }

        private void Clear()
        {
            feedbackText.text = "";
        }

        public void IncrementGuesses(bool isCorrect)
        {
            if (isCorrect)
            {
                wrongGuessesConsecutive = 0;
                correctGuessesConsecutive++;

                DisplayFeedbackCorrect(correctGuessesConsecutive);
            }
            else
            {
                correctGuessesConsecutive = 0;
                wrongGuessesConsecutive++;

                DisplayFeedbackWrong(wrongGuessesConsecutive);
            }
        }

        private void DisplayFeedbackCorrect(int correctGuessesConsecutive)
        {
            FindAndDisplayIndexInFeedbackList(config.feedbackItems_Good, correctGuessesConsecutive);
        }

        private void DisplayFeedbackWrong(int wrongGuessesConsecutive)
        {
            FindAndDisplayIndexInFeedbackList(config.feedbackItems_Bad, wrongGuessesConsecutive);
        }

        private void FindAndDisplayIndexInFeedbackList(List<FeedbackItem> feedbackItems_Good, int correctGuessesConsecutive)
        {
            FeedbackItem feedbackItem = FindIndexInFeedbackList(feedbackItems_Good, correctGuessesConsecutive);
            ActivateFeedback(feedbackItem);
        }

        private FeedbackItem FindIndexInFeedbackList(List<FeedbackItem> feedbackItems, int correctGuessesConsecutive)
        {
            foreach (FeedbackItem fI in feedbackItems)
                if (fI.numConsecutive == correctGuessesConsecutive)
                    return fI;

            return default(FeedbackItem);
        }
    }

    [Serializable]
    public struct FeedbackItem
    {
        public int numConsecutive;
        public string textToDisplay;
        public AudioClipConfig audioToPlay;

        public FeedbackItem(int numConsecutive, string textToDisplay, AudioClipConfig audioToPlay)
        {
            this.numConsecutive = numConsecutive;
            this.textToDisplay = textToDisplay;
            this.audioToPlay = audioToPlay;
        }
    }
#endif
    }
}