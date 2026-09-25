using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;
using Tetris2D.Logica;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
    /// <summary>
    /// Pantalla de inicio estilo arcade. Tiene dos pasos:
    ///  1. Portada: titulo neon, un mensaje distinto cada vez que se abre y
    ///     el boton INICIAR (o ENTER).
    ///  2. Nombre:  caja para escribir el nombre, JUGAR (se habilita al
    ///     escribirlo) y VOLVER (o ESC) para regresar a la portada.
    ///
    /// Al confirmar el nombre se dispara JugarSolicitado y TetrisGame cambia
    /// a la pantalla del juego.
    /// </summary>
    public class PantallaInicio : Pantalla
    {
        private enum EstadoPantalla
        {
            Portada,
            Nombre
        }

        private readonly FondoEstrellas _fondo;
        private readonly TituloNeon _titulo;
        private readonly TarjetaMensaje _tarjeta;
        private readonly CuadroTexto _cuadroNombre;
        private readonly Boton _botonIniciar;
        private readonly Boton _botonJugar;
        private readonly Boton _botonVolver;

        private EstadoPantalla _estado;
        private bool _iniciado;   // evita pedir el juego dos veces

        public PantallaInicio(GestorShader shaders, DibujadorCuadros cuadros, RenderizadorTexto texto)
            : base(shaders, cuadros, texto)
        {
            _fondo = new FondoEstrellas(cuadros);
            _titulo = new TituloNeon(texto);
            _tarjeta = new TarjetaMensaje(cuadros, texto);
            _cuadroNombre = new CuadroTexto(cuadros, texto);

            _botonIniciar = new Boton(cuadros, texto) { Texto = "INICIAR" };
            _botonIniciar.AlPresionar += () => CambiarEstado(EstadoPantalla.Nombre);

            _botonJugar = new Boton(cuadros, texto) { Texto = "JUGAR" };
            _botonJugar.AlPresionar += IniciarJuego;

            _botonVolver = new Boton(cuadros, texto) { Texto = "VOLVER" };
            _botonVolver.AlPresionar += () => CambiarEstado(EstadoPantalla.Portada);
        }

        public override void Cargar()
        {
            base.Cargar();
            _tarjeta.Mostrar(RegistroMensajes.SiguienteMensaje());
            CambiarEstado(EstadoPantalla.Portada);
        }

        private void CambiarEstado(EstadoPantalla estado)
        {
            _estado = estado;
            if (estado == EstadoPantalla.Nombre)
                _cuadroNombre.Limpiar();
        }

        public override void Actualizar(float dt, Vector2 raton)
        {
            base.Actualizar(dt, raton);
            _tarjeta.Actualizar(dt);
            _cuadroNombre.Actualizar(dt);

            // Solo responden los botones del paso actual.
            bool enPortada = _estado == EstadoPantalla.Portada;
            _botonIniciar.Habilitado = enPortada;
            _botonVolver.Habilitado = !enPortada && !_iniciado;
            _botonJugar.Habilitado = !enPortada && !_iniciado && _cuadroNombre.Nombre.Length > 0;

            _botonIniciar.Actualizar(raton);
            _botonJugar.Actualizar(raton);
            _botonVolver.Actualizar(raton);
        }

        public override void AlClick(Vector2 posicion, MouseButton boton)
        {
            base.AlClick(posicion, boton);
            if (boton != MouseButton.Left)
                return;

            if (_estado == EstadoPantalla.Portada)
            {
                _botonIniciar.AlClick(posicion);
            }
            else
            {
                _botonJugar.AlClick(posicion);
                _botonVolver.AlClick(posicion);
            }
        }

        public override void AlTecla(Keys tecla)
        {
            base.AlTecla(tecla);

            switch (_estado)
            {
                case EstadoPantalla.Portada:
                    if (EsEnter(tecla))
                        CambiarEstado(EstadoPantalla.Nombre);
                    break;

                case EstadoPantalla.Nombre:
                    if (_iniciado)
                        break;
                    if (tecla == Keys.Escape)
                        CambiarEstado(EstadoPantalla.Portada);
                    else if (EsEnter(tecla) && _cuadroNombre.Nombre.Length > 0)
                        IniciarJuego();
                    else
                        _cuadroNombre.AlTecla(tecla);
                    break;
            }
        }

        public override void AlTexto(string caracter)
        {
            base.AlTexto(caracter);
            if (_estado == EstadoPantalla.Nombre && !_iniciado)
                _cuadroNombre.AlTexto(caracter);
        }

        private static bool EsEnter(Keys tecla) => tecla == Keys.Enter || tecla == Keys.KeyPadEnter;

        /// <summary>El jugador confirmo su nombre: pedimos abrir el juego.</summary>
        private void IniciarJuego()
        {
            if (_iniciado || _cuadroNombre.Nombre.Length == 0)
                return;
            _iniciado = true;
            SolicitarJuego(_cuadroNombre.Nombre);
        }

        public override void Renderizar(float ancho, float alto)
        {
            _fondo.Renderizar(ancho, alto);

            if (_estado == EstadoPantalla.Portada)
                RenderizarPortada(ancho, alto);
            else
                RenderizarNombre(ancho, alto);

            // --- Pie de pantalla ---------------------------------------------
            string pie = _estado == EstadoPantalla.Portada
                ? "TETRIS2D v2.0 - ENTER para continuar"
                : "ENTER = jugar    ESC = volver";
            DibujarCentrado(pie, ancho * 0.5f, alto - alto * 0.018f * 2.2f, alto * 0.018f, TemaArcade.TextoSuave);
        }

        private void RenderizarPortada(float ancho, float alto)
        {
            float cx = ancho * 0.5f;

            _titulo.Renderizar("TETRIS2D", cx, alto * 0.12f, alto * 0.10f, TemaArcade.Cian);
            DibujarCentrado("GRAFICACIÓN POR COMPUTADORA", cx, alto * 0.26f, alto * 0.028f, TemaArcade.TextoSuave);

            float tarjetaAncho = ancho * 0.64f;
            float tarjetaAlto = alto * 0.26f;
            _tarjeta.Renderizar(cx - tarjetaAncho * 0.5f, alto * 0.35f, tarjetaAncho, tarjetaAlto);

            ColocarBoton(_botonIniciar, cx - ancho * 0.18f, alto * 0.69f, ancho * 0.36f, alto * 0.085f);
            _botonIniciar.Renderizar(alto * 0.042f);
        }

        private void RenderizarNombre(float ancho, float alto)
        {
            float cx = ancho * 0.5f;

            _titulo.Renderizar("TETRIS2D", cx, alto * 0.12f, alto * 0.07f, TemaArcade.Cian);
            DibujarCentrado("INGRESA TU NOMBRE:", cx, alto * 0.34f, alto * 0.03f, TemaArcade.Magenta);

            float cajaAncho = ancho * 0.44f;
            float cajaAlto = alto * 0.075f;
            float cajaY = alto * 0.41f;
            _cuadroNombre.Renderizar(cx - cajaAncho * 0.5f, cajaY, cajaAncho, cajaAlto);

            // JUGAR y VOLVER lado a lado debajo de la caja.
            float botonAncho = ancho * 0.21f;
            float botonAlto = alto * 0.085f;
            float separacion = ancho * 0.02f;
            float botonY = cajaY + cajaAlto + alto * 0.06f;
            ColocarBoton(_botonVolver, cx - separacion * 0.5f - botonAncho, botonY, botonAncho, botonAlto);
            ColocarBoton(_botonJugar, cx + separacion * 0.5f, botonY, botonAncho, botonAlto);
            _botonVolver.Renderizar(alto * 0.038f);
            _botonJugar.Renderizar(alto * 0.038f);
        }

        private static void ColocarBoton(Boton boton, float x, float y, float ancho, float alto)
        {
            boton.X0 = x;
            boton.Y0 = y;
            boton.X1 = x + ancho;
            boton.Y1 = y + alto;
        }

        private void DibujarCentrado(string texto, float centroX, float y, float altoTexto, Vector4 color)
        {
            float anchoTexto = Texto.MedirTexto(texto, altoTexto);
            Texto.DibujarTexto(texto, centroX - anchoTexto * 0.5f, y, altoTexto, color);
        }
    }
}
