using OpenTK.Graphics.OpenGL4;
using SkiaSharp;

namespace Tetris2D.UI
{
    /// <summary>
    /// Informacion de un caracter dentro del atlas de texto:
    /// las coordenadas UV (donde vive la letra en la textura) y su ancho de
    /// avance (cuanto se recorre el cursor al escribirla).
    /// </summary>
    public sealed class Glifo
    {
        public float U0, V0, U1, V1;   // region de la letra en la textura (0..1)
        public float AnchoCelda;       // ancho de la celda en pixeles del atlas
        public float Ancho;            // ancho de avance (espaciado proporcional)
    }

    /// <summary>
    /// Genera en tiempo de ejecucion la "textura de letras" del juego.
    ///
    /// Pasos:
    ///  1. Con SkiaSharp dibujamos cada caracter de la fuente sobre un bitmap.
    ///  2. Ese bitmap se sube como textura de OpenGL (BGRA).
    ///  3. Guardamos por letra sus coordenadas UV y su ancho para que
    ///     RenderizadorTexto la dibuje como un cuadro texturizado.
    ///
    /// Asi cualquier texto (titulos, nombres, numeros, ñ o acentos) se ve bien
    /// sin instalar tipos de letra externos.
    /// </summary>
    public class GeneradorFuenteAtlas : IDisposable
    {
        // Conjunto de caracteres soportados por el atlas (incluye espanol).
        public const string Caracteres =
            "ABCDEFGHIJKLMNÑOPQRSTUVWXYZ" +
            "abcdefghijklmnñopqrstuvwxyz" +
            "0123456789" +
            "ÁÉÍÓÚÜáéíóúü" +
            " .,:;!?¿¡-_'\"()[]{}<>/\\%&@#$*+=|~^`";

        public int Textura { get; private set; }
        public Dictionary<char, Glifo> Glifos { get; } = new();
        public float Altura { get; private set; }

        /// <summary>Nombres de fuente probados en orden hasta encontrar uno instalado.</summary>
        private static readonly string[] FuentesTentativas =
        {
            "Arial Black", "Arial", "Helvetica", "Segoe UI", "DejaVu Sans", "Liberation Sans"
        };

        public GeneradorFuenteAtlas(string nombreFuentePreferida, float tamanoPixeles, bool negrita)
        {
            string[] candidatos = new[] { nombreFuentePreferida }.Concat(FuentesTentativas).ToArray();

            SKFontStyle estilo = negrita ? SKFontStyle.Bold : SKFontStyle.Normal;
            using SKTypeface familia = BuscarFamilia(candidatos, estilo);
            using SKFont fuente = new(familia, tamanoPixeles)
            {
                Edging = SKFontEdging.Antialias,
                Subpixel = true
            };

            // --- Medimos las letras para deducir el tamano de la textura ---
            var anchos = new Dictionary<char, float>();
            float anchoMaximo = 0f;

            foreach (char c in Caracteres)
            {
                float ancho = MathF.Ceiling(fuente.MeasureText(c.ToString()));
                anchos[c] = ancho;
                anchoMaximo = MathF.Max(anchoMaximo, ancho);
            }

            // Cada letra vive en una celda del mismo tamano (celda uniforme), con
            // 1 pixel de margen a cada lado para que no se mezclen letras vecinas.
            float anchoCelda = anchoMaximo + 2f;
            SKFontMetrics metricas = fuente.Metrics;
            float ascenso = -metricas.Ascent;   // Ascent es negativo en Skia
            Altura = MathF.Ceiling(ascenso + metricas.Descent) + 2f;

            int anchoAtlas = (int)MathF.Ceiling(anchoCelda * Caracteres.Length);
            int altoAtlas = (int)MathF.Ceiling(Altura);

            // --- Dibujamos cada letra en su celda (color blanco + alpha) ---
            // BGRA de 8 bits por canal: se sube directo a OpenGL sin conversiones.
            using SKBitmap bmp = new(new SKImageInfo(anchoAtlas, altoAtlas,
                SKColorType.Bgra8888, SKAlphaType.Premul));
            using SKCanvas g = new(bmp);
            g.Clear(SKColors.Transparent);   // el fondo queda transparente

            using SKPaint pincel = new() { Color = SKColors.White, IsAntialias = true };

            for (int i = 0; i < Caracteres.Length; i++)
            {
                char c = Caracteres[i];
                float x = 1f + i * anchoCelda;   // margen izquierdo de la celda
                // Skia dibuja desde la linea base: bajamos el ascenso + 1 px de margen.
                g.DrawText(c.ToString(), x, 1f + ascenso, SKTextAlign.Left, fuente, pincel);

                // UV de toda la celda + ancho de avance proporcional.
                Glifos[c] = new Glifo
                {
                    U0 = (i * anchoCelda) / anchoAtlas,
                    U1 = ((i + 1) * anchoCelda) / anchoAtlas,
                    V0 = 0f,
                    V1 = 1f,
                    AnchoCelda = anchoCelda,
                    Ancho = anchos[c] + 1f
                };
            }
            g.Flush();

            // --- Subimos el bitmap como textura de OpenGL (BGRA) ---
            Textura = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Textura);

            // Las filas del bitmap pueden traer relleno: le indicamos a GL su largo real.
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, bmp.RowBytes / bmp.BytesPerPixel);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
                anchoAtlas, altoAtlas, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bmp.GetPixels());
            GL.PixelStore(PixelStoreParameter.UnpackRowLength, 0);

            // Filtro lineal = letras suaves al escalarlas; repeticion desactivada.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);
        }

        /// <summary>
        /// Devuelve la primera familia tipografica instalada de la lista.
        /// Si ninguna existe, cae en la fuente por defecto del sistema.
        /// </summary>
        private static SKTypeface BuscarFamilia(IEnumerable<string> nombres, SKFontStyle estilo)
        {
            foreach (string nombre in nombres)
            {
                // Skia no lanza error si la fuente no existe: devuelve otra.
                // Por eso comparamos el nombre de la familia obtenida.
                SKTypeface? tipo = SKTypeface.FromFamilyName(nombre, estilo);
                if (tipo != null && string.Equals(tipo.FamilyName, nombre, StringComparison.OrdinalIgnoreCase))
                    return tipo;
                tipo?.Dispose();
            }
            return SKTypeface.FromFamilyName(null, estilo) ?? SKTypeface.Default;
        }

        public void Dispose()
        {
            GL.DeleteTexture(Textura);
        }
    }
}
