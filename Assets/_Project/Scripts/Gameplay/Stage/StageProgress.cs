using System;
using Herbalist.Presentation;
namespace Herbalist.StageOne
{
    // Rules are independent of input, scene objects, and Fusion. Zero is an empty hand.
    public sealed class StageProgress
    {
        public readonly int[] Held = new int[2];
        public readonly int[] Consumed = new int[2];
        public ulong Harvested;
        public uint Crafted;
        public bool Cleared;
        private readonly StageOneSettings settings;
        public StageProgress(StageOneSettings settings) { this.settings = settings; }
        public bool Gather(int slot, int sourceIndex, int item)
        {
            if (!Slot(slot) || sourceIndex < 0 || sourceIndex >= 64 || Held[slot] != 0 || settings.slotRoles[slot] != CharacterRole.Duyeong || settings.Item(item)?.type != StageItemType.Herb || (Harvested & (1UL << sourceIndex)) != 0) return false;
            Held[slot] = item; Harvested |= 1UL << sourceIndex; return true;
        }
        public bool Give(int from, int to)
        {
            if (!Slot(from) || !Slot(to) || from == to || Held[from] == 0 || Held[to] != 0) return false;
            var item = settings.Item(Held[from]);
            if (item == null || (item.type == StageItemType.Herb && settings.slotRoles[to] != CharacterRole.Sodam)) return false;
            Held[to] = Held[from]; Held[from] = 0; return true;
        }
        public bool Craft(int slot)
        {
            if (!Slot(slot) || settings.slotRoles[slot] != CharacterRole.Sodam) return false;
            for (int i = 0; i < settings.recipes.Length; i++)
            {
                var recipe = settings.recipes[i];
                if (Held[slot] != settings.ItemId(recipe.ingredient)) continue;
                Held[slot] = settings.ItemId(recipe.result); Crafted |= 1u << i; return true;
            }
            return false;
        }
        public bool Consume(int slot, Func<StageItemDefinition, bool> grant)
        {
            if (!Slot(slot) || Consumed[slot] != 0) return false;
            var item = settings.Item(Held[slot]);
            if (item == null || item.type != StageItemType.Potion || !grant(item)) return false;
            Consumed[slot] = Held[slot]; Held[slot] = 0; return true;
        }
        public uint ConsumedMask
        {
            get
            {
                uint mask = 0;
                for (int i = 0; i < settings.recipes.Length; i++)
                    foreach (var item in Consumed) if (item == settings.ItemId(settings.recipes[i].result)) mask |= 1u << i;
                return mask;
            }
        }
        public bool GateOpen => (Crafted & settings.RequiredMask) == settings.RequiredMask && (ConsumedMask & settings.RequiredMask) == settings.RequiredMask;
        public bool CheckClear(bool firstInside, bool secondInside)
        {
            if (!Cleared && GateOpen && firstInside && secondInside) { Cleared = true; return true; }
            return false;
        }
        private static bool Slot(int slot) => slot >= 0 && slot < 2;
    }
}
