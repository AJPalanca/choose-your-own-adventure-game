public enum MerchantItem
{
    MagicalArmor,
    MagicalWeapon,
    HealingPotion
}

public enum PurchaseResult
{
    Bought,
    NotEnoughGold,
    AlreadyBought,
    NoWeapon,
    CarryingTooMany
}

// The Merchant met on the first journey down the South path.
// A character may buy at most one magical item from the Merchant, plus any Healing Potions
// they can afford and carry.
public class Merchant
{
    public const int MagicalArmorPrice = 60;
    public const int MagicalWeaponPrice = 80;
    public const int MagicalArmorHealthBonus = 10;
    public const int MagicalWeaponDamageBonus = 4;
    public const int HealingPotionPrice = 20;

    // Ordinary occupation weapon -> its magical version.
    private static readonly Dictionary<string, string> MagicalWeaponNames = new()
    {
        { "long sword", "magical long sword" },
        { "lightning bolt spell", "greater lightning bolt spell" },
        { "dagger", "magical dagger" },
        { "long bow", "magical long bow" }
    };

    public static int PriceOf(MerchantItem item) => item switch
    {
        MerchantItem.MagicalArmor => MagicalArmorPrice,
        MerchantItem.MagicalWeapon => MagicalWeaponPrice,
        _ => HealingPotionPrice
    };

    // Pure purchase rules: no console input or output.
    public PurchaseResult TryBuy(Player player, MerchantItem item)
    {
        if (item == MerchantItem.HealingPotion)
        {
            if (player.HealingPotions >= Player.MaxHealingPotions)
                return PurchaseResult.CarryingTooMany;

            if (player.Gold < HealingPotionPrice)
                return PurchaseResult.NotEnoughGold;

            player.Gold -= HealingPotionPrice;
            player.HealingPotions++;
            return PurchaseResult.Bought;
        }

        if (OwnsMagicalItem(player))
            return PurchaseResult.AlreadyBought;

        if (item == MerchantItem.MagicalWeapon &&
            (player.Weapon == null || !MagicalWeaponNames.ContainsKey(player.Weapon.Type)))
            return PurchaseResult.NoWeapon;

        int price = PriceOf(item);
        if (player.Gold < price)
            return PurchaseResult.NotEnoughGold;

        player.Gold -= price;

        if (item == MerchantItem.MagicalArmor)
        {
            player.HasMagicalArmor = true;
            player.HealthPoints += MagicalArmorHealthBonus;
            player.MaxHealthPoints += MagicalArmorHealthBonus;
        }
        else
        {
            Weapon current = player.Weapon!;
            player.Weapon = new Weapon(
                MagicalWeaponNames[current.Type],
                current.MaxDamage + MagicalWeaponDamageBonus,
                current.AsciiArt + "*",
                isMagical: true);
        }

        return PurchaseResult.Bought;
    }

    private static bool OwnsMagicalItem(Player player) =>
        player.HasMagicalArmor || (player.Weapon?.IsMagical ?? false);

    // Console menu for the Merchant Encounter. Returns only when the player leaves (or input closes).
    public void RunEncounter(Player player, Messages messages)
    {
        player.HasMetMerchant = true;
        Console.WriteLine(messages.GetMessage("merchant_intro"));

        while (true)
        {
            // Only one magical item per character: once bought, both magical offers are hidden.
            bool magicalItemOnSale = !OwnsMagicalItem(player);

            Console.WriteLine(string.Format(messages.GetMessage("merchant_gold"), player.Gold));
            if (magicalItemOnSale)
            {
                Console.WriteLine(string.Format(messages.GetMessage("merchant_offer_armor"), MagicalArmorHealthBonus, MagicalArmorPrice));
                Console.WriteLine(string.Format(messages.GetMessage("merchant_offer_weapon"), MagicalWeaponDamageBonus, MagicalWeaponPrice));
            }
            Console.WriteLine(string.Format(messages.GetMessage("merchant_offer_potion"), Player.HealingPotionStrength, Player.MaxHealingPotions, HealingPotionPrice));
            Console.WriteLine(messages.GetMessage("merchant_offer_leave"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            // null means input was closed (nothing more can be typed), so leave.
            if (input == null || input == "4" || input == "leave")
            {
                Console.WriteLine(messages.GetMessage("merchant_leave"));
                return;
            }

            MerchantItem item;
            if (magicalItemOnSale && (input == "1" || input == "armor"))
                item = MerchantItem.MagicalArmor;
            else if (magicalItemOnSale && (input == "2" || input == "weapon"))
                item = MerchantItem.MagicalWeapon;
            else if (input == "3" || input == "potion")
                item = MerchantItem.HealingPotion;
            else
            {
                Console.WriteLine(messages.GetMessage("invalid"));
                continue;
            }

            PurchaseResult result = TryBuy(player, item);
            switch (result)
            {
                case PurchaseResult.Bought when item == MerchantItem.HealingPotion:
                    Console.WriteLine(string.Format(messages.GetMessage("merchant_bought_potion"), player.HealingPotions, player.Gold));
                    break;
                case PurchaseResult.CarryingTooMany:
                    Console.WriteLine(messages.GetMessage("merchant_carrying_too_many"));
                    break;
                case PurchaseResult.Bought when item == MerchantItem.MagicalArmor:
                    Console.WriteLine(string.Format(messages.GetMessage("merchant_bought_armor"), player.HealthPoints, player.Gold));
                    break;
                case PurchaseResult.Bought:
                    Console.WriteLine(string.Format(
                        messages.GetMessage("merchant_bought_weapon"),
                        messages.TranslateWeaponForDisplay(player.Weapon!.Type),
                        player.Weapon.MaxDamage,
                        player.Gold));
                    break;
                case PurchaseResult.NotEnoughGold:
                    Console.WriteLine(messages.GetMessage("merchant_not_enough_gold"));
                    break;
                case PurchaseResult.NoWeapon:
                    Console.WriteLine(messages.GetMessage("merchant_no_weapon"));
                    break;
                case PurchaseResult.AlreadyBought:
                    Console.WriteLine(messages.GetMessage("invalid"));
                    break;
            }
        }
    }
}
