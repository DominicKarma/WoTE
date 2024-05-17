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
        /// How long the Empress waits before releasing bolts during her Basic Prismatic Bolts attack.
        /// </summary>
        public static int BasicPrismaticBolts_BoltShootDelay => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// How long the Empress spends releasing bolts during her Basic Prismatic Bolts attack.
        /// </summary>
        public int BasicPrismaticBolts_BoltShootTime => Utilities.SecondsToFrames(ByPhase(1.5f, 1.2f));

        /// <summary>
        /// How long the Empress waits before choosing a new attack after all bolts have been shot in her Basic Prismatic Bolts attack.
        /// </summary>
        public int BasicPrismaticBolts_AttackTransitionDelay => Utilities.SecondsToFrames(ByPhase(2f, 1.67f));

        /// <summary>
        /// How rate at which bolts are summoned by the Empress during her Basic Prismatic Bolts attack.
        /// </summary>
        public static int BasicPrismaticBolts_BoltShootRate => Utilities.SecondsToFrames(0.067f);

        /// <summary>
        /// The speed of bolts summoned by the Empress in her Basic Prismatic Bolts attack.
        /// </summary>
        public static float BasicPrismaticBolts_BoltFireSpeed => 10.5f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_BasicPrismaticBolts()
        {
            StateMachine.RegisterTransition(EmpressAIType.BasicPrismaticBolts, null, false, () =>
            {
                return AITimer >= BasicPrismaticBolts_BoltShootDelay + BasicPrismaticBolts_BoltShootTime + BasicPrismaticBolts_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.BasicPrismaticBolts, DoBehavior_BasicPrismaticBolts);
        }

        /// <summary>
        /// Performs the Empress' Basic Prismatic Bolts attack.
        /// </summary>
        public void DoBehavior_BasicPrismaticBolts()
        {
            DoBehavior_BasicPrismaticBolts_HoverAround();

            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PointingUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        public void DoBehavior_BasicPrismaticBolts_HoverAround()
        {
            float redirectSpeed = Utils.Remap(AITimer, 0f, 25f, 0.13f, 0.07f);
            Vector2 hoverDestination = Target.Center + new Vector2(MathF.Cos(MathHelper.TwoPi * AITimer / 90f) * 300f, -250f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, redirectSpeed, 1f - redirectSpeed, 50f);
            NPC.rotation = NPC.velocity.X * 0.01f;

            if (AITimer == BasicPrismaticBolts_BoltShootDelay)
                SoundEngine.PlaySound(SoundID.Item164);

            bool ableToShoot = AITimer >= BasicPrismaticBolts_BoltShootDelay && AITimer <= BasicPrismaticBolts_BoltShootDelay + BasicPrismaticBolts_BoltShootTime && !NPC.WithinRange(Target.Center, 150f);
            Vector2 handPosition = NPC.Center + new Vector2(30f, -64f).RotatedBy(NPC.rotation);
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % BasicPrismaticBolts_BoltShootRate == 0 && ableToShoot)
            {
                Vector2 boltVelocity = (MathHelper.TwoPi * AITimer / 45f).ToRotationVector2() * BasicPrismaticBolts_BoltFireSpeed;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target, AITimer / 45f % 1f);
            }
        }
    }
}
