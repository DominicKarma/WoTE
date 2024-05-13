using Luminance.Common.StateMachines;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Die()
        {
            StateMachine.ApplyToAllStatesExcept(state =>
            {
                StateMachine.RegisterTransition(state, EmpressAIType.Die, false, () => NPC.life <= 1 && CurrentState != EmpressAIType.Teleport);
            }, EmpressAIType.Die);

            StateMachine.RegisterStateBehavior(EmpressAIType.Die, DoBehavior_Die);
        }

        /// <summary>
        /// Performs the Empress' Die state.
        /// </summary>
        public void DoBehavior_Die()
        {
            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;
            NPC.dontTakeDamage = true;

            if (AITimer == 1)
            {
                TeleportTo(NPC.Center - Vector2.UnitY * 5000f, (int)(DefaultTeleportDuration * 1.8f));
                NPC.velocity = Vector2.Zero;
            }

            if (AITimer >= 4)
            {
                // TODO -- Change state to account for the Terraprisma condition.
                NPC.Transform(NPCID.HallowBoss);
                NPC.life = 0;
                NPC.NPCLoot();
                NPC.active = false;
            }
        }

        public override bool CheckDead()
        {
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            return false;
        }
    }
}
