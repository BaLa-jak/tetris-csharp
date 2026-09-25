using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Panel centrado que oscurece la pantalla y muestra un titulo con
    /// algunas lineas de texto. Se usa para la pausa y el fin de la partida.
    /// </summary>
    public class PanelMensaje
    {
        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;

        public PanelMensaje(DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            _cuadros = cuadros;
            _texto = texto;
        }

        public void Renderizar(float ancho, float alto, string titulo, Vector4 colorTitulo, params string[] lineas)
        {
            _cuadros.DibujarRectangulo(new Vector4(0f, 0f, 0f, 0.6f), 0, 0, ancho, alto);

            float cx = ancho * 0.5f;
            float panelAncho = ancho * 0.62f;
            float panelAlto = alto * (0.20f + lineas.Length * 0.06f);
            float panelX = cx - panelAncho * 0.5f;
            float panelY = (alto - panelAlto) * 0.5f;

            _cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, panelX, panelY, panelX + panelAncho, panelY + panelAlto);
            _cuadros.DibujarBordeRectangulo(colorTitulo, panelX, panelY, panelX + panelAncho, panelY + panelAlto, MathF.Max(1f, alto * 0.006f));

            float tituloAlto = alto * 0.07f;
            float tituloAncho = _texto.MedirTexto(titulo, tituloAlto);
            float cursorY = panelY + alto * 0.05f;
            _texto.DibujarTexto(titulo, cx - tituloAncho * 0.5f, cursorY, tituloAlto, colorTitulo);

            float lineaAlto = alto * 0.03f;
            cursorY += tituloAlto + alto * 0.03f;
            foreach (string linea in lineas)
            {
                float lineaAncho = _texto.MedirTexto(linea, lineaAlto);
                _texto.DibujarTexto(linea, cx - lineaAncho * 0.5f, cursorY, lineaAlto, TemaArcade.TextoSuave);
                cursorY += alto * 0.06f;
            }
        }
    }
}
