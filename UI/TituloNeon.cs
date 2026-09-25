using OpenTK.Mathematics;

namespace Tetris2D.UI
{
    /// <summary>
    /// Titulo con resplandor neon. El "glow" se logra dibujando el mismo texto
    /// varias veces, muy translucido, desplazado unos pixeles en cada
    /// direccion; encima se dibuja el texto solido.
    /// </summary>
    public class TituloNeon
    {
        private readonly RenderizadorTexto _texto;

        public TituloNeon(RenderizadorTexto texto)
        {
            _texto = texto;
        }

        /// <summary>Dibuja el titulo centrado en centroX; "y" es la parte superior del texto.</summary>
        public void Renderizar(string titulo, float centroX, float y, float alto, Vector4 color)
        {
            float resplandor = alto * 0.08f;
            Vector4 colorSuave = new(color.Xyz, 0.16f);
            Vector2[] direcciones =
            {
                new(-resplandor, 0), new(resplandor, 0),
                new(0, -resplandor), new(0, resplandor),
                new(-resplandor, -resplandor), new(resplandor, resplandor)
            };

            float x = centroX - _texto.MedirTexto(titulo, alto) * 0.5f;
            foreach (Vector2 d in direcciones)
                _texto.DibujarTexto(titulo, x + d.X, y + d.Y, alto, colorSuave);

            _texto.DibujarTexto(titulo, x, y, alto, color);
        }
    }
}
