using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Awaken()
        {
            StateMachine.RegisterTransition(EmpressAIType.Awaken, EmpressAIType.OutwardRainbows, false, () =>
            {
                return AITimer >= 170;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.Awaken, DoBehavior_Awaken);
        }

        /// <summary>
        /// Performs the Empress' awaken state.
        /// </summary>
        public void DoBehavior_Awaken()
        {
            if (AITimer <= 5)
                NPC.velocity = Vector2.UnitY * 12f;

            NPC.velocity *= 0.84f;
            NPC.Opacity = Utilities.InverseLerp(0f, 30f, AITimer);

            LeftHandFrame = EmpressHandFrame.HandPressedToChest;
            RightHandFrame = EmpressHandFrame.HandPressedToChest;

            EmpressDialogueSystem.DialogueOpacity = Utilities.InverseLerpBump(10f, 40f, 130f, 140f, AITimer - 18);
            EmpressDialogueSystem.DialogueKeySuffix = "BaseIntroduction";
            EmpressDialogueSystem.DialogueColor = new(255, 4, 72);
            EmpressDialogueSystem.ShakyDialogue = true;

            EmpressDialogueSystem.ShakyDialogue = false;
            EmpressDialogueSystem.DialogueColor = Color.HotPink;
        }
    }
}
