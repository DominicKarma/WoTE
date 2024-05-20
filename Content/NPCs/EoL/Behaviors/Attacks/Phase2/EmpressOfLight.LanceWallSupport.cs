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
        /// Whether the Empress is currently summoning lance walls down from above.
        /// </summary>
        /// 
        /// <remarks>
        /// This is intended to be activated by the <see cref="EmpressAIType.LanceWallSupport"/> state, and linger into the one state that occurs afterwards, acting as a temporary support piece.
        /// </remarks>
        public bool PerformingLanceWallSupport
        {
            get;
            set;
        }

        /// <summary>
        /// The X position of the Empress' lance wall. Defaults to 0 if the wall is not active.
        /// </summary>
        public float LanceWallXPosition
        {
            get;
            set;
        }

        /// <summary>
        /// How long the Empress waits before flying up during her Lance Wall Support state.
        /// </summary>
        public static int LanceWallSupport_FlyUpwardDelay => Utilities.SecondsToFrames(0.5f);

        /// <summary>
        /// How long the Empress spends flying up during her Lance Wall Support state.
        /// </summary>
        public static int LanceWallSupport_FlyUpwardTime => Utilities.SecondsToFrames(0.4f);

        /// <summary>
        /// How long the Empress spends flying downs during her Lance Wall Support state.
        /// </summary>
        public static int LanceWallSupport_FlyDownwardTime => Utilities.SecondsToFrames(0.87f);

        /// <summary>
        /// The standard angle at which lance walls are summoned during the Empress' Lance Wall Support.
        /// </summary>
        public static float LanceWallSupport_StandardFallAngle => MathHelper.ToRadians(6f);

        /// <summary>
        /// The horizontal move speed interpolant of the Empress' lance wall.
        /// </summary>
        public static float LanceWallSupport_HorizontalMoveSpeedInterpolant => Main.dayTime ? 0.0385f : 0.025f;

        /// <summary>
        /// The horizontal width of the Empress' lance wall.
        /// </summary>
        public static float LanceWallSupport_WallWidth => 200f;

        /// <summary>
        /// The set of all attack states that are acceptable when used in conjunction with lance wall support.
        /// </summary>
        public static readonly EmpressAIType[] AcceptableAttacksForLanceWallSupport = [EmpressAIType.VanillaPrismaticBolts, EmpressAIType.VanillaPrismaticBolts2, EmpressAIType.ConvergingTerraprismas, EmpressAIType.TwirlingPetalSun, EmpressAIType.SpinSwirlRainbows];

        [AutomatedMethodInvoke]
        public void LoadStateTransitions_LanceWallSupport()
        {
            StateMachine.RegisterTransition(EmpressAIType.LanceWallSupport, null, false, () =>
            {
                return AITimer >= LanceWallSupport_FlyUpwardDelay + LanceWallSupport_FlyUpwardTime + LanceWallSupport_FlyDownwardTime;
            }, () =>
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    return;

                Vector2 lanceWallStart = new(LanceWallXPosition, Target.Center.Y + 1400f);
                TeleportTo(Target.Center + Vector2.UnitX * Target.SafeDirectionTo(lanceWallStart).X.NonZeroSign() * 500f);

                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), lanceWallStart, Vector2.Zero, ModContent.ProjectileType<LanceWallTelegraph>(), 0, 0f);
            });

            StateMachine.RegisterStateBehavior(EmpressAIType.LanceWallSupport, DoBehavior_LanceWallSupport);
        }

        /// <summary>
        /// Performs the Empress' Lance Wall Support state.
        /// </summary>
        public void DoBehavior_LanceWallSupport()
        {
            LeftHandFrame = EmpressHandFrame.PalmRaisedUp;
            RightHandFrame = EmpressHandFrame.PalmRaisedUp;

            NPC.velocity.X *= 1.006f;

            if (AITimer == 1)
                TeleportTo(Target.Center + Vector2.UnitX * Target.direction * 720f);

            if (AITimer <= 10)
                NPC.velocity += NPC.SafeDirectionTo(Target.Center) * 0.9f;

            if (AITimer >= LanceWallSupport_FlyUpwardDelay + LanceWallSupport_FlyUpwardTime)
                NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y + 5f, -60f, 90f);
            else if (AITimer >= LanceWallSupport_FlyUpwardDelay)
                NPC.velocity.Y -= 3.2f;

            DashAfterimageInterpolant = Utilities.InverseLerp(0f, 30f, AITimer - LanceWallSupport_FlyUpwardDelay);

            NPC.spriteDirection = 1;
            NPC.rotation = NPC.velocity.X * 0.0035f;

            // Store lance information for the upcoming state.
            // The lance wall cannot spawn directly above the player, to prevent telefrags.
            if (MathHelper.Distance(NPC.Center.X, Target.Center.X) >= 300f)
                LanceWallXPosition = NPC.Center.X;
            PerformingLanceWallSupport = true;
        }

        /// <summary>
        /// Handles support behaviors for the Lance Wall attack as another attack is ongoing.
        /// </summary>
        public void DoBehavior_LanceWallSupport_HandlePostStateSupportBehaviors()
        {
            if (!PerformingLanceWallSupport)
            {
                LanceWallXPosition = 0f;
                return;
            }

            // Wait until the support activation is completed before actually doing anything.
            if (CurrentState == EmpressAIType.LanceWallSupport || CurrentState == EmpressAIType.Teleport)
                return;

            if (AITimer <= 30)
                return;

            float previousLanceWallX = LanceWallXPosition;
            LanceWallXPosition = MathHelper.Lerp(LanceWallXPosition, Target.Center.X, LanceWallSupport_HorizontalMoveSpeedInterpolant);

            if (AITimer % 16 == 15)
                SoundEngine.PlaySound(SoundID.Item162 with { MaxInstances = 0 });

            float shakePower = Utils.Remap(MathHelper.Distance(Target.Center.X, LanceWallXPosition) - LanceWallSupport_WallWidth, 50f, 320f, 3f, 1.5f);
            ScreenShakeSystem.StartShake(shakePower);

            for (int i = 0; i < 2; i++)
            {
                float lanceHue = (AITimer / 30f + Main.rand.NextFloat(0.23f)) % 1f;
                float lanceWallDirection = (LanceWallXPosition - previousLanceWallX).NonZeroSign();
                float lanceWallAngle = LanceWallSupport_StandardFallAngle * -lanceWallDirection;
                Vector2 horizontalSpawnOffset = Vector2.UnitX.RotatedBy(lanceWallAngle) * Main.rand.NextFloatDirection() * LanceWallSupport_WallWidth;
                Vector2 verticalSpawnOffset = -Vector2.UnitY.RotatedBy(lanceWallAngle) * Main.rand.NextFloat(975f, 1100f);
                Vector2 lanceSpawnPosition = new Vector2(LanceWallXPosition, Target.Center.Y) + horizontalSpawnOffset + verticalSpawnOffset;
                Vector2 lanceVelocity = Vector2.UnitY.RotatedBy(lanceWallAngle) * 80f;

                if (lanceSpawnPosition.Y < 150f)
                    lanceSpawnPosition.Y = 150f;

                Utilities.NewProjectileBetter(NPC.GetSource_FromAI(), lanceSpawnPosition, lanceVelocity, ModContent.ProjectileType<LightLance>(), LightLanceDamage, 0f, -1, 0f, lanceHue, 1f);
            }
        }
    }
}
