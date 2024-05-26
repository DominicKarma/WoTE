using Luminance.Common.Utilities;
using Microsoft.Xna.Framework;

namespace WoTE.Common.EquationSolvers
{
    /// <summary>
    /// Represents a dynamic configuration configuration that may be used via the <see cref="LeapfrogMethodSolver"/>.
    /// </summary>
    public record DynamicMovementConfiguration
    {
        /// <summary>
        /// k1 as defined in the differential equation that this attemps to solve.
        /// </summary>
        public float K1
        {
            get;
            init;
        }

        /// <summary>
        /// k2 as defined in the differential equation that this attemps to solve.
        /// </summary>
        public float K2
        {
            get;
            init;
        }

        /// <summary>
        /// k3 as defined in the differential equation that this attemps to solve.
        /// </summary>
        public float K3
        {
            get;
            init;
        }

        /// <summary>
        /// The frequency of motion resulting from the system.
        /// </summary>
        public float Frequency
        {
            get;
            init;
        }

        /// <summary>
        /// Determines how strongly vibrations resulting from <see cref="Frequency"/> decay.
        /// </summary>
        /// 
        /// <remarks>
        /// At 0, no damping occurs, and the results oscillate forever.<br></br>
        /// Between 0 and 1, the vibrations are weakened, in accordance with the intensity.<br></br>
        /// Beyond 1, vibrations cease entirely, with the greater values resulting in a greater propensity for smooth regression to the norm.
        /// </remarks>
        public float DampingCoefficient
        {
            get;
            init;
        }

        /// <summary>
        /// Determines the initial response of the system.
        /// </summary>
        /// 
        /// <remarks>
        /// At 0, a smooth, natural curve occurs at the threshold of change.<br></br>
        /// Between 0 and 1 the motion becomes harsh and discontinuous at the first derivative, with strength relative to the intensity of the value.<br></br>
        /// Beyond 1, the motion overshoots a bit.<br></br>
        /// Below 0, the motion anticipates a bit.
        /// </remarks>
        public float InitialResponseCoefficient
        {
            get;
            init;
        }

        public DynamicMovementConfiguration(float k1, float k2, float k3, float _)
        {
            K1 = k1;
            K2 = k2;
            K3 = k3;
        }

        public DynamicMovementConfiguration(float frequency, float dampingCoefficient, float initialResponseCoefficient)
        {
            Frequency = frequency;
            DampingCoefficient = dampingCoefficient;
            InitialResponseCoefficient = initialResponseCoefficient;

            K1 = dampingCoefficient / (MathHelper.Pi * frequency);
            K2 = 1f / (MathHelper.TwoPi * frequency).Squared();
            K3 = initialResponseCoefficient * dampingCoefficient / (MathHelper.TwoPi * frequency);
        }

        /// <summary>
        /// Returns a movement configuration as linearly interpolated from one to another.
        /// </summary>
        /// <param name="configA">The starting configuration to interpolate from.</param>
        /// <param name="configB">The ending configuration to interpolate to.</param>
        /// <param name="interpolant"></param>
        public static DynamicMovementConfiguration Lerp(DynamicMovementConfiguration configA, DynamicMovementConfiguration configB, float interpolant)
        {
            float k1 = MathHelper.Lerp(configA.K1, configB.K1, interpolant);
            float k2 = MathHelper.Lerp(configA.K2, configB.K2, interpolant);
            float k3 = MathHelper.Lerp(configA.K3, configB.K3, interpolant);
            return new(k1, k2, k3, 0f);
        }
    }
}
