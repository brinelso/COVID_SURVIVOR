using System.Collections.Generic;
using System.Text;
using UnityEngine;

// Reúne as receitas e as cores-base configuradas no projeto.
[CreateAssetMenu(fileName = "ElementalRecipeCatalog", menuName = "COVID SURVIVOR/Elemental Recipe Catalog")]
public class ElementalRecipeCatalog : ScriptableObject
{
    private const int ExpectedRecipeCount = 34;

    [SerializeField] private ElementalSpellDefinition[] m_definitions;

    [Header("Cores-base dos elementos")]
    [SerializeField] private Color m_waterColor = new Color(0.145098f, 0.560784f, 0.909804f, 1f);
    [SerializeField] private Color m_fireColor = new Color(0.941176f, 0.352941f, 0.196078f, 1f);
    [SerializeField] private Color m_earthColor = new Color(0.596078f, 0.380392f, 0.231373f, 1f);
    [SerializeField] private Color m_airColor = new Color(0.741176f, 0.913725f, 0.956863f, 1f);

    public int DefinitionCount => m_definitions == null ? 0 : m_definitions.Length;

    public bool TryFindDefinition(ElementRecipe recipe, out ElementalSpellDefinition definition)
    {
        definition = null;
        if (!recipe.IsDefinitionRecipe || m_definitions == null) return false;

        foreach (ElementalSpellDefinition candidate in m_definitions)
        {
            if (candidate == null || !candidate.Recipe.Equals(recipe)) continue;

            definition = candidate;
            return true;
        }

        return false;
    }

    public Color GetRecipeColor(ElementRecipe recipe)
    {
        if (!recipe.IsDefinitionRecipe) return Color.white;

        Color weightedColor = m_waterColor * recipe.WaterCount +
                              m_fireColor * recipe.FireCount +
                              m_earthColor * recipe.EarthCount +
                              m_airColor * recipe.AirCount;

        return weightedColor / recipe.TotalCount;
    }

