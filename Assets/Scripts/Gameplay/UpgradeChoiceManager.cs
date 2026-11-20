using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

// Coordena níveis pendentes, pausa, sorteio das cartas e aplicação da escolhida.
public class UpgradeChoiceManager : MonoBehaviour
{
    [SerializeField] private UpgradeChoiceUI m_choiceUI;
    [SerializeField] private UpgradeDefinition[] m_upgradeOptions;
    [SerializeField] private Player m_player;
    [SerializeField] private PlayerStats m_playerStats;
    [SerializeField] private ElementalSpellController m_elementalSpellController;

    [Header("Peso de cada categoria por opção")]
    [SerializeField, Min(0f)] private float m_elementWeight = 0.15f;
    [SerializeField, Min(0f)] private float m_attributeWeight = 0.50f;
    [SerializeField, Min(0f)] private float m_spellUpgradeWeight = 0.35f;

    private readonly Queue<BigInteger> m_pendingLevels = new Queue<BigInteger>();
    private readonly Dictionary<UpgradeDefinition, int> m_applicationCounts = new Dictionary<UpgradeDefinition, int>();
    private ExperienceManager m_experienceManager;
    private UpgradeChoice[] m_currentOptions;
    private float m_previousTimeScale = 1f;
    private bool m_isWaitingForChoice;
    private bool m_forceElementsOnlyNextChoice;

    private void OnEnable()
    {
        m_experienceManager = FindAnyObjectByType<ExperienceManager>();
        if (m_player == null)
            m_player = FindAnyObjectByType<Player>();
        if (m_playerStats == null && m_player != null)
            m_playerStats = m_player.GetComponent<PlayerStats>();
        if (m_elementalSpellController == null && m_player != null)
            m_elementalSpellController = m_player.GetComponent<ElementalSpellController>();

        if (m_experienceManager == null || m_player == null || m_playerStats == null ||
            m_elementalSpellController == null || m_choiceUI == null ||
            m_elementalSpellController.Catalog == null || m_upgradeOptions == null)
        {
            Debug.LogError("Associe a interface, Player, PlayerStats, controlador elemental, catálogo e melhorias no Inspector.", this);
            enabled = false;
            return;
        }

        m_experienceManager.LevelUp += HandleLevelUp;
        m_choiceUI.SetDebugLevelUpAction(HandleDebugLevelUp);
        UpdateDebugButtonAvailability();
    }

    private void OnDisable()
    {
        if (m_experienceManager != null)
            m_experienceManager.LevelUp -= HandleLevelUp;

        if (m_isWaitingForChoice)
            Time.timeScale = m_previousTimeScale;
    }

    private void HandleLevelUp(BigInteger level)
    {
        m_pendingLevels.Enqueue(level);
        if (m_isWaitingForChoice) return;

        m_previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        m_isWaitingForChoice = true;
        ShowNextChoice();
    }

    private void ShowNextChoice()
    {
        string failureReason;
        bool created = m_forceElementsOnlyNextChoice
            ? TryCreateElementDebugChoices(out m_currentOptions, out failureReason)
            : TryCreateFourChoices(out m_currentOptions, out failureReason);
        m_forceElementsOnlyNextChoice = false;

        if (!created)
        {
            Debug.LogError(failureReason, this);
            FinishAllChoices();
            return;
        }

        BigInteger level = m_pendingLevels.Dequeue();
        int displayLevel = level > int.MaxValue ? int.MaxValue : (int)level;
        m_choiceUI.Show(displayLevel, m_currentOptions, HandleOptionSelected);
    }

    private void HandleDebugLevelUp()
    {
        if (m_isWaitingForChoice || !m_elementalSpellController.CanChooseMoreElements)
        {
            UpdateDebugButtonAvailability();
            return;
        }

        m_forceElementsOnlyNextChoice = true;
        m_experienceManager.DebugGrantExperienceForNextLevel();
    }

    private bool TryCreateElementDebugChoices(out UpgradeChoice[] choices, out string failureReason)
    {
        choices = null;
        failureReason = string.Empty;
        List<ElementType> elements = GetEligibleElements();
        if (elements.Count != 4)
        {
            failureReason = $"O modo de debug precisa de quatro elementos distintos; há {elements.Count} elegíveis.";
            return false;
        }

        choices = new UpgradeChoice[4];
        for (int i = 0; i < choices.Length; i++)
        {
            ElementType element = elements[i];
            if (!m_elementalSpellController.TryPreviewElement(element,
                out ElementalSpellDefinition definition))
            {
                failureReason = $"O catálogo não contém uma definição para o elemento {element} no estado atual da receita.";
                choices = null;
                return false;
            }

            string action = m_elementalSpellController.ElementsChosen == 0
                ? "Desbloqueia"
                : "Transforma a magia em";
            choices[i] = UpgradeChoice.FromElement(element,
                $"{action} {definition.SpellName}.",
                m_elementalSpellController.Catalog.GetElementColor(element));
        }

        return true;
    }

