namespace WoTE.Common.EquationSolvers
{
    public static class CommonDynamicMovementConfigurations
    {
        /// <summary>
        /// Represents default movement, simply mapping the equation of y = x.
        /// </summary>
        public static readonly DynamicMovementConfiguration Default = new(0f, 0f, 0f, 0f);

        /// <summary>
        /// Represents robotic movement, resulting in underdamping and jerky starting motion.
        /// </summary>
        public static readonly DynamicMovementConfiguration Robotic = new(2.2f, 0.5f, -2.3f);

        /// <summary>
        /// Represents smooth acceleration movement, resulting in very fast initial motion but a smooth slowdown in the aim of reaching the destination.
        /// </summary>
        public static readonly DynamicMovementConfiguration SmoothAcceleration = new(2.1f, 1.2f, 0.6f);
    }
}
