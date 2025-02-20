using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;


namespace HelloWorldPlugin
{
    public class HelloWorldPlugin : BasePlugin
    {
        public override string ModuleName => "hitbox";
        public override string ModuleAuthor => "Yeezy";
        public override string ModuleVersion => "0.0.4";

        public override void Load(bool hotReload)
        {
            // Register the admin command
         
            AddCommand("size", "Sets a player's size", SetPlayerSizeCommand);
        }

        private void SetPlayerScale(CCSPlayerController player, float scale)
        {
            var skeletonInstance = player.PlayerPawn.Value!.CBodyComponent?.SceneNode?.GetSkeletonInstance();
            if (skeletonInstance != null)
            {
                skeletonInstance.Scale = scale;
            }

            player.PlayerPawn.Value.AcceptInput("SetScale", null, null, scale.ToString());

            Server.NextFrame(() =>
            {
                Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_CBodyComponent");
            });
        }
        [RequiresPermissions("@css/root")]
        [ConsoleCommand("css_size", "Resize a player")]
        [CommandHelper(minArgs: 2, usage: "<target> <size>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        private void SetPlayerSizeCommand(CCSPlayerController? admin, CommandInfo command)
        {
            if (admin == null)
            {
                Console.WriteLine("Command must be executed by a player.");
                return;
            }

            string targetName = command.GetArg(1);
            if (!float.TryParse(command.GetArg(2), out float newScale))
            {
                admin.PrintToChat("Invalid scale value! Use a number between 0.1 and 10.0.");
                return;
            }

            newScale = Math.Clamp(newScale, 0.1f, 10.0f);

            ulong targetSteamID;
            bool isSteamID = ulong.TryParse(targetName, out targetSteamID);

            foreach (var player in Utilities.GetPlayers())
            {
                if (player.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase) ||
                    (isSteamID && player.SteamID == targetSteamID))
                {
                    SetPlayerScale(player, newScale);
                    admin.PrintToChat($"[{ChatColors.Green}Admin{ChatColors.Default}] {ChatColors.Default}Set {ChatColors.Gold}{player.PlayerName}{ChatColors.Default}'s size to {ChatColors.Red}{newScale}.");
                    player.PrintToChat($"[Admin] Your size has been set to {newScale}.");
                    return;
                }
            }

            admin.PrintToChat("Player not found!");
        }
    }
}
