using OpenTK.Mathematics;

namespace Tetris2D.Logica
{
    /// <summary>
    /// Motor de una partida de Tetris: controla la pieza que cae, la
    /// siguiente pieza, la gravedad, el borrado de filas, los puntos, los
    /// combos y los skips. No dibuja nada; la pantalla solo lo consulta.
    ///
    /// Puntos por filas borradas de un golpe (x nivel x multiplicador):
    ///  1 = 100, 2 = 300, 3 = 500, 4 = 800.
    /// Combo: cada pieza seguida que borra filas sube el multiplicador
    /// (x1, x2, x3... hasta x5). Una pieza que no borra nada lo reinicia.
    /// </summary>
    public class PartidaTetris
    {
        /// <summary>Skips disponibles por partida.</summary>
        public const int MaxSkips = 3;

        /// <summary>Tope del multiplicador de combo.</summary>
        public const int MaxMultiplicador = 5;

        /// <summary>Filas que hay que borrar para subir de nivel.</summary>
        public const int LineasPorNivel = 10;

        private static readonly int[] PuntosPorFilas = { 0, 100, 300, 500, 800 };

        // Desplazamientos (columnas, filas) que se prueban al rotar junto a una
        // pared u otros bloques ("wall kick" simplificado). El ultimo baja la
        // pieza una fila: la I recien aparecida no cabe vertical en la fila 0.
        private static readonly Vector2i[] DesplazamientosRotacion =
        {
            new(0, 0), new(-1, 0), new(1, 0), new(-2, 0), new(2, 0), new(0, 1)
        };

        private readonly GeneradorPiezas _generador;
        private float _tiempoCaida;   // segundos acumulados desde el ultimo paso de gravedad

        /// <summary>Se dispara cada vez que una pieza borra filas.</summary>
        public event Action<EventoPuntuacion>? PuntosObtenidos;

        public Tablero Tablero { get; } = new();
        public Pieza PiezaActual { get; private set; }
        public TipoPieza Siguiente => _generador.Siguiente;

        public int Puntos { get; private set; }
        public int Lineas { get; private set; }
        public int Nivel => 1 + Lineas / LineasPorNivel;
        public int Combo { get; private set; }
        public int SkipsRestantes { get; private set; } = MaxSkips;
        public bool Terminada { get; private set; }

        /// <summary>Mientras esta en pausa, ni la gravedad ni los movimientos hacen nada.</summary>
        public bool Pausada { get; private set; }

        /// <summary>Multiplicador de puntos del combo actual (x1 hasta x5).</summary>
        public int Multiplicador => Math.Max(1, Math.Min(Combo, MaxMultiplicador));

        /// <summary>True si la partida acepta movimientos (ni terminada ni en pausa).</summary>
        private bool EnJuego => !Terminada && !Pausada;

        /// <summary>Segundos entre cada paso de gravedad; baja al subir de nivel.</summary>
        public float IntervaloCaida => MathF.Max(0.08f, 0.8f - (Nivel - 1) * 0.07f);

        /// <param name="azar">Generador aleatorio; se puede fijar la semilla para pruebas.</param>
        public PartidaTetris(Random? azar = null)
        {
            _generador = new GeneradorPiezas(azar);
            PiezaActual = Pieza.CrearInicial(_generador.Tomar(), Tablero.Columnas);
        }

        /// <summary>Pausa la partida o la reanuda. Una partida terminada no se puede pausar.</summary>
        public void AlternarPausa()
        {
            if (!Terminada)
                Pausada = !Pausada;
        }

        /// <summary>Aplica la gravedad: la pieza baja una fila cada IntervaloCaida segundos.</summary>
        public void Actualizar(float dt)
        {
            if (!EnJuego)
                return;

            _tiempoCaida += dt;
            while (_tiempoCaida >= IntervaloCaida && !Terminada)
            {
                _tiempoCaida -= IntervaloCaida;
                if (!IntentarMover(PiezaActual.Movida(0, 1)))
                    FijarPieza();
            }
        }

