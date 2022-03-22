using DiscordRPC;                    // Discord

using System;                        // Time
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;        // Threading

using Terraria;                      // Terraria
using Terraria.ID;                   // Networking
using Terraria.ModLoader;            // TML

namespace JourneyDRPC
{
	public class JourneysRPC : Mod
	{
		public DiscordRpcClient Client { get; private set; } // RPC Client

		public string Details = "";
		public string State = "";

		public string PartyCode = "";
		public byte PartyMax = 0;
		public byte PartySize = 0;
		public Party.PrivacySetting PartyPrivacy = Party.PrivacySetting.Private;

		public string LargeAsset = "";
		public string LargeText = "";
		public string SmallAsset = "";
		public string SmallText = "";

		private byte cooldown = 0;

		private string prefix = "";
		private string biome = "";
		

		public ulong TimeStamp = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();

		public override void Load()
		{
			Log("Starting new RPC Client");
			Start();
			Log($"Starting new update thread...");
			Update();
			Log($"Thread started!");
		}

		public override void Unload()
        {
			Log("Killing RPC Client...");
			if (Client is not null)
				Client.Dispose();
			Log("Client killed.");
		}

		void Start()
		{
			Client = new DiscordRpcClient("880995953937616917", autoEvents: false);
			Client.Initialize();
			Log("RPC initialized, starting status display defaults...");
			Log("Client ready, invoking...");
			Client.Invoke();
			Log("All systems nominal!");
		}

		void Update()
		{
			var t = Task.Run(async delegate // Create a new thread
			{
				GetSetData(this.cooldown == 0);
				if (Client == null || Client.IsDisposed)
				{
					Log($"Old update thread closed due to Client disposal!");
					return;
				}
				Client.SetPresence(new RichPresence()
				{
					Details = Details,
					State = State,
					Party = new Party()
					{
						ID = PartyCode,
						Size = PartySize,
						Max = PartyMax,
						Privacy = PartyPrivacy
					},
					Assets = new Assets()
					{
						LargeImageKey = LargeAsset,
						LargeImageText = LargeText,
						SmallImageKey = SmallAsset,
						SmallImageText = SmallText
					},
					Timestamps = new Timestamps()
					{
						StartUnixMilliseconds = TimeStamp
					}/*,
					Secrets = new Secrets()
					{
						JoinSecret = ($"{Netplay.ServerIPText}:{Netplay.ListenPort}").GetHashCode().ToString(),
						MatchSecret = this.PartyCode.GetHashCode().ToString()
					}*/
				});
				await Task.Delay(1000);
				Update();
			});
		}

