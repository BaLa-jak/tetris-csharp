using OpenTK.Mathematics;

namespace Tetris2D.UI
{
    /// <summary>
    /// Mensaje breve con una ligera animacion (ej. "COMBO x3  +900"):
    ///  1. Aparece con un pequeno rebote de tamano.
    ///  2. Sube despacio unos pixeles.
    ///  3. Se desvanece al final.
    /// Un mensaje nuevo reemplaza al anterior.
    /// </summary>
    public class TextoAnimado
    {
        /// <summary>Duracion total de la animacion en segundos.</summary>
        public const float Duracion = 1.6f;

        private const float DuracionRebote = 0.35f;
        private const float InicioDesvanecer = 0.65f;   // fraccion de la duracion

        private readonly RenderizadorTexto _texto;
        private string _mensaje = "";
        private Vector4 _color;
        private float _tiempo = Duracion;   // empieza "terminado": nada que mostrar

        public TextoAnimado(RenderizadorTexto texto)
        {
            _texto = texto;
        }

        public bool Visible => _tiempo < Duracion;

        /// <summary>Inicia la animacion con un mensaje nuevo.</summary>
        public void Mostrar(string mensaje, Vector4 color)
        {
            _mensaje = mensaje;
            _color = color;
            _tiempo = 0f;
        }

        public void Actualizar(float dt)
        {
            if (Visible)
                _tiempo += dt;
        }

        /// <summary>
        /// Dibuja el mensaje centrado en centroX; "y" es la parte superior
        /// del texto al inicio de la animacion.
        /// </summary>
        public void Renderizar(float centroX, float y, float altoTexto)
        {
            if (!Visible)
                return;

            float progreso = _tiempo / Duracion;
            float alto = altoTexto * Escala();
            float subida = altoTexto * 0.6f * progreso;
            float alpha = progreso < InicioDesvanecer
                ? 1f
                : 1f - (progreso - InicioDesvanecer) / (1f - InicioDesvanecer);

            // El texto crece alrededor de su centro para que el rebote no lo desplace.
            float anchoTexto = _texto.MedirTexto(_mensaje, alto);
            float textoY = y + (altoTexto - alto) * 0.5f - subida;
            _texto.DibujarTexto(_mensaje, centroX - anchoTexto * 0.5f, textoY, alto,
                new Vector4(_color.Xyz, _color.W * alpha));
        }

        /// <summary>
        /// Escala del texto: crece de 0.6 a 1.15 y regresa a 1.0 durante el
        /// rebote inicial; despues se mantiene en 1.0.
        /// </summary>
        private float Escala()
        {
            if (_tiempo >= DuracionRebote)
                return 1f;

            float t = _tiempo / DuracionRebote;
            const float pico = 0.6f;   // momento (0..1) del tamano maximo
            return t < pico
                ? MathHelper.Lerp(0.6f, 1.15f, t / pico)
                : MathHelper.Lerp(1.15f, 1f, (t - pico) / (1f - pico));
        }
    }
}
