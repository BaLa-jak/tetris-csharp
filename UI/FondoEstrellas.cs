using OpenTK.Mathematics;
using Tetris2D.Graficos;

namespace Tetris2D.UI
{
    /// <summary>
    /// Fondo oscuro con estrellas decorativas ("cielo" de sala de maquinas)
    /// que comparten todas las pantallas del juego.
    /// </summary>
    public class FondoEstrellas
    {
        private readonly DibujadorCuadros _cuadros;

        // Estrellas decorativas: posicion en fracciones de pantalla (0..1),
        // tamano y brillo fijos para que no cambien de lugar entre frames.
        private readonly Estrella[] _estrellas;

        private readonly struct Estrella
        {
            public readonly float Nx, Ny, Tam, Brillo;
            public Estrella(float nx, float ny, float tam, float brillo)
            {
                Nx = nx; Ny = ny; Tam = tam; Brillo = brillo;
            }
        }

        public FondoEstrellas(DibujadorCuadros cuadros, int cantidad = 60)
        {
            _cuadros = cuadros;

            // Semilla fija: mismas estrellas en cada ejecucion.
            Random rnd = new(2026);
            _estrellas = new Estrella[cantidad];
            for (int i = 0; i < _estrellas.Length; i++)
            {
                _estrellas[i] = new Estrella(
                    (float)rnd.NextDouble(),
                    (float)rnd.NextDouble() * 0.85f,
                    0.5f + (float)rnd.NextDouble() * 2.0f,
                    0.12f + (float)rnd.NextDouble() * 0.6f);
            }
        }

        public void Renderizar(float ancho, float alto)
        {
            _cuadros.DibujarRectangulo(TemaArcade.Fondo, 0, 0, ancho, alto);
            foreach (Estrella e in _estrellas)
            {
                float tam = e.Tam * alto * 0.004f;
                Vector4 blanco = new(TemaArcade.Blanco.X, TemaArcade.Blanco.Y,
                    TemaArcade.Blanco.Z, e.Brillo);
                _cuadros.DibujarRectangulo(blanco, e.Nx * ancho, e.Ny * alto,
                    e.Nx * ancho + tam, e.Ny * alto + tam);
            }
        }
    }
}
