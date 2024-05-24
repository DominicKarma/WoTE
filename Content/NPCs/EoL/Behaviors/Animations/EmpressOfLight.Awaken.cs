using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;
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

        /// <summary>
        /// How long the Empress' awaken state animation goes on for by default.
        /// </summary>
        public static int Awaken_AnimationTime => Utilities.SecondsToFrames(4.5f);

        /// <summary>
        /// How long the Empress waits before speaking in her awaken state animation by default.
        /// </summary>
        public static int Awaken_TextAppearDelay => Utilities.SecondsToFrames(1.5f);

        /// <summary>
        /// How long the Empress' awaken state animation goes on for when enraged.
        /// </summary>
        public static int Awaken_AnimationTime_Enraged => Utilities.SecondsToFrames(3f);

        /// <summary>
        /// How long the Empress waits before speaking in her awaken state when enraged.
        /// </summary>
        public static int Awaken_TextAppearDelay_Enraged => Utilities.SecondsToFrames(1.5f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_Awaken()
        {
            StateMachine.RegisterTransition(EmpressAIType.Awaken, EmpressAIType.VanillaPrismaticBolts, false, () =>
            {
                return AITimer >= Awaken_AnimationTime && !Awaken_IsEnraged;
            });
            StateMachine.RegisterTransition(EmpressAIType.Awaken, EmpressAIType.Enraged, false, () =>
            {
                return AITimer >= Awaken_AnimationTime_Enraged && Awaken_IsEnraged;
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

            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.UnitY * MathF.Sin(MathHelper.TwoPi * AITimer / Awaken_AnimationTime * 2f) * 0.6f, 0.16f);
            NPC.Opacity = Utilities.InverseLerp(0f, 12f, AITimer);

            NPC.dontTakeDamage = true;

            if (AITimer == 2)
            {
                PerformVFXForMultiplayer(() =>
                {
                    ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 33f, shakeStrengthDissipationIncrement: 0.45f);
                    ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center, Vector2.Zero, 32f, 1f, 0.25f, 0.015f);
                });
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);
            }

            LeftHandFrame = EmpressHandFrame.OpenHandDownwardArm;
            RightHandFrame = EmpressHandFrame.OpenHandDownwardArm;
            if (Awaken_IsEnraged)
            {
                Palette = EmpressPalettes.EnragedPaletteSet;

                if (AITimer >= Awaken_TextAppearDelay_Enraged)
                {
                    EmpressDialogueSystem.ShakyDialogue = true;
                    EmpressDialogueSystem.DialogueKeySuffix = "AngryIntroduction";
                    EmpressDialogueSystem.DialogueColor = new(255, 3, 27);
                    EmpressDialogueSystem.DialogueOpacity = Utilities.InverseLerpBump(Awaken_TextAppearDelay_Enraged, Awaken_TextAppearDelay_Enraged + 30f, Awaken_AnimationTime_Enraged - 20f, Awaken_AnimationTime_Enraged - 10f, AITimer);
                }
                Music = 0;
            }
            else if (AITimer >= Awaken_TextAppearDelay)
            {
                EmpressDialogueSystem.ShakyDialogue = false;
                EmpressDialogueSystem.DialogueKeySuffix = "BaseIntroduction";
                EmpressDialogueSystem.DialogueColor = Color.HotPink;
                EmpressDialogueSystem.DialogueOpacity = Utilities.InverseLerpBump(Awaken_TextAppearDelay, Awaken_TextAppearDelay + 30f, Awaken_AnimationTime - 40f, Awaken_AnimationTime - 11f, AITimer);
                if (Main.bloodMoon)
                {
                    EmpressDialogueSystem.DialogueKeySuffix = "BloodMoonIntroduction";
                    EmpressDialogueSystem.DialogueColor = new(255, 9, 32);
                }
                if (Main.dayTime)
                {
                    EmpressDialogueSystem.DialogueKeySuffix = "DayIntroduction";
                    EmpressDialogueSystem.DialogueColor = Color.Orange;
                }
                if (Main.eclipse)
                {
                    EmpressDialogueSystem.DialogueKeySuffix = "EclipseIntroduction";
                    EmpressDialogueSystem.DialogueColor = Color.Yellow;
                }
            }
        }
    }
}