    public Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Water: return m_waterColor;
            case ElementType.Fire: return m_fireColor;
            case ElementType.Earth: return m_earthColor;
            case ElementType.Air: return m_airColor;
            default: return Color.white;
        }
    }

    // Pode ser chamado pelo menu de contexto do asset para conferir o catálogo.
    [ContextMenu("Validate Elemental Catalog")]
    private void ValidateCatalogFromInspector()
    {
        bool isValid = ValidateCatalog(out string report);
        if (isValid)
            Debug.Log(report, this);
        else
            Debug.LogError(report, this);
    }

    public bool ValidateCatalog(out string report)
    {
        StringBuilder details = new StringBuilder();
        HashSet<ElementRecipe> foundRecipes = new HashSet<ElementRecipe>();
        HashSet<ElementRecipe> expectedRecipes = CreateExpectedRecipes();
        int nullDefinitions = 0;
        int invalidDefinitions = 0;
        int duplicateDefinitions = 0;

        if (m_definitions == null)
        {
            report = "O catálogo não possui uma lista de definições.";
            return false;
        }

        for (int i = 0; i < m_definitions.Length; i++)
        {
            ElementalSpellDefinition definition = m_definitions[i];
            if (definition == null)
            {
                nullDefinitions++;
                details.AppendLine($"Entrada {i} está vazia.");
                continue;
            }

            ElementRecipe recipe = definition.Recipe;
            if (!recipe.IsDefinitionRecipe)
            {
                invalidDefinitions++;
                details.AppendLine($"Receita inválida em '{definition.name}': {recipe}.");
                continue;
            }

            if (!foundRecipes.Add(recipe))
            {
                duplicateDefinitions++;
                details.AppendLine($"Receita duplicada em '{definition.name}': {recipe}.");
            }
        }

        int missingRecipes = 0;
        foreach (ElementRecipe expectedRecipe in expectedRecipes)
        {
            if (foundRecipes.Contains(expectedRecipe)) continue;

            missingRecipes++;
            details.AppendLine($"Receita ausente: {expectedRecipe}.");
        }

        bool hasExpectedEntryCount = m_definitions.Length == ExpectedRecipeCount;
        bool isValidCatalog = hasExpectedEntryCount && nullDefinitions == 0 &&
                              invalidDefinitions == 0 && duplicateDefinitions == 0 && missingRecipes == 0;

        if (!hasExpectedEntryCount)
            details.Insert(0, $"Esperadas {ExpectedRecipeCount} entradas, encontradas {m_definitions.Length}.\n");

        if (!isValidCatalog)
        {
            report = $"Catálogo inválido. Vazias: {nullDefinitions}; inválidas: {invalidDefinitions}; " +
                     $"duplicadas: {duplicateDefinitions}; ausentes: {missingRecipes}.\n{details}";
            return false;
        }

        bool examplesAreValid = ValidateRecipeExamples(out string exampleReport);
        report = $"Catálogo válido: {foundRecipes.Count} receitas únicas e completas.\n{exampleReport}";
        return examplesAreValid;
    }

    private static HashSet<ElementRecipe> CreateExpectedRecipes()
    {
        HashSet<ElementRecipe> recipes = new HashSet<ElementRecipe>();

        // Todas as contagens de zero a três formam exatamente as 34 receitas aprovadas.
        for (int water = 0; water <= 3; water++)
        for (int fire = 0; fire <= 3; fire++)
        for (int earth = 0; earth <= 3; earth++)
        for (int air = 0; air <= 3; air++)
        {
            ElementRecipe recipe = new ElementRecipe(water, fire, earth, air);
            if (recipe.IsDefinitionRecipe)
                recipes.Add(recipe);
        }

        return recipes;
    }

    private bool ValidateRecipeExamples(out string exampleReport)
    {
        bool examplesAreValid = true;
        StringBuilder failures = new StringBuilder();

        ElementRecipe emptyRecipe = default;
        examplesAreValid &= CheckAdd(emptyRecipe, ElementType.Water, 1, out ElementRecipe waterRecipe);
        examplesAreValid &= CheckAdd(waterRecipe, ElementType.Water, 2, out ElementRecipe repeatedWaterRecipe);
        examplesAreValid &= CheckAdd(waterRecipe, ElementType.Fire, 2, out ElementRecipe waterFireRecipe);
        examplesAreValid &= CheckAdd(emptyRecipe, ElementType.Fire, 1, out ElementRecipe fireRecipe);
        examplesAreValid &= CheckAdd(fireRecipe, ElementType.Water, 2, out ElementRecipe fireWaterRecipe);
        examplesAreValid &= waterFireRecipe.Equals(fireWaterRecipe);

        ElementRecipe waterEarth = new ElementRecipe(1, 0, 1, 0);
        examplesAreValid &= Mathf.Approximately(GetRecipeColor(waterEarth).r,
            ((m_waterColor.r + m_earthColor.r) / 2f));

        ElementRecipe repeatedRecipe = new ElementRecipe(2, 0, 1, 0);
        examplesAreValid &= repeatedRecipe.Equals(repeatedWaterRecipe.TryAdd(ElementType.Earth, out ElementRecipe repeatedWaterEarth)
            ? repeatedWaterEarth
            : default);
        examplesAreValid &= repeatedRecipe.WaterCount == 2 && repeatedRecipe.TotalCount == 3;

        examplesAreValid &= waterFireRecipe.TryAdd(ElementType.Earth, out ElementRecipe threeElements);
        examplesAreValid &= threeElements.Equals(new ElementRecipe(1, 1, 1, 0));
        examplesAreValid &= threeElements.IsDefinitionRecipe && !threeElements.TryAdd(ElementType.Air, out _);
        ElementRecipe otherOrder = new ElementRecipe(0, 1, 1, 0);
        examplesAreValid &= otherOrder.TryAdd(ElementType.Water, out otherOrder);
        examplesAreValid &= otherOrder.Equals(threeElements);
        examplesAreValid &= TryFindDefinition(waterRecipe, out _);

        if (!examplesAreValid)
            failures.Append("Um ou mais testes internos de receitas falharam.");

        exampleReport = examplesAreValid
            ? "Testes de receita aprovados: elemento único, repetição, comparação por ordem, cor proporcional, limite de três e busca da receita inicial."
            : failures.ToString();

        return examplesAreValid;
    }

    private static bool CheckAdd(ElementRecipe source, ElementType element, int expectedCount,
        out ElementRecipe result)
    {
        return source.TryAdd(element, out result) && result.TotalCount == expectedCount &&
               result.GetCount(element) > 0;
    }
}
