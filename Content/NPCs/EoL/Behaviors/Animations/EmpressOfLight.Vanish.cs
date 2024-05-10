using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the Empress was unable to find a valid target after searching for one or not.
        /// </summary>
        /// 
        /// <remarks>
        /// This is used when determining whether she should go away or not.
        /// </remarks>
        public bool NoTargetCouldBeFound
        {
            get;
            set;
        }

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Vanish()
        {
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.Vanish, false, () => NoTargetCouldBeFound);
            }, EmpressAIType.Vanish);

            StateMachine.RegisterStateBehavior(EmpressAIType.Vanish, DoBehavior_Vanish);
        }

        /// <summary>
        /// Performs the Empress' vanish state.
        /// </summary>
        public void DoBehavior_Vanish()
        {
            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;
            NPC.dontTakeDamage = true;

            if (AITimer == 1)
                TeleportTo(Vector2.One * -20000f);

            if (AITimer >= 2)
                NPC.active = false;
        }
    }
}
