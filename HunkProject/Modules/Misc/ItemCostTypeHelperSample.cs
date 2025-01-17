﻿using RoR2;

namespace HunkMod.Modules.Misc
{
    internal class ItemCostTypeHelperSample
    {
        public static bool IsAffordable(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
        {
            CharacterBody cb = context.activator.GetComponent<CharacterBody>();
            if (!cb)
                return false;

            Inventory inv = cb.inventory;
            if (!inv)
                return false;

            int cost = context.cost;
            int itemCount = inv.GetItemCount(Modules.Survivors.Hunk.gVirusSample);

            if (itemCount >= cost)
                return true;
            else
                return false;
        }

        public static void PayCost(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
        {
            context.activatorMaster.inventory.RemoveItem(Modules.Survivors.Hunk.gVirusSample);

            Modules.Helpers.CreateItemTakenOrb(context.activatorBody.corePosition, context.purchasedObject, Modules.Survivors.Hunk.gVirusSample.itemIndex);
        }
    }
}