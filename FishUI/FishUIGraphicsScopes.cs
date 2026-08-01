using System;
using System.Numerics;

namespace FishUI
{
    /// <summary>Restores the previous graphics clip when disposed.</summary>
    public readonly struct FishUIScissorScope : IDisposable
    {
        private readonly IFishUIGfx _graphics;

        internal FishUIScissorScope(IFishUIGfx graphics, Vector2 position, Vector2 size)
        {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            graphics.PushScissor(position, size);
        }

        public void Dispose() => _graphics?.PopScissor();
    }

    public static class FishUIGraphicsScopes
    {
        public static FishUIScissorScope PushScissorScope(this IFishUIGfx graphics, Vector2 position, Vector2 size) =>
            new FishUIScissorScope(graphics, position, size);
    }
}
