using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace WoTE.Content.NPCs.EoL
{
    public class EmpressDialogueSystem : ModSystem
    {
        /// <summary>
        /// The dialogue key suffix. Used to decide which Empress dialogue should be displayed.
        /// </summary>
        public static string DialogueKeySuffix
        {
            get;
            set;
        }

        /// <summary>
        /// The opacity of dialogue spoken by the empress.
        /// </summary>
        public static float DialogueOpacity
        {
            get;
            set;
        }

        /// <summary>
        /// The color of dialogue spoken by the Empress.
        /// </summary>
        public static Color DialogueColor
        {
            get;
            set;
        }

        /// <summary>
        /// Whether dialogue spoken by the Empress should be shaky or not.
        /// </summary>
        public static bool ShakyDialogue
        {
            get;
            set;
        }

        private bool DrawTextWrapper()
        {
            if (EmpressOfLight.Myself is null)
                return true;

            var font = Mod.Assets.Request<DynamicSpriteFont>("Assets/Fonts/EmpressDialogueText", AssetRequestMode.ImmediateLoad).Value;
            float dialogueScale = 0.8f;
            float spacingPerCharacter = 4f;
            string dialogue = Language.GetTextValue($"Mods.WoTE.Dialogue.{DialogueKeySuffix}");
            Vector2 dialogueDrawPosition = EmpressOfLight.Myself.Top - Vector2.UnitY * 100f - Main.screenPosition;
            foreach (string line in Utils.WordwrapString(dialogue, font, 800, 10, out _))
            {
                if (string.IsNullOrEmpty(line))
                    continue;

                Vector2 characterLineOffset = (font.MeasureString(line) + Vector2.UnitX * line.Length * spacingPerCharacter) * Vector2.UnitX * dialogueScale * -0.5f;
                foreach (char character in line)
                {
                    string characterString = character.ToString();
                    Vector2 characterSize = font.MeasureString(characterString);
                    if (character == 'i')
                        characterSize.X *= 1.25f;

                    Vector2 characterOffset = (ShakyDialogue ? Main.rand.NextVector2CircularEdge(1.7f, 2.4f) * dialogueScale : Vector2.Zero) + characterLineOffset;

                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, characterString, dialogueDrawPosition + characterOffset, DialogueColor * DialogueOpacity, 0f, characterSize * 0.5f, Vector2.One * dialogueScale, -1f, 1f);
                    characterLineOffset.X += dialogueScale * (characterSize.X + spacingPerCharacter);
                }
                dialogueDrawPosition.Y += dialogueScale * 40f;
            }
            return true;
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // Draw the Solyn dialogue UI.
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text", StringComparison.Ordinal));
            if (mouseTextIndex != -1)
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("Wrath of the Empress: Empress Dialogue", DrawTextWrapper, InterfaceScaleType.Game));
        }
    }
}
