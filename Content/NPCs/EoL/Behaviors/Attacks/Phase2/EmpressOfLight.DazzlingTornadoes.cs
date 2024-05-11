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

            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer % 9 == 1 && AITimer % 300 <= 60)
            {
                Vector2 tornadoVelocity = NPC.SafeDirectionTo(Target.Center) * 30f;
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, tornadoVelocity, ModContent.ProjectileType<DazzlingTornado>(), AcceleratingRainbowDamage, 0f, -1, 1f, 0f, Main.rand.NextFloatDirection() * MathHelper.TwoPi / 160f);
            }
        }
    }
}
