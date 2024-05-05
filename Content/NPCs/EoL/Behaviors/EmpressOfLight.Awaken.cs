using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Awaken()
        {
            StateMachine.RegisterTransition(EmpressAIType.Awaken, null, false, () =>
            {
                return AITimer >= 9999999;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.Awaken, DoBehavior_Awaken);
        }

        /// <summary>
        /// Performs the Empress' awaken state.
        /// </summary>
        public void DoBehavior_Awaken()
        {
            if (AITimer <= 10)
                NPC.velocity = Vector2.UnitY * 5f;

            NPC.velocity *= 0.98f;
            NPC.Opacity = Utilities.InverseLerp(0f, 60f, AITimer);
        }
    }
}
