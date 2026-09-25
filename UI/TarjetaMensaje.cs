using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Panel con el "mensaje del dia" de la pantalla de inicio:
    ///  - Fondo oscuro con borde magenta y la etiqueta MENSAJE DEL DÍA.
    ///  - El mensaje se parte en lineas para caber en el ancho del panel.
    ///  - Aparece con un fundido y despues el borde pulsa suavemente.
    /// </summary>
    public class TarjetaMensaje
    {
        private const float DuracionFundido = 0.6f;
        private const string Etiqueta = "MENSAJE DEL DÍA";

        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;
        private string _mensaje = "";
        private float _tiempo;   // segundos desde que se mostro el mensaje

        public TarjetaMensaje(DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            _cuadros = cuadros;
            _texto = texto;
        }

        /// <summary>Cambia el mensaje y reinicia la animacion de entrada.</summary>
        public void Mostrar(string mensaje)
        {
            _mensaje = mensaje;
            _tiempo = 0f;
        }

        public void Actualizar(float dt)
        {
            _tiempo += dt;
        }

        /// <summary>Dibuja el panel en el rectangulo indicado (pixeles).</summary>
        public void Renderizar(float x, float y, float ancho, float alto)
        {
            float alpha = MathF.Min(1f, _tiempo / DuracionFundido);
            float pulso = 0.75f + 0.25f * MathF.Sin(_tiempo * 3f);

            _cuadros.DibujarRectangulo(ConAlpha(TemaArcade.PanelOscuro, alpha), x, y, x + ancho, y + alto);
            float grosorBorde = MathF.Max(1f, alto * 0.02f);
            _cuadros.DibujarBordeRectangulo(ConAlpha(TemaArcade.Magenta, alpha * pulso),
                x, y, x + ancho, y + alto, grosorBorde);

            // --- Etiqueta ----------------------------------------------------
            float cx = x + ancho * 0.5f;
            float etqAlto = alto * 0.14f;
            float etqAncho = _texto.MedirTexto(Etiqueta, etqAlto);
            _texto.DibujarTexto(Etiqueta, cx - etqAncho * 0.5f, y + alto * 0.10f, etqAlto,
                ConAlpha(TemaArcade.Amarillo, alpha));

            // --- Mensaje en varias lineas, centradas --------------------------
            float lineaAlto = alto * 0.16f;
            float interlineado = lineaAlto * 1.3f;
            List<string> lineas = PartirEnLineas(_mensaje, ancho * 0.9f, lineaAlto);

            float zonaY = y + alto * 0.32f;
            float zonaAlto = alto * 0.60f;
            float bloqueAlto = lineas.Count * interlineado - (interlineado - lineaAlto);
            float lineaY = zonaY + (zonaAlto - bloqueAlto) * 0.5f;

            foreach (string linea in lineas)
            {
                float lineaAncho = _texto.MedirTexto(linea, lineaAlto);
                _texto.DibujarTexto(linea, cx - lineaAncho * 0.5f, lineaY, lineaAlto,
                    ConAlpha(TemaArcade.Blanco, alpha));
                lineaY += interlineado;
            }
        }

        /// <summary>Acomoda las palabras en lineas que no pasen de anchoMaximo.</summary>
        private List<string> PartirEnLineas(string texto, float anchoMaximo, float altoTexto)
        {
            List<string> lineas = new();
            string actual = "";

            foreach (string palabra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string prueba = actual.Length == 0 ? palabra : actual + " " + palabra;
                if (actual.Length > 0 && _texto.MedirTexto(prueba, altoTexto) > anchoMaximo)
                {
                    lineas.Add(actual);
                    actual = palabra;
                }
                else
                {
                    actual = prueba;
                }
            }

            if (actual.Length > 0)
                lineas.Add(actual);
            return lineas;
        }

        private static Vector4 ConAlpha(Vector4 color, float factor) => new(color.Xyz, color.W * factor);
    }
}
