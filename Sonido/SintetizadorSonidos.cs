namespace Tetris2D.Sonido
{
    /// <summary>
    /// Genera los efectos de sonido por codigo (sin archivos .wav) como
    /// muestras PCM mono de 16 bits. No usa OpenAL, solo calcula numeros.
    ///
    /// Para que los sonidos sean suaves y no saturen:
    ///  - Solo ondas seno y triangulo (la cuadrada suena aspera).
    ///  - Cada nota entra en unos milisegundos y se apaga hasta 0 al final,
    ///    asi no hay "clics".
    ///  - Un filtro pasa-bajos quita los agudos asperos.
    ///  - El pico de cada efecto se normaliza muy por debajo del maximo.
    /// </summary>
    public static class SintetizadorSonidos
    {
        public const int MuestrasPorSegundo = 44100;

        /// <summary>Pico normal de un efecto (fraccion de la escala completa).</summary>
        public const float PicoMaximo = 0.35f;

        private const float Ataque = 0.008f;               // segundos de entrada de cada nota
        private const float FrecuenciaCorteFiltro = 3000f; // Hz del pasa-bajos

        public enum FormaOnda
        {
            Seno,
            Triangulo
        }

        /// <summary>
        /// Una nota dentro de un efecto.
        /// FrecuenciaFinal distinta de 0 hace que la nota se deslice hacia ella.
        /// Caida es que tan rapido se apaga (mayor = mas corta y seca).
        /// </summary>
        private readonly record struct Nota(float Frecuencia, float Inicio, float Duracion, FormaOnda Onda,
            float Volumen = 1f, float Caida = 4f, float FrecuenciaFinal = 0f);

        /// <summary>
        /// Muestras de un efecto. En el combo, el multiplicador (2 a 5) sube
        /// el tono un semitono por nivel.
        /// </summary>
        public static short[] Generar(Efecto efecto, int multiplicador = 1)
        {
            return efecto switch
            {
                // "Toc" grave y corto: es el mas frecuente, asi que es el mas discreto.
                Efecto.PiezaColocada => Mezclar(0.18f,
                    new Nota(180f, 0f, 0.07f, FormaOnda.Seno, Caida: 30f, FrecuenciaFinal: 120f)),

                // Arpegio ascendente Do - Mi - Sol.
                Efecto.FilaBorrada => Mezclar(PicoMaximo,
                    new Nota(Midi(72), 0.00f, 0.10f, FormaOnda.Triangulo, Caida: 12f),
                    new Nota(Midi(76), 0.07f, 0.10f, FormaOnda.Triangulo, Caida: 12f),
                    new Nota(Midi(79), 0.14f, 0.12f, FormaOnda.Triangulo, Caida: 10f)),

                // Cuatro notas y un Do agudo final mas largo.
                Efecto.Tetris => Mezclar(PicoMaximo,
                    new Nota(Midi(72), 0.00f, 0.10f, FormaOnda.Triangulo, Caida: 12f),
                    new Nota(Midi(76), 0.07f, 0.10f, FormaOnda.Triangulo, Caida: 12f),
                    new Nota(Midi(79), 0.14f, 0.10f, FormaOnda.Triangulo, Caida: 12f),
                    new Nota(Midi(84), 0.21f, 0.22f, FormaOnda.Triangulo, Caida: 6f),
                    new Nota(Midi(72), 0.21f, 0.22f, FormaOnda.Seno, Volumen: 0.4f, Caida: 6f)),

                // Dos notas brillantes, mas agudas mientras mayor sea el combo.
                Efecto.Combo => GenerarCombo(multiplicador),

                // Fanfarria corta de inicio: Sol - Do - Mi - Sol.
                Efecto.InicioPartida => Mezclar(PicoMaximo,
                    new Nota(Midi(67), 0.00f, 0.10f, FormaOnda.Triangulo, Caida: 8f),
                    new Nota(Midi(72), 0.09f, 0.10f, FormaOnda.Triangulo, Caida: 8f),
                    new Nota(Midi(76), 0.18f, 0.10f, FormaOnda.Triangulo, Caida: 8f),
                    new Nota(Midi(79), 0.27f, 0.20f, FormaOnda.Triangulo, Caida: 5f),
                    new Nota(Midi(67), 0.27f, 0.20f, FormaOnda.Seno, Volumen: 0.4f, Caida: 5f)),

                // Descendente lento: Sol - Mi - Do - Sol grave.
                Efecto.FinJuego => Mezclar(PicoMaximo,
                    new Nota(Midi(67), 0.00f, 0.20f, FormaOnda.Triangulo, Caida: 5f),
                    new Nota(Midi(64), 0.18f, 0.20f, FormaOnda.Triangulo, Caida: 5f),
                    new Nota(Midi(60), 0.36f, 0.20f, FormaOnda.Triangulo, Caida: 5f),
                    new Nota(Midi(55), 0.54f, 0.40f, FormaOnda.Triangulo, Caida: 3f),
                    new Nota(Midi(43), 0.54f, 0.40f, FormaOnda.Seno, Volumen: 0.5f, Caida: 3f)),

                _ => Array.Empty<short>()
            };
        }

        private static short[] GenerarCombo(int multiplicador)
        {
            int subida = Math.Clamp(multiplicador, 2, 5) - 2;   // x2 = 0 ... x5 = 3 semitonos
            return Mezclar(0.33f,
                new Nota(Midi(76 + subida), 0.00f, 0.09f, FormaOnda.Seno, Caida: 10f),
                new Nota(Midi(83 + subida), 0.07f, 0.13f, FormaOnda.Triangulo, Caida: 9f),
                new Nota(Midi(88 + subida), 0.07f, 0.13f, FormaOnda.Seno, Volumen: 0.35f, Caida: 9f));
        }

        /// <summary>Frecuencia en Hz de una nota MIDI (69 = La 440 Hz).</summary>
        private static float Midi(int midi) => 440f * MathF.Pow(2f, (midi - 69) / 12f);

        /// <summary>
        /// Suma las notas, aplica el pasa-bajos y escala el resultado para que
        /// su pico sea exactamente "pico" (fraccion de la escala completa).
        /// </summary>
        private static short[] Mezclar(float pico, params Nota[] notas)
        {
            float duracion = notas.Max(n => n.Inicio + n.Duracion);
            float[] mezcla = new float[(int)(duracion * MuestrasPorSegundo) + 1];

            foreach (Nota nota in notas)
                SumarNota(mezcla, nota);

            FiltrarPasaBajos(mezcla);

            float maximo = mezcla.Max(MathF.Abs);
            float escala = maximo > 0f ? pico / maximo : 0f;

            short[] muestras = new short[mezcla.Length];
            for (int i = 0; i < mezcla.Length; i++)
                muestras[i] = (short)Math.Clamp(mezcla[i] * escala * short.MaxValue, short.MinValue, short.MaxValue);
            return muestras;
        }

        private static void SumarNota(float[] mezcla, Nota nota)
        {
            int inicio = (int)(nota.Inicio * MuestrasPorSegundo);
            int total = (int)(nota.Duracion * MuestrasPorSegundo);
            float fase = 0f;   // en ciclos (0..1)

            for (int i = 0; i < total && inicio + i < mezcla.Length; i++)
            {
                float t = i / (float)MuestrasPorSegundo;
                float progreso = i / (float)total;

                float frecuencia = nota.FrecuenciaFinal > 0f
                    ? nota.Frecuencia + (nota.FrecuenciaFinal - nota.Frecuencia) * progreso
                    : nota.Frecuencia;
                fase = (fase + frecuencia / MuestrasPorSegundo) % 1f;

                float onda = nota.Onda == FormaOnda.Seno
                    ? MathF.Sin(fase * MathF.Tau)
                    : 1f - 4f * MathF.Abs(fase - 0.5f);

                mezcla[inicio + i] += onda * Envolvente(t, progreso, nota.Caida) * nota.Volumen;
            }
        }

        /// <summary>
        /// Volumen de la nota en el tiempo: entrada rapida, caida exponencial
        /// y un apagado final (coseno) que la lleva exactamente a 0.
        /// </summary>
        private static float Envolvente(float t, float progreso, float caida)
        {
            float entrada = MathF.Min(1f, t / Ataque);
            float cuerpo = MathF.Exp(-caida * t);
            float salida = progreso < 0.7f ? 1f : 0.5f + 0.5f * MathF.Cos((progreso - 0.7f) / 0.3f * MathF.PI);
            return entrada * cuerpo * salida;
        }

        /// <summary>Pasa-bajos de un polo: suaviza los agudos asperos.</summary>
        private static void FiltrarPasaBajos(float[] muestras)
        {
            float alfa = 1f - MathF.Exp(-MathF.Tau * FrecuenciaCorteFiltro / MuestrasPorSegundo);
            float anterior = 0f;
            for (int i = 0; i < muestras.Length; i++)
            {
                anterior += alfa * (muestras[i] - anterior);
                muestras[i] = anterior;
            }
        }
    }
}
