using System.Diagnostics;
using System.Text;

namespace Tetris;
internal class Program
{
    //static Timer gameTimer = new Timer(GameLoop, null, 0, 1000); // Temporizador de 100 ms

    private static int currentX = fieldWidth / 2;
    private static int currentY = 0;
    private static int currentRotation = 0;
    private static int currentPiece = 0;
    private static Random random = new Random();


    //static bool isMovingLeft = false;
    //static bool isMovingRight = false;

    // Variables globales
    private const int fieldWidth = 12;
    private const int fieldHeight = 18;

    // Pieza del tetris
    private static readonly string[] tetromino = new string[]
    {
        "..X...X...X...X.", // Tetromino 0
        "..X..XX...X.....", // Tetromino 1
        ".....XX..XX.....", // Tetromino 2
        "..X..XX..X......", // Tetromino 3
        ".X...XX...X.....", // Tetromino 4
        ".X...X...XX.....", // Tetromino 5
        "..X...X..XX....."  // Tetromino 6
    };
    private static readonly string[] pieces = new string[]
    {
        "🟨",    // 0 Amarillo
        "🟥",    // 1 Rojo
        "🟩",    // 2 Verde
        "🟫",    // 3 Marron
        "🟪",    // 4 Morado
        "🟦",    // 5 Azul
        "🟧",    // 6 Naranja
        "🔳",   // 7 Borde
        "🔲"    // 8 Vacio
    };

    // Campo de juego
    private static byte[] field = new byte[fieldWidth * fieldHeight];

    // pantalla 
    private static string[] screen = new string[fieldWidth * fieldHeight];

