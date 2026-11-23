using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Player m_target;

    private float m_originalZ;

    private void Awake()
    {
        m_originalZ = transform.position.z;

        if (m_target == null)
        {
            Debug.LogError("Player precisa ser associado no Inspector da câmera.", this);
        }
    }

    private void LateUpdate()
    {
        if (m_target == null) return;

        // Mantém o Player no centro e conserva a profundidade original da câmera.
        transform.position = new Vector3(
            m_target.transform.position.x,
            m_target.transform.position.y,
            m_originalZ);

        // A câmera top-down permanece sem rotação.
        transform.rotation = Quaternion.identity;
    }
}
