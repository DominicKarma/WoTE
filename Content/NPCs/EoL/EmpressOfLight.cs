using System.Collections.Generic;
using System.IO;
using Luminance.Common.Utilities;
using Luminance.Core.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        #region Fields and Properties

        /// <summary>
        /// Private backing field for <see cref="Myself"/>.
        /// </summary>
        private static NPC myself;

        /// <summary>
        /// The current AI state the Empress is using. This uses the <see cref="StateMachine"/> under the hood.
        /// </summary>
        public EmpressAIType CurrentState
        {
            get
            {
                // Add the relevant phase cycle if it has been exhausted, to ensure that the Empress' attacks are cyclic.
                if ((StateMachine?.StateStack?.Count ?? 1) <= 0)
                    StateMachine?.StateStack.Push(StateMachine.StateRegistry[EmpressAIType.ResetCycle]);

                return StateMachine?.CurrentState?.Identifier ?? EmpressAIType.Awaken;
            }
        }

        /// <summary>
        /// The current phase of the Empress.
        /// </summary>
        public int Phase
        {
            get;
            set;
        }

        /// <summary>
        /// The volume of the idle drizzle.
        /// </summary>
        public float DrizzleVolume
        {
            get;
            set;
        }

        /// <summary>
        /// The ideal volume of the idle drizzle.
        /// </summary>
        public float IdealDrizzleVolume
        {
            get;
            set;
        }

        /// <summary>
        /// The looped sound instance of the drizzle.
        /// </summary>
        public LoopedSoundInstance DrizzleSoundLoop
        {
            get;
            set;
        }

        /// <summary>
        /// The Empress' target.
        /// </summary>
        public Player Target => Main.player[NPC.target];

        /// <summary>
        /// The frame of the Empress' left hand.
        /// </summary>
        public EmpressHandFrame LeftHandFrame
        {
            get;
            set;
        }

        /// <summary>
        /// The frame of the Empress' right hand.
        /// </summary>
        public EmpressHandFrame RightHandFrame
        {
            get;
            set;
        }

        /// <summary>
        /// The AI timer for the Empress.
        /// </summary>
        public ref int AITimer => ref StateMachine.CurrentState.Time;

        /// <summary>
        /// A shorthand accessor for the Empress NPC. Returns null if not currently present.
        /// </summary>
        public static NPC? Myself
        {
            get
            {
                if (myself is not null && !myself.active)
                    return null;

                return myself;
            }
            internal set
            {
                if (value is not null)
                    myself = value;
            }
        }

        /// <summary>
        /// The amount of damage prismatic bolts summoned by the Empress do.
        /// </summary>
        public static int PrismaticBoltDamage => Main.expertMode ? 160 : 95;

        /// <summary>
        /// The amount of damage star bursts summoned by the Empress do.
        /// </summary>
        public static int StarBurstDamage => Main.expertMode ? 160 : 95;

        /// <summary>
        /// The amount of damage accelerating rainbows summoned by the Empress do.
        /// </summary>
        public static int AcceleratingRainbowDamage => Main.expertMode ? 170 : 105;

        /// <summary>
        /// The amount of damage dazzling petals summoned by the Empress do.
        /// </summary>
        public static int DazzlingPetalDamage => Main.expertMode ? 185 : 120;

        /// <summary>
        /// The amount of damage dazzling tornadoes summoned by the Empress do.
        /// </summary>
        public static int DazzlingTornadoDamage => Main.expertMode ? 185 : 120;

        /// <summary>
        /// The amount of damage terraprismas summoned by the Empress do.
        /// </summary>
        public static int TerraprismaDamage => Main.expertMode ? 185 : 120;

        /// <summary>
        /// The standard volume that the drizzle sound play at.
        /// </summary>
        public static float StandardDrizzleVolume => 0.1f;

        /// <summary>
        /// The ambient drizzle sound that plays throughout the fight.
        /// </summary>
        public static readonly SoundStyle DrizzleSound = new("WoTE/Assets/Sounds/Custom/Drizzle");

        public override string Texture => $"Terraria/Images/NPC_{NPCID.HallowBoss}";

        #endregion Fields and Properties

        #region Loading

        public override void SetStaticDefaults()
        {
            this.ExcludeFromBestiary();

            NPCID.Sets.BossHeadTextures[Type] = NPCID.Sets.BossHeadTextures[NPCID.HallowBoss];

            IDictionary<int, int> npcToBossHead = (IDictionary<int, int>)(typeof(NPCHeadLoader)?.GetField("npcToBossHead", Utilities.UniversalBindingFlags)?.GetValue(null) ?? new Dictionary<int, int>());
            npcToBossHead[Type] = NPCID.Sets.BossHeadTextures[Type];

            Main.npcFrameCount[Type] = 2;

            NPCID.Sets.MustAlwaysDraw[Type] = true;
            NPCID.Sets.TrailingMode[Type] = 3;
            NPCID.Sets.TrailCacheLength[Type] = 30;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 100f;
            NPC.damage = 180;
            NPC.width = 136;
            NPC.height = 124;
            NPC.defense = 100;
            NPC.SetLifeMaxByMode(200000, 300000, 400000);

            if (Main.expertMode)
            {
                NPC.lifeMax /= 2;
                NPC.damage = 105;
            }

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = null;
            NPC.value = Item.buyPrice(1, 0, 0, 0) / 5;
            NPC.netAlways = true;
            NPC.Opacity = 0f;
            Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sounds/Music/EmpressOfLight");
        }

        #endregion Loading

        #region Syncing

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Phase);

            WritePreviousStates(writer);
            WriteStateMachineStack(writer);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Phase = reader.ReadInt32();

            ReadPreviousStates(reader);
            ReadStateMachineStack(reader);
        }

        #endregion Syncing

        #region AI
        public override void AI()
        {
            // Do not despawn.
            NPC.timeLeft = 7200;

            Myself = NPC;

            HandleTargetSelection();
            PerformPreUpdateResets();
            HandleMiscAmbienceTweaks();

            StateMachine.PerformBehaviors();
            StateMachine.PerformStateTransitionCheck();

            if (CurrentState != EmpressAIType.Teleport)
                TeleportCompletionRatio = 0f;

            UpdateLoopingSounds();

            if ((StateMachine?.StateStack?.Count ?? 1) <= 0)
                StateMachine?.StateStack.Push(StateMachine.StateRegistry[EmpressAIType.ResetCycle]);

            // Increment timers.
            AITimer++;

            foreach (Player player in Main.ActivePlayers)
            {
                player.GrantInfiniteFlight();
                player.AddBuff(ModContent.BuffType<GracedWings>(), 2);
            }

            Lighting.AddLight(NPC.Center, Vector3.One * NPC.Opacity);
        }

        /// <summary>
        /// Handles target selection code for the Empress, along with instructing her to leave if no valid target could be found.
        /// </summary>
        public void HandleTargetSelection()
        {
            NoTargetCouldBeFound = false;

            // Pick a target if the current one is invalid.
            if (Target.dead || !Target.active)
                NPC.TargetClosest();

            if (!NPC.WithinRange(Target.Center, 3300f))
                NPC.TargetClosest();

            // Hey bozo the player's gone. Leave.
            if (Target.dead || !Target.active)
                NoTargetCouldBeFound = true;
        }

        /// <summary>
        /// Performs various ambience related changes to the world while the Empress is present.
        /// </summary>
        public void HandleMiscAmbienceTweaks()
        {
            Main.windSpeedTarget = 0.04f;
            Main.moonPhase = 4;
            if (!Main.dayTime)
                Main.time = MathHelper.Lerp((float)Main.time, 16200f, 0.1f);

            // The ambient sounds of these things absolutely kill the mood of the fight.
            // Kill them.
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.type == ProjectileID.FallingStar)
                    proj.active = false;
            }

            IdealDrizzleVolume = StandardDrizzleVolume;
        }

        /// <summary>
        /// Resets various things pertaining to the fight state prior to behavior updates.
        /// </summary>
        /// 
        /// <remarks>
        /// This serves as a means of ensuring that changes to the fight state are gracefully reset if something suddenly changes, while affording the ability to make changes during updates.<br></br>
        /// As a result, this alleviates behaviors AI states from the burden of having to assume that they may terminate at any time and must account for that to ensure that the state is reset.
        /// </remarks>
        public void PerformPreUpdateResets()
        {
            NPC.damage = 0;
            NPC.defense = NPC.defDefense;
            NPC.dontTakeDamage = false;
            NPC.immortal = false;
            NPC.hide = false;
            NPC.ShowNameOnHover = true;
            DashAfterimageInterpolant = Utilities.Saturate(DashAfterimageInterpolant - 0.01f);
        }

        /// <summary>
        /// Updates all played looping sounds.
        /// </summary>
        public void UpdateLoopingSounds()
        {
            DrizzleVolume = MathHelper.Lerp(DrizzleVolume, IdealDrizzleVolume, 0.14f);

            DrizzleSoundLoop ??= LoopedSoundManager.CreateNew(DrizzleSound, () => !NPC.active);
            DrizzleSoundLoop.Update(Main.LocalPlayer.Center, sound =>
            {
                Vector2 groundPosition = Utilities.FindGround(Main.LocalPlayer.Center.ToTileCoordinates(), Vector2.UnitY).ToWorldCoordinates();
                float distanceFromGround = Main.LocalPlayer.Distance(groundPosition);
                float groundDistanceInterpolant = Utilities.InverseLerp(500f, 1774f, distanceFromGround);
                float groundDistanceVolumeFactor = MathHelper.Lerp(1f, 0.4f, groundDistanceInterpolant);
                sound.Volume = DrizzleVolume * groundDistanceVolumeFactor;
            });
        }

        #endregion AI

        #region Iframes

        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses;
            return true;
        }

        #endregion Iframes
    }
}
