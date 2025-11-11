using System;
using System.Collections.Generic;
using System.Linq;
using MonopolyGameLogic; // Para DiceRoller e EspacoComercial
using MonopolyBoard; // Para Board

namespace Resisto_dos_jogadores
{
    public class SistemaJogo
    {
        // ==================================================================
        // === CLASSE INTERNA JOGADOR =======================================
        // ==================================================================
        public class Jogador
        {
            // --- Propriedades do Jogador ---
            public string Nome { get; set; }
            public int Jogos { get; set; }
            public int Vitorias { get; set; }
            public int Empates { get; set; }
            public int Derrotas { get; set; }
            public int PosicaoX { get; set; }
            public int PosicaoY { get; set; }
            public int Dinheiro { get; set; }
            public bool JaLancouDadosTurno { get; set; }
            
            // --- Construtor do Jogador ---
            public Jogador(string nome)
            {
                Nome = nome;
                Jogos = 0;
                Vitorias = 0;
                Empates = 0;
                Derrotas = 0;
                PosicaoX = 3; 
                PosicaoY = 3; // Posição "Start" [3, 3]
                Dinheiro = 1500; // Dinheiro inicial
                JaLancouDadosTurno = false; 
            }

            // --- Método de Ação do Jogador ---
            // Esta é a lógica da jogada, agora dentro do próprio Jogador
            public void RealizarJogada(DiceRoller diceRoller, Board board, Dictionary<string, EspacoComercial> espacos)
            {
                // 1. Lançar dados e mover
                DiceResult resultado = diceRoller.Roll();
                int novaPosX = this.PosicaoX + resultado.HorizontalMove;
                int novaPosY = this.PosicaoY + resultado.VerticalMove;
                
                // 'this' refere-se a este próprio jogador
                this.PosicaoX = Math.Clamp(novaPosX, 0, 6);
                this.PosicaoY = Math.Clamp(novaPosY, 0, 6);
                
                string nomeCasa = board.GetSpaceName(this.PosicaoY, this.PosicaoX);
                this.JaLancouDadosTurno = true; 

                // 2. Mostrar resultado do movimento
                Console.WriteLine($"{this.Nome} (${this.Dinheiro}) - Posição: ({this.PosicaoX}, {this.PosicaoY}) [{nomeCasa}]");
                Console.WriteLine($"  (Dados lançados: X={resultado.HorizontalMove}, Y={resultado.VerticalMove})");

                // 3. Verificar se o espaço é comercial
                if (EspacoComercial.EspacoEComercial(nomeCasa))
                {
                    // 4. Se for, "avisar" o espaço para ele tratar do resto.
                    var espaco = espacos[nomeCasa];
                    // Passamos 'this' (este jogador) para o método do espaço
                    espaco.AterrarNoEspaco(this); 
                }
                // Se não for comercial (ex: Prisão), a jogada termina aqui.
            }
        }
        // ==================================================================
        // === FIM DA CLASSE JOGADOR ========================================
        // ==================================================================


        // --- Propriedades do SistemaJogo ---
        private readonly List<Jogador> jogadores = new();
        private readonly DiceRoller diceRoller = new(); // O sistema "tem" os dados
        private readonly Board board; // O sistema "tem" o tabuleiro
        private readonly Dictionary<string, EspacoComercial> espacosComerciais = new(); // O sistema "tem" os espaços
        private bool jogoIniciado = false;
        
        // Propriedades públicas para o Program.cs
        public bool JogoIniciado => jogoIniciado;
        public int ContagemJogadores => jogadores.Count;
        
        
        // --- Construtor do SistemaJogo ---
        public SistemaJogo(Board board)
        {
            this.board = board;
            InicializarEspacos(); // Preenche o dicionário espacosComerciais
        }

