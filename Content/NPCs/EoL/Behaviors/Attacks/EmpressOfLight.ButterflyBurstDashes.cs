using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// The amount of dashes the Empress has performed so far during her Butterfly Burst Dashes attack.
        /// </summary>
        public ref float ButterflyBurstDashes_DashCounter => ref NPC.ai[0];

        /// <summary>
        /// How long the Empress waits before exploding into butterflies during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_ButterflyTransitionDelay => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// How long the Empress' butterflies spend redirecting during her Butterfly Burst Dashes attack.
        /// </summary>
        public int ButterflyBurstDashes_RedirectTime => Utilities.SecondsToFrames(ButterflyBurstDashes_DashCounter <= 0f ? 1.45f : 1.1f);

        /// <summary>
        /// How long the Empress' butterflies wait before dashing during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashDelay => Utilities.SecondsToFrames(0.17f);

        /// <summary>
        /// How long the Empress' butterflies spend repositioning for the dash during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashRepositionTime => Utilities.SecondsToFrames(0.1f);

        /// <summary>
        /// How long the Empress' butterflies spend dashing during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashTime => Utilities.SecondsToFrames(0.2f);

        /// <summary>
        /// How long the Empress' butterflies spend slowing down after a dash during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashSlowdownTime => Utilities.SecondsToFrames(0.1f);

        /// <summary>
        /// The amount of dashes that should be performed during the Empress' Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_DashCount => 3;

        /// <summary>
        /// The amount of butterflies the Empress explodes into during her Butterfly Burst Dashes attack.
        /// </summary>
        public static int ButterflyBurstDashes_ButterflyCount => 40;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_ButterflyBurstDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.ButterflyBurstDashes, null, false, () =>
            {
                return AITimer >= 5 && !NPC.AnyNPCs(ModContent.NPCType<Lacewing>()) && ButterflyBurstDashes_DashCounter >= ButterflyBurstDashes_DashCount;
            }, () =>
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.ButterflyBurstDashes, DoBehavior_ButterflyBurstDashes);
        }

        /// <summary>
        /// Performs the Empress' Butterfly Burst Dashes attack.
        /// </summary>
        public void DoBehavior_ButterflyBurstDashes()
        {
            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            if (AITimer == ButterflyBurstDashes_ButterflyTransitionDelay - Utilities.SecondsToFrames(0.3f))
                SoundEngine.PlaySound(SoundID.Item165);

            int redirectTime = ButterflyBurstDashes_RedirectTime;
            int dashRepositionTime = ButterflyBurstDashes_DashRepositionTime;
            int dashDelay = ButterflyBurstDashes_DashDelay;
            int dashTime = ButterflyBurstDashes_DashTime;
            int slowdownTime = ButterflyBurstDashes_DashSlowdownTime;
            int attackCycleTime = redirectTime + dashRepositionTime + dashDelay + dashTime + slowdownTime;
            if (AITimer >= ButterflyBurstDashes_ButterflyTransitionDelay)
            {
                if (AITimer == ButterflyBurstDashes_ButterflyTransitionDelay)
                    DoBehavior_ButterflyBurstDashes_SummonLacewings();

                bool doneDashing = ButterflyBurstDashes_DashCounter >= SequentialDashes_DashCount;
                int lacewingCount = NPC.CountNPCS(ModContent.NPCType<Lacewing>());
                if (lacewingCount >= 1 && !doneDashing)
                    DoBehavior_ButterflyBurstDashes_InheritHP();

                NPC.Opacity = lacewingCount >= 1 ? 0f : 1f;
                if (NPC.Opacity <= 0f)
                    NPC.Center = Target.Center - Vector2.UnitY * 300f;
                if (NPC.Opacity >= 1f)
                    ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 9f);

                NPC.hide = NPC.Opacity <= 0f;
                NPC.dontTakeDamage = true;
                NPC.ShowNameOnHover = false;
            }

            if (AITimer >= ButterflyBurstDashes_ButterflyTransitionDelay + attackCycleTime)
            {
                ButterflyBurstDashes_DashCounter++;
                AITimer = ButterflyBurstDashes_ButterflyTransitionDelay + 1;
                NPC.netUpdate = true;
            }

            NPC.velocity *= 0.9f;
            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;
        }

        /// <summary>
        /// Makes the Empress' burst into Lacewings for her Butterfly Burst Dashes attack.
        /// </summary>
        public void DoBehavior_ButterflyBurstDashes_SummonLacewings()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PrismaticBurst>(), 0, 0f);

            int lacewingHealth = (int)MathF.Ceiling(NPC.life / (float)ButterflyBurstDashes_ButterflyCount);
            for (int i = 0; i < ButterflyBurstDashes_ButterflyCount; i++)
            {
                float offsetAngle = Main.rand.NextFloatDirection() * 0.31f;
                if (i >= ButterflyBurstDashes_ButterflyCount / 2)
                    offsetAngle += MathHelper.Pi;

                int lacewingIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Lacewing>(), NPC.whoAmI, offsetAngle, i);
                if (lacewingIndex >= 0 && lacewingIndex < Main.maxNPCs)
                {
                    Main.npc[lacewingIndex].velocity = Main.rand.NextVector2Circular(38f, 24f);
                    Main.npc[lacewingIndex].life = lacewingHealth;
                    Main.npc[lacewingIndex].lifeMax = lacewingHealth;
                }
            }
        }

        /// <summary>
        /// Makes the Empress' collective HP pool depend on all summoned Lacewings.
        /// </summary>
        public void DoBehavior_ButterflyBurstDashes_InheritHP()
        {
            int lacewingID = ModContent.NPCType<Lacewing>();

            NPC.life = 0;
            foreach (NPC lacewing in Main.ActiveNPCs)
            {
                if (lacewing.type == lacewingID)
                    NPC.life += lacewing.life;
            }

            // A minimum HP of 1 is used to ensure that the Empress doesn't unexpectedly die during the attack while technically invisible if all of the lacewings are killed.
            NPC.life = Utils.Clamp(NPC.life, 1, NPC.lifeMax);
        }
    }
}
