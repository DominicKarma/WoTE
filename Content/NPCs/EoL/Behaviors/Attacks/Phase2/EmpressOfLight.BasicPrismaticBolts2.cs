using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// How rate at which bolts are summoned by the Empress during her Basic Prismatic Bolts attack.
        /// </summary>
        public static int BasicPrismaticBolts2_BoltShootRate => Utilities.SecondsToFrames(0.05f);

        /// <summary>
        /// The speed of bolts summoned by the Empress in her Basic Prismatic Bolts attack.
        /// </summary>
        public static float BasicPrismaticBolts2_BoltFireSpeed => 13.2f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_BasicPrismaticBolts2()
        {
            StateMachine.RegisterTransition(EmpressAIType.BasicPrismaticBolts2, null, false, () =>
            {
                return AITimer >= BasicPrismaticBolts_BoltShootDelay + BasicPrismaticBolts_BoltShootTime + BasicPrismaticBolts_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.BasicPrismaticBolts2, DoBehavior_BasicPrismaticBolts2);
        }

        /// <summary>
        /// Performs the Empress' Basic Prismatic Bolts 2 attack.
        /// </summary>
        public void DoBehavior_BasicPrismaticBolts2()
        {
            DoBehavior_BasicPrismaticBolts2_HoverAround();

            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PointingUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        public void DoBehavior_BasicPrismaticBolts2_HoverAround()
        {
            float redirectSpeed = Utils.Remap(AITimer, 0f, 25f, 0.14f, 0.08f);
            Vector2 hoverDestination = Target.Center + new Vector2(MathF.Cos(MathHelper.TwoPi * AITimer / 90f) * 360f, -250f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, redirectSpeed, 1f - redirectSpeed * 1.2f, 50f);
            NPC.rotation = NPC.velocity.X * 0.01f;

            if (AITimer == 1)
                SoundEngine.PlaySound(SoundID.Item164, NPC.Center);

            bool ableToShoot = AITimer >= BasicPrismaticBolts_BoltShootDelay && AITimer <= BasicPrismaticBolts_BoltShootDelay + BasicPrismaticBolts_BoltShootTime && !NPC.WithinRange(Target.Center, 150f);
            Vector2 handPosition = NPC.Center + new Vector2(30f, -64f).RotatedBy(NPC.rotation);
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % BasicPrismaticBolts2_BoltShootRate == 0 && ableToShoot)
            {
                Vector2 boltVelocity = Main.rand.NextVector2Circular(BasicPrismaticBolts2_BoltFireSpeed, BasicPrismaticBolts2_BoltFireSpeed);
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target, AITimer / 45f % 1f);
            }
        }
    }
}
