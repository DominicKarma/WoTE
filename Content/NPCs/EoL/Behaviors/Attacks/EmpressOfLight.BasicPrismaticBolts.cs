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
                return AITimer >= 210;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.BasicPrismaticBolts, DoBehavior_BasicPrismaticBolts);
        }

        /// <summary>
        /// Performs the Empress' Basic Prismatic Bolts attack.
        /// </summary>
        public void DoBehavior_BasicPrismaticBolts()
        {
            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PointingUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0026f;

            DoBehavior_BasicPrismaticBolts_HoverAround();
        }

        public void DoBehavior_BasicPrismaticBolts_HoverAround()
        {
            Vector2 hoverDestination = Target.Center + new Vector2(MathF.Cos(MathHelper.TwoPi * AITimer / 90f) * 300f, -150f);
            NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, 0.07f, 0.93f, 50f);
            NPC.rotation = NPC.velocity.X * 0.01f;

            if (AITimer == 1)
                SoundEngine.PlaySound(SoundID.Item164, NPC.Center);

            Vector2 handPosition = NPC.Center + new Vector2(30f, -64f).RotatedBy(NPC.rotation);
            if (AITimer % 4 == 3 && AITimer <= 90)
            {
                Vector2 boltVelocity = (MathHelper.TwoPi * AITimer / 45f).ToRotationVector2() * 10.5f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, boltVelocity, ModContent.ProjectileType<PrismaticBolt>(), 150, 0f, -1, NPC.target, AITimer / 45f % 1f);
            }
        }
    }
}