		public void GetSetData(bool full = true)
        {
			if (!Main.gameMenu)
			{
				NPC boss = new NPC();
				bool foundBoss1 = false;
				bool foundBoss2 = false;

				for (int indexNPC = 0; indexNPC < Main.npc.Length; indexNPC++)
				{
					if (Main.npc[indexNPC].type <= NPCID.None)
						continue;
					if (Main.npc[indexNPC].active && Main.npc[indexNPC].boss && !foundBoss1)
					{
						foundBoss1 = true;
						boss = Main.npc[indexNPC];
					}
                    else if (Main.npc[indexNPC].active && Main.npc[indexNPC].boss)
                    {
						foundBoss2 = true;
						break;
                    }
				}

				if (Netplay.ServerPassword == "" && Main.netMode != NetmodeID.SinglePlayer)
					this.PartyPrivacy = Party.PrivacySetting.Public;
				else
					this.PartyPrivacy = Party.PrivacySetting.Private;

				this.LargeAsset = "terraria";

				if (foundBoss1 && !foundBoss2) // Boss checks take priority
				{
					this.LargeAsset = $"{boss.TypeName.ToLower().Replace(" ", "").Replace("'", "")}";
					this.prefix = "";
					this.biome = boss.FullName;
				}
				else if (foundBoss2) // Boss checks take priority
				{
					this.LargeAsset = $"{boss.TypeName.ToLower().Replace(" ", "").Replace("'", "")}";
					this.prefix = "";
					this.biome = "multiple Bosses";
				}
				/*else if (Main.player[Main.myPlayer].InModBiome()) // Mod Biome
				{

				}*/
				else if (Main.player[Main.myPlayer].ZoneTowerNebula)
				{
					this.LargeAsset = "nebula";
					this.prefix = "in ";
					this.biome = "the Nebula Zone";
				}
				else if (Main.player[Main.myPlayer].ZoneTowerSolar)
				{
					this.LargeAsset = "solar";
					this.prefix = "in ";
					this.biome = "the Solar Zone";
				}
				else if (Main.player[Main.myPlayer].ZoneTowerStardust)
				{
					this.LargeAsset = "stardust";
					this.prefix = "in ";
					this.biome = "the Stardust Zone";
				}
				else if (Main.player[Main.myPlayer].ZoneTowerVortex)
				{
					this.LargeAsset = "vortex";
					this.prefix = "in ";
					this.biome = "the Vortex Zone";
				}
				else if (Main.player[Main.myPlayer].ZoneDungeon)
				{
					this.LargeAsset = "dungeon";
					this.prefix = "in ";
					this.biome = "the Dungeon";
				}
				else if (Main.player[Main.myPlayer].ZoneMeteor)
				{
					this.LargeAsset = "meteor";
					this.prefix = "by ";
					this.biome = "a Meteor";
				}
				else if (Main.player[Main.myPlayer].ZoneGemCave)
				{
					this.LargeAsset = "gemcave";
					this.prefix = "in ";
					this.biome = "a Gem Cave";
				}
				else if (Main.player[Main.myPlayer].ZoneGlowshroom)
				{
					this.LargeAsset = "glowshroom";
					this.prefix = "in ";
					this.biome = "a Glowshroom Forest";
				}
				else if (Main.player[Main.myPlayer].ZoneGranite)
				{
					this.LargeAsset = "granite";
					this.prefix = "in ";
					this.biome = "a Granite Cave";
				}
				else if (Main.player[Main.myPlayer].ZoneMarble)
				{
					this.LargeAsset = "marble";
					this.prefix = "in ";
					this.biome = "a Marble Cave";
				}
				else if (Main.player[Main.myPlayer].ZoneGraveyard)
				{
					this.LargeAsset = "surface";
					this.prefix = "in ";
					this.biome = "a Graveyard";
				}
				else if (Main.player[Main.myPlayer].ZoneSnow)
				{
					this.LargeAsset = "tundra";
					this.prefix = "in ";
					this.biome = "the Tundra";
				}
				else if (Main.player[Main.myPlayer].ZoneBeach)
                {
					this.LargeAsset = "beach";
					this.prefix = "on ";
					this.biome = "the Beach";
				}
				else if (Main.player[Main.myPlayer].ZoneUndergroundDesert)
				{
					this.LargeAsset = "underdesert";
					this.prefix = "in ";
					this.biome = "the Underground Desert";
				}
				else if (Main.player[Main.myPlayer].ZoneDesert)
				{
					this.LargeAsset = "desert";
					this.prefix = "in ";
					this.biome = "the Desert";
				}
				else if (Main.player[Main.myPlayer].ZoneSkyHeight)
				{
					//Scene metric check for clouds?
					this.LargeAsset = "space";
					this.prefix = "in ";
					this.biome = "Space";
				}
				else if (Main.player[Main.myPlayer].ZoneUnderworldHeight)
				{
					this.LargeAsset = "underworld";
					this.prefix = "in ";
					this.biome = "the Underworld";
				}
				else if (Main.player[Main.myPlayer].ZoneCorrupt)
				{
					this.LargeAsset = "corruption";
					this.prefix = "in ";
					this.biome = "the Corruption";
				}
				else if (Main.player[Main.myPlayer].ZoneCrimson)
				{
					this.LargeAsset = "crimson";
					this.prefix = "in ";
					this.biome = "the Crimson";
				}
				else if (Main.player[Main.myPlayer].ZoneHallow)
				{
					this.LargeAsset = "hallow";
					this.prefix = "in ";
					this.biome = "the Hallow";
				}
				else if (Main.player[Main.myPlayer].ZoneLihzhardTemple)
				{
					this.LargeAsset = "temple";
					this.prefix = "in ";
					this.biome = "the Jungle Temple";
				}
				else if (Main.player[Main.myPlayer].ZoneHive)
				{
					this.LargeAsset = "hive";
					this.prefix = "in ";
					this.biome = "a Hive";
				}
				else if (Main.player[Main.myPlayer].ZoneJungle)
				{
					this.LargeAsset = "jungle";
					this.prefix = "in ";
					this.biome = "the Jungle";
				}
				else if (Main.player[Main.myPlayer].ZoneDirtLayerHeight)
				{
					this.LargeAsset = "surfacecaves";
					this.prefix = "in ";
					this.biome = "the Surface Caves";
				}
				else if (Main.player[Main.myPlayer].ZoneOverworldHeight)
				{
					this.LargeAsset = "surface";
					this.prefix = "on ";
					this.biome = "the Surface";
				}
				else if (Main.player[Main.myPlayer].ZoneRockLayerHeight)
				{
					this.LargeAsset = "caves";
					this.prefix = "";
					this.biome = "Underground";
				}
				else if (Main.player[Main.myPlayer].ZonePurity)
				{
					this.LargeAsset = "surface";
					this.prefix = "in ";
					this.biome = "Purity";
				}
				if (Main.player[Main.myPlayer].townNPCs > 21)
				{
					this.prefix = "in ";
					this.biome = "a City";
				}
				else if (Main.player[Main.myPlayer].townNPCs > 10)
				{
					this.prefix = "in ";
					this.biome = "a Town";
				}
				else if (Main.player[Main.myPlayer].townNPCs > 5)
				{
					this.prefix = "in ";
					this.biome = "a Settlement";
				}

				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					this.LargeText = $"On \"{Main.worldName}\", \n{(foundBoss1 ? "Fighting" : "Currently")} {this.prefix}{this.biome}";
					this.PartyCode = Main.worldName.Replace(' ', '_') + WorldGen.currentWorldSeed;
					this.PartySize = (byte)Main.player.Count(p => p.whoAmI < Main.maxPlayers && p.active);
					this.PartyMax = Netplay.MaxConnections - 1;
					this.State = $"{ModLoader.Mods.Length - 1} Mods - Multiplayer";
				}
				else
				{
					this.LargeText = $"On \"{Main.worldName}\", \n{(foundBoss1 ? "Fighting" : "Currently")} {this.prefix}{this.biome}";
					this.PartyCode = "";
					this.PartySize = 0;
					this.PartyMax = 0;
					this.State = $"{ModLoader.Mods.Length - 1} Mods - Singleplayer";
				}

				if (Main.gamePaused)
                {
					this.Details = "Currently Paused";
				}
				else if (Main.player[Main.myPlayer].dead && this.cooldown == 0)
                {
					if (foundBoss1)
						this.Details = $"Died fighting {this.biome}";
					else if (Main.player[Main.myPlayer].breath < 1)
						this.Details = "Died by drowning";
					else
						this.Details = "Pondering how they died";
					this.cooldown = (byte)Main.player[Main.myPlayer].respawnTimer;
				}
				else if (Main.player[Main.myPlayer].ghost)
				{ 
					this.Details = "Busy being a ghost";
				}
				else if (this.cooldown == 0)
				{
					if (foundBoss1)
					{
						if (this.HealthAsPercent(Main.player[Main.myPlayer]) < 8 && foundBoss1)
							this.Details = $"Accepting their impending death";
						else if (this.HealthAsPercent(Main.player[Main.myPlayer]) < 35)
							this.Details = $"Fleeing from {this.biome}";
						else
							this.Details = $"Fighting {this.biome}";
					}
					else if (Math.Abs(Main.player[Main.myPlayer].velocity.Length()) <= 0.1)
					{
						if (this.HealthAsPercent(Main.player[Main.myPlayer]) < 8 && Main.player[Main.myPlayer].nearbyActiveNPCs >= 8)
							this.Details = $"Accepting their impending death";
						else if (this.HealthAsPercent(Main.player[Main.myPlayer]) >= 95 && Main.player[Main.myPlayer].nearbyActiveNPCs >= 8)
							this.Details = $"Farming {this.prefix}{this.biome}";
						else
							this.Details = $"Idling {this.prefix}{this.biome}";
					}
					
					else if (Main.player[Main.myPlayer].breath < 1)
						this.Details = $"Drowning {this.prefix}{this.biome}";
					else if (Main.player[Main.myPlayer].breath < Main.player[Main.myPlayer].breathMax)
						this.Details = $"Swimming through {this.biome}";
					else if (Main.player[Main.myPlayer].velocity.Y >= 10)
						this.Details = $"Falling through {this.biome}";
					else if (Main.player[Main.myPlayer].velocity.Y <= -10)
						this.Details = $"Flying through {this.biome}";
					else if (Math.Abs(Main.player[Main.myPlayer].velocity.Y) < 10 && Math.Abs(Main.player[Main.myPlayer].velocity.Y) >= 6)
						this.Details = $"Parkouring through {this.biome}";
					else if (Math.Abs(Main.player[Main.myPlayer].velocity.X) <= 1.1 && Math.Abs(Main.player[Main.myPlayer].velocity.X) < 0.1)
						this.Details = $"Crawling through {this.biome}";
					else if (Math.Abs(Main.player[Main.myPlayer].velocity.X) <= 3.5)
					{
						if (Main.player[Main.myPlayer].ZoneRockLayerHeight)
							this.Details = $"Spelunking {this.prefix}{this.biome}";
						else if (Main.player[Main.myPlayer].ZoneDirtLayerHeight)
							this.Details = $"Exploring {this.biome}";
					}
					else if (Math.Abs(Main.player[Main.myPlayer].velocity.X) <= 4.5)
					{
						if (Main.player[Main.myPlayer].ZoneRockLayerHeight)
							this.Details = $"Scouring {this.biome}";
						else if (Main.player[Main.myPlayer].ZoneDirtLayerHeight)
							this.Details = $"Searching {this.biome}";
					}
					else if (Main.player[Main.myPlayer].velocity.X < 1)
						this.Details = $"Running through {this.biome}";
					else
						this.Details = $"Moving through {this.biome}";

					this.cooldown = 2;
				}

				this.SmallAsset = "heart";
				this.SmallText = $"{Main.player[Main.myPlayer].statLife}/{Main.player[Main.myPlayer].statLifeMax} HP \n| " +
					$"{Main.player[Main.myPlayer].statDefense} DEF \n| " +
					$"{Main.player[Main.myPlayer].statMana}/{Main.player[Main.myPlayer].statManaMax} MP";

				if (Main.netMode != NetmodeID.SinglePlayer)
                    switch (Main.player[Main.myPlayer].team)
                    {
						case 0:
							this.SmallText = $"{this.SmallText} \n| No Team";
							break;
						case 1:
							this.SmallText = $"{this.SmallText} \n| Red Team";
							break;
						case 2:
							this.SmallText = $"{this.SmallText} \n| Green Team";
							break;
						case 3:
							this.SmallText = $"{this.SmallText} \n| Blue Team";
							break;
						case 4:
							this.SmallText = $"{this.SmallText} \n| Yellow Team";
							break;
						case 5:
							this.SmallText = $"{this.SmallText} \n| Pink Team";
							break;
                    }
            }
			else
            {
				this.cooldown = 0;
				this.Details = "Perusing the menus, how fun!";
				this.State = $"{ModLoader.Mods.Length - 1} mods loaded";
				this.PartyCode = "";
				this.LargeAsset = "terraria";
				this.LargeText = "Idling on the menus...";
				this.SmallAsset = "";
				this.SmallText = "";
			}
			if (this.cooldown > 0)
				this.cooldown--;
        }

		float HealthAsPercent(Player player)
        {
			return ((player.statLife * 100) / player.statLifeMax );
		}

		void Log(string msg, byte level = 0)
        {
            switch (level)
            {
				case 1:
					this.Logger.Debug(msg);
					break;
				case 2:
					this.Logger.Warn(msg);
					break;
				case 3:
					this.Logger.Error(msg);
					break;
				default:
					this.Logger.Info(msg);
					break;
            }
		}
	}
}