using GameEnum;

[System.Serializable]
public class CardData
{
    // Stats
    public int CardSide;

    // Properties
    public bool IsSelected;

    // Spawn Info
    public string AssetKey;

    // Constructors
    public CardData() : this(GameSides.Flemish, false, string.Empty) { }

    public CardData(GameSides cardSide, bool isSelected, string assetKey)
    {
        CardSide = (int)cardSide;

        IsSelected = isSelected;

        AssetKey = assetKey;
    }
}
