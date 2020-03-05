using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Reproductor
{
    class EfectoDelay : ISampleProvider
    {
        private ISampleProvider fuente;

        private int offsetMiliSegundos;
        private float ganancia;

        public float Ganancia
        {
            get
            {
                return ganancia;
            }
            set
            {
                if (value > 1)
                {
                    ganancia = 1.0f;
                }
                else if(value < 0)
                {
                    ganancia = 0.0f;
                }
                else
                {
                    ganancia = value;
                }
            }
        }

        public int OffsetMiliSegundos
        {
            get
            {
                return OffsetMiliSegundos;
            }
            set
            {
                if (value > 20000)
                {
                    offsetMiliSegundos = 20000;
                } else if (value < 0)
                {
                    offsetMiliSegundos = 0;
                } else
                {
                    offsetMiliSegundos = value;
                }
            }
        }
        private List<float> muestras = new List<float>();
        private int tamañoBuffer;

        private int cantidadMuestrasTranscurridas = 0;
        private int cantidadMuestrasBorradas = 0;


        public EfectoDelay(ISampleProvider fuente, int offsetMiliSegundos, float ganancia)
        {
            this.fuente = fuente;
            this.OffsetMiliSegundos = offsetMiliSegundos;
            this.Ganancia = ganancia;

            tamañoBuffer = fuente.WaveFormat.SampleRate * 20 * 
                fuente.WaveFormat.Channels;
        }

        public WaveFormat WaveFormat
        {
            get
            {
                return fuente.WaveFormat;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = fuente.Read(buffer, offset, count);
            //Cálculo de tiempos
            float milisegundosTranscurridos =
                ((float)(cantidadMuestrasTranscurridas) /
                (float)(fuente.WaveFormat.SampleRate * 
                    fuente.WaveFormat.Channels)) *
                1000.0f;
            int numeroMuestrasOffset =
                (int)
                    ((offsetMiliSegundos / 1000.0f)
                    * (float) fuente.WaveFormat.SampleRate);

            //Llenar el buffer
            for (int i=0; i<read; i++)
            {
                muestras.Add(buffer[i + offset]);
            }
            //Si el buffer excede el tamaño, ajustar el
            //número de elementos
            if(muestras.Count > tamañoBuffer)
            {
                //Eliminar excedente
                int diferencia =
                    muestras.Count - tamañoBuffer;
                muestras.RemoveRange(0, diferencia);
                cantidadMuestrasBorradas += diferencia;
            }
            //Aplicar el efecto
            if (milisegundosTranscurridos > 
                offsetMiliSegundos)
            {
                for(int i=0; i<read;i++)
                {
                    buffer[i + offset] +=
                        ganancia *
                        muestras[
                            (cantidadMuestrasTranscurridas -
                            cantidadMuestrasBorradas) + i
                            - numeroMuestrasOffset];
                }
            }

            cantidadMuestrasTranscurridas += read;
            return read;
        }
    }
}
