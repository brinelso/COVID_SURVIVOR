using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private BaseCharacter m_character;
    [SerializeField] private Image m_fillImage;

    private void OnEnable()
    {
        if (m_character == null || m_fillImage == null)
        {
            Debug.LogError("Personagem e imagem de preenchimento precisam ser associados no Inspector.", this);
            return;
        }

        m_character.HealthChanged += UpdateHealthBar;
        UpdateHealthBar(m_character.Health, m_character.MaxHealth);
    }

    private void OnDisable()
    {
        if (m_character == null) return;

        m_character.HealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (m_fillImage == null) return;

        float healthRatio = maxHealth > 0
            ? (float)currentHealth / maxHealth
            : 0f;

        m_fillImage.fillAmount = Mathf.Clamp01(healthRatio);
    }
}
