using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Panel que se muestra un momento antes de empezar la partida: lista de
    /// controles del juego y una cuenta regresiva hasta el inicio.
    /// </summary>
    public class PanelControles
    {
        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;

        public PanelControles(DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            _cuadros = cuadros;
            _texto = texto;
        }

        /// <summary>
        /// Dibuja el panel sobre toda la pantalla con la lista de controles;
        /// segundosRestantes es el tiempo que falta para que empiece la partida.
        /// </summary>
        public void Renderizar(IReadOnlyList<DescripcionControl> controles, float ancho, float alto, float segundosRestantes)
        {
            _cuadros.DibujarRectangulo(new Vector4(0f, 0f, 0f, 0.65f), 0, 0, ancho, alto);

            float cx = ancho * 0.5f;
            float panelAncho = ancho * 0.70f;
            float panelAlto = alto * 0.80f;
            float panelX = cx - panelAncho * 0.5f;
            float panelY = (alto - panelAlto) * 0.5f;

            _cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, panelX, panelY, panelX + panelAncho, panelY + panelAlto);
            _cuadros.DibujarBordeRectangulo(TemaArcade.Cian, panelX, panelY, panelX + panelAncho, panelY + panelAlto, MathF.Max(1f, alto * 0.006f));

            // --- Titulo ----------------------------------------------------
            string titulo = "CONTROLES";
            float tituloAlto = alto * 0.065f;
            float tituloAncho = _texto.MedirTexto(titulo, tituloAlto);
            _texto.DibujarTexto(titulo, cx - tituloAncho * 0.5f, panelY + alto * 0.04f, tituloAlto, TemaArcade.Cian);

            // --- Lista de controles en dos columnas ------------------------
            float filaAlto = alto * 0.032f;
            float paso = alto * 0.058f;
            float teclaX = panelX + panelAncho * 0.10f;
            float accionX = panelX + panelAncho * 0.42f;
            float cursorY = panelY + alto * 0.16f;

            foreach (DescripcionControl control in controles)
            {
                _texto.DibujarTexto(control.Tecla, teclaX, cursorY, filaAlto, TemaArcade.Amarillo);
                _texto.DibujarTexto(control.Accion, accionX, cursorY, filaAlto, TemaArcade.Blanco);
                cursorY += paso;
            }

            // --- Cuenta regresiva ------------------------------------------
            int segundos = (int)MathF.Ceiling(MathF.Max(0f, segundosRestantes));
            string cuenta = segundos > 0 ? $"EMPIEZA EN {segundos}" : "¡YA!";
            float cuentaAlto = alto * 0.05f;
            float cuentaAncho = _texto.MedirTexto(cuenta, cuentaAlto);
            _texto.DibujarTexto(cuenta, cx - cuentaAncho * 0.5f, panelY + panelAlto - alto * 0.15f, cuentaAlto, TemaArcade.Magenta);

            string ayuda = "Presiona ENTER para empezar ya";
            float ayudaAlto = alto * 0.022f;
            float ayudaAncho = _texto.MedirTexto(ayuda, ayudaAlto);
            _texto.DibujarTexto(ayuda, cx - ayudaAncho * 0.5f, panelY + panelAlto - alto * 0.07f, ayudaAlto, TemaArcade.TextoSuave);
        }
    }
}