        public bool MoverIzquierda() => EnJuego && IntentarMover(PiezaActual.Movida(-1, 0));

        public bool MoverDerecha() => EnJuego && IntentarMover(PiezaActual.Movida(1, 0));

        /// <summary>
        /// Caida suave: baja una fila y suma 1 punto. Si ya no puede bajar,
        /// la pieza se fija en su lugar.
        /// </summary>
        public void BajarUnPaso()
        {
            if (!EnJuego)
                return;

            if (IntentarMover(PiezaActual.Movida(0, 1)))
            {
                Puntos += 1;
                _tiempoCaida = 0f;
            }
            else
            {
                FijarPieza();
            }
        }

        /// <summary>Caida rapida: la pieza baja hasta el fondo (2 puntos por fila) y se fija.</summary>
        public void CaidaRapida()
        {
            if (!EnJuego)
                return;

            int filasBajadas = 0;
            while (IntentarMover(PiezaActual.Movida(0, 1)))
                filasBajadas++;

            Puntos += filasBajadas * 2;
            FijarPieza();
        }

        /// <summary>
        /// Gira la pieza (+1 horario, -1 antihorario). Si al girar choca, se
        /// intenta recorrerla un poco a los lados antes de darse por vencido.
        /// </summary>
        public bool Rotar(int sentido)
        {
            if (!EnJuego)
                return false;

            Pieza rotada = PiezaActual.Rotada(sentido);
            foreach (Vector2i d in DesplazamientosRotacion)
            {
                if (IntentarMover(rotada.Movida(d.X, d.Y)))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Descarta la pieza actual y pasa a la siguiente. Solo se permite
        /// MaxSkips veces por partida.
        /// </summary>
        public bool Saltar()
        {
            if (!EnJuego || SkipsRestantes == 0)
                return false;

            SkipsRestantes--;
            AparecerSiguientePieza();
            return true;
        }

        /// <summary>
        /// Posicion donde caeria la pieza actual si se soltara ahora
        /// (se dibuja como "fantasma" para ayudar a apuntar).
        /// </summary>
        public Pieza PiezaFantasma()
        {
            Pieza fantasma = PiezaActual;
            while (Tablero.Cabe(fantasma.Movida(0, 1)))
                fantasma = fantasma.Movida(0, 1);
            return fantasma;
        }

        /// <summary>Reemplaza la pieza actual si la nueva posicion cabe en el tablero.</summary>
        private bool IntentarMover(Pieza nueva)
        {
            if (!Tablero.Cabe(nueva))
                return false;

            PiezaActual = nueva;
            return true;
        }

        /// <summary>
        /// La pieza toca fondo: se graba en el tablero, se borran las filas
        /// completas, se calculan puntos y combo, y aparece la siguiente pieza.
        /// </summary>
        private void FijarPieza()
        {
            Tablero.Fijar(PiezaActual);
            int filas = Tablero.LimpiarFilasCompletas();

            if (filas > 0)
            {
                int nivelJugada = Nivel;   // el nivel antes de sumar las lineas nuevas
                Combo++;
                int multiplicador = Multiplicador;
                int ganados = PuntosPorFilas[filas] * nivelJugada * multiplicador;

                Puntos += ganados;
                Lineas += filas;
                PuntosObtenidos?.Invoke(new EventoPuntuacion(filas, Combo, multiplicador, ganados));
            }
            else
            {
                Combo = 0;
            }

            AparecerSiguientePieza();
        }

        /// <summary>Saca la siguiente pieza; si ya no cabe arriba, la partida termina.</summary>
        private void AparecerSiguientePieza()
        {
            PiezaActual = Pieza.CrearInicial(_generador.Tomar(), Tablero.Columnas);
            _tiempoCaida = 0f;

            if (!Tablero.Cabe(PiezaActual))
                Terminada = true;
        }
    }
}
