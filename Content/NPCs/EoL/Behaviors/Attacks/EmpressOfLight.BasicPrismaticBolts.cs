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
        [AutomatedMethodInvoke]
        public void LoadStateTransitions_BasicPrismaticBolts()
        {
            StateMachine.RegisterTransition(EmpressAIType.BasicPrismaticBolts, null, false, () =>
            {
                return AITimer >= 240;
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

            DashAfterimageInterpolant = Utilities.InverseLerp(0f, 30f, AITimer) * 0.1f;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        public void DoBehavior_BasicPrismaticBolts_HoverAround()
        {
            float redirectSpeed = Utils.Remap(AITimer, 0f, 25f, 0.13f, 0.07f);
            Vector2 hoverDestination = Target.Center + new Vector2(MathF.Cos(MathHelper.TwoPi * AITimer / 90f) * 300f, -250f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, redirectSpeed, 1f - redirectSpeed, 50f);
            NPC.rotation = NPC.velocity.X * 0.01f;

            if (AITimer == 1)
                SoundEngine.PlaySound(SoundID.Item164, NPC.Center);

            Vector2 handPosition = NPC.Center + new Vector2(30f, -64f).RotatedBy(NPC.rotation);
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 4 == 3 && AITimer >= 30 && AITimer <= 120 && !NPC.WithinRange(Target.Center, 150f))
            {
                Vector2 boltVelocity = (MathHelper.TwoPi * AITimer / 45f).ToRotationVector2() * 10.5f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), PrismaticBoltDamage, 0f, -1, NPC.target, AITimer / 45f % 1f);
            }
        }
    }
}
