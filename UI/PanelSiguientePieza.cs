using OpenTK.Mathematics;
using Tetris2D.Graficos;
using Tetris2D.Logica;

namespace Tetris2D.UI
{
    /// <summary>
    /// Recuadro "SIGUIENTE" que se muestra arriba del tablero con la pieza
    /// que aparecera despues de la actual, centrada dentro del panel.
    /// </summary>
    public class PanelSiguientePieza
    {
        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;
        private readonly DibujadorBloques _bloques;

        public PanelSiguientePieza(DibujadorCuadros cuadros, RenderizadorTexto texto, DibujadorBloques bloques)
        {
            _cuadros = cuadros;
            _texto = texto;
            _bloques = bloques;
        }

        /// <summary>
        /// Dibuja el panel en el rectangulo indicado (pixeles): la etiqueta
        /// arriba y la pieza "siguiente" centrada en el espacio restante.
        /// </summary>
        public void Renderizar(TipoPieza siguiente, float x, float y, float ancho, float alto)
        {
            _cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, x, y, x + ancho, y + alto);
            _cuadros.DibujarBordeRectangulo(TemaArcade.Magenta, x, y, x + ancho, y + alto, MathF.Max(1f, alto * 0.025f));

            // --- Etiqueta centrada arriba ----------------------------------
            string etiqueta = "SIGUIENTE";
            float etqAlto = alto * 0.20f;
            float etqAncho = _texto.MedirTexto(etiqueta, etqAlto);
            _texto.DibujarTexto(etiqueta, x + (ancho - etqAncho) * 0.5f, y + alto * 0.06f, etqAlto, TemaArcade.Magenta);

            // --- Pieza centrada debajo de la etiqueta ----------------------
            // Se usan los limites reales de la pieza (no su caja) para que
            // todas queden bien centradas, incluso la I y la O.
            float areaY = y + alto * 0.30f;
            float areaAlto = alto * 0.64f;
            float tamBloque = MathF.Min(areaAlto / 2.4f, ancho * 0.8f / 4f);

            (Vector2i minimo, Vector2i maximo) = FormasPiezas.Limites(siguiente, 0);
            float anchoPieza = (maximo.X - minimo.X + 1) * tamBloque;
            float altoPieza = (maximo.Y - minimo.Y + 1) * tamBloque;

            float piezaX = x + (ancho - anchoPieza) * 0.5f - minimo.X * tamBloque;
            float piezaY = areaY + (areaAlto - altoPieza) * 0.5f - minimo.Y * tamBloque;

            _bloques.DibujarPieza(siguiente, 0, TemaArcade.ColorPieza(siguiente), piezaX, piezaY, tamBloque);
        }
    }
}
