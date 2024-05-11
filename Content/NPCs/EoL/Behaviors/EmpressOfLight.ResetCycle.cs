using System.Collections.Generic;
using System.Linq;
using Luminance.Common.StateMachines;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// The set of phase 1 attack combinations that may be selected.
        /// </summary>
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

                var phaseCycle = ChooseNextCycle();
                for (int i = phaseCycle.Length - 1; i >= 0; i--)
                    StateMachine.StateStack.Push(StateMachine.StateRegistry[phaseCycle[i]]);
            });
        }

        /// <summary>
        /// Chooses an attack state cycle that the empress should perform at random, based on phase.
        /// </summary>
        /// <returns>The new attack state cycle to perform.</returns>
        public EmpressAIType[] ChooseNextCycle()
        {
            EmpressAIType[] phaseCycle;

            var statesToAvoid = PreviousStatesReversed.Take(4);
            do
            {
                phaseCycle = Main.rand.Next(Phase1AttackCombos);
            }
            while (statesToAvoid.Contains(phaseCycle[0]));

            return phaseCycle;
        }
    }
}