    // Funcion principal
    static void Main(string[] args)
    {
        // Instancia para que la consola imprima caracteres UTF8
        Console.OutputEncoding = Encoding.UTF8;
        InitField();
        currentPiece = random.Next() % 7;
        // time

        const int frameRate = 1; // Velocidad de fotogramas por segundo (FPS)
        const double frameInterval = 1000.0 / frameRate; // Intervalo de tiempo entre fotogramas

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();



        while (true)
        {
            // Registrar el tiempo actual
            double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

            // Realizar una actualización del juego si ha pasado el intervalo de tiempo
            if (elapsedMilliseconds >= frameInterval)
            {
                // Llamar a tu método GameLoop para actualizar el juego
                GameLoop();

                // Reiniciar el reloj para el siguiente intervalo
                stopwatch.Restart();
            }

            #region Implementacion de las entradas (input)
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.LeftArrow && DoesPieceFit(currentPiece, currentRotation, currentX - 1, currentY))
                {
                    ClearPiece();
                    currentX--;
                }
                else if (key == ConsoleKey.RightArrow && DoesPieceFit(currentPiece, currentRotation, currentX + 1, currentY))
                {
                    ClearPiece();
                    currentX++;
                }
                else if (key == ConsoleKey.DownArrow && DoesPieceFit(currentPiece, currentRotation, currentX, currentY + 1))
                {
                    ClearPiece();
                    currentY++;
                }
                else if (key == ConsoleKey.Z && DoesPieceFit(currentPiece, currentRotation + 1, currentX, currentY))
                {
                    ClearPiece();
                    currentRotation++;
                }
                // Añade aquí más lógica para otras teclas si es necesario
            }
            #endregion
        }
    }

    private static void GameLoop()
    {
        Console.Clear();

        // Comprobamos si puede bajar
        if (DoesPieceFit(currentPiece, currentRotation, currentX, currentY + 1))
        {
            // Movemos la pieza hacia abajo
            ClearPiece();
            currentY++;

        }
        else
        {
            // Bloquea la pieza
            // Si la pieza ha alcanzado el fondo o colisiona, agregarla al campo de juego
            DrawPiece(true);

            // Comprobar si se ha completado alguna fila y eliminarla si es necesario
            for (int py = 0; py < 4; py++)
            {
                if (currentY + py < fieldHeight - 1)
                {
                    bool isRowComplete = true;
                    for (int px = 1; px < fieldWidth - 1; px++)
                    {
                        if (field[(currentY + py) * fieldWidth + px] == 8)
                        {
                            isRowComplete = false;
                            break;
                        }
                    }

                    if (isRowComplete)
                    {
                        // Eliminar la fila completa
                        for (int px = 1; px < fieldWidth - 1; px++)
                        {
                            field[(currentY + py) * fieldWidth + px] = 8;
                        }
                    }
                }
            }

            // Generar una nueva pieza aleatoria
            currentPiece = random.Next(0, 10000) % 7;
            currentX = fieldWidth / 2;
            currentY = 0;

            // Verifica si es fin del juego
            // Implementar la logica
        }


        // Display

        // Dibuja la pieza actual
        DrawPiece(false);

        // Dibuja la el campo
        DrawField();

        // Dibujar la animacion
        // <--------------------------->

        // Dibuja la pantalla
        DrawScreen();

    }

    // Metodo para rotar la pieza
    public static int Rotate(int px, int py, int r)
    {
        int pi = 0;
        switch (r % 4)
        {
            case 0: // 0 degrees			// 0  1  2  3
                pi = py * 4 + px;           // 4  5  6  7
                break;                      // 8  9 10 11
                                            //12 13 14 15

            case 1: // 90 degrees			//12  8  4  0
                pi = 12 + py - (px * 4);    //13  9  5  1
                break;                      //14 10  6  2
                                            //15 11  7  3

            case 2: // 180 degrees			//15 14 13 12
                pi = 15 - (py * 4) - px;    //11 10  9  8
                break;                      // 7  6  5  4
                                            // 3  2  1  0

            case 3: // 270 degrees			// 3  7 11 15
                pi = 3 - py + (px * 4);     // 2  6 10 14
                break;                      // 1  5  9 13
        }                               // 0  4  8 12

        return pi;
    }
    
    // Metodo para pintar la pantalla
    public static void DrawScreen()
    {
        for (int py = 0; py < fieldHeight; py++)
        {
            for (int px = 0; px < fieldWidth; px++)
            {
                Console.Write(screen[py * fieldWidth + px]);
            }
            Console.WriteLine();
        }
    }

    // Metodo para borrar la pieza
    public static void ClearPiece()
    {
        for (int py = 0; py < 4; py++)
        {
            for (int px = 0; px < 4; px++)
            {
                if (tetromino[currentPiece][Rotate(px, py, currentRotation)] == 'X')
                {
                    int index = (currentY + py) * fieldWidth + px + currentX;
                    field[index] = 8;
                }
            }
        }
    }

    // Llena el campo de juegos
    public static void DrawField()
    {
        for (int py = 0; py < fieldHeight; py++)
        {
            for (int px = 0; px < fieldWidth; px++)
            {
                int index = py * fieldWidth + px;
                int pi = field[index] == 9 ? currentPiece : field[index];
                screen[index] = pieces[pi];
            }
        }
    }
    
    // Inicializa el campo de juego
    public static void InitField()
    {
        for (int py = 0; py < fieldHeight; py++)
        {
            for (int px = 0; px < fieldWidth; px++)
            {
                int index = py * fieldWidth + px;
                if (px == 0 || px == fieldWidth - 1 || py == fieldHeight - 1)
                {
                    field[index] = 7;
                }
                else
                {
                    field[index] = 8;
                }
            }
        }
    }


    public static void DrawPiece(bool lockPiece)
    {
        for (int py = 0; py < 4; py++)
        {
            for (int px = 0; px < 4; px++)
            {
                if (tetromino[currentPiece][Rotate(px, py, currentRotation)] == 'X')
                {
                    int index = (currentY + py) * fieldWidth + px + currentX;
                    field[index] = (byte)(!lockPiece ? 9 : currentPiece);
                }
            }
        }
    }

    public static bool DoesPieceFit(int nTetromino, int nRotation, int nPosX, int nPosY)
    {
        for (int py = 0; py < 4; py++)
        {
            for (int px = 0; px < 4; px++)
            {
                int pi = Rotate(px, py, nRotation);
                int fi = (nPosY + py) * fieldWidth + (nPosX + px);
                if (tetromino[nTetromino][pi] == 'X')
                {
                    // Basta que un cuadro este ocupado para que retorne false
                    if (field[fi] != 8 && field[fi] != 9)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}