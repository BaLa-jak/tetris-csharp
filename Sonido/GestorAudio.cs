using System.Diagnostics;
using OpenTK.Audio.OpenAL;

namespace Tetris2D.Sonido
{
    /// <summary>
    /// Unico lugar que usa OpenAL. Al crearse abre el dispositivo de audio,
    /// genera todos los efectos con SintetizadorSonidos y los guarda en
    /// buffers; despues solo hay que pedir Reproducir(efecto).
    ///
    /// Si OpenAL no esta disponible (por ejemplo en Windows sin
    /// openal32.dll), Disponible queda en false y el juego sigue en silencio.
    ///
    /// Para que no se sature la mezcla:
    ///  - A lo mas suenan MaxSimultaneos efectos a la vez.
    ///  - El mismo efecto no se repite si acaba de sonar (EsperaRepeticion).
    ///  - El volumen general es moderado (VolumenGeneral).
    /// </summary>
    public class GestorAudio : IDisposable
    {
        private const int MaxSimultaneos = 4;
        private const float VolumenGeneral = 0.6f;
        private const double EsperaRepeticion = 0.05;   // segundos

        private readonly Dictionary<(Efecto, int), int> _buffers = new();
        private readonly Dictionary<(Efecto, int), double> _ultimaVez = new();
        private readonly int[] _sources = new int[MaxSimultaneos];
        private readonly double[] _inicioSource = new double[MaxSimultaneos];
        private readonly Stopwatch _reloj = Stopwatch.StartNew();

        private ALDevice _dispositivo;
        private ALContext _contexto;

        /// <summary>True si el audio se inicio bien y los efectos pueden sonar.</summary>
        public bool Disponible { get; private set; }

        /// <summary>Con true no suena nada (tecla M).</summary>
        public bool Silenciado { get; private set; }

        public GestorAudio()
        {
            try
            {
                _dispositivo = ALC.OpenDevice(null);
                if (_dispositivo == ALDevice.Null)
                    throw new InvalidOperationException("No se encontro un dispositivo de audio.");

                _contexto = ALC.CreateContext(_dispositivo, new int[] { 0 });
                ALC.MakeContextCurrent(_contexto);

                foreach (Efecto efecto in Enum.GetValues<Efecto>())
                {
                    if (efecto == Efecto.Combo)
                    {
                        for (int multiplicador = 2; multiplicador <= 5; multiplicador++)
                            CrearBuffer(efecto, multiplicador);
                    }
                    else
                    {
                        CrearBuffer(efecto, 1);
                    }
                }

                for (int i = 0; i < _sources.Length; i++)
                    _sources[i] = AL.GenSource();

                AL.Listener(ALListenerf.Gain, VolumenGeneral);

                ALError error = AL.GetError();
                if (error != ALError.NoError)
                    throw new InvalidOperationException($"Error de OpenAL: {error}");

                Disponible = true;
            }
            catch (Exception ex)
            {
                // El audio es opcional: sin OpenAL el juego corre en silencio.
                Console.Error.WriteLine($"Audio desactivado: {ex.Message}");
                Liberar();
            }
        }

        private void CrearBuffer(Efecto efecto, int variante)
        {
            int buffer = AL.GenBuffer();
            AL.BufferData(buffer, ALFormat.Mono16, SintetizadorSonidos.Generar(efecto, variante),
                SintetizadorSonidos.MuestrasPorSegundo);
            _buffers[(efecto, variante)] = buffer;
        }

        /// <summary>
        /// Toca un efecto. En el combo, multiplicador (2 a 5) elige la
        /// variante: mas agudo mientras mayor sea.
        /// </summary>
        public void Reproducir(Efecto efecto, int multiplicador = 1)
        {
            if (!Disponible || Silenciado)
                return;

            int variante = efecto == Efecto.Combo ? Math.Clamp(multiplicador, 2, 5) : 1;
            var clave = (efecto, variante);
            double ahora = _reloj.Elapsed.TotalSeconds;

            if (_ultimaVez.TryGetValue(clave, out double ultima) && ahora - ultima < EsperaRepeticion)
                return;
            _ultimaVez[clave] = ahora;

            int indice = ElegirSource();
            int source = _sources[indice];
            AL.SourceStop(source);
            AL.Source(source, ALSourcei.Buffer, _buffers[clave]);
            AL.SourcePlay(source);
            _inicioSource[indice] = ahora;
        }

        /// <summary>Una source libre; si todas suenan, la que empezo hace mas tiempo.</summary>
        private int ElegirSource()
        {
            int masVieja = 0;
            for (int i = 0; i < _sources.Length; i++)
            {
                AL.GetSource(_sources[i], ALGetSourcei.SourceState, out int estado);
                if ((ALSourceState)estado != ALSourceState.Playing)
                    return i;
                if (_inicioSource[i] < _inicioSource[masVieja])
                    masVieja = i;
            }
            return masVieja;
        }

        /// <summary>Activa o desactiva el sonido (tecla M).</summary>
        public void AlternarSilencio()
        {
            Silenciado = !Silenciado;
            if (Disponible)
                AL.Listener(ALListenerf.Gain, Silenciado ? 0f : VolumenGeneral);
        }

        private void Liberar()
        {
            Disponible = false;
            try
            {
                foreach (int source in _sources.Where(s => s != 0))
                {
                    AL.SourceStop(source);
                    AL.DeleteSource(source);
                }
                foreach (int buffer in _buffers.Values)
                    AL.DeleteBuffer(buffer);
                _buffers.Clear();

                if (_contexto != ALContext.Null)
                {
                    ALC.MakeContextCurrent(ALContext.Null);
                    ALC.DestroyContext(_contexto);
                    _contexto = ALContext.Null;
                }
                if (_dispositivo != ALDevice.Null)
                {
                    ALC.CloseDevice(_dispositivo);
                    _dispositivo = ALDevice.Null;
                }
            }
            catch (Exception)
            {
                // Si OpenAL nunca cargo no hay nada que liberar.
            }
        }

        public void Dispose()
        {
            Liberar();
        }
    }
}
