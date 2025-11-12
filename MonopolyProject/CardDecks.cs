// Nome do Ficheiro: CardDecks.cs
using System;
using System.Linq;
using System.Collections.Generic;
using Resisto_dos_jogadores; // Para aceder a SistemaJogo e Jogador
using MonopolyBoard; // Para aceder ao Board

namespace MonopolyGameLogic
{
    public static class CardDecks
    {
        private static readonly Random random = new Random();

        // ==================================================================
        // === MÉTODOS DE SORTE/AZAR (CHANCE) ===============================
        // ==================================================================
        public static void DrawChanceCard(SistemaJogo.Jogador jogador, SistemaJogo sistema, Board board)
        {
            Console.WriteLine("  Sorte/Azar! A tirar uma carta...");
            int roll = random.Next(1, 101); 

            if (roll <= 20) // 20%
            {
                Console.WriteLine("  Recebeu $150! (Bónus do Banco)");
                jogador.Dinheiro += 150;
            }
            else if (roll <= 30) // 10%
            {
                Console.WriteLine("  Recebeu $200! (Erro do Banco a seu favor)");
                jogador.Dinheiro += 200;
            }
            else if (roll <= 40) // 10%
            {
                Console.WriteLine("  Pague $70. (Multa por excesso de velocidade)");
                jogador.Dinheiro -= 70;
                sistema.AdicionarDinheiroFreePark(70); 
            }
            else if (roll <= 60) // 20%
            {
                Console.WriteLine("  Avance para a casa 'Start'.");
                MovePlayerTo(jogador, "Start", board);
            }
            else if (roll <= 80) // 20%
            {
                Console.WriteLine("  Vá para a 'Police' (Prisão)!");
                MovePlayerTo(jogador, "Police", board);
            }
            else // 20%
            {
                Console.WriteLine("  Avance para 'FreePark'.");
                MovePlayerTo(jogador, "FreePark", board);
            }
            
            Console.Write("  Pressione Enter para continuar...");
            Console.ReadLine();
        }

        // ==================================================================
        // === MÉTODOS DA CAIXA DA COMUNIDADE (COMMUNITY) ===================
        // ==================================================================
        public static void DrawCommunityCard(SistemaJogo.Jogador jogador, SistemaJogo sistema, Board board)
        {
            Console.WriteLine("  Caixa da Comunidade! A tirar uma carta...");
            int roll = random.Next(1, 101); 

            if (roll <= 10) // 10%
            {
                int totalCasas = sistema.Espacos.Values
                    .Where(p => p.Dono == jogador && p.Cor != null)
                    .Sum(p => p.NivelCasa);
                
                int totalAPagar = totalCasas * 20;
                Console.WriteLine($"  Taxa de manutenção! Pague $20 por casa. (Total: {totalCasas} casas = ${totalAPagar})");
                jogador.Dinheiro -= totalAPagar;
                sistema.AdicionarDinheiroFreePark(totalAPagar);
            }
            else if (roll <= 20) // 10%
            {
                Console.WriteLine("  É o seu aniversário! Receba $10 de cada jogador.");
                var outrosJogadores = sistema.ObterOutrosJogadores(jogador);
                int totalRecebido = 0;
                foreach (var outro in outrosJogadores)
                {
                    int aPagar = Math.Min(10, outro.Dinheiro); 
                    outro.Dinheiro -= aPagar;
                    totalRecebido += aPagar;
                    Console.WriteLine($"    {outro.Nome} pagou ${aPagar}");
                }
                jogador.Dinheiro += totalRecebido;
                Console.WriteLine($"  Total recebido: ${totalRecebido}");
            }
            else if (roll <= 40) // 20%
            {
                Console.WriteLine("  Recebeu $100! (Prémio de beleza)");
                jogador.Dinheiro += 100;
            }
            else if (roll <= 60) // 20%
            {
                Console.WriteLine("  Recebeu $170! (Herança)");
                jogador.Dinheiro += 170;
            }
            else if (roll <= 70) // 10%
            {
                Console.WriteLine("  Pague $40. (Consulta médica)");
                jogador.Dinheiro -= 40;
                sistema.AdicionarDinheiroFreePark(40);
            }
            else if (roll <= 80) // 10%
            {
                Console.WriteLine("  Avance para 'Pink1'.");
                MovePlayerTo(jogador, "Pink1", board);
            }
            else if (roll <= 90) // 10%
            {
                Console.WriteLine("  Avance para 'Teal2'.");
                MovePlayerTo(jogador, "Teal2", board);
            }
            else // 10%
            {
                Console.WriteLine("  Avance para 'White2'.");
                MovePlayerTo(jogador, "White2", board);
            }
            
            Console.Write("  Pressione Enter para continuar...");
            Console.ReadLine();
        }

        // ==================================================================
        // === MÉTODO PRIVADO DE MOVIMENTO ==================================
        // ==================================================================
        private static void MovePlayerTo(SistemaJogo.Jogador jogador, string spaceName, Board board)
        {
            // <-- MUDANÇA AQUI: Esta é a forma "antiga" de fazer -->
            // 1. Recebe o 'Tuple'
            Tuple<int, int> coords = board.GetSpaceCoords(spaceName);
            
            // 2. Acede aos valores com 'Item1' (Linha) e 'Item2' (Coluna)
            int newRow = coords.Item1;
            int newCol = coords.Item2;
            
            jogador.PosicaoY = newRow;
            jogador.PosicaoX = newCol;
            
            Console.WriteLine($"  {jogador.Nome} moveu-se para ({newCol}, {newRow}) [{spaceName}]");
        }
    }
}