public enum MerchantItem
{
    MagicalArmor,
    MagicalWeapon
}

public enum PurchaseResult
{
    Bought,
    NotEnoughGold,
    AlreadyBought,
    NoWeapon
}

// The Merchant met on the first journey down the South path.
// A character may buy exactly one item from the Merchant, or leave with nothing.
public class Merchant
{
    public const int MagicalArmorPrice = 60;
    public const int MagicalWeaponPrice = 80;
    public const int MagicalArmorHealthBonus = 10;
    public const int MagicalWeaponDamageBonus = 4;

    // Ordinary occupation weapon -> its magical version.
    private static readonly Dictionary<string, string> MagicalWeaponNames = new()
    {
        { "long sword", "magical long sword" },
        { "lightning bolt spell", "greater lightning bolt spell" },
        { "dagger", "magical dagger" },
        { "long bow", "magical long bow" }
    };

    public static int PriceOf(MerchantItem item) =>
        item == MerchantItem.MagicalArmor ? MagicalArmorPrice : MagicalWeaponPrice;

    // Pure purchase rules: no console input or output.
    public PurchaseResult TryBuy(Player player, MerchantItem item)
    {
        if (player.HasMagicalArmor || (player.Weapon?.IsMagical ?? false))
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

    // Console menu for the Merchant Encounter. Returns once the player buys an item or leaves.
    public void RunEncounter(Player player, Messages messages)
    {
        player.HasMetMerchant = true;
        Console.WriteLine(messages.GetMessage("merchant_intro"));

        while (true)
        {
            Console.WriteLine(string.Format(messages.GetMessage("merchant_gold"), player.Gold));
            Console.WriteLine(string.Format(messages.GetMessage("merchant_offer_armor"), MagicalArmorHealthBonus, MagicalArmorPrice));
            Console.WriteLine(string.Format(messages.GetMessage("merchant_offer_weapon"), MagicalWeaponDamageBonus, MagicalWeaponPrice));
            Console.WriteLine(messages.GetMessage("merchant_offer_leave"));
            Console.Write(messages.GetMessage("enter_choice"));
            string? input = Console.ReadLine()?.Trim().ToLower();

            // null means input was closed (nothing more can be typed), so leave.
            if (input == null || input == "3" || input == "leave")
            {
                Console.WriteLine(messages.GetMessage("merchant_leave"));
                return;
            }

            MerchantItem item;
            if (input == "1" || input == "armor")
                item = MerchantItem.MagicalArmor;
            else if (input == "2" || input == "weapon")
                item = MerchantItem.MagicalWeapon;
            else
            {
                Console.WriteLine(messages.GetMessage("invalid"));
                continue;
            }

            PurchaseResult result = TryBuy(player, item);
            switch (result)
            {
                case PurchaseResult.Bought when item == MerchantItem.MagicalArmor:
                    Console.WriteLine(string.Format(messages.GetMessage("merchant_bought_armor"), player.HealthPoints, player.Gold));
                    return;
                case PurchaseResult.Bought:
                    Console.WriteLine(string.Format(
                        messages.GetMessage("merchant_bought_weapon"),
                        messages.TranslateWeaponForDisplay(player.Weapon!.Type),
                        player.Weapon.MaxDamage,
                        player.Gold));
                    return;
                case PurchaseResult.NotEnoughGold:
                    Console.WriteLine(messages.GetMessage("merchant_not_enough_gold"));
                    break;
                case PurchaseResult.NoWeapon:
                    Console.WriteLine(messages.GetMessage("merchant_no_weapon"));
                    break;
                case PurchaseResult.AlreadyBought:
                    Console.WriteLine(messages.GetMessage("merchant_leave"));
                    return;
            }
        }
    }
}
