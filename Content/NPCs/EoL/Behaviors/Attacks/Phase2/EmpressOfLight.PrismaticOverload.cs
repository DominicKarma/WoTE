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

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the butterfly dashes attack should be avoided completely if possible, due to the Prismatic Overload's timing to the music coming up.
        /// </summary>
        public bool PrismaticOverload_ShouldntDoButterflyDashes => MusicTimer / (float)PrismaticOverload_HighBeatStartTime >= 0.898f && MusicTimer / (float)PrismaticOverload_HighBeatStartTime <= 1.01f;

        /// <summary>
        /// Whether the music is at a point where the Empress can start her Prismatic Overload attack.
        /// </summary>
        public bool PrismaticOverload_CanDanceToBeat => MathHelper.Distance(MusicTimer - PrismaticOverload_RotateUpwardDelay, PrismaticOverload_HighBeatStartTime) <= 3f;

        /// <summary>
        /// The spin angle of the magic circle during the Empress' Prismatic Overload attack.
        /// </summary>
        public ref float PrismaticOverload_MagicCircleSpinAngle => ref NPC.ai[0];

        /// <summary>
        /// How long it takes for the magic circle to appear during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_MagicCircleAppearTime => Utilities.SecondsToFrames(1.75f);

        /// <summary>
        /// How long it takes for the magic circle to appear during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_RotateUpwardDelay => Utilities.SecondsToFrames(0.75f);

        /// <summary>
        /// How long it takes for the magic circle to begin aiming aim towards the player during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_AimTowardsTargetDelay => Utilities.SecondsToFrames(1.25f);

        /// <summary>
        /// How long it takes for the magic circle to correct its aim towards the player during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_AimTowardsTargetTime => Utilities.SecondsToFrames(1.167f);

        /// <summary>
        /// How long it takes for the magic circle to prepare for firing during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_ShootPrepareDelay => Utilities.SecondsToFrames(2f);

        /// <summary>
        /// How long it takes for the magic circle to perform its suspsense the player during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_ShootSuspenseTime => Utilities.SecondsToFrames(2.5f);

        /// <summary>
        /// How long it takes for the magic circle to fire lances outward during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_ShootDelay => PrismaticOverload_ShootPrepareDelay + PrismaticOverload_ShootSuspenseTime + Utilities.SecondsToFrames(1f);

        /// <summary>
        /// How long the magic circle spends releasing projectiles during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_ShootTime => Utilities.SecondsToFrames(11.5f);

        /// <summary>
        /// How long it takes for the magic circle to scale up during the Empress' Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_ScaleIntoExistenceTime => Utilities.SecondsToFrames(1.25f);

        /// <summary>
        /// The starting time of the high beat. For use when determining whether the Empress should perform her Prismatic Overload attack.
        /// </summary>
        public static int PrismaticOverload_HighBeatStartTime => Utilities.MinutesToFrames(2.8756f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_PrismaticOverload()
        {
            StateMachine.RegisterTransition(EmpressAIType.PrismaticOverload, null, false, () =>
            {
                return AITimer >= PrismaticOverload_ShootDelay + PrismaticOverload_ShootTime;
            }, IProjOwnedByBoss<EmpressOfLight>.KillAll);
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.PrismaticOverload, false, () => PrismaticOverload_CanDanceToBeat);
            }, EmpressAIType.Phase2Transition, EmpressAIType.Die, EmpressAIType.Vanish, EmpressAIType.Teleport, EmpressAIType.PrismaticOverload, EmpressAIType.ButterflyBurstDashes);

            StateMachine.RegisterStateBehavior(EmpressAIType.PrismaticOverload, DoBehavior_PrismaticOverload);
        }

        /// <summary>
        /// Performs the Empress' Prismatic Overload attack.
        /// </summary>
        public void DoBehavior_PrismaticOverload()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 1)
            {
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MagicCircle>(), 0, 0f);
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);
            }

            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PalmRaisedUp;

            if (Main.mouseRight && Main.mouseRightRelease)
            {
                IProjOwnedByBoss<EmpressOfLight>.KillAll();
                AITimer = 0;
            }

            float appearanceInterpolant = Utilities.InverseLerp(0f, PrismaticOverload_MagicCircleAppearTime, AITimer);
            float shootSuspenseInterpolant = Utilities.InverseLerp(0f, PrismaticOverload_ShootSuspenseTime, AITimer - PrismaticOverload_ShootPrepareDelay);

            if (shootSuspenseInterpolant > 0f && shootSuspenseInterpolant < 1f)
                ScreenShakeSystem.StartShake(shootSuspenseInterpolant * 4.1f, shakeStrengthDissipationIncrement: 0.6f);

            if (AITimer == PrismaticOverload_ShootDelay)
            {
                ScreenShakeSystem.StartShake(25f, shakeStrengthDissipationIncrement: 0.45f);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 fistPosition = NPC.Center + new Vector2(NPC.spriteDirection * 64f, 8f).RotatedBy(NPC.rotation);
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), fistPosition, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);
                }
                SoundEngine.PlaySound(SoundID.Item122);
            }

            if (AITimer >= PrismaticOverload_ShootDelay)
            {
                ScreenShakeSystem.StartShake(1.85f, shakeStrengthDissipationIncrement: 0.33f);
                LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
                RightHandFrame = EmpressHandFrame.FistedOutstretchedArm;

                if (AITimer % 20 == 19)
                    SoundEngine.PlaySound(SoundID.Item162 with { MaxInstances = 0 });

                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(Target.Center) * -2.4f, 0.03f);
            }
            else
            {
                NPC.spriteDirection = -NPC.OnRightSideOf(Target.Center).ToDirectionInt();

                float flySpeedInterpolant = Utilities.InverseLerp(PrismaticOverload_ShootPrepareDelay, 0f, AITimer - PrismaticOverload_ShootSuspenseTime);
                if (flySpeedInterpolant <= 0f)
                    NPC.velocity *= 0.9f;
                else
                    NPC.SmoothFlyNearWithSlowdownRadius(Target.Center, flySpeedInterpolant * 0.02f, 1f - flySpeedInterpolant * 0.13f, 250f);
            }

            NPC.rotation = NPC.velocity.X * 0.0035f;

            PrismaticOverload_MagicCircleSpinAngle += MathHelper.TwoPi * (1f - shootSuspenseInterpolant) * Utilities.InverseLerp(0.35f, 0.95f, appearanceInterpolant).Squared() / 90f;
            PrismaticOverload_MagicCircleSpinAngle += MathHelper.TwoPi * Utilities.InverseLerp(0f, 20, AITimer - PrismaticOverload_ShootDelay) / 33f;
        }
    }
}
