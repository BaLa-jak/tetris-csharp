namespace Tetris2D.Logica
{
    /// <summary>
    /// Vista de solo lectura del tablero. Las vistas la usan para dibujarlo
    /// sin poder modificarlo (no pueden fijar piezas ni borrar filas).
    /// </summary>
    public interface ITableroLectura
    {
        int Columnas { get; }
        int Filas { get; }

        /// <summary>Tipo del bloque en la casilla indicada (null = vacia).</summary>
        TipoPieza? Celda(int columna, int fila);
    }
}
