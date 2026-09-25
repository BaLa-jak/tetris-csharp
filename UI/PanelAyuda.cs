using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Panel lateral con un resumen de los controles, visible durante la
    /// partida. Muestra la tecla y debajo su accion.
    /// </summary>
    public class PanelAyuda
    {
        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;

        public PanelAyuda(DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            _cuadros = cuadros;
            _texto = texto;
        }

        /// <summary>Dibuja el panel en el rectangulo indicado (pixeles).</summary>
        public void Renderizar(IReadOnlyList<DescripcionControl> controles, float x, float y, float ancho, float alto)
        {
            _cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, x, y, x + ancho, y + alto);
            _cuadros.DibujarBordeRectangulo(TemaArcade.Magenta, x, y, x + ancho, y + alto, MathF.Max(1f, ancho * 0.012f));

            float margen = ancho * 0.08f;
            float tituloAlto = alto * 0.045f;
            _texto.DibujarTexto("CONTROLES", x + margen, y + alto * 0.04f, tituloAlto, TemaArcade.Magenta);

            float teclaAlto = alto * 0.032f;
            float accionAlto = alto * 0.027f;
            float cursorY = y + alto * 0.14f;
            foreach (DescripcionControl control in controles)
            {
                _texto.DibujarTexto(control.Tecla, x + margen, cursorY, teclaAlto, TemaArcade.Amarillo);
                _texto.DibujarTexto(control.Accion, x + margen, cursorY + teclaAlto * 1.2f, accionAlto, TemaArcade.TextoSuave);
                cursorY += alto * 0.118f;
            }
        }
    }
}
