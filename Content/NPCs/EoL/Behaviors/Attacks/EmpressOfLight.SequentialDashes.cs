﻿using System;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;
using WoTE.Content.Particles;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// The amount of dashes the Empress has performed so far during the Sequential Dashes attack.
        /// </summary>
        public ref float SequentialDashes_DashCounter => ref NPC.ai[0];

        /// <summary>
        /// The direction the Empress will attempt to dash in during the Sequential Dashes attack when she's ready.
        /// </summary>
        public ref float SequentialDashes_DashDirection => ref NPC.ai[1];

        /// <summary>
        /// How long the Empress spends redirecting during her Sequential Dashes attack.
        /// </summary>
        public static int SequentialDashes_RedirectTime => Utilities.SecondsToFrames(Main.dayTime ? 0.39f : 0.63f);

        /// <summary>
        /// How long the Empress spends dashing during her Sequential Dashes attack.
        /// </summary>
        public static int SequentialDashes_DashTime => Utilities.SecondsToFrames(Main.dayTime ? 0.34f : 0.42f);

        /// <summary>
        /// How long the Empress spends slowing down during her Sequential Dashes attack, after her dash.
        /// </summary>
        public static int SequentialDashes_SlowDownTime => Utilities.SecondsToFrames(0.15f);

        /// <summary>
        /// The amount of dashes that the Empress should perform during her Sequential Dashes attack before choosing a new attack. 
        /// </summary>
        public static int SequentialDashes_DashCount => 3;

        /// <summary>
        /// The amount of star bursts that the Empress should perform release her Sequential Dashes attack after a dash concludes. 
        /// </summary>
        public int SequentialDashes_StarBurstReleaseCount => ByPhase(9, 12) + Main.dayTime.ToInt() * 8;

        /// <summary>
        /// The speed of dashes performed by the Empress during her Sequential Dashes attack.
        /// </summary>
        public float SequentialDashes_DashSpeed => ByPhase(69f, 78f) + Main.dayTime.ToInt() * 23.5f;

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_SequentialDashes()
        {
            StateMachine.RegisterTransition(EmpressAIType.SequentialDashes, null, false, () =>
            {
                return SequentialDashes_DashCounter >= SequentialDashes_DashCount && AITimer >= SequentialDashes_RedirectTime * 0.5f;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.SequentialDashes, DoBehavior_SequentialDashes);
        }

        /// <summary>
        /// Performs the Empress' Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes()
        {
            if (AITimer <= SequentialDashes_RedirectTime)
            {
                if (AITimer == 1 && SequentialDashes_DashCounter < SequentialDashes_DashCount)
                    TeleportTo(Target.Center + Main.rand.NextVector2CircularEdge(500f, 400f), (int)(DefaultTeleportDuration * 1.15f));

                DoBehavior_SequentialDashes_Redirect();

                if (AITimer == SequentialDashes_RedirectTime)
                    DoBehavior_SequentialDashes_PerformStartDashEffects();
            }
            else if (AITimer <= SequentialDashes_RedirectTime + SequentialDashes_DashTime)
            {
                DoBehavior_SequentialDashes_PerformDash();
            }
            else if (AITimer <= SequentialDashes_RedirectTime + SequentialDashes_DashTime + SequentialDashes_SlowDownTime)
            {
                NPC.velocity *= 0.86f;
                DashAfterimageInterpolant *= 0.85f;
            }
            else
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    AITimer = 0;
                    SequentialDashes_DashCounter++;
                    NPC.netUpdate = true;

                    if (SequentialDashes_DashCounter < SequentialDashes_DashCount)
                    {
                        for (int i = 0; i < SequentialDashes_StarBurstReleaseCount; i++)
                        {
                            Vector2 boltVelocity = NPC.SafeDirectionTo(Target.Center).RotatedBy(MathHelper.TwoPi * i / SequentialDashes_StarBurstReleaseCount) * 1.14f;
                            Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), NPC.Center, boltVelocity, ModContent.ProjectileType<StarBolt>(), StarBurstDamage, 0f, -1);
                        }
                    }
                }
            }

            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.OutstretchedDownwardHand;

            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
            NPC.rotation = NPC.velocity.X * 0.00175f;
        }

        /// <summary>
        /// Performs initial redirecting behaviors for the Empress' Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_Redirect()
        {
            float flySpeedInterpolant = MathF.Pow(AITimer / (float)SequentialDashes_RedirectTime, 0.6f);
            Vector2 hoverOffsetDirection = Target.SafeDirectionTo(NPC.Center) * new Vector2(1f, 0.995f);
            float backwardsWindUpOffset = Utilities.InverseLerp(-26f, -4f, SequentialDashes_RedirectTime - AITimer).Squared() * 670f;
            Vector2 hoverDestination = Target.Center + hoverOffsetDirection * new Vector2(400f, 390f) - NPC.SafeDirectionTo(Target.Center) * backwardsWindUpOffset;

            NPC.SmoothFlyNear(hoverDestination, flySpeedInterpolant * 0.26f, 1f - flySpeedInterpolant * 0.28f);
        }

        /// <summary>
        /// Performs frame-one effects for the Empress' dashes during the Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_PerformStartDashEffects()
        {
            // Reset the old position and rotation arrays, to ensure that the dash starts off with a brand new
            // afterimage state.
            NPC.oldRot = new float[NPC.oldRot.Length];
            NPC.oldPos = new Vector2[NPC.oldPos.Length];

            SoundEngine.PlaySound(SoundID.Item160 with { MaxInstances = 0 });

            SequentialDashes_DashDirection = NPC.AngleTo(Target.Center);
            NPC.netUpdate = true;

            ScreenShakeSystem.StartShakeAtPoint(NPC.Center, 4f);
        }

        /// <summary>
        /// Performs during-dash effects for the Empress' during the Sequential Dashes attack.
        /// </summary>
        public void DoBehavior_SequentialDashes_PerformDash()
        {
            float dashInterpolant = Utilities.InverseLerp(0f, 11f, AITimer - SequentialDashes_RedirectTime);
            float targetDirectionErringInterpolant = Utilities.InverseLerp(0f, SequentialDashes_DashTime, AITimer - SequentialDashes_RedirectTime) * Utilities.InverseLerp(200f, 400f, NPC.Distance(Target.Center));
            Vector2 targetDirectionErring = NPC.SafeDirectionTo(Target.Center - Vector2.UnitY * 9f) * targetDirectionErringInterpolant;
            Vector2 idealVelocity = (SequentialDashes_DashDirection.ToRotationVector2() + targetDirectionErring * 0.66f) * SequentialDashes_DashSpeed;

            NPC.velocity = Vector2.Lerp(NPC.velocity, idealVelocity, dashInterpolant * 0.16f);
            NPC.damage = NPC.defDamage;
            DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, 1f, 0.06f);
            BlurInterpolant = dashInterpolant * 0.775f;

            if (AITimer % 5 == 0)
            {
                PerformVFXForMultiplayer(() =>
                {
                    ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center + Main.rand.NextVector2Circular(75f, 75f) - NPC.velocity, Vector2.Zero, 120f, 1f, 0.1f, 0.016f);
                });
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 particleVelocity = -NPC.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(15f, 25f) + Main.rand.NextVector2Circular(3f, 3f);
                Color particleColor = Palette.MulticolorLerp(EmpressPaletteType.Phase2Dress, Main.rand.NextFloat()) * 0.8f;
                BloomCircleParticle particle = new(NPC.Center + Main.rand.NextVector2Circular(80f, 80f), particleVelocity, Vector2.One * Main.rand.NextFloat(0.015f, 0.042f), Color.Wheat, particleColor, 120, 1.8f, 1.75f);
                particle.Spawn();
            }
        }
    }
}
