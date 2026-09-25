using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
    /// <summary>
    /// Clase base de todas las pantallas del juego. TetrisGame reparte los
    /// eventos del sistema (mouse, teclado y texto) hacia la pantalla activa.
    ///
    /// Las pantallas no se cambian a si mismas: piden el cambio con los
    /// eventos JugarSolicitado y MenuSolicitado, y TetrisGame lo realiza.
    /// </summary>
    public abstract class Pantalla
    {
        // Herramientas de dibujo compartidas por todas las pantallas.
        protected readonly GestorShader Shaders;
        protected readonly DibujadorCuadros Cuadros;
        protected readonly RenderizadorTexto Texto;

        /// <summary>
        /// Se dispara cuando la pantalla pide abrir el juego con el nombre del
        /// jugador (la bienvenida al presionar INICIAR).
        /// </summary>
        public event Action<string>? JugarSolicitado;

        /// <summary>
        /// Se dispara cuando la pantalla pide volver al menu de bienvenida
        /// (el juego al terminar la partida).
        /// </summary>
        public event Action? MenuSolicitado;

        protected Pantalla(GestorShader shaders, DibujadorCuadros cuadros, RenderizadorTexto texto)
        {
            Shaders = shaders;
            Cuadros = cuadros;
            Texto = texto;
        }

        /// <summary>Carga de recursos, se llama una sola vez al crear la pantalla.</summary>
        public virtual void Cargar() { }

        /// <summary>Logica por frame: dt en segundos, raton en pixeles.</summary>
        public virtual void Actualizar(float dt, Vector2 raton) { }

        /// <summary>Dibuja la pantalla; ancho/alto son pixeles del framebuffer.</summary>
        public abstract void Renderizar(float ancho, float alto);

        /// <summary>Click del mouse en la posicion indicada (pixeles).</summary>
        public virtual void AlClick(Vector2 posicion, MouseButton boton) { }

        /// <summary>Tecla presionada (ej. Enter para iniciar, Backspace para borrar).</summary>
        public virtual void AlTecla(Keys tecla) { }

        /// <summary>Caracter de texto escrito (evento OnTextInput).</summary>
        public virtual void AlTexto(string caracter) { }

        /// <summary>Avisa a la ventana que esta pantalla quiere abrir el juego.</summary>
        protected void SolicitarJuego(string nombreJugador)
        {
            JugarSolicitado?.Invoke(nombreJugador);
        }

        /// <summary>Avisa a la ventana que esta pantalla quiere volver al menu.</summary>
        protected void SolicitarMenu()
        {
            MenuSolicitado?.Invoke();
        }
    }
}