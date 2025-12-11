using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCP_682
{
    public class Main:Plugin<Config>
    {
        //无关紧要的东西
        public override string Author => "_mt馒头mt_";
        public override string Name => "SCP-682";
        public override string Prefix => "SCP682";
        public override Version Version => new Version(1,0,0);
        public override Version RequiredExiledVersion => new Version(9,10,1);
        //插件入口
        public override void OnEnabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted += start;
            Exiled.Events.Handlers.Player.Died += die;
            Exiled.Events.Handlers.Player.Hurting += hurt;

            base.OnEnabled();
        }
        public override void OnDisabled()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= start;
            Exiled.Events.Handlers.Player.Died -= die;
            Exiled.Events.Handlers.Player.Hurting -= hurt;
            base.OnDisabled();
        }
        public override void OnReloaded()
        {
            health = 6000;
            Ahp = 200;
            base.OnReloaded();
        }
        //插件事件
        public int health = 6000;
        public int Ahp = 200;
        public void start()//回合开始时
        {
            health = 6000;
            Ahp = 200;
            //核弹室的位置
            Room room = Room.Get(Exiled.API.Enums.RoomType.HczNuke);
            Vector3 vector3 = room.Position;
            //选一位D级人员当682
            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.ClassD)
                {
                    player.Role.Set(RoleTypeId.Scp939);
                    //给玩家丢核弹室去
                    player.Position= vector3+Vector3.up;
                    //给狗子戴上眼镜
                    player.EnableEffect(Exiled.API.Enums.EffectType.Scp1344,0,false);
                    //给狗子加点血量
                    player.MaxHealth = health;
                    player.Health = health;
                    //给狗子加点盾
                    player.AddAhp(Ahp, Ahp, 0, 1, 0, false);
                    //给狗子的通话频道改为SCP(我也不知道是不是这么用的)
                    player.VoiceChannel = VoiceChat.VoiceChatChannel.ScpChat;
                    AddTag(player, "SCP682");
                    break;
                }
            }
        }
        //682是不是似了
        public void die(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            if (HasTag(ev.Player, "SCP682")&&health>2000)
            {   
                //给682扣血
                health -= 2000;
                Ahp -= 50;
                //给丢管理塔上
                ev.Player.Role.Set(RoleTypeId.Tutorial);
                ev.Player.IsGodModeEnabled= true;
                ev.Player.VoiceChannel = VoiceChat.VoiceChatChannel.ScpChat;
                Timing.RunCoroutine(wait(ev.Player),"682isDead");
            }else if(HasTag(ev.Player, "SCP682") && health == 0)
            {
                ev.Player.VoiceChannel = VoiceChat.VoiceChatChannel.Spectator;
                if (ev.DamageHandler.IsSuicide)
                {
                    Cassie.MessageTranslated($"SCP 6 8 2 has been contained by SCP 6 8 2", $"SCP-682已被SCP-682收容");
                }else if (ev.DamageHandler.Type == Exiled.API.Enums.DamageType.Warhead)
                {
                    Cassie.MessageTranslated($"SCP 6 8 2 has been contained by Warhead", $"SCP-682已被核弹收容");
                }else if (ev.DamageHandler.Type == Exiled.API.Enums.DamageType.Tesla)
                {
                    Cassie.MessageTranslated($"SCP 6 8 2 has been contained by security system", $"SCP-682已被自动安保系统收容");
                }else if(ev.DamageHandler.Type == Exiled.API.Enums.DamageType.Unknown)
                {
                    Cassie.MessageTranslated($"SCP 6 8 2 has been contained", $"SCP-682已被收容,死亡原因未知");
                }
                else
                {
                    Cassie.MessageTranslated($"SCP 6 8 2 has been contained", $"SCP-682已被\"{ev.Attacker.Nickname}\"收容");
                }
                RemoveTag(ev.Player, "SCP682");
            }
            //击杀加血
            if (HasTag(ev.Attacker, "SCP682")){
                ev.Attacker.AddAhp(10,1000,0,1,0, false);
                ev.Attacker.Health += 15;
            }
        }
        //加点伤害
        public void hurt(Exiled.Events.EventArgs.Player.HurtingEventArgs ev)
        {
            if (HasTag(ev.Attacker, "SCP682"))
            {
                ev.Player.Hurt(ev.Attacker,25,Exiled.API.Enums.DamageType.Scp096);

            }
            
        }
        //682似的时候
        public IEnumerator<float> wait(Player player)
        {
            yield return Timing.WaitForSeconds(120f);
            player.IsGodModeEnabled = false;
            Room room = Room.Get(Exiled.API.Enums.RoomType.HczNuke);
            Vector3 vector3 = room.Position;
            player.Role.Set(RoleTypeId.Scp939);
            //给玩家丢到他似的地方
            player.Position = vector3 + Vector3.up;
            //给狗子戴上眼镜
            player.EnableEffect(Exiled.API.Enums.EffectType.Scp1344, 0, false);
            //给狗子加点血量
            player.MaxHealth = health;
            player.Health = health;
            //给狗子加点盾
            player.AddAhp(Ahp, Ahp, 0, 1, 0, false);
            //给狗子的通话频道改为SCP(我也不知道是不是这么用的)
            player.VoiceChannel = VoiceChat.VoiceChatChannel.ScpChat;

        }
        //给玩家打标签(不可见)
        public static void AddTag(Player player, string tag)
        {
            if (!player.SessionVariables.ContainsKey("tags"))
            {
                player.SessionVariables["tags"] = new HashSet<string>();
            }

            var tags = player.SessionVariables["tags"] as HashSet<string>;
            tags.Add(tag);

            Log.Debug($"已为玩家 {player.Nickname} 添加标签: {tag}");
        }
        //检查玩家是否含有该标签
        public static bool HasTag(Player player, string tag)
        {
            if (player.SessionVariables.ContainsKey("tags"))
            {
                var tags = player.SessionVariables["tags"] as HashSet<string>;
                return tags.Contains(tag);
            }
            return false;
        }
        //移除玩家的一个标签
        public static void RemoveTag(Player player, string tag)
        {
            if (player.SessionVariables.ContainsKey("tags"))
            {
                var tags = player.SessionVariables["tags"] as HashSet<string>;
                tags.Remove(tag);

                Log.Debug($"已从玩家 {player.Nickname} 移除标签: {tag}");
            }
        }
    }
    //命令
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class command : ICommand {

        public static void AddTag(Player player, string tag)
        {
            if (!player.SessionVariables.ContainsKey("tags"))
            {
                player.SessionVariables["tags"] = new HashSet<string>();
            }

            var tags = player.SessionVariables["tags"] as HashSet<string>;
            tags.Add(tag);

            Log.Debug($"已为玩家 {player.Nickname} 添加标签: {tag}");
        }
        public static bool HasTag(Player player, string tag)
        {
            if (player.SessionVariables.ContainsKey("tags"))
            {
                var tags = player.SessionVariables["tags"] as HashSet<string>;
                return tags.Contains(tag);
            }
            return false;
        }

        public static void RemoveTag(Player player, string tag)
        {
            if (player.SessionVariables.ContainsKey("tags"))
            {
                var tags = player.SessionVariables["tags"] as HashSet<string>;
                tags.Remove(tag);

                Log.Debug($"已从玩家 {player.Nickname} 移除标签: {tag}");
            }
        }
        public string Command => "set682";
        public string[] Aliases => new[] { "scp682"};
        public string Description => "将玩家设置为SCP682";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            string args1 = arguments.At(0);
            foreach (Player player in Player.List)
            {
                if (player.Id == int.Parse(args1))
                {
                    //核弹室的位置
                    Room room = Room.Get(Exiled.API.Enums.RoomType.HczNuke);
                    Vector3 vector3 = room.Position;
                    player.Role.Set(RoleTypeId.Scp939);
                    //给玩家丢核弹室去
                    player.Position = vector3 + Vector3.up;
                    //给狗子戴上眼镜
                    player.EnableEffect(Exiled.API.Enums.EffectType.Scp1344, 0, false);
                    //给狗子加点血量
                    player.MaxHealth = 6000;
                    player.Health = 6000;
                    //给狗子加点盾
                    player.AddAhp(200, 200, 0, 1, 0, false);
                    //给狗子的通话频道改为SCP(我也不知道是不是这么用的)
                    player.VoiceChannel = VoiceChat.VoiceChatChannel.ScpChat;
                    AddTag(player, "SCP682");
                    break;
                }
            }

            response = " ";
            return true;
        }

    }

    //配置文件
    public class Config:IConfig{
        public bool IsEnabled {  get; set; }=true;
        public bool Debug {  get; set; }=false;
    }

}
