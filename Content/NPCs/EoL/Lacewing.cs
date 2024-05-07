using System;
using System.IO;
using Luminance.Assets;
using Luminance.Common.Utilities;
using Luminance.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace WoTE.Content.NPCs.EoL
{
    public class Lacewing : ModNPC
    {
        /// <summary>
        /// Represents a variant that this Lacewing can be.
        /// </summary>
        /// <param name="VanillaNPCID">The vanilla NPC ID that should be used for the purposes of texturing.</param>
        /// <param name="FrameStart">Which frame indicates the starting frame for this variant.</param>
        public record LacewingType(int VanillaNPCID, int FrameStart)
        {
            private static readonly WeightedRandom<LacewingType> lacewingRng = new();

            /// <summary>
            /// The monarch butterfly variant.
            /// </summary>
            public static readonly LacewingType Monarch = RegisterNew(NPCID.Butterfly, 0, 1f);

            /// <summary>
            /// The purple emperor variant.
            /// </summary>
            public static readonly LacewingType PurpleEmperor = RegisterNew(NPCID.Butterfly, 3, 1f);

            /// <summary>
            /// The red admiral variant.
            /// </summary>
            public static readonly LacewingType RedAdmiral = RegisterNew(NPCID.Butterfly, 6, 1f);

            /// <summary>
            /// The ulysses butterfly variant.
            /// </summary>
            public static readonly LacewingType Ulysses = RegisterNew(NPCID.Butterfly, 9, 1f);

            /// <summary>
            /// The sulphur butterfly variant.
            /// </summary>
            public static readonly LacewingType Sulphur = RegisterNew(NPCID.Butterfly, 12, 1f);

            /// <summary>
            /// The tree nymph variant.
            /// </summary>
            public static readonly LacewingType TreeNymph = RegisterNew(NPCID.Butterfly, 15, 1f);

            /// <summary>
            /// The zebra swallowtail variant.
            /// </summary>
            public static readonly LacewingType ZebraSwallowtail = RegisterNew(NPCID.Butterfly, 18, 1f);

            /// <summary>
            /// The julia butterfly variant.
            /// </summary>
            public static readonly LacewingType Julia = RegisterNew(NPCID.Butterfly, 21, 1f);

            /// <summary>
            /// The prismatic lacewing variant.
            /// </summary>
            public static readonly LacewingType Prismatic = RegisterNew(NPCID.EmpressButterfly, 0, 3.2f);

            /// <summary>
            /// The gold butterfly variant.
            /// </summary>
            public static readonly LacewingType Gold = RegisterNew(NPCID.GoldButterfly, 0, 0.1f);

            /// <summary>
            /// Registers a new lacewing variant in the designated RNG.
            /// </summary>
            /// <param name="vanillaNPCID">The vanilla NPC ID that should be used for the purposes of texturing.</param>
            /// <param name="frameStart">Which frame indicates the starting frame for this variant.</param>
            /// <param name="probabilityWeight">The probability weight, for use when selecting a random lacewing type.</param>
            /// <returns>The new lacewing variant.</returns>
            public static LacewingType RegisterNew(int vanillaNPCID, int frameStart, float probabilityWeight)
            {
                LacewingType result = new(vanillaNPCID, frameStart);
                lacewingRng.Add(result, probabilityWeight);
                return result;
            }

            /// <summary>
            /// Selects a random lacewing type.
            /// </summary>
            public static LacewingType GetRandom() => lacewingRng.Get();
        }

        /// <summary>
        /// The variant that this Lacewing is.
        /// </summary>
        public LacewingType Variant
        {
            get;
            set;
        }

        public int AITimer => EmpressOfLight.Myself.As<EmpressOfLight>().AITimer;

        /// <summary>
        /// The frame of this Lacewing.
        /// </summary>
        public ref float Frame => ref NPC.localAI[0];

        public ref float DashAfterimageInterpolant => ref NPC.localAI[1];

        public override string Texture => $"Terraria/Images/NPC_{Variant?.VanillaNPCID ?? NPCID.Butterfly}";

        public override void SetStaticDefaults()
        {
            this.ExcludeFromBestiary();

            Main.npcFrameCount[Type] = 3;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            NPCID.Sets.TrailingMode[Type] = 3;
            NPCID.Sets.TrailCacheLength[Type] = 12;
        }

        public override void SetDefaults()
        {
            NPC.npcSlots = 100f;
            NPC.damage = 200;
            NPC.width = 20;
            NPC.height = 20;
            NPC.defense = 0;
            NPC.SetLifeMaxByMode(200000, 300000, 400000);

            if (Main.expertMode)
                NPC.damage = 125;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = null;
            NPC.value = 0;
            NPC.netAlways = true;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Variant.VanillaNPCID);
            writer.Write(Variant.FrameStart);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            int variantNPCID = reader.ReadInt32();
            int variantFrameStart = reader.ReadInt32();
            Variant = new(variantNPCID, variantFrameStart);
        }

        public override void OnSpawn(IEntitySource source) => Variant = LacewingType.GetRandom();

        public override void AI()
        {
            if (EmpressOfLight.Myself is null)
            {
                NPC.active = false;
                return;
            }

            NPC.realLife = EmpressOfLight.Myself.whoAmI;
            NPC.life = EmpressOfLight.Myself.life;
            NPC.lifeMax = EmpressOfLight.Myself.lifeMax;
            NPC.target = EmpressOfLight.Myself.target;
            Player target = Main.player[NPC.target];

            if (Variant == LacewingType.Prismatic && Main.rand.NextBool(3))
            {
                Dust rainbow = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(16f, 16f), 261);
                rainbow.color = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.5f);
                rainbow.velocity = Main.rand.NextVector2Circular(0.3f, 1f);
                rainbow.fadeIn = 1f;
                rainbow.noGravity = true;
            }

            if (MathF.Abs(NPC.velocity.X) >= 0.4f)
                NPC.spriteDirection = NPC.velocity.X.NonZeroSign();

            // Ensure that Lacewings do not despawn naturally.
            NPC.timeLeft = 7200;

            int redirectTime = EmpressOfLight.ButterflyBurstDashes_RedirectTime;
            int dashRepositionTime = EmpressOfLight.ButterflyBurstDashes_DashRepositionTime;
            int dashTime = EmpressOfLight.SequentialDashes_DashTime;
            int slowdownTime = EmpressOfLight.ButterflyBurstDashes_DashSlowdownTime;
            int attackCycleTime = redirectTime + dashRepositionTime + dashTime + slowdownTime;
            int wrappedAITimer = AITimer % attackCycleTime;
            bool doneDashing = AITimer >= attackCycleTime * EmpressOfLight.ButterflyBurstDashes_DashCount;

            if (doneDashing)
            {
                NPC.SmoothFlyNear(EmpressOfLight.Myself.Center, 0.17f, 0.8f);
                if (NPC.WithinRange(EmpressOfLight.Myself.Center, 60f))
                    NPC.active = false;
                NPC.damage = 0;
            }
            else if (wrappedAITimer <= redirectTime)
            {
                float flySpeedInterpolant = MathHelper.Lerp(0.4f, 0.05f, wrappedAITimer / (float)redirectTime);
                if (wrappedAITimer <= 1)
                    NPC.ai[1] = target.velocity.ToRotation();

                Vector2 baseDirectionOffset = NPC.ai[1].ToRotationVector2();
                Vector2 hoverOffset = (baseDirectionOffset * new Vector2(620f, 400f)).RotatedBy(NPC.ai[0]);
                hoverOffset += (NPC.whoAmI * 2f).ToRotationVector2() * 50f;

                if (Vector2.Dot(hoverOffset, baseDirectionOffset) * (NPC.ai[0] >= 2f).ToDirectionInt() < 0f)
                    hoverOffset *= -1f;

                Vector2 hoverDestination = target.Center + hoverOffset;
                NPC.SmoothFlyNear(hoverDestination, flySpeedInterpolant, 1f - flySpeedInterpolant);
            }
            else if (wrappedAITimer <= redirectTime + dashRepositionTime)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, NPC.SafeDirectionTo(target.Center) * 70f, 0.27f);
            }
            else if (wrappedAITimer <= redirectTime + dashRepositionTime + dashTime)
                NPC.velocity += NPC.velocity.SafeNormalize(Vector2.Zero) * 5f;
            else
                NPC.velocity *= 0.5f;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                Frame = (Frame + 1f) % 3f;
                NPC.frameCounter = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (Variant is null)
                return false;

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            SpriteEffects direction = (-NPC.spriteDirection).ToSpriteDirection();

            int totalFrames = Main.npcFrameCount[Variant.VanillaNPCID];
            Rectangle frame = texture.Frame(1, totalFrames, 0, (int)(Frame + Variant.FrameStart));
            NPC.frame = frame;

            RenderTrail();
            DrawRainbowBack(texture, drawPosition, direction);
            Main.EntitySpriteDraw(texture, drawPosition, frame, NPC.GetAlpha(Color.White), NPC.rotation, frame.Size() * 0.5f, NPC.scale, direction);

            return false;
        }
        public float BoltWidthFunction(float completionRatio)
        {
            float baseWidth = 20f;
            float tipCutFactor = Utilities.InverseLerp(0.02f, 0.05f, completionRatio);
            float slownessFactor = Utils.Remap(NPC.velocity.Length(), 3f, 9f, 0.18f, 1f);
            return baseWidth * tipCutFactor * slownessFactor;
        }

        public Color BoltColorFunction(float completionRatio)
        {
            float sineOffset = CalculateSinusoidalOffset(completionRatio);
            return Color.Lerp(Color.White, Color.Black, sineOffset * 0.5f + 0.5f);
        }

        public float CalculateSinusoidalOffset(float completionRatio)
        {
            return MathF.Sin(MathHelper.TwoPi * completionRatio + Main.GlobalTimeWrappedHourly * -9f + NPC.whoAmI) * Utilities.InverseLerp(0.01f, 0.9f, completionRatio);
        }

        public void RenderTrail()
        {
            ManagedShader trailShader = ShaderManager.GetShader("WoTE.PrismaticBoltShader");
            trailShader.TrySetParameter("localTime", Main.GlobalTimeWrappedHourly + NPC.whoAmI * 1.8f);
            trailShader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.FlameLashTrailShape], 2, SamplerState.LinearWrap);
            trailShader.SetTexture(TextureAssets.Extra[ExtrasID.HallowBossGradient], 3, SamplerState.LinearWrap);
            trailShader.Apply();

            float perpendicularOffset = Utils.Remap(NPC.velocity.Length(), 4f, 20f, 23f, 90f) * Utilities.InverseLerp(60f, 15f, NPC.velocity.Length());
            Vector2 perpendicular = NPC.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * perpendicularOffset;
            Vector2[] trailPositions = new Vector2[NPC.oldPos.Length];
            for (int i = 0; i < trailPositions.Length; i++)
            {
                if (NPC.oldPos[i] == Vector2.Zero)
                    continue;

                float sine = CalculateSinusoidalOffset(i / (float)trailPositions.Length);
                trailPositions[i] = NPC.oldPos[i] + perpendicular * sine;
            }

            PrimitiveSettings settings = new(BoltWidthFunction, BoltColorFunction, _ => NPC.Size * 0.5f, Shader: trailShader);
            PrimitiveRenderer.RenderTrail(trailPositions, settings, 25);
        }

        public void DrawRainbowBack(Texture2D texture, Vector2 drawPosition, SpriteEffects direction)
        {
            float offset = MathF.Sin(Main.GlobalTimeWrappedHourly * 2.4f + NPC.whoAmI * 3f) * 16f;
            if (offset < 4f)
                offset = 4f;

            for (int i = 0; i < 6; i++)
            {
                Color color = NPC.GetAlpha(new(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0)) * 0.5f;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f + NPC.rotation).ToRotationVector2() * 2f;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, direction, 0f);
            }

            for (int i = 0; i < 6; i++)
            {
                Color color = new Color(127 - NPC.alpha, 127 - NPC.alpha, 127 - NPC.alpha, 0).MultiplyRGBA(Main.hslToRgb((Main.GlobalTimeWrappedHourly + i / 6f) % 1f, 1f, 0.5f));
                color = NPC.GetAlpha(color);
                color.A = 0;
                Vector2 drawOffset = (MathHelper.TwoPi * i / 6f + NPC.rotation).ToRotationVector2() * offset;
                Main.spriteBatch.Draw(texture, drawPosition + drawOffset, NPC.frame, color, NPC.rotation, NPC.frame.Size() * 0.5f, NPC.scale, direction, 0f);
            }
        }
    }
}
