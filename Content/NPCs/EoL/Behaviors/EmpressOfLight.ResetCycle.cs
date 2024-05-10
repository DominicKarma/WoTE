using System.Collections.Generic;
using System.Linq;
using Luminance.Common.StateMachines;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        public static List<EmpressAIType[]> Phase1AttackCombos => new()
        {
            new EmpressAIType[] { EmpressAIType.BasicPrismaticBolts, EmpressAIType.ButterflyBurstDashes },
            new EmpressAIType[] { EmpressAIType.RadialStarBurst, EmpressAIType.TwirlingPetalSun, EmpressAIType.PrismaticBoltDashes },
            new EmpressAIType[] { EmpressAIType.OutwardRainbows, EmpressAIType.SequentialDashes, EmpressAIType.ConvergingTerraprismas },
            new EmpressAIType[] { EmpressAIType.OutwardRainbows, EmpressAIType.ButterflyBurstDashes },
            new EmpressAIType[] { EmpressAIType.ConvergingTerraprismas, EmpressAIType.RadialStarBurst },
        };

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ResetCycle()
        {
            StateMachine.RegisterTransition(EmpressAIType.ResetCycle, null, false, () => true, () =>
            {
                StateMachine.StateStack.Clear();

                EmpressAIType[] phaseCycle;
                List<EmpressAIType> lastToFirstStates = [];
                for (int i = PreviousStates.Count - 1; i >= 0; i--)
                    lastToFirstStates.Add(PreviousStates[i]);

                var statesToAvoid = lastToFirstStates.Take(4);
                do
                {
                    phaseCycle = Main.rand.Next(Phase1AttackCombos);
                }
                while (statesToAvoid.Contains(phaseCycle[0]));

                for (int i = phaseCycle.Length - 1; i >= 0; i--)
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[phaseCycle[i]]);
            });
        }
    }
}
