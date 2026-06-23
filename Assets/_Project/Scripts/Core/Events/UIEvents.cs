namespace SlotRogue.Core.Events
{
    public struct SkillRewardChosenEvent
    {
        public string SkillId;
        public int ReelIndex;
    }

    public struct SkipRewardEvent { }

    public struct CharacterSelectedEvent
    {
        public string CharacterId;
    }

    public struct ShopPurchaseEvent
    {
        public string ItemId;
        public int Cost;
    }

    public struct RestChoiceEvent
    {
        public bool ChoseHeal; // true = heal, false = upgrade
    }
}
