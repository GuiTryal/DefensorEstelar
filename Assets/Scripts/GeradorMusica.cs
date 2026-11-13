using UnityEngine;

/// <summary>
/// Gera as músicas do jogo por código (síntese simples, estilo chiptune).
/// Cada música é um loop de 8 compassos montado a partir de uma sequência de acordes.
/// </summary>
public static class GeradorMusica
{
    const int TAXA = 22050; // taxa de amostragem (metade da de CD: suficiente e mais leve)

    enum Onda { Seno, Triangulo, Pulso25, Pulso12 }

    /// <summary>Acorde: nota grave (baixo) + três notas (em números MIDI).</summary>
    struct Acorde
    {
        public int baixo;
        public int[] notas;
        public Acorde(int baixo, params int[] notas) { this.baixo = baixo; this.notas = notas; }
    }

    // Lá menor, Fá, Dó, Sol e Mi menor (notas MIDI: 60 = Dó central).
    static readonly Acorde Am = new Acorde(45, 57, 60, 64);
    static readonly Acorde F  = new Acorde(41, 53, 57, 60);
    static readonly Acorde C  = new Acorde(48, 55, 60, 64);
    static readonly Acorde G  = new Acorde(43, 55, 59, 62);
    static readonly Acorde Em = new Acorde(40, 55, 59, 64);

    static readonly Acorde[] SEQUENCIA = { Am, F, C, G, Am, F, G, Em };

    // ------------------------------------------------------------------
    // Música do menu: lenta, com acordes longos, arpejo suave e eco.
    // ------------------------------------------------------------------
    public static AudioClip Menu()
    {
        float batida = 60f / 72f; // 72 batidas por minuto
        float compasso = batida * 4f;
        float[] buf = new float[Mathf.RoundToInt(SEQUENCIA.Length * compasso * TAXA)];
        int[] padrao = { 0, 1, 2, 4, 2, 1, 0, 1 }; // ordem das notas do arpejo (4 = 1ª nota uma oitava acima)

        for (int c = 0; c < SEQUENCIA.Length; c++)
        {
            Acorde a = SEQUENCIA[c];
            float inicio = c * compasso;

            // Acorde de fundo ("pad"): duas ondas levemente desafinadas para soar mais cheio.
            foreach (int n in a.notas)
            {
                Nota(buf, inicio, compasso, Freq(n) * 1.003f, Onda.Seno, 0.035f, 0.8f, 0.8f);
                Nota(buf, inicio, compasso, Freq(n) * 0.997f, Onda.Seno, 0.035f, 0.8f, 0.8f);
            }

            // Baixo: a nota grave do acorde durante o compasso inteiro.
            Nota(buf, inicio, compasso, Freq(a.baixo), Onda.Seno, 0.12f, 0.05f, 0.6f);

            // Arpejo: colcheias tocando as notas do acorde uma oitava acima.
            for (int i = 0; i < 8; i++)
            {
                int p = padrao[i];
                int midi = (p < 3 ? a.notas[p] : a.notas[p - 3] + 12) + 12;
                Nota(buf, inicio + i * batida / 2f, batida * 0.9f, Freq(midi), Onda.Triangulo, 0.045f, 0.005f, batida * 0.9f);
            }
        }

        Eco(buf, batida * 0.75f, 0.35f);
        Normalizar(buf, 0.8f);
        return Criar("MusicaMenu", buf);
    }

    // ------------------------------------------------------------------
    // Música da partida: mais rápida, com baixo marcado, arpejo e bateria.
    // ------------------------------------------------------------------
    public static AudioClip Jogo()
    {
        float batida = 60f / 128f; // 128 batidas por minuto
        float compasso = batida * 4f;
        float[] buf = new float[Mathf.RoundToInt(SEQUENCIA.Length * compasso * TAXA)];
        var aleatorio = new System.Random(3);

        for (int c = 0; c < SEQUENCIA.Length; c++)
        {
            Acorde a = SEQUENCIA[c];
            float inicio = c * compasso;

            // Baixo em colcheias, alternando a nota grave e a mesma nota uma oitava acima.
            for (int i = 0; i < 8; i++)
            {
                int midi = a.baixo + (i % 2 == 1 ? 12 : 0);
                Nota(buf, inicio + i * batida / 2f, batida * 0.45f, Freq(midi), Onda.Pulso25, 0.09f, 0.003f, batida * 0.4f);
            }

            // Arpejo rápido (semicolcheias) subindo pelas notas do acorde.
            for (int i = 0; i < 16; i++)
            {
                int midi = a.notas[i % 3] + 12 + (i % 6 >= 3 ? 12 : 0);
                Nota(buf, inicio + i * batida / 4f, batida * 0.22f, Freq(midi), Onda.Pulso12, 0.03f, 0.002f, batida * 0.2f);
            }

            // Bateria: bumbo nos tempos 1 e 3, caixa no 2 e 4, chimbal nos contratempos.
            for (int b = 0; b < 4; b++)
            {
                float t = inicio + b * batida;
                if (b % 2 == 0) Bumbo(buf, t, 0.35f);
                else Caixa(buf, t, 0.12f, aleatorio);
                Chimbal(buf, t + batida / 2f, 0.04f, aleatorio);
            }
            // Um bumbo extra no fim de cada 2 compassos dá mais movimento.
            if (c % 2 == 1) Bumbo(buf, inicio + 3.5f * batida, 0.25f);
        }

        Normalizar(buf, 0.8f);
        return Criar("MusicaJogo", buf);
    }

