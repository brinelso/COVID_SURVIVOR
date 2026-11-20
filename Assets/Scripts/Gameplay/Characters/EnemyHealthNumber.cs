using UnityEngine;

[RequireComponent(typeof(BaseEnemy))]
public class EnemyHealthNumber : MonoBehaviour
{
    [SerializeField] private Vector3 m_localOffset = new Vector3(0f, 0.8f, -0.1f);
    [SerializeField, Min(1)] private int m_fontSize = 48;
    [SerializeField, Min(0.01f)] private float m_characterSize = 0.06f;

    private BaseEnemy m_enemy;
    private TextMesh m_healthText;
    private GameObject m_textObject;

    private void Awake()
    {
        m_enemy = GetComponent<BaseEnemy>();
        CreateHealthText();
        m_enemy.HealthChanged += UpdateHealthText;
        m_enemy.Died += HandleEnemyDied;
    }

    private void CreateHealthText()
    {
        m_textObject = new GameObject("HealthNumber");
        m_textObject.transform.SetParent(transform, false);
        m_textObject.transform.localPosition = m_localOffset;

        m_healthText = m_textObject.AddComponent<TextMesh>();
        m_healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        m_healthText.fontSize = m_fontSize;
        m_healthText.characterSize = m_characterSize;
        m_healthText.anchor = TextAnchor.MiddleCenter;
        m_healthText.alignment = TextAlignment.Center;
        m_healthText.fontStyle = FontStyle.Bold;
        m_healthText.color = Color.white;

        MeshRenderer textRenderer = m_textObject.GetComponent<MeshRenderer>();
        SpriteRenderer enemyRenderer = GetComponentInChildren<SpriteRenderer>();
        if (enemyRenderer != null)
        {
            textRenderer.sortingLayerID = enemyRenderer.sortingLayerID;
            textRenderer.sortingOrder = enemyRenderer.sortingOrder + 1;
        }
        else
        {
            textRenderer.sortingOrder = 10;
        }
    }

    private void UpdateHealthText(int currentHealth, int maximumHealth)
    {
        if (currentHealth <= 0)
        {
            m_textObject.SetActive(false);
            return;
        }

        m_healthText.text = currentHealth.ToString();
    }

    private void HandleEnemyDied(BaseCharacter deadCharacter)
    {
        if (m_textObject != null)
            m_textObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (m_enemy == null) return;

        m_enemy.HealthChanged -= UpdateHealthText;
        m_enemy.Died -= HandleEnemyDied;
    }
}
