using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
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
        /// The horizontal direction of the Empress' dashes during the Prismatic Bolt Dashes attack.
        /// </summary>
        public ref float PrismaticBoltDashes_DashDirection => ref NPC.ai[0];

        /// <summary>
        /// The amount of dashes the Empress has performed so far, relative to the Prismatic Bolt Dashes attack.
        /// </summary>
        public ref float PrismaticBoltDashes_DashCounter => ref NPC.ai[1];

        /// <summary>
        /// The spin offset angle of projectiles summoned by the Empress during the Prismatic Bolt Dashes attack.
        /// </summary>
        public ref float PrismaticBoltDashes_BoltSpinOffsetAngle => ref NPC.ai[2];

        /// <summary>
        /// The amount of time the Empress spends redirecting during the Prismatic Bolt Dashes attack.
        /// </summary>
        public static int PrismaticBoltDashes_HoverRedirectTime => Utilities.SecondsToFrames(0.75f);

        /// <summary>
        /// The amount of time the Empress spends dashing during the Prismatic Bolt Dashes attack.
        /// </summary>
        public static int PrismaticBoltDashes_DashTime => Utilities.SecondsToFrames(0.75f);

        /// <summary>
        /// The amount of dashes that the Empress should perform during the Prismatic Bolt Dashes attack.
        /// </summary>
        public static int PrismaticBoltDashes_DashCount => 2;

        /// <summary>
        /// The speed of dashes performed during the Prismatic Bolt Dashes attack.
        /// </summary>
        public static float PrismaticBoltDashes_DashSpeed => 60f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_PrismaticBoltDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.PrismaticBoltDashes, null, false, () =>
            {
                return PrismaticBoltDashes_DashCounter >= PrismaticBoltDashes_DashCount && AITimer >= 60;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.PrismaticBoltDashes, DoBehavior_PrismaticBoltDashes);
        }

        /// <summary>
        /// Performs the Empress' Prismatic Bolt Dashes attack.
        /// </summary>
        public void DoBehavior_PrismaticBoltDashes()
        {
            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0031f;

            bool doneAttacking = PrismaticBoltDashes_DashCounter >= PrismaticBoltDashes_DashCount;
            if (doneAttacking)
            {
                NPC.velocity *= 0.9f;
                return;
            }

            LeftHandFrame = EmpressHandFrame.FistedOutstretchedArm;
            RightHandFrame = EmpressHandFrame.FistedOutstretchedArm;

            if (AITimer <= PrismaticBoltDashes_HoverRedirectTime)
            {
                DoBehavior_PrismaticBoltDashes_PerformRedirect();
                DoBehavior_PrismaticBoltDashes_PerformDashBehaviors();
            }
            else
                DoBehavior_PrismaticBoltDashes_PerformPostDashBehaviors();

            if (AITimer >= PrismaticBoltDashes_HoverRedirectTime + PrismaticBoltDashes_DashTime)
            {
                AITimer = 0;
                PrismaticBoltDashes_DashCounter++;
                NPC.netUpdate = true;
            }
        }

        /// <summary>
        /// Performs redirecting behaviors for the Empress' Prismatic Bolt Dashes attack.
        /// </summary>
        public void DoBehavior_PrismaticBoltDashes_PerformRedirect()
        {
            if (AITimer == 1)
                PrismaticBoltDashes_DashDirection = -NPC.OnRightSideOf(Target.Center).ToDirectionInt();

            if (AITimer == PrismaticBoltDashes_HoverRedirectTime - 20)
                SoundEngine.PlaySound(SoundID.Item160 with { MaxInstances = 0 }, NPC.Center);

            float hoverFlySpeed = Utilities.InverseLerp(0f, AITimer, PrismaticBoltDashes_HoverRedirectTime).Cubed() * 0.11f;
            Vector2 hoverDestination = Target.Center - Vector2.UnitX * PrismaticBoltDashes_DashDirection * 800f;
            NPC.SmoothFlyNear(hoverDestination, hoverFlySpeed, 1f - hoverFlySpeed);

            if (NPC.WithinRange(hoverDestination, 35f) && AITimer < PrismaticBoltDashes_HoverRedirectTime)
            {
                SoundEngine.PlaySound(SoundID.Item160 with { MaxInstances = 0 }, NPC.Center);
                AITimer = PrismaticBoltDashes_HoverRedirectTime;
                NPC.netUpdate = true;
            }
        }

        /// <summary>
        /// Performs frame-one behaviors for dashes that the Empress performs during her Prismatic Bolt Dashes attack.
        /// </summary>
        public void DoBehavior_PrismaticBoltDashes_PerformDashBehaviors()
        {
            SoundEngine.PlaySound(SoundID.Item164 with { MaxInstances = 0 });
            NPC.velocity *= 0.3f;
            PrismaticBoltDashes_BoltSpinOffsetAngle = Main.rand.NextFloat(MathHelper.TwoPi);
            NPC.netUpdate = true;
        }

        /// <summary>
        /// Performs post-dash behaviors for the Empress' Prismatic Bolt Dashes attack.
        /// </summary>
        public void DoBehavior_PrismaticBoltDashes_PerformPostDashBehaviors()
        {
            NPC.damage = NPC.defDamage;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.UnitX * PrismaticBoltDashes_DashDirection * PrismaticBoltDashes_DashSpeed, 0.1f);

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 2 == 0)
            {
                Vector2 boltVelocity = (MathHelper.TwoPi * AITimer / PrismaticBoltSpin_SpinTime * PrismaticBoltDashes_DashDirection * 2f + PrismaticBoltDashes_BoltSpinOffsetAngle).ToRotationVector2() * 1.25f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, boltVelocity, ModContent.ProjectileType<StarBolt>(), StarBurstDamage, 0f, -1);
            }

            if (AITimer >= PrismaticBoltDashes_HoverRedirectTime + PrismaticBoltDashes_DashTime - 2)
            {
                NPC.velocity *= 0.6f;
                DashAfterimageInterpolant *= 0.9f;
            }
            else
                DashAfterimageInterpolant = Utilities.InverseLerp(0.6f, 0.95f, NPC.velocity.Length() / PrismaticBoltDashes_DashSpeed);
        }
    }
}
