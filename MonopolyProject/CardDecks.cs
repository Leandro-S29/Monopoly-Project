// Nome do Ficheiro: CardDecks.cs
using System;
using System.Linq;
using System.Collections.Generic;
using Resisto_dos_jogadores; 
using MonopolyBoard; 

namespace MonopolyGameLogic
{
    public static class CardDecks
    {
        private static readonly Random random = new Random();
        private const int CentroTabuleiro = 3; // Offset para traduzir coordenadas

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
                // <-- MUDANÇA AQUI: Usar TentarPagar -->
                if (sistema.TentarPagar(jogador, 70, "multa por excesso de velocidade"))
                {
                    sistema.AdicionarDinheiroFreePark(70); 
                }
            }
            else if (roll <= 60) // 20%
            {
                Console.WriteLine("  Avance para a casa 'Start'.");
                // (O "MovePlayerTo" agora está no SistemaJogo)
                sistema.MoverJogadorPara(jogador, "Start");
            }
            else if (roll <= 80) // 20%
            {
                Console.WriteLine("  Vá para a 'Police' (Prisão)!");
                // (O "MovePlayerTo" agora está no SistemaJogo)
                sistema.MoverJogadorPara(jogador, "Police");
            }
            else // 20%
            {
                Console.WriteLine("  Avance para 'FreePark'.");
                // (O "MovePlayerTo" agora está no SistemaJogo)
                sistema.MoverJogadorPara(jogador, "FreePark");
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
                
                // <-- MUDANÇA AQUI: Usar TentarPagar -->
                if (sistema.TentarPagar(jogador, totalAPagar, "taxa de manutenção"))
                {
                    sistema.AdicionarDinheiroFreePark(totalAPagar);
                }
            }
            else if (roll <= 20) // 10%
            {
                Console.WriteLine("  É o seu aniversário! Receba $10 de cada jogador.");
                var outrosJogadores = sistema.ObterOutrosJogadores(jogador);
                int totalRecebido = 0;
                foreach (var outro in outrosJogadores)
                {
                    // (Pagamento entre jogadores não causa bancarrota, apenas paga o que pode)
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
                // <-- MUDANÇA AQUI: Usar TentarPagar -->
                if (sistema.TentarPagar(jogador, 40, "consulta médica"))
                {
                    sistema.AdicionarDinheiroFreePark(40);
                }
            }
            else if (roll <= 80) // 10%
            {
                Console.WriteLine("  Avance para 'Pink1'.");
                sistema.MoverJogadorPara(jogador, "Pink1");
            }
            else if (roll <= 90) // 10%
            {
                Console.WriteLine("  Avance para 'Teal2'.");
                sistema.MoverJogadorPara(jogador, "Teal2");
            }
            else // 10%
            {
                Console.WriteLine("  Avance para 'White2'.");
                sistema.MoverJogadorPara(jogador, "White2");
            }
            
            Console.Write("  Pressione Enter para continuar...");
            Console.ReadLine();
        }

        // ==================================================================
        // === MÉTODO PRIVADO DE MOVIMENTO (REMOVIDO) =======================
        // ==================================================================
        // (O método "MovePlayerTo" foi movido para a classe SistemaJogo
        // para que possa ser usado por "RealizarJogada" e pelas Cartas)
    }
}