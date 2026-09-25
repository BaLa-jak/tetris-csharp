using OpenTK.Mathematics;
using Tetris2D.Graficos;
using Tetris2D.Logica;

namespace Tetris2D.UI
{
    /// <summary>
    /// Dibuja el tablero del Tetris: fondo, cuadricula, bloques fijos, la
    /// pieza fantasma (donde caeria) y la pieza que esta cayendo.
    /// Solo lee los datos que recibe; no puede modificar la partida.
    /// </summary>
    public class VistaTablero
    {
        private readonly DibujadorCuadros _cuadros;
        private readonly DibujadorBloques _bloques;

        public VistaTablero(DibujadorCuadros cuadros, DibujadorBloques bloques)
        {
            _cuadros = cuadros;
            _bloques = bloques;
        }

        /// <summary>
        /// Dibuja el tablero con su esquina superior izquierda en (x, y) y
        /// casillas de lado "tam". Si "actual" es null no se dibuja la pieza
        /// en juego (por ejemplo, al terminar la partida).
        /// </summary>
        public void Renderizar(ITableroLectura tablero, Pieza? actual, Pieza? fantasma, float x, float y, float tam)
        {
            float x1 = x + tablero.Columnas * tam;
            float y1 = y + tablero.Filas * tam;

            _cuadros.DibujarRectangulo(TemaArcade.FondoTablero, x, y, x1, y1);

            float grosorLinea = MathF.Max(1f, tam * 0.04f);
            for (int col = 1; col < tablero.Columnas; col++)
                _cuadros.DibujarRectangulo(TemaArcade.Cuadricula, x + col * tam, y, x + col * tam + grosorLinea, y1);
            for (int fila = 1; fila < tablero.Filas; fila++)
                _cuadros.DibujarRectangulo(TemaArcade.Cuadricula, x, y + fila * tam, x1, y + fila * tam + grosorLinea);

            // --- Bloques ya fijos ------------------------------------------
            for (int fila = 0; fila < tablero.Filas; fila++)
            {
                for (int col = 0; col < tablero.Columnas; col++)
                {
                    if (tablero.Celda(col, fila) is TipoPieza tipo)
                        _bloques.DibujarBloque(TemaArcade.ColorPieza(tipo), x + col * tam, y + fila * tam, tam);
                }
            }

            // --- Fantasma y pieza en juego ---------------------------------
            if (fantasma != null)
            {
                Vector4 color = TemaArcade.ColorPieza(fantasma.Tipo);
                foreach (Vector2i c in fantasma.Celdas())
                    _bloques.DibujarBloqueFantasma(color, x + c.X * tam, y + c.Y * tam, tam);
            }

            if (actual != null)
            {
                Vector4 color = TemaArcade.ColorPieza(actual.Tipo);
                foreach (Vector2i c in actual.Celdas())
                    _bloques.DibujarBloque(color, x + c.X * tam, y + c.Y * tam, tam);
            }

            _cuadros.DibujarBordeRectangulo(TemaArcade.Cian, x, y, x1, y1, MathF.Max(1f, tam * 0.1f));
        }
    }
}
