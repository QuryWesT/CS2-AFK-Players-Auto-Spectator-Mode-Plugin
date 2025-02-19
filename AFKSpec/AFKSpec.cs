using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace AFKSpec
{
    public class Config: BasePluginConfig
    {
        public float szAfkTime { get; set; } = 30.0f;
        public float szWarningTime { get; set; } = 15.0f;
    }
    public class AFKSpec : BasePlugin, IPluginConfig<Config>
    {
        public override string ModuleName => "AFK Spec";
        public override string ModuleVersion => "1.0";
        public override string ModuleAuthor => "QuryWesT";

        private Dictionary<int, float> _lastActivityTimes = new();
        public Config Config { get; set; } = new Config();
        public void OnConfigParsed(Config config)
        {
            Config = config;
            Logger.LogInformation("[AFK Spec] Config dosyası yüklendi!");
            Logger.LogInformation($"[AFK Spec] AFK Süresi: {Config.szAfkTime}, Uyarı Süresi: {Config.szWarningTime}");
        }
        public override void Load(bool hotReload)
        {
            AddTimer(1.0f, CheckAFKPlayers, TimerFlags.REPEAT);
        }
    private void CheckAFKPlayers()
    {
            foreach (var IsPlayer in Utilities.GetPlayers())
            {
                if (!IsPlayer.IsValid || IsPlayer.IsBot || IsPlayer.Team == CsTeam.Spectator) continue; 

                int slot = IsPlayer.Slot;

                if (IsPlayer.PawnIsAlive) {  
                if (Math.Sqrt(IsPlayer.Pawn.Value!.Velocity.X * IsPlayer.Pawn.Value!.Velocity.X + IsPlayer.Pawn.Value!.Velocity.Y * IsPlayer.Pawn.Value!.Velocity.Y) > 0)
                {
                    _lastActivityTimes[slot] = Server.CurrentTime;
                    
                }
                else if (_lastActivityTimes.TryGetValue(slot, out var lastActivityTime))
                {
                   if (Server.CurrentTime - lastActivityTime == Config.szWarningTime)
                    {
                            IsPlayer.PrintToCenter($"[UYARI] {Config.szWarningTime} Saniye içinde izleyiciye alınacaksınız.");
                    }
                    if (Server.CurrentTime - lastActivityTime > Config.szAfkTime)
                    {
                        IsPlayer.ChangeTeam(CsTeam.Spectator);
                        IsPlayer.PrintToChat($"{ChatColors.Red}AFK olduğunuz için spectator moduna alındınız!");
                        _lastActivityTimes.Remove(slot);
                    }
                }
                else
                {
                    _lastActivityTimes[slot] = Server.CurrentTime;
                }
                }
                else
                {
                    _lastActivityTimes.Remove(slot);
                }
            }
        }
                
    }
}
