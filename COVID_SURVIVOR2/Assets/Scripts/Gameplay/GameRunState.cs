// Dados simples preservados apenas enquanto o aplicativo estiver aberto.
public static class GameRunState
{
    public static long LastScore { get; private set; }
    public static int LastEliminations { get; private set; }
    public static int LastWave { get; private set; }
    public static float LastSeconds { get; private set; }

    public static void Reset()
    {
        LastScore = 0;
        LastEliminations = 0;
        LastWave = 1;
        LastSeconds = 0f;
    }

    public static void Save(long score, int eliminations, int wave, float seconds)
    {
        LastScore = score;
        LastEliminations = eliminations;
        LastWave = wave;
        LastSeconds = seconds;
    }
}
