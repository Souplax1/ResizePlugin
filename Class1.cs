using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace HelloWorldPlugin
{
    public class HelloWorldPlugin : BasePlugin
    {
        public override string ModuleName => "hitbox";
        public override string ModuleAuthor => "Yeezy";
        public override string ModuleVersion => "0.0.5";

        public override void Load(bool hotReload)
        {
            AddCommand("size", "Sets a player's size", SetPlayerSizeCommand);
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
        }

        private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
        {
            foreach (var player in Utilities.GetPlayers())
            {
                if (player?.PlayerPawn?.Value == null) continue;


                SetPlayerScale(player, 1.0f);
            }

            return HookResult.Continue;
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
        [ConsoleCommand("css_size", "Resize a player or team")]
        [CommandHelper(minArgs: 2, usage: "<@all | @CT | @T | player_name | SteamID> <size>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
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

            List<CCSPlayerController> targetPlayers = new List<CCSPlayerController>();

            if (targetName.Equals("@all", StringComparison.OrdinalIgnoreCase))
            {
                targetPlayers.AddRange(Utilities.GetPlayers());
            }
  
            else if (targetName.Equals("@CT", StringComparison.OrdinalIgnoreCase))
            {
                targetPlayers.AddRange(Utilities.GetPlayers().Where(p => p.Team == CsTeam.CounterTerrorist));
            }
   
            else if (targetName.Equals("@T", StringComparison.OrdinalIgnoreCase))
            {
                targetPlayers.AddRange(Utilities.GetPlayers().Where(p => p.Team == CsTeam.Terrorist));
            }
            else
            {
                // Check by name or SteamID
                ulong targetSteamID;
                bool isSteamID = ulong.TryParse(targetName, out targetSteamID);

                var player = Utilities.GetPlayers().FirstOrDefault(p =>
                    p.PlayerName.Equals(targetName, StringComparison.OrdinalIgnoreCase) ||
                    (isSteamID && p.SteamID == targetSteamID));

                if (player != null)
                {
                    targetPlayers.Add(player);
                }
            }

            // Apply scaling to selected players
            if (targetPlayers.Count > 0)
            {
                foreach (var player in targetPlayers)
                {
                    SetPlayerScale(player, newScale);
                    player.PrintToChat($"[Admin] Your size has been set to {newScale}.");
                }

                admin.PrintToChat($"[Admin] Set size to {newScale} for {targetPlayers.Count} player(s).");
            }
            else
            {
                admin.PrintToChat("Player or team not found!");
            }
        }
      }
    }
