using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        [AutomatedMethodInvoke]
        public void LoadStateTransitions_PrismaticOverload()
        {
            StateMachine.RegisterTransition(EmpressAIType.PrismaticOverload, null, false, () =>
            {
                return AITimer >= 99999999;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.PrismaticOverload, DoBehavior_PrismaticOverload);
        }

        /// <summary>
        /// Performs the Empress' Prismatic Overload attack.
        /// </summary>
        public void DoBehavior_PrismaticOverload()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient && AITimer == 1)
                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<MagicCircle>(), 0, 0f);

            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PalmRaisedUp;

            NPC.velocity *= 0.9f;
            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }
    }
}
