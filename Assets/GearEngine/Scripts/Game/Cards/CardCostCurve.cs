namespace GearEngine.Cards
{
    /// <summary>sample: Gold cost for unlocking slot index (0-based). Server should own the real curve later.</summary>
    public static class CardCostCurve
    {
        public static long GoldCostForSlot(int slotIndex, long baseCost = 100, long incrementPerSlot = 50)
        {
            if (slotIndex < 0)
            {
                slotIndex = 0;
            }

            return baseCost + incrementPerSlot * slotIndex;
        }
    }
}
