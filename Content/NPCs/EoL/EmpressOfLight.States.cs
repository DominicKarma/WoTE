using System.Collections.Generic;
using Luminance.Common.StateMachines;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        private PushdownAutomata<EntityAIState<EmpressAIType>, EmpressAIType> stateMachine;

        // TODO -- Sync this.
        /// <summary>
        /// The set of attacks that the Empress previously sequentially performed.
        /// </summary>
        public readonly List<EmpressAIType> PreviousStates = [];

        /// <summary>
        /// The maximum amount of states that the Empress should remember in the <see cref="PreviousStates"/> list before forgetting the oldest one.
        /// </summary>
        public static int MaximumStatesToRemember => 8;

        /// <summary>
        /// The state machine of the Empress. This governs all of her behavior related code.
        /// </summary>
        public PushdownAutomata<EntityAIState<EmpressAIType>, EmpressAIType> StateMachine
        {
            get
            {
                if (stateMachine is null)
                    LoadStates();
                return stateMachine!;
            }
            set => stateMachine = value;
        }

        public void LoadStates()
        {
            // Initialize the AI state machine.
            StateMachine = new(new(EmpressAIType.Awaken));
            StateMachine.OnStateTransition += ResetGenericVariables;

            // Register all states in the state machine.
            for (int i = 0; i < (int)EmpressAIType.Count; i++)
                StateMachine.RegisterState(new((EmpressAIType)i));

            // Load state transitions.
            AutomatedMethodInvokeAttribute.InvokeWithAttribute(this);
        }

        private void ResetGenericVariables(bool stateWasPopped, EntityAIState<EmpressAIType> oldState)
        {
            if (!stateWasPopped || oldState.Identifier == EmpressAIType.Teleport)
                return;

            if (oldState.Identifier != EmpressAIType.ResetCycle)
                PreviousStates.Add(oldState.Identifier);
            if (PreviousStates.Count > MaximumStatesToRemember)
                PreviousStates.RemoveAt(0);

            for (int i = 0; i < 4; i++)
                NPC.ai[i] = 0f;

            NPC.TargetClosest();
            NPC.Opacity = 1f;
            NPC.netUpdate = true;
        }
    }
}
