namespace Tetris2D.Logica
{
    /// <summary>
    /// Datos de una jugada que borro filas, para que la interfaz pueda
    /// anunciarla (por ejemplo "COMBO x3 +900").
    /// </summary>
    /// <param name="Filas">Filas borradas de un solo golpe (1 a 4).</param>
    /// <param name="Combo">Piezas seguidas que han borrado filas (1 = sin combo).</param>
    /// <param name="Multiplicador">Multiplicador de puntos aplicado por el combo.</param>
    /// <param name="Puntos">Puntos ganados en la jugada, ya multiplicados.</param>
    public readonly record struct EventoPuntuacion(int Filas, int Combo, int Multiplicador, int Puntos);
}
