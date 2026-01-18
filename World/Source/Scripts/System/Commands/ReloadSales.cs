using System;
using Server;
using Server.Commands;
using Server.Mobiles;

namespace Server.Commands
{
    public class ReloadSalesCommand
    {
        public static void Initialize()
        {
            CommandSystem.Register("ReloadSales", AccessLevel.GameMaster, new CommandEventHandler(ReloadSales_OnCommand));
        }

        [Usage("ReloadSales")]
        [Description("Reloads the sale info (SBInfo) for all vendors in the world.")]
        public static void ReloadSales_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendMessage("Forcing vendor reload...");

            BaseVendor.ReloadAllVendors();

            e.Mobile.SendMessage("Done.");
        }
    }
}