    // ------------------------------------------------------------------
    // Ferramentas de síntese
    // ------------------------------------------------------------------

    static float Freq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

    /// <summary>Soma uma nota no buffer, com ataque e soltura (o volume sobe e desce suavemente).</summary>
    static void Nota(float[] buf, float inicio, float duracao, float freq, Onda onda, float volume, float ataque, float soltura)
    {
        int i0 = Mathf.RoundToInt(inicio * TAXA);
        int total = Mathf.RoundToInt(duracao * TAXA);
        float passo = freq / TAXA;
        float fase = 0f;

        for (int k = 0; k < total; k++)
        {
            float t = (float)k / TAXA;
            float env = Mathf.Min(1f, t / Mathf.Max(ataque, 0.0001f));
            if (t > duracao - soltura) env *= Mathf.Max(0f, (duracao - t) / soltura);

            fase += passo;
            fase -= Mathf.Floor(fase);
            float s;
            switch (onda)
            {
                case Onda.Seno: s = Mathf.Sin(fase * 2f * Mathf.PI); break;
                case Onda.Triangulo: s = 1f - 4f * Mathf.Abs(fase - 0.5f); break;
                case Onda.Pulso25: s = fase < 0.25f ? 1f : -1f; break;
                default: s = fase < 0.125f ? 1f : -1f; break;
            }

            buf[(i0 + k) % buf.Length] += s * env * volume; // "% buf.Length" faz o fim emendar no começo (loop)
        }
    }

    static void Bumbo(float[] buf, float inicio, float volume)
    {
        int i0 = Mathf.RoundToInt(inicio * TAXA);
        int total = Mathf.RoundToInt(0.18f * TAXA);
        float fase = 0f;
        for (int k = 0; k < total; k++)
        {
            float t = (float)k / TAXA;
            float freq = Mathf.Lerp(150f, 45f, Mathf.Clamp01(t / 0.12f)); // grave que "cai"
            fase += freq / TAXA;
            buf[(i0 + k) % buf.Length] += Mathf.Sin(fase * 2f * Mathf.PI) * Mathf.Exp(-t * 22f) * volume;
        }
    }

    static void Caixa(float[] buf, float inicio, float volume, System.Random r)
    {
        int i0 = Mathf.RoundToInt(inicio * TAXA);
        int total = Mathf.RoundToInt(0.14f * TAXA);
        for (int k = 0; k < total; k++)
        {
            float t = (float)k / TAXA;
            float ruido = (float)(r.NextDouble() * 2.0 - 1.0);
            float tom = Mathf.Sin(t * 2f * Mathf.PI * 190f) * 0.4f;
            buf[(i0 + k) % buf.Length] += (ruido + tom) * Mathf.Exp(-t * 28f) * volume;
        }
    }

    static void Chimbal(float[] buf, float inicio, float volume, System.Random r)
    {
        int i0 = Mathf.RoundToInt(inicio * TAXA);
        int total = Mathf.RoundToInt(0.04f * TAXA);
        float anterior = 0f;
        for (int k = 0; k < total; k++)
        {
            float t = (float)k / TAXA;
            float ruido = (float)(r.NextDouble() * 2.0 - 1.0);
            float agudo = ruido - anterior; // diferença entre amostras = só os agudos
            anterior = ruido;
            buf[(i0 + k) % buf.Length] += agudo * Mathf.Exp(-t * 90f) * volume;
        }
    }

    /// <summary>Eco que se repete e vai sumindo (calculado em volta do loop para emendar sem corte).</summary>
    static void Eco(float[] buf, float atraso, float retorno)
    {
        int d = Mathf.RoundToInt(atraso * TAXA);
        int n = buf.Length;
        float[] original = (float[])buf.Clone();
        for (int volta = 0; volta < 2; volta++)
            for (int i = 0; i < n; i++)
                buf[i] = original[i] + buf[(i - d + n) % n] * retorno;
    }

    static void Normalizar(float[] buf, float pico)
    {
        float maior = 0f;
        foreach (float s in buf) maior = Mathf.Max(maior, Mathf.Abs(s));
        if (maior <= 0f) return;
        float fator = pico / maior;
        for (int i = 0; i < buf.Length; i++) buf[i] *= fator;
    }

    static AudioClip Criar(string nome, float[] buf)
    {
        AudioClip clip = AudioClip.Create(nome, buf.Length, 1, TAXA, false);
        clip.SetData(buf, 0);
        return clip;
    }
}
