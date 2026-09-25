namespace Tetris2D.UI
{
    /// <summary>
    /// Texto de un control para mostrarlo en pantalla, por ejemplo
    /// ("ESPACIO", "Soltar pieza").
    /// </summary>
    /// <param name="Tecla">Nombre de la(s) tecla(s) tal como se muestra.</param>
    /// <param name="Accion">Lo que hace la tecla.</param>
    public readonly record struct DescripcionControl(string Tecla, string Accion);
}
