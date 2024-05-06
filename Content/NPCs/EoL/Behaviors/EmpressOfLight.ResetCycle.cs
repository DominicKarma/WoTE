﻿using System.Collections.Generic;
using Luminance.Common.StateMachines;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ResetCycle()
        {
            StateMachine.RegisterTransition(EmpressAIType.ResetCycle, null, false, () => true, () =>
            {
                StateMachine.StateStack.Clear();

                List<EmpressAIType> phaseCycle = [EmpressAIType.SequentialDashes, EmpressAIType.BasicPrismaticBolts];

                // Supply the state stack with the attack cycle.
                for (int i = phaseCycle.Count - 1; i >= 0; i--)
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[phaseCycle[i]]);
            });
        }
    }
}