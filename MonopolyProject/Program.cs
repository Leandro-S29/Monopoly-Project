using System;
using System.Linq; 
using MonopolyBoard;
using MonopolyGameLogic; 
using Resisto_dos_jogadores;

class Program
{
    static Board board = new Board();
    static SistemaJogo sistema = new SistemaJogo(board); 

    static void Main()
    {
        RedesenharUI();
        
        while (true)
        {
            Console.Write("> ");
            string linha = Console.ReadLine();
            
            string[] partes = linha.Trim().Split(' ');
            string comando = (partes.Length > 0) ? partes[0].ToUpper() : "";

            bool sucesso = sistema.ExecutarComando(linha);

            if (sucesso)
            {
                RedesenharUI();
            }
            else
            {
                RedesenharUI();
            }
        }
    }

    static void RedesenharUI()
    {
        Console.Clear(); 
        board.Display(); 

        Console.WriteLine("\n--- Jogadores Registados ---");
        
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine($"({sistema.ContagemJogadores}/5 jogadores registados)");
        }
        
        var jogadores = sistema.ObterJogadoresOrdenados();
        var jogadorAtual = sistema.JogadorAtual; 
        
        if (!jogadores.Any())
        {
            Console.WriteLine("(Sem jogadores registados)");
        }
        else
        {
            foreach (var j in jogadores)
            {
                string prefixo = (j == jogadorAtual) ? "→ " : "- ";
                string nomeCasa = board.GetSpaceName(j.PosicaoY, j.PosicaoX);
                Console.WriteLine($"{prefixo}{j.Nome} (${j.Dinheiro}) Posição: [{nomeCasa}]");
            }
        }

        if (sistema.JogoIniciado)
        {
            Console.WriteLine($"\n💰 Pote do FreePark: ${sistema.DinheiroFreePark}");
            if (jogadorAtual != null)
            {
                Console.WriteLine($"➡️  É a vez de: {jogadorAtual.Nome}"); 
            }
        }

        Console.WriteLine("\n=== Sistema de Jogo ===");
        Console.WriteLine("Comandos disponíveis:");
        
        // <-- MUDANÇA: Menu de comandos atualizado -->
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine("  RJ NomeJogador  → Regista novo jogador (Máx 5)");
            Console.WriteLine("  IJ              → Inicia o Jogo (Mín 2)");
            Console.WriteLine("  LS              → Lista estatísticas dos jogadores");;
        }
        else
        {
            Console.WriteLine("  LD              → Lança os dados (inicia o turno)");
            Console.WriteLine("  CC              → Abre o menu de compra de casas");
            Console.WriteLine("  PROPS           → Vê as suas propriedades");
            Console.WriteLine("  ET              → Encerrar o seu turno");
        }
        
        Console.WriteLine("  Q               → Termina o programa");
        Console.WriteLine("----------------------------------------");
    }
}