using UnityEngine;

public class Virus : BaseEnemy
{
    private const int ExperienceReward = 5;
    private bool m_hasGrantedExperience;

    protected override void UpdateEnemyLogic()
    {
        if (m_player == null || !m_player.IsAlive) return;

        // Normalizar mantém a velocidade constante em qualquer direção.
        Vector2 direction = m_player.transform.position - transform.position;
        if (direction == Vector2.zero) return;

        direction.Normalize();

        Vector3 movement = new Vector3(direction.x, direction.y, 0f)
            * CurrentMovementSpeed
            * Time.deltaTime;

        transform.Translate(movement, Space.World);
    }

    protected override void OnDeath()
    {
        // A recompensa direta é concedida uma única vez quando a vida chega a zero.
        if (!m_hasGrantedExperience)
        {
            m_hasGrantedExperience = true;
            if (m_player != null)
                m_player.GainExperience(ExperienceReward);
        }

        // Mantém a desativação dos colliders, visual e Rigidbody feita pelo inimigo-base.
        base.OnDeath();
    }
}
