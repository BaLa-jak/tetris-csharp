using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>Datos que muestra el marcador (solo lectura).</summary>
    /// <param name="Jugador">Nombre del jugador.</param>
    /// <param name="Puntos">Puntos acumulados.</param>
    /// <param name="Nivel">Nivel actual.</param>
    /// <param name="Lineas">Filas borradas en la partida.</param>
    /// <param name="Multiplicador">Multiplicador del combo actual (1 = sin combo).</param>
    /// <param name="SkipsRestantes">Skips que todavia se pueden usar.</param>
    /// <param name="SkipsTotales">Skips disponibles al inicio de la partida.</param>
    public readonly record struct DatosMarcador(
        string Jugador, int Puntos, int Nivel, int Lineas,
        int Multiplicador, int SkipsRestantes, int SkipsTotales);

    /// <summary>
    /// Panel lateral con los datos de la partida: jugador, puntos, nivel,
    /// lineas, combo actual y los skips que quedan (un indicador por skip).
    /// </summary>
    public class MarcadorPuntos
    {
        private readonly DibujadorCuadros _cuadros;
        private readonly RenderizadorTexto _texto;

        public MarcadorPuntos(DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            _cuadros = cuadros;
            _texto = texto;
        }

        /// <summary>Dibuja el marcador en el rectangulo indicado (pixeles).</summary>
        public void Renderizar(DatosMarcador datos, float x, float y, float ancho, float alto)
        {
            _cuadros.DibujarRectangulo(TemaArcade.PanelOscuro, x, y, x + ancho, y + alto);
            _cuadros.DibujarBordeRectangulo(TemaArcade.Cian, x, y, x + ancho, y + alto, MathF.Max(1f, ancho * 0.012f));

            float margen = ancho * 0.08f;
            float etqAlto = alto * 0.035f;
            float valorAlto = alto * 0.055f;
            float paso = alto * 0.135f;
            float cursorY = y + alto * 0.04f;

            // --- Datos de la partida ---------------------------------------
            (string Etiqueta, string Valor, Vector4 Color)[] filas =
            {
                ("JUGADOR", datos.Jugador, TemaArcade.Blanco),
                ("PUNTOS", datos.Puntos.ToString(), TemaArcade.Amarillo),
                ("NIVEL", datos.Nivel.ToString(), TemaArcade.Verde),
                ("LÍNEAS", datos.Lineas.ToString(), TemaArcade.Cian),
                ("COMBO", datos.Multiplicador > 1 ? $"x{datos.Multiplicador}" : "-", TemaArcade.Magenta)
            };

            foreach ((string etiqueta, string valor, Vector4 color) in filas)
            {
                _texto.DibujarTexto(etiqueta, x + margen, cursorY, etqAlto, TemaArcade.TextoSuave);
                _texto.DibujarTexto(valor, x + margen, cursorY + etqAlto * 1.3f, valorAlto, color);
                cursorY += paso;
            }

            // --- Skips: un cuadro encendido por cada skip disponible -------
            _texto.DibujarTexto("SKIPS", x + margen, cursorY, etqAlto, TemaArcade.TextoSuave);
            float lado = valorAlto * 0.8f;
            float indicadorY = cursorY + etqAlto * 1.5f;
            for (int i = 0; i < datos.SkipsTotales; i++)
            {
                float ix = x + margen + i * lado * 1.5f;
                bool disponible = i < datos.SkipsRestantes;
                Vector4 color = disponible ? TemaArcade.Amarillo : TemaArcade.Gris;

                if (disponible)
                    _cuadros.DibujarRectangulo(color, ix, indicadorY, ix + lado, indicadorY + lado);
                _cuadros.DibujarBordeRectangulo(color, ix, indicadorY, ix + lado, indicadorY + lado, MathF.Max(1f, lado * 0.12f));
            }
        }
    }
}