    private void UpdateDebugButtonAvailability()
    {
        if (m_choiceUI != null && m_elementalSpellController != null)
            m_choiceUI.SetDebugLevelUpAvailable(m_elementalSpellController.CanChooseMoreElements);
    }

    private bool TryCreateFourChoices(out UpgradeChoice[] choices, out string failureReason)
    {
        choices = null;
        failureReason = string.Empty;
        List<UpgradeDefinition> attributes = GetEligibleOptions(UpgradeCategory.Attribute);
        List<UpgradeDefinition> spellUpgrades = GetEligibleOptions(UpgradeCategory.SpellUpgrade);
        List<ElementType> elements = GetEligibleElements();

        // Peso zero significa que essa categoria não participa do sorteio.
        if (m_elementWeight <= 0f) elements.Clear();
        if (m_attributeWeight <= 0f) attributes.Clear();
        if (m_spellUpgradeWeight <= 0f) spellUpgrades.Clear();

        int availableCount = CountUsableOptions(elements, attributes, spellUpgrades);
        if (availableCount < 4)
        {
            failureReason = $"Level-up sem opções suficientes: há {availableCount} opções distintas elegíveis " +
                            $"(elementos: {elements.Count}, atributos gerais: {attributes.Count}, " +
                            $"melhorias da magia: {spellUpgrades.Count}); são necessárias 4. " +
                            "Revise as opções associadas, limites de aplicações e pesos no Inspector.";
            return false;
        }

        choices = new UpgradeChoice[4];

        for (int i = 0; i < choices.Length; i++)
        {
            UpgradeCategory category = ChooseCategory(elements.Count > 0, attributes.Count > 0,
                spellUpgrades.Count > 0);

            if (category == UpgradeCategory.Element)
            {
                int elementIndex = Random.Range(0, elements.Count);
                ElementType element = elements[elementIndex];
                elements.RemoveAt(elementIndex);
                if (!m_elementalSpellController.TryPreviewElement(element,
                    out ElementalSpellDefinition definition))
                {
                    failureReason = $"O catálogo não contém uma definição para o elemento {element} " +
                                    "no estado atual da receita.";
                    choices = null;
                    return false;
                }

                string action = m_elementalSpellController.ElementsChosen == 0
                    ? "Desbloqueia"
                    : "Transforma a magia em";
                choices[i] = UpgradeChoice.FromElement(element,
                    $"{action} {definition.SpellName}.",
                    m_elementalSpellController.Catalog.GetElementColor(element));
            }
            else if (category == UpgradeCategory.Attribute || category == UpgradeCategory.SpellUpgrade)
            {
                List<UpgradeDefinition> categoryOptions = category == UpgradeCategory.Attribute
                    ? attributes
                    : spellUpgrades;
                int optionIndex = Random.Range(0, categoryOptions.Count);
                UpgradeDefinition definition = categoryOptions[optionIndex];
                categoryOptions.RemoveAt(optionIndex);
                choices[i] = UpgradeChoice.FromDefinition(definition);
            }
            else
            {
                failureReason = "Não foi possível sortear uma categoria com opções elegíveis. Revise os pesos no Inspector.";
                choices = null;
                return false;
            }
        }

        return true;
    }

    private List<ElementType> GetEligibleElements()
    {
        List<ElementType> elements = new List<ElementType>();
        if (!m_elementalSpellController.CanChooseMoreElements) return elements;

        // Cada tipo entra uma vez por tela; ele pode voltar em outro level-up.
        for (int i = 0; i < 4; i++)
        {
            ElementType element = (ElementType)i;
            if (m_elementalSpellController.TryPreviewElement(element, out _))
                elements.Add(element);
        }

        return elements;
    }

    private static int CountUsableOptions(List<ElementType> elements,
        List<UpgradeDefinition> attributes, List<UpgradeDefinition> spellUpgrades)
    {
        int count = 0;
        if (elements.Count > 0) count += elements.Count;
        if (attributes.Count > 0) count += attributes.Count;
        if (spellUpgrades.Count > 0) count += spellUpgrades.Count;
        return count;
    }

