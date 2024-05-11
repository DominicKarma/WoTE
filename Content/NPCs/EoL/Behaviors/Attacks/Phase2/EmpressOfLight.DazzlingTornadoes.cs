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
            RightHandFrame = EmpressHandFrame.PointingUp;

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 240 == 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 tornadoVelocity = NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.Lerp(-1.89f, 1.89f, i / 4f)) * 25f;
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, tornadoVelocity, ModContent.ProjectileType<DazzlingTornado>(), AcceleratingRainbowDamage, 0f);
                }
            }
        }
    }
}
