using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris2D.Graficos;
using Tetris2D.Logica;
using Tetris2D.Sonido;
using Tetris2D.UI;

namespace Tetris2D.Pantallas
{
    /// <summary>
    /// Pantalla principal del Tetris. Es el puente entre la logica
    /// (PartidaTetris) y los componentes visuales: lee el estado de la
    /// partida, se lo pasa a los componentes y traduce el teclado con
    /// MapaControles. No contiene reglas del juego ni codigo de dibujo.
    ///
    /// Estados de la pantalla:
    ///  1. Controles: muestra los controles unos segundos antes de empezar.
    ///  2. Jugando:   la partida avanza (puede estar en pausa).
    ///  3. Terminado: muestra los puntos finales (ENTER = otra vez, ESC = menu).
    ///
    /// Distribucion: tablero al centro, la siguiente pieza arriba, el marcador
    /// a la izquierda, la ayuda a la derecha y los combos debajo del tablero.
    ///
    /// Sonidos: la partida avisa con eventos y aqui se elige que tocar. Solo
    /// suena un efecto por jugada (el de mayor prioridad), asi al borrar una
    /// fila no suena tambien el de pieza colocada.
    /// </summary>
    public class PantallaJuego : Pantalla
    {
        /// <summary>Segundos que se muestran los controles antes de empezar.</summary>
        private const float DuracionControles = 5f;

        /// <summary>Tiempo minimo antes de poder saltar los controles con ENTER.</summary>
        private const float EsperaMinimaControles = 0.5f;

        private enum EstadoPantalla
        {
            Controles,
            Jugando,
            Terminado
        }

        private readonly string _jugador;
        private readonly FondoEstrellas _fondo;
        private readonly VistaTablero _vistaTablero;
        private readonly PanelSiguientePieza _panelSiguiente;
        private readonly MarcadorPuntos _marcador;
        private readonly PanelAyuda _panelAyuda;
        private readonly PanelControles _panelControles;
        private readonly PanelMensaje _panelMensaje;
        private readonly TextoAnimado _textoJugada;
        private readonly GestorAudio _audio;

        private PartidaTetris _partida = null!;
        private EstadoPantalla _estado;
        private float _tiempoEstado;   // segundos desde que se entro al estado actual

        // Efecto de la jugada en curso; se toca al terminar de procesar la tecla o el frame.
        private Efecto? _efectoPendiente;
        private int _multiplicadorPendiente;

        public PantallaJuego(GestorShader shaders, DibujadorCuadros cuadros, RenderizadorTexto texto, GestorAudio audio,
            string jugador)
            : base(shaders, cuadros, texto)
        {
            _jugador = jugador;
            _audio = audio;

            DibujadorBloques bloques = new(cuadros);
            _fondo = new FondoEstrellas(cuadros);
            _vistaTablero = new VistaTablero(cuadros, bloques);
            _panelSiguiente = new PanelSiguientePieza(cuadros, texto, bloques);
            _marcador = new MarcadorPuntos(cuadros, texto);
            _panelAyuda = new PanelAyuda(cuadros, texto);
            _panelControles = new PanelControles(cuadros, texto);
            _panelMensaje = new PanelMensaje(cuadros, texto);
            _textoJugada = new TextoAnimado(texto);
        }

        public override void Cargar()
        {
            base.Cargar();
            NuevaPartida();
        }

        /// <summary>Crea una partida desde cero y vuelve a mostrar los controles.</summary>
        private void NuevaPartida()
        {
            _partida = new PartidaTetris();
            _partida.PuntosObtenidos += AnunciarJugada;
            _partida.PiezaFijada += () => PedirEfecto(Efecto.PiezaColocada);
            _partida.PartidaTerminada += () => PedirEfecto(Efecto.FinJuego);
            CambiarEstado(EstadoPantalla.Controles);
        }

        private void CambiarEstado(EstadoPantalla estado)
        {
            _estado = estado;
            _tiempoEstado = 0f;

            if (estado == EstadoPantalla.Jugando)
                _audio.Reproducir(Efecto.InicioPartida);
        }

        public override void Actualizar(float dt, Vector2 raton)
        {
            base.Actualizar(dt, raton);
            _tiempoEstado += dt;

            switch (_estado)
            {
                case EstadoPantalla.Controles:
                    if (_tiempoEstado >= DuracionControles)
                        CambiarEstado(EstadoPantalla.Jugando);
                    break;

                case EstadoPantalla.Jugando:
                    _partida.Actualizar(dt);
                    if (!_partida.Pausada)
                        _textoJugada.Actualizar(dt);
                    if (_partida.Terminada)
                        CambiarEstado(EstadoPantalla.Terminado);
                    break;
            }

            EmitirEfectoPendiente();
        }

