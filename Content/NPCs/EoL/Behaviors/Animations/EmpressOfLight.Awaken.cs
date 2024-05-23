using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using WoTE.Content.Particles.Metaballs;

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
                return AITimer >= 240 && !Awaken_IsEnraged;
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
            NPC.Opacity = Utilities.InverseLerp(0f, 12f, AITimer);

            if (AITimer == 2)
            {
                ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 40f, shakeStrengthDissipationIncrement: 0.6f);
                ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center, Vector2.Zero, 32f, 1f, 0.25f, 0.015f);
            }

            LeftHandFrame = EmpressHandFrame.OpenHandDownwardArm;
            RightHandFrame = EmpressHandFrame.OpenHandDownwardArm;
            if (Awaken_IsEnraged)
            {
                Palette = EmpressPalettes.EnragedPaletteSet;

                if (AITimer >= 50)
                {
                    EmpressDialogueSystem.ShakyDialogue = true;
                    EmpressDialogueSystem.DialogueKeySuffix = "AngryIntroduction";
                    EmpressDialogueSystem.DialogueColor = new(255, 3, 27);
                    EmpressDialogueSystem.DialogueOpacity = Utilities.InverseLerpBump(60f, 90f, 174f, 179f, AITimer);
                }
                Music = 0;
            }
            else if (AITimer >= 50)
            {
                EmpressDialogueSystem.ShakyDialogue = false;
                EmpressDialogueSystem.DialogueKeySuffix = "BaseIntroduction";
                EmpressDialogueSystem.DialogueColor = Color.HotPink;
                EmpressDialogueSystem.DialogueOpacity = Utilities.InverseLerpBump(60f, 90f, 234f, 239f, AITimer);
                if (Main.dayTime)
                {
                    EmpressDialogueSystem.DialogueKeySuffix = "DayIntroduction";
                    EmpressDialogueSystem.DialogueColor = Color.Orange;
                }
            }
        }
    }
}