        private void InicializarEspacos()
        {
            // Pede os preços à classe EspacoComercial
            var precosBase = EspacoComercial.ObterPrecosBase();

            // Cria as instâncias (com Dono = null) para o nosso jogo
            foreach (var par in precosBase)
            {
                string nome = par.Key;
                int preco = par.Value;
                espacosComerciais.Add(nome, new EspacoComercial(nome, preco));
            }
        }

        public IEnumerable<Jogador> ObterJogadoresOrdenados()
        {
            return jogadores
                .OrderByDescending(j => j.Vitorias)
                .ThenBy(j => j.Nome);
        }

        // --- Método Principal de Comandos ---
        public bool ExecutarComando(string linha)
        {
            string linhaLimpa = linha.Trim();

            if (linhaLimpa.Equals("q", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
                return true; 
            }

            string[] partes = linhaLimpa.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (partes.Length == 0)
            {
                MostrarInstrucaoInvalida();
                return false; 
            }

            string instrucao = partes[0].ToUpper(); 

            switch (instrucao)
            {
                case "IJ": // Iniciar Jogo
                    if (partes.Length != 1)
                    {
                        MostrarInstrucaoInvalida();
                        return false;
                    }
                    else
                    {
                        IniciarJogo();
                    }
                    break;

                case "RJ": // Registar Jogador
                    if (jogoIniciado)
                    {
                        Console.WriteLine("Erro: Não pode registar jogadores depois do jogo começar.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
                    
                    if (jogadores.Count >= 5)
                    {
                        Console.WriteLine("Erro: O limite máximo de 5 jogadores foi atingido.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false; 
                    }
                    
                    if (partes.Length != 2)
                    {
                        MostrarInstrucaoInvalida();
                        return false;
                    }
                    else
                    {
                        RegistarJogador(partes[1]);
                    }
                    break;
                
                // O comando "CE" já não existe aqui

                case "LD": // Lançar Dados
                    if (!jogoIniciado)
                    {
                        Console.WriteLine("Erro: O jogo ainda não começou. Use o comando 'IJ'.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
                    
                    if (partes.Length != 2)
                    {
                        MostrarInstrucaoInvalida();
                        return false;
                    }
                    else
                    {
                        LancarDados(partes[1]);
                    }
                    break;

                default:
                    MostrarInstrucaoInvalida();
                    return false;
            }
            
            return true;
        }

        // --- Métodos de Lógica do Jogo ---

        private void IniciarJogo()
        {
            if (jogoIniciado)
            {
                Console.WriteLine("O jogo já foi iniciado.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            if (jogadores.Count < 2)
            {
                Console.WriteLine($"Erro: São necessários pelo menos 2 jogadores para iniciar. (Atuais: {jogadores.Count})");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            jogoIniciado = true;
            Console.WriteLine("--- O JOGO COMEÇOU ---");
            Console.WriteLine($"A jogar com {jogadores.Count} jogadores.");
            Console.WriteLine("Registo de jogadores bloqueado.");
            Console.WriteLine("Lançamento de dados (LD) disponível.");
        }


        private void RegistarJogador(string nome)
        {
            if (jogadores.Any(j => j.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Jogador existente.");
                Console.WriteLine("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            jogadores.Add(new Jogador(nome));
        }
        
        // Método "LancarDados" do SistemaJogo (agora muito simples)
        private void LancarDados(string nomeJogador)
        {
            var jogador = jogadores.FirstOrDefault(j => j.Nome.Equals(nomeJogador, StringComparison.OrdinalIgnoreCase));

            if (jogador == null)
            {
                Console.WriteLine($"Jogador '{nomeJogador}' não encontrado.");
                return; 
            }

            // O SistemaJogo delega a ação ao Jogador,
            // fornecendo as "ferramentas do mundo" necessárias.
            jogador.RealizarJogada(diceRoller, board, espacosComerciais);
        }
        
        private void MostrarInstrucaoInvalida()
        {
            Console.WriteLine("2025-2026");
            Console.WriteLine("Instrução inválida.");
            Console.Write("Pressione Enter para reiniciar...");
            Console.ReadLine();
        }
    }
}