        public override void AlTecla(Keys tecla)
        {
            base.AlTecla(tecla);

            switch (_estado)
            {
                case EstadoPantalla.Controles:
                    bool puedeSaltar = _tiempoEstado >= EsperaMinimaControles;
                    if (puedeSaltar && (EsEnter(tecla) || tecla == Keys.Space))
                        CambiarEstado(EstadoPantalla.Jugando);
                    break;

                case EstadoPantalla.Jugando:
                    // Al mantener una tecla OpenTK repite el evento, asi que
                    // mantener IZQ/DER o ABAJO mueve la pieza de forma continua.
                    MapaControles.Ejecutar(tecla, _partida);
                    break;

                case EstadoPantalla.Terminado:
                    if (EsEnter(tecla))
                        NuevaPartida();
                    else if (tecla == Keys.Escape)
                        SolicitarMenu();
                    break;
            }

            EmitirEfectoPendiente();
        }

        private static bool EsEnter(Keys tecla) => tecla == Keys.Enter || tecla == Keys.KeyPadEnter;

        /// <summary>
        /// Anuncia con texto animado las filas borradas; si hay combo se
        /// destaca el multiplicador.
        /// </summary>
        private void AnunciarJugada(EventoPuntuacion jugada)
        {
            if (jugada.Multiplicador > 1)
                PedirEfecto(Efecto.Combo, jugada.Multiplicador);
            else
                PedirEfecto(jugada.Filas == 4 ? Efecto.Tetris : Efecto.FilaBorrada);

            if (jugada.Multiplicador > 1)
                _textoJugada.Mostrar($"COMBO x{jugada.Multiplicador}  +{jugada.Puntos}", TemaArcade.Magenta);
            else if (jugada.Filas == 4)
                _textoJugada.Mostrar($"¡TETRIS!  +{jugada.Puntos}", TemaArcade.Amarillo);
            else
                _textoJugada.Mostrar($"+{jugada.Puntos}", TemaArcade.Verde);
        }

        /// <summary>
        /// Guarda el efecto de la jugada si tiene mas prioridad que el que ya
        /// estaba pendiente (FinJuego > Combo > Tetris > FilaBorrada > PiezaColocada).
        /// </summary>
        private void PedirEfecto(Efecto efecto, int multiplicador = 1)
        {
            if (_efectoPendiente is Efecto actual && Prioridad(actual) >= Prioridad(efecto))
                return;
            _efectoPendiente = efecto;
            _multiplicadorPendiente = multiplicador;
        }

        private static int Prioridad(Efecto efecto) => efecto switch
        {
            Efecto.FinJuego => 4,
            Efecto.Combo => 3,
            Efecto.Tetris => 2,
            Efecto.FilaBorrada => 1,
            _ => 0
        };

        private void EmitirEfectoPendiente()
        {
            if (_efectoPendiente is Efecto efecto)
                _audio.Reproducir(efecto, _multiplicadorPendiente);
            _efectoPendiente = null;
        }

        public override void Renderizar(float ancho, float alto)
        {
            _fondo.Renderizar(ancho, alto);

            // --- Distribucion (todo proporcional al tamano de la ventana) --
            float tam = MathF.Min(alto * 0.66f / Tablero.Filas, ancho * 0.42f / Tablero.Columnas);
            float tableroAncho = Tablero.Columnas * tam;
            float tableroAlto = Tablero.Filas * tam;
            float tableroX = (ancho - tableroAncho) * 0.5f;
            float tableroY = alto * 0.18f;
            float margen = ancho * 0.03f;
            float lateralAncho = tableroX - margen * 2f;

            // --- Componentes: cada uno recibe solo los datos que muestra ---
            bool enJuego = !_partida.Terminada;
            _vistaTablero.Renderizar(_partida.Tablero,
                enJuego ? _partida.PiezaActual : null,
                enJuego ? _partida.PiezaFantasma() : null,
                tableroX, tableroY, tam);

            _panelSiguiente.Renderizar(_partida.Siguiente, tableroX, alto * 0.02f, tableroAncho, alto * 0.14f);
            _marcador.Renderizar(CrearDatosMarcador(), margen, tableroY, lateralAncho, tableroAlto);
            _panelAyuda.Renderizar(MapaControles.Descripciones, tableroX + tableroAncho + margen, tableroY, lateralAncho, tableroAlto);
            _textoJugada.Renderizar(ancho * 0.5f, tableroY + tableroAlto + alto * 0.04f, alto * 0.06f);

            // --- Paneles superpuestos segun el estado ----------------------
            if (_estado == EstadoPantalla.Controles)
            {
                _panelControles.Renderizar(MapaControles.Descripciones, ancho, alto, DuracionControles - _tiempoEstado);
            }
            else if (_estado == EstadoPantalla.Terminado)
            {
                _panelMensaje.Renderizar(ancho, alto, "FIN DEL JUEGO", TemaArcade.Rojo,
                    $"{_jugador}: {_partida.Puntos} PUNTOS",
                    "ENTER = jugar otra vez    ESC = menú");
            }
            else if (_partida.Pausada)
            {
                _panelMensaje.Renderizar(ancho, alto, "PAUSA", TemaArcade.Cian,
                    "Presiona P o ESC para continuar");
            }
        }

        /// <summary>Copia de solo lectura de los datos que muestra el marcador.</summary>
        private DatosMarcador CrearDatosMarcador()
        {
            return new DatosMarcador(_jugador, _partida.Puntos, _partida.Nivel, _partida.Lineas,
                _partida.Multiplicador, _partida.SkipsRestantes, PartidaTetris.MaxSkips);
        }
    }
}
