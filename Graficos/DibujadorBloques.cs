using OpenTK.Mathematics;
using Tetris2D.Logica;

namespace Tetris2D.Graficos
{
    /// <summary>
    /// Dibuja los bloques del Tetris con estilo arcade usando DibujadorCuadros:
    /// cada bloque es un cuadro de color con un brillo arriba/izquierda y una
    /// sombra abajo/derecha, lo que le da un ligero relieve.
    /// </summary>
    public class DibujadorBloques
    {
        private readonly DibujadorCuadros _cuadros;

        public DibujadorBloques(DibujadorCuadros cuadros)
        {
            _cuadros = cuadros;
        }

        /// <summary>Bloque solido de lado "tam" con esquina superior izquierda en (x, y).</summary>
        public void DibujarBloque(Vector4 color, float x, float y, float tam)
        {
            float separacion = MathF.Max(1f, tam * 0.04f);   // hueco entre bloques vecinos
            float relieve = MathF.Max(1f, tam * 0.14f);      // grosor del brillo y la sombra

            float x0 = x + separacion, y0 = y + separacion;
            float x1 = x + tam - separacion, y1 = y + tam - separacion;

            _cuadros.DibujarRectangulo(Aclarar(color, 0.45f), x0, y0, x1, y1);                  // brillo
            _cuadros.DibujarRectangulo(Oscurecer(color, 0.45f), x0 + relieve, y0 + relieve, x1, y1); // sombra
            _cuadros.DibujarRectangulo(color, x0 + relieve, y0 + relieve, x1 - relieve, y1 - relieve); // cara
        }

        /// <summary>
        /// Bloque "fantasma": solo un contorno translucido que marca donde
        /// caeria la pieza.
        /// </summary>
        public void DibujarBloqueFantasma(Vector4 color, float x, float y, float tam)
        {
            float separacion = MathF.Max(1f, tam * 0.04f);
            float x0 = x + separacion, y0 = y + separacion;
            float x1 = x + tam - separacion, y1 = y + tam - separacion;

            _cuadros.DibujarRectangulo(new Vector4(color.Xyz, 0.10f), x0, y0, x1, y1);
            _cuadros.DibujarBordeRectangulo(new Vector4(color.Xyz, 0.55f), x0, y0, x1, y1, MathF.Max(1f, tam * 0.08f));
        }

        /// <summary>
        /// Dibuja una pieza completa (por ejemplo, la vista previa de la
        /// siguiente). (x, y) es la esquina superior izquierda de su caja.
        /// </summary>
        public void DibujarPieza(TipoPieza tipo, int rotacion, Vector4 color, float x, float y, float tam)
        {
            foreach (Vector2i c in FormasPiezas.Celdas(tipo, rotacion))
                DibujarBloque(color, x + c.X * tam, y + c.Y * tam, tam);
        }

        private static Vector4 Aclarar(Vector4 color, float cantidad)
        {
            Vector3 rgb = Vector3.Lerp(color.Xyz, Vector3.One, cantidad);
            return new Vector4(rgb, color.W);
        }

        private static Vector4 Oscurecer(Vector4 color, float cantidad)
        {
            return new Vector4(color.Xyz * (1f - cantidad), color.W);
        }
    }
}
