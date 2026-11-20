using UnityEngine;

// Dados de uma receita específica; não contém estado da partida nem lógica de combate.
[CreateAssetMenu(fileName = "ElementalSpellDefinition", menuName = "COVID SURVIVOR/Elemental Spell Definition")]
public class ElementalSpellDefinition : ScriptableObject
{
    [SerializeField] private ElementRecipe m_recipe;
    [SerializeField] private string m_spellName;
    [SerializeField, TextArea] private string m_description;
    [SerializeField] private Sprite m_icon;
    [SerializeField] private ElementalSpellBehavior m_behavior;

    public ElementRecipe Recipe => m_recipe;
    public string SpellName => m_spellName;
    public string Description => m_description;
    public Sprite Icon => m_icon;
    public ElementalSpellBehavior Behavior => m_behavior;
}
