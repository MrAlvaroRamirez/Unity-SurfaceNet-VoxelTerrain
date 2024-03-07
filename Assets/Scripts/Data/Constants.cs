public static class Constants
{
    public const byte WORLD_WIDTH = 1;
    public const byte WORLD_HEIGHT = 1;
    public const byte WORLD_DEPTH = WORLD_WIDTH;
    public const int WORLD_AREA = WORLD_WIDTH * WORLD_DEPTH;
    public const int WORLD_VOL = WORLD_WIDTH * WORLD_DEPTH * WORLD_HEIGHT;


    public const byte CHUNK_SIZE = 64;
    public const int CHUNK_AREA = CHUNK_SIZE * CHUNK_SIZE;
    public const int CHUNK_VOL = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
}