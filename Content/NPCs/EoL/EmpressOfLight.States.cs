using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminance.Common.StateMachines;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        private PushdownAutomata<EntityAIState<EmpressAIType>, EmpressAIType> stateMachine;

        /// <summary>
        /// The set of attacks that the Empress previously sequentially performed, with the newest being at the end and the oldest being at the start.
        /// </summary>
        public readonly List<EmpressAIType> PreviousStates = new(MaximumStatesToRemember);

        /// <summary>
        /// The set of attacks that the Empress previously sequentially performed, with the newest being at the start and the oldest being at the end.
        /// </summary>
        public EmpressAIType[] PreviousStatesReversed
        {
            get
            {
                EmpressAIType[] lastToFirstStates = new EmpressAIType[PreviousStates.Count];
                for (int i = PreviousStates.Count - 1; i >= 0; i--)
                    lastToFirstStates[PreviousStates.Count - i - 1] = PreviousStates[i];

                return lastToFirstStates;
            }
        }

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

        /// <summary>
        /// Write the status of the <see cref="StateMachine"/> to a <see cref="BinaryWriter"/> for use with syncing.
        /// </summary>
        /// <param name="writer">The writer to supply state machine data to.</param>
        private void WriteStateMachineStack(BinaryWriter writer)
        {
            var stateStack = (StateMachine?.StateStack ?? new()).ToList();
            writer.Write(stateStack.Count);
            for (int i = stateStack.Count - 1; i >= 0; i--)
                writer.Write((byte)stateStack[i].Identifier);
        }

        /// <summary>
        /// Reads the status of the <see cref="StateMachine"/> from a <see cref="BinaryReader"/> for use with syncing.
        /// </summary>
        /// <param name="reader">The reader to read state machine data from.</param>
        private void ReadStateMachineStack(BinaryReader reader)
        {
            int stateStackCount = reader.ReadInt32();
            StateMachine.StateStack.Clear();
            for (int i = 0; i < stateStackCount; i++)
                StateMachine.StateStack.Push(StateMachine.StateRegistry[(EmpressAIType)reader.ReadByte()]);
        }

        /// <summary>
        /// Writes the contents of the <see cref="PreviousStates"/> list to a <see cref="BinaryWriter"/> for use with syncing.
        /// </summary>
        /// <param name="writer">The writer to supply state data to.</param>
        private void WritePreviousStates(BinaryWriter writer)
        {
            writer.Write(PreviousStates.Count);
            for (int i = 0; i < PreviousStates.Count; i++)
                writer.Write((int)PreviousStates[i]);
        }

        /// <summary>
        /// Reads the contents of the <see cref="PreviousStates"/> list to a <see cref="BinaryReader"/> for use with syncing.
        /// </summary>
        /// <param name="reader">The reader to read state data from.</param>
        private void ReadPreviousStates(BinaryReader reader)
        {
            PreviousStates.Clear();
            int previousStateCount = reader.ReadInt32();
            for (int i = 0; i < previousStateCount; i++)
                PreviousStates.Add((EmpressAIType)reader.ReadInt32());
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
