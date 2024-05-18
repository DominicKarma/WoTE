using System;
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
        /// How rate at which bolts are summoned by the Empress during her RadialStarBurst attack.
        /// </summary>
        public static int VanillaPrismaticBolts2_BoltShootRate => Utilities.SecondsToFrames(0.05f);

        /// <summary>
        /// The speed of bolts summoned by the Empress in her RadialStarBurst attack.
        /// </summary>
        public static float VanillaPrismaticBolts2_BoltFireSpeed => 13.2f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_VanillaPrismaticBolts2()
        {
            StateMachine.RegisterTransition(EmpressAIType.VanillaPrismaticBolts2, null, false, () =>
            {
                return AITimer >= VanillaPrismaticBolts_BoltShootDelay + VanillaPrismaticBolts_BoltShootTime + VanillaPrismaticBolts_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.VanillaPrismaticBolts2, DoBehavior_VanillaPrismaticBolts2);
        }

        /// <summary>
        /// Performs the Empress' RadialStarBurst 2 attack.
        /// </summary>
        public void DoBehavior_VanillaPrismaticBolts2()
        {
            DoBehavior_VanillaPrismaticBolts2_HoverAround();

            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PointingUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        public void DoBehavior_VanillaPrismaticBolts2_HoverAround()
        {
            float redirectSpeed = Utils.Remap(AITimer, 0f, 25f, 0.14f, 0.08f);
            Vector2 hoverDestination = Target.Center + new Vector2(MathF.Cos(MathHelper.TwoPi * AITimer / 90f) * 360f, -250f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, redirectSpeed, 1f - redirectSpeed * 1.2f, 50f);
            NPC.rotation = NPC.velocity.X * 0.01f;

            if (AITimer == VanillaPrismaticBolts_BoltShootDelay)
                SoundEngine.PlaySound(SoundID.Item164);

            bool ableToShoot = AITimer >= VanillaPrismaticBolts_BoltShootDelay && AITimer <= VanillaPrismaticBolts_BoltShootDelay + VanillaPrismaticBolts_BoltShootTime && !NPC.WithinRange(Target.Center, 150f);
            Vector2 handPosition = NPC.Center + new Vector2(30f, -64f).RotatedBy(NPC.rotation);
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % VanillaPrismaticBolts2_BoltShootRate == 0 && ableToShoot)
            {
                Vector2 boltVelocity = Main.rand.NextVector2Circular(VanillaPrismaticBolts2_BoltFireSpeed, VanillaPrismaticBolts2_BoltFireSpeed);
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target, AITimer / 45f % 1f);
            }
        }
    }
}
