using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class ExperienceOrb : MonoBehaviour
{
    [Header("Experiência")]
    [SerializeField] private int m_experienceValue = 5;

    [Header("Atração")]
    [SerializeField] private float m_attractionSpeed = 5f;

    private Rigidbody2D m_rigidbody;
    private Player m_player;
    private bool m_isAttracted;
    private bool m_isCollected;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
        m_rigidbody.gravityScale = 0f;

        // O trigger detecta o contato sem empurrar ou bloquear o jogador.
        GetComponent<CircleCollider2D>().isTrigger = true;

        if (m_experienceValue <= 0)
        {
            Debug.LogError("O valor de XP do cristal precisa ser maior que zero.", this);
        }

        if (m_attractionSpeed <= 0f)
        {
            Debug.LogError("A velocidade de atração precisa ser maior que zero.", this);
        }
    }

    private void Start()
    {
        m_player = FindAnyObjectByType<Player>();
        if (m_player == null)
        {
            Debug.LogWarning("ExperienceOrb não encontrou um Player na cena.", this);
        }
    }

    private void FixedUpdate()
    {
        if (m_isCollected) return;

        if (m_player == null)
        {
            m_player = FindAnyObjectByType<Player>();
            if (m_player == null) return;
        }

        if (!m_player.IsAlive) return;

        float pickupRange = Mathf.Max(0f, m_player.ExperiencePickupRange);
        Vector2 playerPosition = m_player.transform.position;
        float distance = Vector2.Distance(m_rigidbody.position, playerPosition);

        // O raio decide quando começa a atração; a velocidade é um atributo separado.
        if (pickupRange > 0f && distance <= pickupRange)
        {
            m_isAttracted = true;
        }

        if (!m_isAttracted) return;

        Vector2 nextPosition = Vector2.MoveTowards(
            m_rigidbody.position,
            playerPosition,
            m_attractionSpeed * Time.fixedDeltaTime);

        m_rigidbody.MovePosition(nextPosition);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isCollected) return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null || !player.IsAlive) return;

        Collect(player);
    }

    private void Collect(Player player)
    {
        if (m_isCollected) return;

        // Marcar primeiro impede que mais de um callback colete o mesmo cristal.
        m_isCollected = true;
        CovidAudioManager.Play(SoundCue.Pickup);
        player.GainExperience(m_experienceValue);
        Destroy(gameObject);
    }
}
