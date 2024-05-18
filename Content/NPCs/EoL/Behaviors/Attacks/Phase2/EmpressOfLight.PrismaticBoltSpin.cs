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
        /// The angle at which the Empress' is at relative to the player during her Prismatic Bolt Spin attack.
        /// </summary>
        public ref float PrismaticBoltSpin_SpinAngle => ref NPC.ai[0];

        /// <summary>
        /// The Empress' spin direction for her Prismatic Bolt Spin attack.
        /// </summary>
        public ref float PrismaticBoltSpin_SpinDirection => ref NPC.ai[1];

        /// <summary>
        /// How long the Empress spends spinning during her Prismatic Bolt Spin attack.
        /// </summary>
        public static int PrismaticBoltSpin_SpinTime => Utilities.SecondsToFrames(1f);

        /// <summary>
        /// How long the Empress waits after her Prismatic Bolt Spin attack to transition to a different attack.
        /// </summary>
        public static int PrismaticBoltSpin_AttackTransitionDelay => Utilities.SecondsToFrames(2.3f);

        /// <summary>
        /// The radius of the spin the Empress performs during her Prismatic Bolt Spin attack.
        /// </summary>
        public static float PrismaticBoltSpin_SpinRadius => 700f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_PrismaticBoltSpin()
        {
            StateMachine.RegisterTransition(EmpressAIType.PrismaticBoltSpin, null, false, () =>
            {
                return AITimer >= PrismaticBoltSpin_SpinTime + PrismaticBoltSpin_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.PrismaticBoltSpin, DoBehavior_PrismaticBoltSpin);
        }

        /// <summary>
        /// Performs the Empress' Prismatic Bolt Spin attack.
        /// </summary>
        public void DoBehavior_PrismaticBoltSpin()
        {
            if (AITimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item164, NPC.Center);
                PrismaticBoltSpin_SpinDirection = NPC.OnRightSideOf(Target.Center).ToDirectionInt();
                PrismaticBoltSpin_SpinAngle = NPC.AngleFrom(Target.Center) - MathHelper.PiOver2 * PrismaticBoltSpin_SpinDirection;
                NPC.netUpdate = true;
            }

            if (AITimer <= PrismaticBoltSpin_SpinTime)
                DoBehavior_PrismaticBoltSpin_Spin();
            else
            {
                NPC.velocity *= 0.85f;
                DashAfterimageInterpolant *= 0.9f;
            }

            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        /// <summary>
        /// Performs spinning and projectle summoning behaviors for the Empress' Prismatic Bolt Spin attack.
        /// </summary>
        public void DoBehavior_PrismaticBoltSpin_Spin()
        {
            float spinSpeedInterpolant = Utilities.InverseLerp(0f, 75f, AITimer);
            Vector2 spinDestination = Target.Center + PrismaticBoltSpin_SpinAngle.ToRotationVector2() * PrismaticBoltSpin_SpinRadius;
            NPC.SmoothFlyNear(spinDestination, spinSpeedInterpolant * 0.6f, 1f - spinSpeedInterpolant * 0.54f);
            DashAfterimageInterpolant = spinSpeedInterpolant;

            float spinSpeed = Utilities.InverseLerp(0f, 40f, AITimer) * MathHelper.TwoPi / 36f;
            PrismaticBoltSpin_SpinAngle += spinSpeed * PrismaticBoltSpin_SpinDirection;

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 5 == 4 && AITimer >= 36)
            {
                Vector2 boltVelocity = NPC.velocity.SafeNormalize(Vector2.UnitY) * 10f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target, AITimer / 45f % 1f);
            }
        }
    }
}
