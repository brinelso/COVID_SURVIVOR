using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceBarUI : MonoBehaviour
{
    [SerializeField] private ExperienceManager m_experienceManager;
    [SerializeField] private Image m_fillImage;
    [SerializeField] private Text m_experienceText;

    private void OnEnable()
    {
        if (m_experienceManager == null || m_fillImage == null || m_experienceText == null)
        {
            Debug.LogError("ExperienceManager, imagem e texto precisam ser associados no Inspector.", this);
            return;
        }

        m_experienceManager.ExperienceChanged += UpdateExperienceBar;

        // A leitura inicial acontece uma vez; depois, a interface usa somente eventos.
        UpdateExperienceBar(
            m_experienceManager.CurrentLevel,
            m_experienceManager.CurrentExperience,
            m_experienceManager.ExperienceRequired,
            m_experienceManager.CurrentProgress);
    }

    private void OnDisable()
    {
        if (m_experienceManager == null) return;

        m_experienceManager.ExperienceChanged -= UpdateExperienceBar;
    }

    private void UpdateExperienceBar(
        BigInteger currentLevel,
        BigInteger currentExperience,
        BigInteger experienceRequired,
        float progress)
    {
        m_fillImage.fillAmount = Mathf.Clamp01(progress);
        m_experienceText.text =
            $"Nível {currentLevel} — {currentExperience} / {experienceRequired} XP";
    }
}
