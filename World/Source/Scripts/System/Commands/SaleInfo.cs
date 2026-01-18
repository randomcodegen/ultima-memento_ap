using Server;
using Server.Commands;
using Server.Items;
using Server.Targeting;
using System;
using System.Linq;

namespace Server.Commands
{
    public class SaleLineGenerator
    {
        public static void Initialize()
        {
            CommandSystem.Register("SaleInfo", AccessLevel.Administrator, new CommandEventHandler(OnCommand));
        }

        [Usage("SaleInfo")]
        [Description("Targets an item and generates a formatted ItemSales string in the console.")]
        public static void OnCommand(CommandEventArgs e)
        {
            e.Mobile.BeginTarget(-1, false, TargetFlags.None, new TargetCallback(OnTarget));
            e.Mobile.SendMessage("Target an item to generate its sale data line in the console.");
        }

        public static void OnTarget(Mobile from, object targeted)
        {
            if (targeted is Item)
            {
                Item item = (Item)targeted;

                // 1. Get the Enum values based on Inheritance
                string category = GetCategory(item);
                string material = GetMaterial(item);
                string market = GetMarket(category);

                // 2. Format and Output
                string typeName = item.GetType().Name.PadRight(30);

                string output = String.Format(
                    "{0} |   100  |    5 |    0 | false  | true   | World.None         | {1} | {2} | {3}",
                    typeName,
                    category.PadRight(20),
                    material.PadRight(20),
                    market
                );

                Console.WriteLine("\n--- Sale Line Generated ---");
                Console.WriteLine(output);
                Console.WriteLine("---------------------------\n");

                from.SendMessage("Line generated for: {0}", item.GetType().Name);
            }
        }

        private static string GetCategory(Item item)
        {
            // 1. Check Inheritance first
            if (item is BaseWeapon) return "Category.Weapon";
            if (item is BaseArmor) return "Category.Armor";
            if (item is BasePotion) return "Category.Potion";
            if (item is BaseReagent) return "Category.Reagent";

            // Check for resources (Ingots, Ore, Wood, Leather)
            if (item is BaseIngot || item is BaseOre || item is BaseLeather || item is BaseLog)
                return "Category.Resource";

            // 2. Fallback: Keyword Search for custom/Relic items
            string name = item.GetType().Name.ToLower();

            if (name.Contains("relic") || name.Contains("alchemy") || name.Contains("scroll"))
                return "Category.Reagent";

            if (name.Contains("potion") || name.Contains("vial") || name.Contains("elixir"))
                return "Category.Potion";

            return "Category.None";
        }

        private static string GetMarket(string category)
        {
            switch (category)
            {
                case "Category.Potion":
                case "Category.Reagent":
                    return "Market.Alchemy";
                case "Category.Weapon":
                case "Category.Armor":
                    return "Market.Smith";
                case "Category.Resource":
                    // Smith Alchemy or Carpenter?
                    return "Market.Smith";
                default:
                    return "Market.None";
            }
        }

        private static string GetMaterial(Item item)
        {
            if (item is BaseArmor || item is BaseWeapon)
            {
                // Default to metal
                return "Material.Metal";
            }

            return "Material.None";
        }
    }
}