    private UpgradeCategory ChooseCategory(bool hasElements, bool hasAttributes, bool hasSpellUpgrades)
    {
        float elementWeight = hasElements ? m_elementWeight : 0f;
        float attributeWeight = hasAttributes ? m_attributeWeight : 0f;
        float spellWeight = hasSpellUpgrades && m_elementalSpellController.IsMagicUnlocked
            ? m_spellUpgradeWeight
            : 0f;

        // Categorias impossíveis são removidas e o sorteio é normalizado entre as restantes.
        float totalWeight = elementWeight + attributeWeight + spellWeight;
        if (totalWeight <= 0f) return (UpgradeCategory)(-1);
        float roll = Random.value * totalWeight;
        if (roll >= totalWeight) roll = 0f;

        if (roll < elementWeight) return UpgradeCategory.Element;
        roll -= elementWeight;
        if (roll < attributeWeight) return UpgradeCategory.Attribute;
        return UpgradeCategory.SpellUpgrade;
    }

    private List<UpgradeDefinition> GetEligibleOptions(UpgradeCategory category)
    {
        List<UpgradeDefinition> eligible = new List<UpgradeDefinition>();
        HashSet<UpgradeDefinition> seenDefinitions = new HashSet<UpgradeDefinition>();
        foreach (UpgradeDefinition option in m_upgradeOptions)
        {
            if (option == null || option.Category != category || !seenDefinitions.Add(option)) continue;

            int applicationCount = GetApplicationCount(option);
            if (option.IsEligible(m_playerStats, m_elementalSpellController, applicationCount))
                eligible.Add(option);
        }

        return eligible;
    }

    private void HandleOptionSelected(UpgradeChoice choice)
    {
        if (choice == null || m_currentOptions == null ||
            !System.Array.Exists(m_currentOptions, item => item == choice))
            return;

        if (choice.IsElementChoice)
        {
            if (!m_elementalSpellController.TryChooseElement(choice.Element)) return;
        }
        else
        {
            UpgradeDefinition option = choice.Definition;
            int applicationCount = GetApplicationCount(option);
            if (option == null || !option.Apply(m_playerStats, m_elementalSpellController, applicationCount))
                return;

            m_applicationCounts[option] = applicationCount + 1;
        }

        UpdateDebugButtonAvailability();

        m_currentOptions = null;
        if (m_pendingLevels.Count > 0)
        {
            ShowNextChoice();
            return;
        }

        FinishAllChoices();
    }

    private int GetApplicationCount(UpgradeDefinition option)
    {
        if (option == null) return 0;
        return m_applicationCounts.TryGetValue(option, out int count) ? count : 0;
    }

    private void FinishAllChoices()
    {
        m_choiceUI.Hide();
        m_isWaitingForChoice = false;
        Time.timeScale = m_previousTimeScale;
        m_currentOptions = null;
        m_pendingLevels.Clear();
    }

    [ContextMenu("Validate Four Choice Rules")]
    private void ValidateFourChoiceRules()
    {
        for (int sample = 0; sample < 100; sample++)
        {
            if (!TryCreateFourChoices(out UpgradeChoice[] choices, out string failureReason))
            {
                Debug.LogError($"Validação falhou na amostra {sample + 1}: {failureReason}", this);
                return;
            }

            HashSet<ElementType> selectedElements = new HashSet<ElementType>();
            HashSet<UpgradeDefinition> selectedDefinitions = new HashSet<UpgradeDefinition>();
            foreach (UpgradeChoice choice in choices)
            {
                bool isDistinct = choice != null && (choice.IsElementChoice
                    ? selectedElements.Add(choice.Element)
                    : selectedDefinitions.Add(choice.Definition));
                bool isEligibleForState = choice != null &&
                    (!choice.IsElementChoice || m_elementalSpellController.CanChooseMoreElements) &&
                    (choice.IsElementChoice || choice.Category != UpgradeCategory.SpellUpgrade ||
                     m_elementalSpellController.IsMagicUnlocked);

                if (!isDistinct || !isEligibleForState)
                {
                    Debug.LogError($"Validação falhou na amostra {sample + 1}: opção duplicada ou incompatível com o estado atual.", this);
                    return;
                }
            }
        }

        Debug.Log("Validação aprovada em 100 amostras: cada tela tem quatro opções distintas e compatíveis com o estado atual.", this);
    }
}
