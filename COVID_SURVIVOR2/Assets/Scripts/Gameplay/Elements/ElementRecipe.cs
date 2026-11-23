using System;
using UnityEngine;

public enum ElementType
{
    Water,
    Fire,
    Earth,
    Air
}

// Guarda uma receita por quantidade, sem depender da ordem das escolhas.
[Serializable]
public struct ElementRecipe : IEquatable<ElementRecipe>
{
    [SerializeField, Min(0)] private int m_waterCount;
    [SerializeField, Min(0)] private int m_fireCount;
    [SerializeField, Min(0)] private int m_earthCount;
    [SerializeField, Min(0)] private int m_airCount;

    public int WaterCount => m_waterCount;
    public int FireCount => m_fireCount;
    public int EarthCount => m_earthCount;
    public int AirCount => m_airCount;
    public int TotalCount => m_waterCount + m_fireCount + m_earthCount + m_airCount;

    public ElementRecipe(int waterCount, int fireCount, int earthCount, int airCount)
    {
        m_waterCount = waterCount;
        m_fireCount = fireCount;
        m_earthCount = earthCount;
        m_airCount = airCount;
    }

    public int GetCount(ElementType element)
    {
        switch (element)
        {
            case ElementType.Water: return m_waterCount;
            case ElementType.Fire: return m_fireCount;
            case ElementType.Earth: return m_earthCount;
            case ElementType.Air: return m_airCount;
            default: return 0;
        }
    }

    public bool TryAdd(ElementType element, out ElementRecipe updatedRecipe)
    {
        updatedRecipe = this;
        if (!IsValid || TotalCount >= 3 || !Enum.IsDefined(typeof(ElementType), element))
            return false;

        switch (element)
        {
            case ElementType.Water: updatedRecipe.m_waterCount++; break;
            case ElementType.Fire: updatedRecipe.m_fireCount++; break;
            case ElementType.Earth: updatedRecipe.m_earthCount++; break;
            case ElementType.Air: updatedRecipe.m_airCount++; break;
            default: return false;
        }

        return true;
    }

    public bool IsValid => m_waterCount >= 0 && m_fireCount >= 0 &&
                           m_earthCount >= 0 && m_airCount >= 0 && TotalCount <= 3;

    public bool IsDefinitionRecipe => IsValid && TotalCount >= 1;

    public bool Equals(ElementRecipe other)
    {
        return m_waterCount == other.m_waterCount &&
               m_fireCount == other.m_fireCount &&
               m_earthCount == other.m_earthCount &&
               m_airCount == other.m_airCount;
    }

    public override bool Equals(object obj)
    {
        return obj is ElementRecipe other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = m_waterCount;
            hash = hash * 31 + m_fireCount;
            hash = hash * 31 + m_earthCount;
            hash = hash * 31 + m_airCount;
            return hash;
        }
    }

    public override string ToString()
    {
        return $"Water:{m_waterCount}, Fire:{m_fireCount}, Earth:{m_earthCount}, Air:{m_airCount}";
    }
}
