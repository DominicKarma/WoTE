using Luminance.Common.Easings;
using Luminance.Common.StateMachines;
using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using WoTE.Content.NPCs.EoL.Projectiles;
using WoTE.Content.Particles.Metaballs;

namespace WoTE.Content.NPCs.EoL
{
    public partial class EmpressOfLight : ModNPC
    {
        /// <summary>
        /// Whether the Empress started her Outward Rainbows attack on the right side of the target.
        /// </summary>
        public bool OutwardRainbows_StartedOnRightSideOfTarget
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value.ToInt();
        }

        /// <summary>
        /// How long the Empress spends redirecting during her Outward Rainbows attack.
        /// </summary>
        public int OutwardRainbows_RainbowShootDelay => Utilities.SecondsToFrames(ByPhase(0.9f, 0.78f));

        /// <summary>
        /// How long the Empress spends releasing rainbows on her finger during her Outward Rainbows attack.
        /// </summary>
        public int OutwardRainbows_RainbowShootTime => Utilities.SecondsToFrames(ByPhase(2f, 1.7f));

        /// <summary>
        /// How long the Empress waits before choosing a new attack during her Outward Rainbows attack.
        /// </summary>
        public static int OutwardRainbows_AttackTransitionDelay => Utilities.SecondsToFrames(0.75f);

        /// <summary>
        /// How speed of rainbows summoned during the Empress' Outward Rainbows attack.
        /// </summary>
        public float OutwardRainbows_RainbowSpeed => ByPhase(3f, 5f);

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_OutwardRainbows()
        {
            StateMachine.RegisterTransition(EmpressAIType.OutwardRainbows, null, false, () =>
            {
                return AITimer >= OutwardRainbows_RainbowShootDelay + OutwardRainbows_RainbowShootTime + OutwardRainbows_AttackTransitionDelay;
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.OutwardRainbows, DoBehavior_OutwardRainbows);
        }

        /// <summary>
        /// Performs the Empress' Outward Rainbows attack.
        /// </summary>
        public void DoBehavior_OutwardRainbows()
        {
            LeftHandFrame = EmpressHandFrame.OutstretchedDownwardHand;
            RightHandFrame = EmpressHandFrame.PointingUp;

            if (AITimer <= OutwardRainbows_RainbowShootDelay)
            {
                if (AITimer == 2)
                {
                    SoundEngine.PlaySound(SoundID.Item160, NPC.Center);
                    NPC.oldPos = new Vector2[NPC.oldPos.Length];
                    NPC.oldRot = new float[NPC.oldRot.Length];

                    OutwardRainbows_StartedOnRightSideOfTarget = NPC.OnRightSideOf(Target.Center);
                    NPC.netUpdate = true;
                }

                float dashInterpolant = AITimer / (float)OutwardRainbows_RainbowShootDelay;
                float spinInterpolant = EasingCurves.Quadratic.Evaluate(EasingType.InOut, dashInterpolant);
                Vector2 hoverDestination = Target.Center + Vector2.UnitY.RotatedBy(MathHelper.TwoPi * OutwardRainbows_StartedOnRightSideOfTarget.ToDirectionInt() * spinInterpolant * 1.5f) * 900f;
                NPC.SmoothFlyNearWithSlowdownRadius(hoverDestination, (1f - dashInterpolant) * 0.15f + 0.06f, 0.67f, 30f);
                DashAfterimageInterpolant = MathHelper.Lerp(DashAfterimageInterpolant, Utilities.InverseLerp(4f, 30f, NPC.velocity.Length()), 0.2f);

                if (AITimer == OutwardRainbows_RainbowShootDelay)
                    SoundEngine.PlaySound(SoundID.Item163);

                if (DashAfterimageInterpolant >= 0.4f)
                    ModContent.GetInstance<DistortionMetaball>().CreateParticle(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), Vector2.Zero, 30f, 0.75f, 0.25f, 0.02f);
            }
            else
            {
                NPC.Center = Vector2.Lerp(NPC.Center, Target.Center, 0.013f);
                if (!NPC.WithinRange(Target.Center, 75f))
                    NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(Target.Center) * 2f, 0.08f);
                else
                    NPC.velocity *= 0.98f;

                Vector2 handPosition = NPC.Center + new Vector2(NPC.spriteDirection * 42f, -68f).RotatedBy(NPC.rotation);
                if (Main.netMode != NetmodeID.MultiplayerClient && AITimer <= OutwardRainbows_RainbowShootTime)
                {
                    Vector2 rainbowVelocity = (MathHelper.TwoPi * AITimer / 25f).ToRotationVector2() * OutwardRainbows_RainbowSpeed;
                    Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), handPosition, rainbowVelocity, ModContent.ProjectileType<AcceleratingRainbow>(), AcceleratingRainbowDamage, 0f, -1, AITimer / 20f % 1f);
                }
            }

            if (AITimer == 1)
            {
                TeleportTo(Target.Center - Vector2.UnitX * Target.direction * 400f);
                NPC.velocity = Vector2.Zero;
            }

            NPC.spriteDirection = NPC.velocity.X.NonZeroSign();
            NPC.rotation = NPC.velocity.X * 0.003f;
        }
    }
}
