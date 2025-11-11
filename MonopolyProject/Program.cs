using System;
using System.Linq; 
using MonopolyBoard;
using Resisto_dos_jogadores;

namespace MonopolyProject
{

class Program
{
    static Board board = new Board();
    
    // Passa o 'board' para o construtor do 'sistema'
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

            if (!sucesso)
            {
                // Se o comando falhar (instrução inválida, limites, etc.)
                RedesenharUI();
            }
            // Redesenha a UI se um jogador for registado, o jogo começar, ou uma compra for feita
            else if (comando == "RJ" || comando == "IJ" || comando == "CE") 
            {
                RedesenharUI(); 
            }
            // Nota: "LD" (Lançar Dados) não redesenha a UI inteira,
            // apenas imprime a sua própria saída.
        }
    }

    static void RedesenharUI()
    {
        Console.Clear(); 
        board.Display(); 

        Console.WriteLine("\n--- Jogadores Registados ---");
        
        // Mostra a contagem de jogadores (ex: 2/5) antes de o jogo começar
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine($"({sistema.ContagemJogadores}/5 jogadores registados)");
        }
        
        var jogadores = sistema.ObterJogadoresOrdenados();
        
        if (!jogadores.Any())
        {
            Console.WriteLine("(Sem jogadores registados)");
        }
        else
        {
            foreach (var j in jogadores)
            {
                string nomeCasa = board.GetSpaceName(j.PosicaoY, j.PosicaoX);
                
                // Mostra o dinheiro do jogador
                Console.WriteLine($"- {j.Nome} (${j.Dinheiro}) (Jogos:{j.Jogos} V:{j.Vitorias} E:{j.Empates} D:{j.Derrotas}) - Posição: ({j.PosicaoX}, {j.PosicaoY}) [{nomeCasa}]");
            }
        }

        Console.WriteLine("\n=== Sistema de Registo de Jogadores ===");
        Console.WriteLine("Comandos disponíveis:");
        
        // Menu dinâmico: muda consoante o jogo tenha começado
        if (!sistema.JogoIniciado)
        {
            Console.WriteLine("  RJ NomeJogador  → Regista novo jogador (Máx 5)");
            Console.WriteLine("  IJ              → Inicia o Jogo (Mín 2)");
        }
        else
        {
            Console.WriteLine("  LD NomeJogador  → Lança os dados para um jogador");
            Console.WriteLine("  CE NomeJogador  → Compra o espaço atual");
        }
        
        Console.WriteLine("  Q               → Termina o programa");
        Console.WriteLine("----------------------------------------");
    }
}

}