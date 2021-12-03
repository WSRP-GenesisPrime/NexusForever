using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.Entity.Static;
using System;

namespace NexusForever.WorldServer.Game.Entity
{
    public class ItemInfo
    {
        public uint Id => Entry.Id;
        public Item2Entry Entry { get; }
        public Item2FamilyEntry FamilyEntry { get; }
        public Item2CategoryEntry CategoryEntry { get; }
        public Item2TypeEntry TypeEntry { get; }
        public ItemSlotEntry SlotEntry { get; }
        public ItemQualityEntry QualityEntry { get; }

        /// <summary>
        /// Create a new <see cref="ItemInfo"/> from <see cref="Item2Entry"/> entry.
        /// </summary>
        public ItemInfo(Item2Entry entry)
        {
            Entry         = entry;
            FamilyEntry   = GameTableManager.Instance.Item2Family.GetEntry(Entry.Item2FamilyId);
            CategoryEntry = GameTableManager.Instance.Item2Category.GetEntry(Entry.Item2CategoryId);
            TypeEntry     = GameTableManager.Instance.Item2Type.GetEntry(Entry.Item2TypeId);
            SlotEntry     = GameTableManager.Instance.ItemSlot.GetEntry(TypeEntry.ItemSlotId);
            QualityEntry  = GameTableManager.Instance.ItemQuality.GetEntry(Entry.ItemQualityId);
        }

        /// <summary>
        /// Returns if item can be equipped into an item slot.
        /// </summary>
        public bool IsEquippable()
        {
            return SlotEntry != null && SlotEntry?.EquippedSlotFlags != 0u;
        }

        /// <summary>
        /// Returns if item can be equipped into item slot <see cref="EquippedItem"/>.
        /// </summary>
        public bool IsEquippableIntoSlot(EquippedItem bagIndex)
        {
            return (SlotEntry?.EquippedSlotFlags & (1u << (int)bagIndex)) != 0;
        }

        /// <summary>
        /// Returns if item can be stacked with other items of the same type.
        /// </summary>
        public bool IsStackable()
        {
            // TODO: Figure out other non-stackable items, which have MaxStackCount > 1
            return !IsEquippableBag() && Entry.MaxStackCount > 1u;
        }

        /// <summary>
        /// Returns if item can be used as a bag for expanding inventory slots.
        /// </summary>
        public bool IsEquippableBag()
        {
            // client checks this flag to show bag tutorial, should be enough
            return (FamilyEntry.Flags & 0x100) != 0;
        }

        /// <summary>
        /// Returns the <see cref="CurrencyType"/> this <see cref="Item"/> sells for at a vendor.
        /// </summary>
        public CurrencyType GetVendorSellCurrency(byte index)
        {
            if (Entry.CurrencyTypeIdSellToVendor[index] != 0u)
                return (CurrencyType)Entry.CurrencyTypeIdSellToVendor[index];

            return CurrencyType.None;
        }

        /// <summary>
        /// Returns the amount of <see cref="CurrencyType"/> this <see cref="Item"/> sells for at a vendor.
        /// </summary>
        public uint GetVendorSellAmount(byte index)
        {
            if (Entry.CurrencyTypeIdSellToVendor[index] != 0u)
                return Entry.CurrencyAmountSellToVendor[index];

            // most items that sell for credits have their sell amount calculated and not stored in the tbl
            return CalculateVendorSellAmount();
        }

        public uint CalculateVendorSellAmount()
        {
            // TODO: Rawaho was lazy and didn't finish this
            // GameFormulaEntry entry = GameTableManager.Instance.GameFormula.GetEntry(559);
            // uint cost = Entry.PowerLevel * entry.Dataint01;

            // Kirmmin's Temporary Sell Value (Accurate for items between PowerLevel 20 and 50)
            float baseVal = ((((Entry.PowerLevel * Entry.PowerLevel) * Entry.ItemQualityId) * TypeEntry.VendorMultiplier) * CategoryEntry.VendorMultiplier);
            float moddedValue = MathF.Floor(baseVal * 1.125f);
            return (uint)(moddedValue > 0f ? moddedValue : 1u);
        }
    }
}
