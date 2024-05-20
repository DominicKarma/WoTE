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
        /// How long the Empress waits before releasing bolts during her RadialStarBurst attack.
        /// </summary>
        public static int VanillaPrismaticBolts_BoltShootDelay => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// How long the Empress spends releasing bolts during her RadialStarBurst attack.
        /// </summary>
        public int VanillaPrismaticBolts_BoltShootTime => Utilities.SecondsToFrames(ByPhase(1.5f, 1.2f));

        /// <summary>
        /// How long the Empress waits before choosing a new attack after all bolts have been shot in her RadialStarBurst attack.
        /// </summary>
        public int VanillaPrismaticBolts_AttackTransitionDelay => Utilities.SecondsToFrames(ByPhase(2f, 1.67f) - Main.dayTime.ToInt() * 0.8f);

        /// <summary>
        /// How rate at which bolts are summoned by the Empress during her RadialStarBurst attack.
        /// </summary>
        public static int VanillaPrismaticBolts_BoltShootRate => Utilities.SecondsToFrames(0.067f);

        /// <summary>
        /// The speed of bolts summoned by the Empress in her RadialStarBurst attack.
        /// </summary>
        public static float VanillaPrismaticBolts_BoltFireSpeed => 10.5f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_VanillaPrismaticBolts()
        {
            StateMachine.RegisterTransition(EmpressAIType.VanillaPrismaticBolts, null, false, () =>
            {
                return AITimer >= VanillaPrismaticBolts_BoltShootDelay + VanillaPrismaticBolts_BoltShootTime + VanillaPrismaticBolts_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.VanillaPrismaticBolts, DoBehavior_VanillaPrismaticBolts);
        }

        /// <summary>
        /// Performs the Empress' RadialStarBurst attack.
        /// </summary>
        public void DoBehavior_VanillaPrismaticBolts()
        {
            DoBehavior_VanillaPrismaticBolts_HoverAround();

            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PointingUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        public void DoBehavior_VanillaPrismaticBolts_HoverAround()
        {
            float redirectSpeed = Utils.Remap(AITimer, 0f, 25f, 0.13f, 0.07f);
            Vector2 hoverDestination = Target.Center + new Vector2(MathF.Cos(MathHelper.TwoPi * AITimer / 90f) * 300f, -285f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, redirectSpeed, 1f - redirectSpeed, 50f);
            NPC.rotation = NPC.velocity.X * 0.01f;

            if (AITimer == VanillaPrismaticBolts_BoltShootDelay)
                SoundEngine.PlaySound(SoundID.Item164);

            bool ableToShoot = AITimer >= VanillaPrismaticBolts_BoltShootDelay && AITimer <= VanillaPrismaticBolts_BoltShootDelay + VanillaPrismaticBolts_BoltShootTime && !NPC.WithinRange(Target.Center, 150f);
            Vector2 handPosition = NPC.Center + new Vector2(30f, -64f).RotatedBy(NPC.rotation);
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % VanillaPrismaticBolts_BoltShootRate == 0 && ableToShoot)
            {
                Vector2 boltVelocity = (MathHelper.TwoPi * AITimer / 45f).ToRotationVector2() * VanillaPrismaticBolts_BoltFireSpeed;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target, AITimer / 45f % 1f);
            }
        }
    }
}
