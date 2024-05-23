using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the Empress was summoned in an enraged state or not.
        /// </summary>
        public bool Awaken_IsEnraged
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value.ToInt();
        }

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Awaken()
        {
            StateMachine.RegisterTransition(EmpressAIType.Awaken, EmpressAIType.VanillaPrismaticBolts, false, () =>
            {
                return AITimer >= 180 && !Awaken_IsEnraged;
            });
            StateMachine.RegisterTransition(EmpressAIType.Awaken, EmpressAIType.Enraged, false, () =>
            {
                return AITimer >= 180 && Awaken_IsEnraged;
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
            if (Awaken_IsEnraged)
            {
                Palette = EmpressPalettes.EnragedPaletteSet;

                LeftHandFrame = EmpressHandFrame.OpenHandDownwardArm;
                RightHandFrame = EmpressHandFrame.OpenHandDownwardArm;
                if (AITimer >= 50)
                {
                    EmpressDialogueSystem.ShakyDialogue = true;
                    EmpressDialogueSystem.DialogueOpacity = Utilities.InverseLerpBump(60f, 90f, 174f, 179f, AITimer);
                    EmpressDialogueSystem.DialogueKeySuffix = "AngryIntroduction";
                    EmpressDialogueSystem.DialogueColor = new(255, 3, 27);
                }
                Music = 0;
            }
        }
    }
}
