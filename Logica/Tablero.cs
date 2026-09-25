using OpenTK.Mathematics;

namespace Tetris2D.Logica
{
    /// <summary>
    /// Cuadricula del juego (10 columnas x 18 filas). Cada casilla guarda el
    /// tipo de pieza del bloque que la ocupa, o null si esta vacia.
    /// La fila 0 es la de arriba.
    /// </summary>
    public class Tablero : ITableroLectura
    {
        public const int Columnas = 10;
        public const int Filas = 18;

        private readonly TipoPieza?[,] _celdas = new TipoPieza?[Filas, Columnas];

        // La interfaz necesita propiedades; se exponen las mismas constantes.
        int ITableroLectura.Columnas => Columnas;
        int ITableroLectura.Filas => Filas;

        /// <summary>Tipo del bloque en la casilla indicada (null = vacia).</summary>
        public TipoPieza? Celda(int columna, int fila)
        {
            return _celdas[fila, columna];
        }

        /// <summary>
        /// True si todos los bloques de la pieza quedan dentro del tablero y
        /// sobre casillas vacias.
        /// </summary>
        public bool Cabe(Pieza pieza)
        {
            foreach (Vector2i c in pieza.Celdas())
            {
                bool dentro = c.X >= 0 && c.X < Columnas && c.Y >= 0 && c.Y < Filas;
                if (!dentro || _celdas[c.Y, c.X] != null)
                    return false;
            }
            return true;
        }

        /// <summary>Graba los bloques de la pieza en el tablero (la pieza deja de moverse).</summary>
        public void Fijar(Pieza pieza)
        {
            foreach (Vector2i c in pieza.Celdas())
                _celdas[c.Y, c.X] = pieza.Tipo;
        }

        /// <summary>
        /// Borra las filas completas y hace bajar las de arriba para ocupar
        /// su lugar. Devuelve cuantas filas se borraron.
        /// </summary>
        public int LimpiarFilasCompletas()
        {
            int borradas = 0;

            // Se recorre de abajo hacia arriba: cada fila que sobrevive se
            // copia "destino" filas mas abajo, donde destino avanza solo
            // cuando la fila no estaba completa.
            int destino = Filas - 1;
            for (int fila = Filas - 1; fila >= 0; fila--)
            {
                if (FilaCompleta(fila))
                {
                    borradas++;
                    continue;
                }

                if (destino != fila)
                    CopiarFila(fila, destino);
                destino--;
            }

            // Las filas que quedaron libres arriba se vacian.
            for (int fila = destino; fila >= 0; fila--)
                VaciarFila(fila);

            return borradas;
        }

        private bool FilaCompleta(int fila)
        {
            for (int col = 0; col < Columnas; col++)
            {
                if (_celdas[fila, col] == null)
                    return false;
            }
            return true;
        }

        private void CopiarFila(int origen, int destino)
        {
            for (int col = 0; col < Columnas; col++)
                _celdas[destino, col] = _celdas[origen, col];
        }

        private void VaciarFila(int fila)
        {
            for (int col = 0; col < Columnas; col++)
                _celdas[fila, col] = null;
        }
    }
}
