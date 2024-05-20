using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace WoTE.Content.NPCs.EoL
{
    public class EmpressSkyColorationSystem : ModSystem
    {
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            float backgroundInterpolationIntensity = 0.2f;
            if (Main.dayTime)
                backgroundInterpolationIntensity += Utilities.InverseLerpBump(0f, 2700f, (float)Main.dayLength - 2700f, (float)Main.dayLength, (float)Main.time) * 0.3f;

            if (EmpressSky.BackgroundTint != Color.Transparent)
                backgroundColor = Color.Lerp(backgroundColor, EmpressSky.BackgroundTint, EmpressSky.Opacity * backgroundInterpolationIntensity);
            tileColor = Color.Lerp(tileColor, Color.Lavender, EmpressSky.Opacity * 0.25f);
        }
    }
}
