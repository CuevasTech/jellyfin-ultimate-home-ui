namespace Jellyfin.Plugin.UltimateHomeUI.Models;

/// <summary>
/// Tipo de tarjeta visual para las secciones del carrusel.
/// </summary>
public enum CardViewType
{
    /// <summary>Tarjeta cuadrada (1:1).</summary>
    Square,

    /// <summary>Tarjeta miniatura/thumb (16:9 pequeña).</summary>
    Thumb,

    /// <summary>Tarjeta poster/portrait (2:3).</summary>
    Portrait,

    /// <summary>Tarjeta panorámica/backdrop (16:9 grande).</summary>
    Landscape,
}
