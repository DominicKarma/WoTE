using Luminance.Common.DataStructures;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the music is at a point where the Empress can start her Beat Synced Bolts attack.
        /// </summary>
        public bool BeatSyncedBolts_CanDanceToBeat => MusicTimer >= BeatSyncedBolts_LightBeatStartTime - BeatSyncedBolts_AttackStartDelay && MusicTimer <= BeatSyncedBolts_LightBeatEndTime - BeatSyncedBolts_AttackStartDelay;

        /// <summary>
        /// The starting direction angle that the Empress started her dash with during her Beat Synced Bolts attack.
        /// </summary>
        public ref float BeatSyncedBolts_StartingDashDirection => ref NPC.ai[0];

        /// <summary>
        /// The rate at which bolts are shot during the Empress' Beat Synced Bolts attack.
        /// </summary>
        /// 
        /// <remarks>
        /// As the name suggests, this corresponds with beats during the song. As such, this value should not be changed.
        /// </remarks>
        public static int BeatSyncedBolts_ShootRate => Utilities.SecondsToFrames(0.46f);

        /// <summary>
        /// How long the Empress' Beat Synced Bolts attack goes on for.
        /// </summary>
        public static int BeatSyncedBolts_AttackDuration => BeatSyncedBolts_ShootRate * 18;

        /// <summary>
        /// The initial speed of sniper star bolts shot during the Empress' Beat Synced Bolts attack.
        /// </summary>
        public static float BeatSyncedBolts_StarBoltShootSpeed => 6.25f;

        /// <summary>
        /// The initial speed of homing prismatic bolts shot during the Empress' Beat Synced Bolts attack.
        /// </summary>
        public static float BeatSyncedBolts_PrismaticBoltShootSpeed => 3.2f;

        /// <summary>
        /// How long the Empress waits, prepating before starting her Beat Synced Bolts attack.
        /// </summary>
        public static int BeatSyncedBolts_AttackStartDelay => Utilities.SecondsToFrames(1.12f);

        /// <summary>
        /// The starting time of the light beat. For use when determining whether the Empress should perform her Beat Synced Bolts attack.
        /// </summary>
        public static int BeatSyncedBolts_LightBeatStartTime => Utilities.MinutesToFrames(0.991f);

        /// <summary>
        /// The ending time of the light beat. For use when determining whether the Empress should perform her Beat Synced Bolts attack.
        /// </summary>
        public static int BeatSyncedBolts_LightBeatEndTime => Utilities.MinutesToFrames(1.1207f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_BeatSyncedBolts()
        {
            StateMachine.RegisterTransition(EmpressAIType.BeatSyncedBolts, null, false, () =>
            {
                return AITimer >= BeatSyncedBolts_AttackDuration;
            });
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.BeatSyncedBolts, false, () => BeatSyncedBolts_CanDanceToBeat);
            }, EmpressAIType.Phase2Transition, EmpressAIType.Die, EmpressAIType.Vanish, EmpressAIType.Teleport, EmpressAIType.BeatSyncedBolts, EmpressAIType.ButterflyBurstDashes);

            StateMachine.RegisterStateBehavior(EmpressAIType.BeatSyncedBolts, DoBehavior_BeatSyncedBolts);
        }

        /// <summary>
        /// Performs the Empress' Beat Synced Bolts attack.
        /// </summary>
        public void DoBehavior_BeatSyncedBolts()
        {
            if (AITimer == 1)
                IProjOwnedByBoss<EmpressOfLight>.KillAll();

            if (AITimer <= BeatSyncedBolts_AttackStartDelay)
            {
                float flySpeed = (1f - AITimer / (float)BeatSyncedBolts_AttackStartDelay) * 0.3f;
                Vector2 hoverDestination = Target.Center + Target.SafeDirectionTo(NPC.Center).RotatedBy(MathHelper.PiOver2 * (1f - AITimer / BeatSyncedBolts_AttackStartDelay)) * 400f;

                NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, flySpeed, 1f - flySpeed * 0.78f, 100f);
                return;
            }

            int beatCycleTimer = (AITimer - BeatSyncedBolts_AttackStartDelay) % BeatSyncedBolts_ShootRate;

            LeftHandFrame = EmpressHandFrame.FistedOutstretchedArm;
            RightHandFrame = EmpressHandFrame.FistedOutstretchedArm;
            if (beatCycleTimer >= BeatSyncedBolts_ShootRate / 2)
            {
                LeftHandFrame = EmpressHandFrame.HandPressedToChest;
                RightHandFrame = EmpressHandFrame.HandPressedToChest;
            }

            NPC.spriteDirection = NPC.OnRightSideOf(Target.Center).ToDirectionInt();
            NPC.rotation = NPC.velocity.X * 0.0015f;

            if (beatCycleTimer == 0)
            {
                DoBehavior_BeatSyncedBolts_PerformDash();
                DoBehavior_BeatSyncedBolts_ReleaseBolts();
            }
            else if (beatCycleTimer <= BeatSyncedBolts_ShootRate - 5)
            {
                float flySpeedInterpolant = Utilities.InverseLerp(0f, 6f, beatCycleTimer) * 0.4f;
                Vector2 hoverOffsetDirection = (MathHelper.PiOver4 * -beatCycleTimer / BeatSyncedBolts_ShootRate + BeatSyncedBolts_StartingDashDirection).ToRotationVector2();
                NPC.SmoothFlyNear(Target.Center + hoverOffsetDirection * new Vector2(700f, 560f), flySpeedInterpolant, 1f - flySpeedInterpolant);
            }
            else
                NPC.velocity *= 0.79f;

            DashAfterimageInterpolant = Utilities.InverseLerpBump(0f, 10f, BeatSyncedBolts_AttackDuration - 10f, BeatSyncedBolts_AttackDuration, AITimer - BeatSyncedBolts_AttackStartDelay) * 0.25f;
        }

        /// <summary>
        /// Releases bolts from the Empress for the Beat Synced Bolts attack.
        /// </summary>
        public void DoBehavior_BeatSyncedBolts_ReleaseBolts()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.WithinRange(Target.Center, 356f))
            {
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, NPC.SafeDirectionTo(Target.Center) * BeatSyncedBolts_StarBoltShootSpeed, ModContent.ProjectileType<StarBolt>(), StarBurstDamage, 0f);
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, NPC.SafeDirectionTo(Target.Center) * -BeatSyncedBolts_PrismaticBoltShootSpeed, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f);
            }

            // Normally, the sound of the projectile shots line up with the beat.
            // If the music isn't active, however, play a general shoot sound instead.
            if (Main.musicVolume <= 0f)
                SoundEngine.PlaySound(SoundID.Item122);

            // Be careful with this. This shake effect is subtle, but it plays a massive role in selling the impact of the beat to the player.
            ScreenShakeSystem.StartShake(6f, shakeStrengthDissipationIncrement: 0.5f);

            ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center, Vector2.Zero, 32f, 1f, 0.2f, 0.03f);
        }

        /// <summary>
        /// Makes the Empress dash for her Beat Synced Bolts attack.
        /// </summary>
        public void DoBehavior_BeatSyncedBolts_PerformDash()
        {
            NPC.velocity += NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.Pi / 3f) * 60f;
            BeatSyncedBolts_StartingDashDirection = NPC.velocity.ToRotation();

            NPC.oldPos = new Vector2[NPC.oldPos.Length];
            NPC.oldRot = new float[NPC.oldPos.Length];
        }
    }
}
