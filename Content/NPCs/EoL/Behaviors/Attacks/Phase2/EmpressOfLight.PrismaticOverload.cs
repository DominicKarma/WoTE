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
        public ref float PrismaticOverload_MagicCircleSpinAngle => ref NPC.ai[0];

        public static int PrismaticOverload_MagicCircleAppearTime => Utilities.SecondsToFrames(1.75f);

        public static int PrismaticOverload_RotateUpwardDelay => Utilities.SecondsToFrames(0.75f);

        public static int PrismaticOverload_AimTowardsTargetDelay => Utilities.SecondsToFrames(1.25f);

        public static int PrismaticOverload_AimTowardsTargetTime => Utilities.SecondsToFrames(1.167f);

        public static int PrismaticOverload_ShootPrepareDelay => Utilities.SecondsToFrames(2f);

        public static int PrismaticOverload_ShootSuspenseTime => Utilities.SecondsToFrames(2.5f);

        public static int PrismaticOverload_ShootDelay => PrismaticOverload_ShootPrepareDelay + PrismaticOverload_ShootSuspenseTime + Utilities.SecondsToFrames(1f);

        public static int PrismaticOverload_ScaleIntoExistenceTime => Utilities.SecondsToFrames(1.25f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_PrismaticOverload()
        {
            StateMachine.RegisterTransition(EmpressAIType.PrismaticOverload, null, false, () =>
            {
                return AITimer >= 99999999;
            });

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

            float flySpeedInterpolant = Utilities.InverseLerp(PrismaticOverload_ShootPrepareDelay, 0f, AITimer - PrismaticOverload_ShootSuspenseTime);
            if (flySpeedInterpolant <= 0f)
                NPC.velocity *= 0.9f;
            else
                NPC.SmoothFlyNearWithSlowdownRadius(Target.Center, flySpeedInterpolant * 0.02f, 1f - flySpeedInterpolant * 0.13f, 250f);

            NPC.rotation = NPC.velocity.X * 0.0035f;

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
            }
            else
                NPC.spriteDirection = -NPC.OnRightSideOf(Target.Center).ToDirectionInt();

            PrismaticOverload_MagicCircleSpinAngle += MathHelper.TwoPi * (1f - shootSuspenseInterpolant) * Utilities.InverseLerp(0.35f, 0.95f, appearanceInterpolant).Squared() / 90f;
            PrismaticOverload_MagicCircleSpinAngle += MathHelper.TwoPi * Utilities.InverseLerp(0f, 20, AITimer - PrismaticOverload_ShootDelay) / 33f;
        }
    }
}
