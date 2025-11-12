using System;
using System.Linq; 
using MonopolyBoard;
using MonopolyGameLogic; 
using Resisto_dos_jogadores;

class Program
{
    static Board board = new Board();
    static SistemaJogo sistema = new SistemaJogo(board); 
    
    private const int CentroTabuleiro = 3;

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
        
        var jogadores = sistema.ObterJogadoresOrdenados();
        
        board.Display(jogadores); 

        Console.WriteLine("\n--- Jogadores Registados ---");
        
        // <-- MUDANÇA AQUI: Mostrar o estado de TODAS as opções de jogo -->
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine($"({sistema.ContagemJogadores}/5 jogadores registados)");
            string eLeilao = sistema.LeiloesAtivos ? "LIGADOS" : "DESLIGADOS";
            string eVenda = sistema.VendaCasasAtiva ? "LIGADA" : "DESLIGADA";
            string eHipot = sistema.HipotecasAtivas ? "LIGADA" : "DESLIGADA";
            
            Console.WriteLine($"  Opções: Leilões ({eLeilao}) | Venda Casas ({eVenda}) | Hipotecas ({eHipot})");
        }
        
        var jogadorAtual = sistema.JogadorAtual; 
        
        if (!jogadores.Any())
        {
            Console.WriteLine("(Sem jogadores registados)");
        }
        else
        {
            foreach (var j in jogadores)
            {
                if (sistema.JogoIniciado && !j.EstaEmJogo)
                {
                    Console.WriteLine($"- {j.Nome} (Fora de Jogo)");
                }
                else
                {
                    string prefixo = (j == jogadorAtual) ? "→ " : "- ";
                    
                    int arrayRow = j.PosicaoY + CentroTabuleiro;
                    int arrayCol = j.PosicaoX + CentroTabuleiro;
                    string nomeCasa = board.GetSpaceName(arrayRow, arrayCol);
                    
                    string estado = j.EstaPreso ? " (Preso)" : "";
                    
                    Console.WriteLine($"{prefixo}{j.Nome} (${j.Dinheiro}){estado} Posição: ({j.PosicaoX}, {j.PosicaoY}) [{nomeCasa}]");
                }
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
        
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine("  RJ NomeJogador  → Regista novo jogador (Máx 5)");
            Console.WriteLine("  IJ              → Inicia o Jogo (Mín 2)");
            Console.WriteLine("  LS              → Lista estatísticas dos jogadores");
            Console.WriteLine("  EF              → Abre o menu de funcionalidades extras");
        }
        else
        {
            Console.WriteLine("  LD              → Lança os dados (inicia o turno)");
            Console.WriteLine("  CC              → Abre o menu de compra de casas");
            
            // <-- MUDANÇA AQUI: Novos comandos VC e H adicionados -->
            if (sistema.VendaCasasAtiva)
            {
                Console.WriteLine("  VC              → Abre o menu de venda de casas");
            }
            if (sistema.HipotecasAtivas)
            {
                Console.WriteLine("  H               → Abre o menu de hipotecas");
            }
            
            Console.WriteLine("  PROPS           → Vê as suas propriedades");
            Console.WriteLine("  EPT             → Propõe um empate aos outros jogadores");
            Console.WriteLine("  DS              → Desistir do jogo (perde)");
            Console.WriteLine("  ET              → Encerrar o seu turno");
        }
        
        Console.WriteLine("  Q               → Termina o programa");
        Console.WriteLine("----------------------------------------");
    }
}