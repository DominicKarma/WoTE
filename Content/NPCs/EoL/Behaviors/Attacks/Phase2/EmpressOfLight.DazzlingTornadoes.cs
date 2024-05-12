using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        [AutomatedMethodInvoke]
        public void LoadStateTransitions_DazzlingTornadoes()
        {
            StateMachine.RegisterTransition(EmpressAIType.DazzlingTornadoes, null, false, () =>
            {
                return AITimer >= 9999999;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.DazzlingTornadoes, DoBehavior_DazzlingTornadoes);
        }

        /// <summary>
        /// Performs the Empress' Dazzling Tornadoes attack.
        /// </summary>
        public void DoBehavior_DazzlingTornadoes()
        {
            NPC.velocity *= 0.94f;

            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PalmRaisedUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;

            int shootRate = DazzlingTornado.Lifetime / 2;
            if (AITimer == 1)
                TeleportTo(Target.Center + Target.velocity.SafeNormalize(Main.rand.NextVector2Unit()) * 450f);

            float horizontalOffset = MathF.Sin(MathHelper.TwoPi * AITimer / 150f) * 300f;
            NPC.SimpleFlyMovement(NPC.SafeDirectionTo(Target.Center + new Vector2(horizontalOffset, -300f)) * 20f, 0.37f);

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % shootRate == 1)
            {
                NPC.velocity -= NPC.SafeDirectionTo(Target.Center) * 38f;
                NPC.netUpdate = true;

                Vector2 tornadoVelocity = NPC.SafeDirectionTo(Target.Center) * 150f;
                Vector2 perpendicularToTarget = NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.PiOver2);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 tornadoSpawnPosition = NPC.Center + Main.rand.NextVector2Circular(120f, 120f) + perpendicularToTarget * MathHelper.Lerp(-400f, 400f, i / 3f);
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), tornadoSpawnPosition, tornadoVelocity * Main.rand.NextFloat(0.75f, 1f), ModContent.ProjectileType<DazzlingTornado>(), AcceleratingRainbowDamage, 0f, -1, 1f, 0f);
                }
            }
        }
    }
}
