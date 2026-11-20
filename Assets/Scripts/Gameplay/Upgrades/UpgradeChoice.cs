using UnityEngine;

// Representa uma carta sorteada: pode ser um elemento ou um asset de melhoria.
public class UpgradeChoice
{
    public UpgradeCategory Category { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public Color Color { get; }
    public UpgradeDefinition Definition { get; }
    public ElementType Element { get; }
    public bool IsElementChoice { get; }

    private UpgradeChoice(UpgradeCategory category, string displayName, string description,
        Color color, UpgradeDefinition definition, ElementType element, bool isElementChoice)
    {
        Category = category;
        DisplayName = displayName;
        Description = description;
        Color = color;
        Definition = definition;
        Element = element;
        IsElementChoice = isElementChoice;
    }

    public static UpgradeChoice FromDefinition(UpgradeDefinition definition)
    {
        return new UpgradeChoice(definition.Category, definition.UpgradeName, definition.Description,
            Color.clear, definition, default, false);
    }

    public static UpgradeChoice FromElement(ElementType element, string description, Color color)
    {
        return new UpgradeChoice(UpgradeCategory.Element, GetElementName(element), description,
            color, null, element, true);
    }

    private static string GetElementName(ElementType element)
    {
        switch (element)
        {
            case ElementType.Water: return "Água";
            case ElementType.Fire: return "Fogo";
            case ElementType.Earth: return "Terra";
            case ElementType.Air: return "Ar";
            default: return "Elemento";
        }
    }
}
