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
        /// The rate at which bolts are shot during the Empress' Beat Synced Bolts attack.
        /// </summary>
        /// 
        /// <remarks>
        /// As the name suggests, this corresponds with beats during the song. As such, this value should not be changed.
        /// </remarks>
        public static int BeatSyncedBolts_ShootRate => Utilities.SecondsToFrames(0.43f);

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
                StateMachine.RegisterTransition(state, EmpressAIType.BeatSyncedBolts, false, () => MusicTimer >= BeatSyncedBolts_LightBeatStartTime && MusicTimer <= BeatSyncedBolts_LightBeatEndTime);
            }, EmpressAIType.Phase2Transition, EmpressAIType.Die, EmpressAIType.Vanish, EmpressAIType.Teleport, EmpressAIType.BeatSyncedBolts);

            StateMachine.RegisterStateBehavior(EmpressAIType.BeatSyncedBolts, DoBehavior_BeatSyncedBolts);
        }

        /// <summary>
        /// Performs the Empress' Prismatic Bolt Spin attack.
        /// </summary>
        public void DoBehavior_BeatSyncedBolts()
        {
            if (AITimer == 1)
                IProjOwnedByBoss<EmpressOfLight>.KillAll();

            int beatCycleTimer = AITimer % BeatSyncedBolts_ShootRate;

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
                ScreenShakeSystem.StartShake(6f, shakeStrengthDissipationIncrement: 0.5f);
                NPC.velocity += NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.Pi / 3f) * 60f;
                NPC.ai[1] = NPC.velocity.ToRotation();

                NPC.oldPos = new Vector2[NPC.oldPos.Length];
                NPC.oldRot = new float[NPC.oldPos.Length];

                if (Main.netMode != NetmodeID.MultiplayerClient && !NPC.WithinRange(Target.Center, 285f))
                {
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, NPC.SafeDirectionTo(Target.Center) * BeatSyncedBolts_StarBoltShootSpeed, ModContent.ProjectileType<StarBolt>(), StarBurstDamage, 0f);
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, NPC.SafeDirectionTo(Target.Center) * -BeatSyncedBolts_PrismaticBoltShootSpeed, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f);
                }

                if (Main.musicVolume <= 0f)
                    SoundEngine.PlaySound(SoundID.Item122);

                ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center, Vector2.Zero, 32f, 1f, 0.2f, 0.03f);
            }
            else
            {
                float flySpeedInterpolant = Utilities.InverseLerp(0f, 6f, beatCycleTimer) * 0.25f;
                NPC.SmoothFlyNear(Target.Center + (MathHelper.PiOver4 * -beatCycleTimer / BeatSyncedBolts_ShootRate + NPC.ai[1]).ToRotationVector2() * new Vector2(700f, 560f), flySpeedInterpolant, 1f - flySpeedInterpolant);
                DashAfterimageInterpolant *= 0.85f;
            }
        }
    }
}
