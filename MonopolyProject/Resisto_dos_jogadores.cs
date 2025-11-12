// Nome do Ficheiro: Resisto_dos_jogadores.cs
using System;
using System.Collections.Generic;
using System.Linq; 
using MonopolyGameLogic; // Para CardDecks
using MonopolyBoard; 

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
            public bool EstaEmJogo { get; set; } 
            public bool EstaPreso { get; set; }
            public int TurnosPreso { get; set; }
            
            private const int CentroTabuleiro = 3;

            // --- Construtor do Jogador ---
            public Jogador(string nome)
            {
                Nome = nome; Jogos = 0; Vitorias = 0; Empates = 0; Derrotas = 0;
                PosicaoX = 0; 
                PosicaoY = 0; 
                Dinheiro = 1500; 
                JaLancouDadosTurno = false; 
                EstaEmJogo = true; 
                EstaPreso = false; 
                TurnosPreso = 0;   
            }

            // --- Método de Ação do Jogador ---
            public DiceResult RealizarJogada(DiceRoller diceRoller, Board board, Dictionary<string, EspacoComercial> espacos, SistemaJogo sistema)
            {
                // 1. Lançar dados e mover (Lógica)
                DiceResult resultado = diceRoller.Roll();
                int novaPosX_Logica = this.PosicaoX + resultado.HorizontalMove;
                int novaPosY_Logica = this.PosicaoY + resultado.VerticalMove;
                
                this.PosicaoX = Math.Clamp(novaPosX_Logica, -CentroTabuleiro, CentroTabuleiro);
                this.PosicaoY = Math.Clamp(novaPosY_Logica, -CentroTabuleiro, CentroTabuleiro);
                
                int arrayRow = this.PosicaoY + CentroTabuleiro; 
                int arrayCol = this.PosicaoX + CentroTabuleiro; 
                
                string nomeCasa = board.GetSpaceName(arrayRow, arrayCol);

                // 2. Mostrar resultado do movimento (Mostra as coordenadas lógicas)
                Console.WriteLine($"{this.Nome} (${this.Dinheiro}) - Posição: ({this.PosicaoX}, {this.PosicaoY}) [{nomeCasa}]");
                Console.WriteLine($"  (Dados lançados: X={resultado.HorizontalMove}, Y={resultado.VerticalMove})");

                // 3. Verificar a ação do espaço
                if (EspacoComercial.EspacoEComercial(nomeCasa))
                {
                    var espaco = espacos[nomeCasa];
                    espaco.AterrarNoEspaco(this, sistema); 
                }
                else if (nomeCasa == "Lux Tax")
                {
                    int imposto = 80;
                    Console.WriteLine($"  Você aterrou em [Lux Tax]! Pague ${imposto}.");
                    if (sistema.TentarPagar(this, imposto, "Imposto de Luxo"))
                    {
                        sistema.AdicionarDinheiroFreePark(imposto); 
                    }
                    else
                    {
                        return resultado; // Bancarrota, termina a jogada
                    }
                }
                else if (nomeCasa == "FreePark")
                {
                    int premio = sistema.DinheiroFreePark;
                    if (premio > 0)
                    {
                        Console.WriteLine($"  Você aterrou em [FreePark] e recolheu ${premio}!");
                        this.Dinheiro += premio;
                        sistema.ZerarDinheiroFreePark();
                        Console.WriteLine($"  O seu novo saldo é ${this.Dinheiro}.");
                    }
                    else
                    {
                        Console.WriteLine("  Você aterrou em [FreePark]. (Não há dinheiro acumulado.)");
                    }
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine();
                }
                else if (nomeCasa.Equals("Chance", StringComparison.OrdinalIgnoreCase))
                {
                    CardDecks.DrawChanceCard(this, sistema, board);
                }
                else if (nomeCasa.Equals("Community", StringComparison.OrdinalIgnoreCase))
                {
                    CardDecks.DrawCommunityCard(this, sistema, board);
                }
                else if (nomeCasa.Equals("Start", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("  Você aterrou na casa [Start]! Recebe $200.");
                    this.Dinheiro += 200;
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine(); 
                }
                else if (nomeCasa.Equals("Police", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("  Vá para a Prisão (Police)!");
                    sistema.MoverJogadorPara(this, "Prison"); 
                    this.EstaPreso = true;                    
                    this.TurnosPreso = 0;                     
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine(); 
                }
                else
                {
                    // "Prison" (visita), "BackToStart", etc.
                    Console.WriteLine($"  Você aterrou em [{nomeCasa}].");
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine(); 
                }
                
                return resultado;
            }
        }
        // ==================================================================
        // === FIM DA CLASSE JOGADOR ========================================
        // ==================================================================


        // --- Propriedades do SistemaJogo ---
        private readonly List<Jogador> jogadores = new();
        private readonly DiceRoller diceRoller = new(); 
        private readonly Board board; 
        private readonly Dictionary<string, EspacoComercial> espacosComerciais = new(); 
        private bool jogoIniciado = false;
        private int dinheiroFreePark = 0;
        private int indiceJogadorAtual = 0;
        private bool jogadorJaLancouDadosEsteTurno = false;
        
        // Opções de Jogo
        private bool leiloesAtivos = true; 
        private bool hipotecasAtivas = true;
        private bool vendaCasasAtiva = true;
        
        public bool JogoIniciado => jogoIniciado;
        public int ContagemJogadores => jogadores.Count;
        public int DinheiroFreePark => dinheiroFreePark;
        
        public Jogador JogadorAtual => (jogoIniciado && jogadores.Any() && jogadores[indiceJogadorAtual].EstaEmJogo) 
                                      ? jogadores[indiceJogadorAtual] 
                                      : null;
        
        public bool LeiloesAtivos => leiloesAtivos;
        public bool HipotecasAtivas => hipotecasAtivas;
        public bool VendaCasasAtiva => vendaCasasAtiva;
        
        public int ContagemJogadoresAtivos => jogadores.Count(j => j.EstaEmJogo);
        
        public IReadOnlyDictionary<string, EspacoComercial> Espacos => espacosComerciais;

        
        // --- Construtor e Métodos de Gestão ---
        public SistemaJogo(Board board)
        {
            this.board = board;
            InicializarEspacos(); 
        }

        private void InicializarEspacos()
        {
            var precosBase = EspacoComercial.ObterPrecosBase();
            foreach (var par in precosBase)
            {
                string nome = par.Key;
                int preco = par.Value;
                espacosComerciais.Add(nome, new EspacoComercial(nome, preco));
            }
        }
        public void AdicionarDinheiroFreePark(int valor) { dinheiroFreePark += valor; }
        public void ZerarDinheiroFreePark() { dinheiroFreePark = 0; }
        
        public List<Jogador> ObterOutrosJogadores(Jogador jogadorAtual)
        {
            return jogadores.Where(j => j != jogadorAtual && j.EstaEmJogo).ToList();
        }
        
        public IEnumerable<EspacoComercial> ObterPropriedadesDoJogador(Jogador jogador)
        {
            return espacosComerciais.Values
                .Where(espaco => espaco.Dono == jogador)
                .OrderBy(espaco => espaco.Cor) 
                .ThenBy(espaco => espaco.Nome);
        }

        public IEnumerable<Jogador> ObterJogadoresOrdenados()
        {
            return jogadores
                .OrderByDescending(j => j.Vitorias)
                .ThenBy(j => j.Empates)
                .ThenBy(j => j.Nome);
        }

        // --- Método Principal de Comandos ---
        public bool ExecutarComando(string linha)
        {
            string linhaLimpa = linha.Trim();
            if (linhaLimpa.Equals("q", StringComparison.OrdinalIgnoreCase)) { Environment.Exit(0); return true; }
            string[] partes = linhaLimpa.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length == 0) { MostrarInstrucaoInvalida(); return false; }
            string instrucao = partes[0].ToUpper(); 

            if (!jogoIniciado && instrucao != "IJ" && instrucao != "RJ" && instrucao != "Q" && instrucao != "LS" && instrucao != "EF")
            {
                if (instrucao != "CC" && instrucao != "PROPS")
                {
                    Console.WriteLine("Erro: O jogo ainda não começou. Use 'RJ', 'IJ' ou 'LS'.");
                    Console.Write("Pressione Enter para continuar...");
                    Console.ReadLine();
                    return false;
                }
            }

            switch (instrucao)
            {
                case "IJ": 
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    IniciarJogo();
                    break;
                
                case "RJ": 
                    if (jogoIniciado) 
                    { 
                        Console.WriteLine("Erro: Não pode registar novos jogadores depois do jogo começar.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false; 
                    }
                    if (jogadores.Count >= 5) 
                    { 
                        Console.WriteLine("Erro: Já atingiu o nº máximo de 5 jogadores.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false; 
                    }
                    if (partes.Length != 2) 
                    { 
                        MostrarInstrucaoInvalida(); 
                        return false; 
                    }
                    RegistarJogador(partes[1]);
                    break;
                    
                case "LD": 
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    LancarDados(); 
                    break;
                
                case "CC":
                    if (partes.Length != 1) { /*...*/ return false; }
                    MenuComprarCasas();
                    break;
                
                case "VC":
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    MenuVenderCasas();
                    break;
                
                case "H":
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    MenuHipoteca();
                    break;

                case "EPT":
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    IniciarVotacaoEmpate();
                    break;

                case "DS":
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    ExecutarDesistencia(JogadorAtual, true);
                    break;

                case "ET": 
                    if (!jogoIniciado) { /*...*/ return false; }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    EncerrarTurno();
                    break;
                case "PROPS": 
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    VerPropriedades();
                    break;
                case "LS": 
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    ListarEstatisticas();
                    break;

                case "EF":
                    if (jogoIniciado)
                    {
                        Console.WriteLine("Erro: Não pode alterar esta opção depois do jogo começar.");
                        Console.Write("Pressione Enter para continuar...");
                        Console.ReadLine();
                        return false;
                    }
                    if (partes.Length != 1) { MostrarInstrucaoInvalida(); return false; }
                    MenuFuncionalidadesExtras();
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
            if (jogoIniciado) { return; }
            if (jogadores.Count < 2) 
            {
                Console.WriteLine("Erro: São necessários pelo menos 2 jogadores para iniciar.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return; 
            }
            
            foreach(var j in jogadores) { j.EstaEmJogo = true; }
            
            jogoIniciado = true;
            indiceJogadorAtual = 0; 
            jogadorJaLancouDadosEsteTurno = false; 
            Console.WriteLine("--- O JOGO COMEÇOU ---");
            Console.WriteLine($"A jogar com {jogadores.Count(j => j.EstaEmJogo)} jogadores.");
            Console.WriteLine($"É a vez de: {JogadorAtual.Nome}");
        }
        private void RegistarJogador(string nome) 
        {
            if (jogadores.Any(j => j.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase))) 
            {
                Console.WriteLine("Erro: Já existe um jogador com esse nome.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return; 
            }
            jogadores.Add(new Jogador(nome));
        }

        private void LancarDados()
        {
            if (jogadorJaLancouDadosEsteTurno)
            {
                Console.WriteLine($"Erro: {JogadorAtual.Nome} já lançou os dados neste turno.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            
            var jogador = JogadorAtual; 
            
            if (jogador.EstaPreso)
            {
                TentarSairDaPrisao(jogador);
            }
            else
            {
                Console.WriteLine($"\n--- Turno de {jogador.Nome} (Lançamento) ---");
                DiceResult resultadoJogada = jogador.RealizarJogada(diceRoller, board, espacosComerciais, this);

                if (resultadoJogada.IsDoubles)
                {
                    Console.WriteLine($"\n  *** VALORES IGUAIS! ({resultadoJogada.HorizontalMove}, {resultadoJogada.VerticalMove}) ***");
                    Console.WriteLine($"  {jogador.Nome} pode lançar os dados (LD) novamente!");
                    jogadorJaLancouDadosEsteTurno = false; 
                }
                else
                {
                    jogadorJaLancouDadosEsteTurno = true;
                }
            }
        }
        
        private void EncerrarTurno()
        {
            if (!jogadorJaLancouDadosEsteTurno)
            {
                Console.WriteLine("Erro: Deve lançar os dados (LD) antes de encerrar o turno.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            Console.WriteLine($"\n--- Fim do turno de {JogadorAtual.Nome} ---");
            
            AvancarParaProximoJogador(); 
            
            jogadorJaLancouDadosEsteTurno = false;
            
            if (JogadorAtual.EstaPreso)
            {
                Console.WriteLine($"Próximo a jogar: {JogadorAtual.Nome} (Está na Prisão)");
            }
            else
            {
                Console.WriteLine($"Próximo a jogar: {JogadorAtual.Nome}");
            }
            Console.Write("Pressione Enter para continuar...");
            Console.ReadLine();
        }
        
        private void ComprarCasa(string nomePropriedade)
        {
            var jogador = JogadorAtual;
            if (!jogoIniciado)
            {
                Console.WriteLine("Erro: O jogo ainda não começou. Registe jogadores (RJ) e inicie (IJ).");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            if (!espacosComerciais.TryGetValue(nomePropriedade, out var espaco))
            {
                Console.WriteLine($"Erro: Propriedade '{nomePropriedade}' não encontrada.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            if (espaco.Dono != jogador)
            {
                Console.WriteLine($"Erro: Você não é o dono de [{espaco.Nome}].");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            if (!VerificarMonopolio(jogador, espaco.Cor))
            {
                Console.WriteLine($"Erro: Deve possuir todas as propriedades do grupo '{espaco.Cor}' para comprar casas.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            string corDoGrupo = espaco.Cor;
            int nivelAtualAlvo = espaco.NivelCasa; 
            var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(corDoGrupo);
            foreach (string nomePropIrma in nomesPropsDoGrupo)
            {
                if (nomePropIrma == espaco.Nome) continue; 
                var espacoIrmao = espacosComerciais[nomePropIrma];
                
                if (espacoIrmao.NivelCasa < nivelAtualAlvo)
                {
                    Console.WriteLine($"Erro: Deve construir uniformemente.");
                    Console.WriteLine($"  [{espaco.Nome}] está no nível {nivelAtualAlvo}, mas [{espacoIrmao.Nome}] está no nível {espacoIrmao.NivelCasa}.");
                    Console.WriteLine($"  Deve primeiro construir em [{espacoIrmao.Nome}].");
                    Console.Write("Pressione Enter para continuar...");
                    Console.ReadLine();
                    return; 
                }
            }
            int precoCasa = espaco.PrecoCasa;
            if (jogador.Dinheiro < precoCasa)
            {
                Console.WriteLine($"Erro: Sem dinheiro. Uma casa em [{espaco.Nome}] custa ${precoCasa}.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            if (espaco.NivelCasa >= 5)
            {
                Console.WriteLine($"Erro: [{espaco.Nome}] já atingiu o nível máximo (Hotel).");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            
            if (TentarPagar(jogador, precoCasa, "compra de casa"))
            {
                espaco.NivelCasa++;
                Console.WriteLine($"Casa comprada para [{espaco.Nome}]!");
                Console.WriteLine($"Novo Nível: {espaco.NivelCasa} (Custo: ${precoCasa})");
                Console.WriteLine($"Saldo restante: ${jogador.Dinheiro}");
            }

            Console.Write("Pressione Enter para continuar...");
            Console.ReadLine();
        }
        
        private bool VerificarMonopolio(Jogador jogador, string cor)
        {
            if (string.IsNullOrEmpty(cor)) { return false; }
            var propriedadesDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(cor);
            if (propriedadesDoGrupo == null) return false; 
            
            foreach (string nomeProp in propriedadesDoGrupo)
            {
                if (espacosComerciais[nomeProp].Dono != jogador)
                {
                    return false; 
                }
            }
            return true; 
        }
        
        private void MenuComprarCasas()
        {
            if (!jogoIniciado)
            {
                Console.WriteLine("Erro: O jogo ainda não começou. Registe jogadores (RJ) e inicie (IJ).");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }
            var jogador = JogadorAtual;
            while (true)
            {
                Console.Clear(); 
                var elegiveis = new List<EspacoComercial>();
                var minhasPropriedades = espacosComerciais.Values.Where(p => p.Dono == jogador);
                foreach (var prop in minhasPropriedades)
                {
                    if (string.IsNullOrEmpty(prop.Cor)) continue;
                    if (!VerificarMonopolio(jogador, prop.Cor)) continue;
                    if (prop.NivelCasa >= 5) continue;
                    if (jogador.Dinheiro < prop.PrecoCasa) continue;
                    string corDoGrupo = prop.Cor;
                    int nivelAtualAlvo = prop.NivelCasa;
                    var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(corDoGrupo);
                    bool construcaoUniforme = true;
                    foreach (string nomePropIrma in nomesPropsDoGrupo)
                    {
                        if (nomePropIrma == prop.Nome) continue; 
                        var espacoIrmao = espacosComerciais[nomePropIrma];
                        if (espacoIrmao.NivelCasa < nivelAtualAlvo)
                        {
                            construcaoUniforme = false; 
                            break; 
                        }
                    }
                    if (construcaoUniforme)
                    {
                        elegiveis.Add(prop);
                    }
                }
                Console.WriteLine($"--- Menu de Compra de Casas ({jogador.Nome} | Saldo: ${jogador.Dinheiro}) ---");
                if (elegiveis.Count == 0)
                {
                    Console.WriteLine("\n  Nenhuma propriedade elegível.");
                    Console.WriteLine("  (Lembre-se: precisa do grupo completo, dinheiro,");
                    Console.WriteLine("  espaço para construir, e construir uniformemente.)");
                }
                else
                {
                    Console.WriteLine("\n  Propriedades elegíveis para comprar casas:");
                    foreach (var prop in elegiveis.OrderBy(p => p.NivelCasa).ThenBy(p => p.Cor))
                    {
                        Console.WriteLine($"  - [{prop.Nome}] ({prop.Cor}) | Custo: ${prop.PrecoCasa} | Nível Atual: {prop.NivelCasa}");
                    }
                }
                Console.WriteLine("\n  Digite o nome da propriedade para comprar (ex: Brown1)");
                Console.WriteLine("  Ou digite 'SAIR' para voltar ao jogo.");
                Console.Write("  > ");
                string input = (Console.ReadLine() ?? "").Trim();
                if (input.Equals("SAIR", StringComparison.OrdinalIgnoreCase))
                {
                    break; 
                }
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue; 
                }
                ComprarCasa(input);
            }
        }
        
        private void VerPropriedades()
        {
            if (!jogoIniciado) 
            {
                if (jogadores.Count == 0)
                {
                    Console.WriteLine("Erro: Sem jogadores registados.");
                    Console.Write("Pressione Enter para continuar...");
                    Console.ReadLine();
                    return;
                }
            }
            var jogador = JogadorAtual ?? jogadores.FirstOrDefault();
            if (jogador == null) { return; } 
            Console.WriteLine($"\n--- Propriedades de {jogador.Nome} (${jogador.Dinheiro}) ---");
            var props = ObterPropriedadesDoJogador(jogador);
            if (!props.Any())
            {
                Console.WriteLine("  (Nenhuma propriedade.)");
            }
            else
            {
                var grupos = props.GroupBy(p => p.Cor ?? "Outros").OrderBy(g => g.Key); 
                foreach (var grupo in grupos)
                {
                    Console.WriteLine($"\n  Grupo: {grupo.Key}");
                    foreach (var p in grupo)
                    {
                        string status;
                        if (p.Hipotecado)
                        {
                            status = "(HIPOTECADO)";
                        }
                        else
                        {
                            status = (p.NivelCasa > 0) ? $"(Nível {p.NivelCasa})" : "(sem casas)";
                        }
                        
                        string precoCasaStr = (p.Cor != null && !p.Hipotecado) ? $" (Casa: ${p.PrecoCasa})" : ""; 
                        Console.WriteLine($"    - [{p.Nome}] {status}{precoCasaStr}");
                    }
                }
            }
            Console.Write("\n  Pressione Enter para continuar...");
            Console.ReadLine();
        }
        
        private void ListarEstatisticas()
        {
            Console.WriteLine($"\n--- Estatísticas dos Jogadores ---");
            var jogadoresOrdenados = ObterJogadoresOrdenados();
            if (!jogadoresOrdenados.Any())
            {
                Console.WriteLine("  (Sem jogadores registados)");
            }
            else
            {
                Console.WriteLine($"  {"Nome", -15} | {"Jogos", -7} | {"Vitórias", -9} | {"Empates", -8} | {"Derrotas", -8}");
                Console.WriteLine("  ---------------------------------------------------------------------");
                foreach (var j in jogadoresOrdenados)
                {
                    Console.WriteLine($"  {j.Nome, -15} | {j.Jogos, -7} | {j.Vitorias, -9} | {j.Empates, -8} | {j.Derrotas, -8}");
                }
            }
            Console.Write("\n  Pressione Enter para continuar...");
            Console.ReadLine();
        }
        
        // --- Métodos de Fim de Jogo, Pagamento, Prisão ---

        private void TentarSairDaPrisao(Jogador jogador)
        {
            jogador.TurnosPreso++;
            Console.WriteLine($"\n--- Turno de {jogador.Nome} (Na Prisão - Tentativa {jogador.TurnosPreso}/3) ---");
            Console.WriteLine("A lançar dados para tentar sair (precisa de valores iguais)...");
            
            DiceResult resultado = diceRoller.Roll();
            Console.WriteLine($"  Lançou: X={resultado.HorizontalMove}, Y={resultado.VerticalMove}");

            if (resultado.IsDoubles)
            {
                Console.WriteLine("  Valores iguais! Você está livre!");
                jogador.EstaPreso = false;
                jogador.TurnosPreso = 0;
                jogadorJaLancouDadosEsteTurno = false; 
                Console.WriteLine("  Pode agora lançar os dados (LD) para se mover.");
                return; 
            }
            
            if (jogador.TurnosPreso >= 3)
            {
                Console.WriteLine("  Terceira tentativa falhada. Você deve sair.");
                Console.WriteLine("  Pague $50 de multa para sair.");
                
                jogador.EstaPreso = false;
                jogador.TurnosPreso = 0;
                
                if (TentarPagar(jogador, 50, "multa da prisão"))
                {
                    AdicionarDinheiroFreePark(50);
                    Console.WriteLine("  Multa paga. Pode lançar os dados (LD) no próximo turno.");
                }
                
                jogadorJaLancouDadosEsteTurno = true; // Acabou o turno
                return;
            }

            Console.WriteLine("  Não saíram valores iguais. Fica na prisão.");
            jogadorJaLancouDadosEsteTurno = true; // Acabou o turno
        }

        public bool TentarPagar(Jogador jogador, int valor, string motivo)
        {
            if (valor <= 0) return true; 

            if (jogador.Dinheiro >= valor)
            {
                jogador.Dinheiro -= valor;
                return true;
            }
            else
            {
                Console.WriteLine($"  {jogador.Nome} não tem ${valor} para pagar ({motivo})!");
                Console.WriteLine("  FALÊNCIA!");
                ExecutarDesistencia(jogador, false); 
                return false;
            }
        }

        public void ExecutarDesistencia(Jogador jogador, bool voluntaria)
        {
            if (jogador == null || !jogador.EstaEmJogo) return; 

            if (voluntaria)
            {
                Console.WriteLine($"\n--- {jogador.Nome} pondera DESISTIR ---");
                string resposta = "";
                while (true)
                {
                    Console.Write($"  Tem a certeza que quer desistir do jogo, {jogador.Nome}? (S/N) > ");
                    resposta = (Console.ReadLine() ?? "").Trim().ToUpper();
                    if (resposta == "S" || resposta == "N") break;
                    Console.WriteLine("  Resposta inválida. Por favor, digite 'S' (Sim) ou 'N' (Não).");
                }
                if (resposta == "N")
                {
                    Console.WriteLine("  A desistência foi cancelada. O jogo continua!");
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine();
                    return; 
                }
                Console.WriteLine($"  {jogador.Nome} desistiu e está fora deste jogo.");
            }
            else
            {
                 Console.WriteLine($"  {jogador.Nome} faliu e está fora deste jogo.");
            }

            jogador.EstaEmJogo = false;
            jogador.Jogos++;
            jogador.Derrotas++;
            
            DevolverBensAoBanco(jogador);
            Console.WriteLine($"  Todas as propriedades de {jogador.Nome} foram devolvidas ao banco.");

            var jogadoresRestantes = jogadores.Where(j => j.EstaEmJogo).ToList();
            
            if (jogadoresRestantes.Count < 2)
            {
                if (jogadoresRestantes.Count == 1)
                {
                    TerminarJogoComVitoria(jogadoresRestantes[0]);
                }
                else
                {
                    Console.WriteLine("  Não restam jogadores. O jogo será reiniciado.");
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine();
                    ResetarJogo();
                }
            }
            else
            {
                Console.WriteLine($"  Restam {jogadoresRestantes.Count} jogadores.");
                
                if (jogador == JogadorAtual)
                {
                    Console.WriteLine($"\n--- Fim do turno de {jogador.Nome} ---");
                    AvancarParaProximoJogador();
                    jogadorJaLancouDadosEsteTurno = false;
                    Console.WriteLine($"Próximo a jogar: {JogadorAtual.Nome}");
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine();
                }
            }
        }

        private void DevolverBensAoBanco(Jogador jogador)
        {
            var propsDoJogador = espacosComerciais.Values.Where(e => e.Dono == jogador);
            foreach (var espaco in propsDoJogador)
            {
                espaco.Dono = null;
                espaco.NivelCasa = 0;
                espaco.Hipotecado = false;
            }
        }

        private void AvancarParaProximoJogador()
        {
            int totalJogadores = jogadores.Count;
            int jogadoresAtivos = jogadores.Count(j => j.EstaEmJogo);

            if (jogadoresAtivos == 0) return; 

            do
            {
                indiceJogadorAtual = (indiceJogadorAtual + 1) % totalJogadores;
            } 
            while (!jogadores[indiceJogadorAtual].EstaEmJogo); 
        }

        private void TerminarJogoComVitoria(Jogador vencedor)
        {
            Console.WriteLine($"\n--- FIM DE JOGO ---");
            Console.WriteLine($"Todos os outros jogadores saíram.");
            Console.WriteLine($"🎉 {vencedor.Nome} é o VENCEDOR! 🎉");
            
            vencedor.Jogos++;
            vencedor.Vitorias++;
            
            Console.Write("A reiniciar o tabuleiro... Pressione Enter...");
            Console.ReadLine();
            
            ResetarJogo();
        }

        private void IniciarVotacaoEmpate()
        {
            var proponente = JogadorAtual;
            var outrosJogadores = ObterOutrosJogadores(proponente); 

            if (!outrosJogadores.Any())
            {
                Console.WriteLine("Não pode propor um empate se estiver a jogar sozinho.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"\n--- VOTAÇÃO DE EMPATE ---");
            Console.WriteLine($"O jogador {proponente.Nome} propôs um empate.");
            Console.WriteLine("Todos os outros jogadores ativos devem aceitar.");

            foreach (var votante in outrosJogadores)
            {
                string resposta = "";
                while (true)
                {
                    Console.Write($"\n  {votante.Nome}, aceita o empate? (S/N) > ");
                    resposta = (Console.ReadLine() ?? "").Trim().ToUpper();
                    if (resposta == "S" || resposta == "N") break;
                    Console.WriteLine("  Resposta inválida. Por favor, digite 'S' (Sim) ou 'N' (Não).");
                }

                if (resposta == "N")
                {
                    Console.WriteLine($"  {votante.Nome} recusou o empate. O jogo continua!");
                    Console.Write("  Pressione Enter para continuar...");
                    Console.ReadLine();
                    return; 
                }
                else
                {
                    Console.WriteLine($"  {votante.Nome} aceitou o empate.");
                }
            }

            Console.WriteLine($"\nTodos os jogadores aceitaram a proposta!");
            TerminarJogoComEmpate();
        }

        private void TerminarJogoComEmpate()
        {
            Console.WriteLine("O jogo terminou em empate!");
            foreach (var jogador in jogadores)
            {
                if (jogador.EstaEmJogo)
                {
                    jogador.Jogos++;
                    jogador.Empates++;
                }
            }
            
            Console.Write("A reiniciar o tabuleiro... Pressione Enter...");
            Console.ReadLine();
            
            ResetarJogo();
        }

        private void ResetarJogo()
        {
            jogoIniciado = false;
            indiceJogadorAtual = 0;
            jogadorJaLancouDadosEsteTurno = false; 
            dinheiroFreePark = 0;

            foreach (var jogador in jogadores)
            {
                jogador.PosicaoX = 0;
                jogador.PosicaoY = 0;
                jogador.Dinheiro = 1500;
                jogador.EstaEmJogo = true; 
                jogador.EstaPreso = false; 
                jogador.TurnosPreso = 0; 
            }

            foreach (var espaco in espacosComerciais.Values.Where(e => e.Dono != null))
            {
                espaco.Dono = null;
                espaco.NivelCasa = 0;
                espaco.Hipotecado = false;
            }
            
            Console.WriteLine("Tabuleiro reiniciado.");
            Console.WriteLine("Podem registar novos jogadores (RJ) ou iniciar um novo jogo (IJ).");
        }
        
        public void MoverJogadorPara(Jogador jogador, string nomeCasa)
        {
            const int CentroTabuleiro = 3; 

            Tuple<int, int> coordsMatriz = board.GetSpaceCoords(nomeCasa);
            
            int newRow_Matriz = coordsMatriz.Item1; 
            int newCol_Matriz = coordsMatriz.Item2; 
            
            jogador.PosicaoY = newRow_Matriz - CentroTabuleiro;
            jogador.PosicaoX = newCol_Matriz - CentroTabuleiro;
            
            Console.WriteLine($"  {jogador.Nome} moveu-se para ({jogador.PosicaoX}, {jogador.PosicaoY}) [{nomeCasa}]");
        }
        
        // --- NOVOS MÉTODOS: Vender Casas e Hipotecar ---

        private void MenuVenderCasas()
        {
            if (!vendaCasasAtiva)
            {
                Console.WriteLine("Erro: A funcionalidade 'Vender Casas' está desligada.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            var jogador = JogadorAtual;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"--- Menu de Venda de Casas ({jogador.Nome} | Saldo: ${jogador.Dinheiro}) ---");
                
                var elegiveis = new List<EspacoComercial>();
                var minhasProps = ObterPropriedadesDoJogador(jogador).Where(p => !string.IsNullOrEmpty(p.Cor));

                foreach (var prop in minhasProps)
                {
                    if (prop.NivelCasa == 0) continue; 
                    
                    var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(prop.Cor);
                    int nivelMaximoNoGrupo = 0;
                    foreach(string nomePropIrma in nomesPropsDoGrupo)
                    {
                        int nivelIrma = espacosComerciais[nomePropIrma].NivelCasa;
                        if (nivelIrma > nivelMaximoNoGrupo) nivelMaximoNoGrupo = nivelIrma;
                    }

                    if (prop.NivelCasa == nivelMaximoNoGrupo)
                    {
                        elegiveis.Add(prop);
                    }
                }

                if (elegiveis.Count == 0)
                {
                    Console.WriteLine("\n  Nenhuma propriedade elegível para vender casas.");
                    Console.WriteLine("  (Lembre-se: deve vender uniformemente,");
                    Console.WriteLine("  começando pela propriedade com mais casas no grupo.)");
                }
                else
                {
                    Console.WriteLine("\n  Propriedades elegíveis para vender casas:");
                    foreach (var prop in elegiveis.OrderByDescending(p => p.NivelCasa).ThenBy(p => p.Cor))
                    {
                        int valorVenda = prop.PrecoCasa / 2;
                        Console.WriteLine($"  - [{prop.Nome}] ({prop.Cor}) | Nível: {prop.NivelCasa} | Recebe: ${valorVenda}");
                    }
                }

                Console.WriteLine("\n  Digite o nome da propriedade para vender (ex: Brown1)");
                Console.WriteLine("  Ou digite 'SAIR' para voltar ao jogo.");
                Console.Write("  > ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (input.Equals("SAIR", StringComparison.OrdinalIgnoreCase)) break;
                if (string.IsNullOrWhiteSpace(input)) continue; 
                
                VenderCasa(input);
            }
        }

        private void VenderCasa(string nomePropriedade)
        {
            var jogador = JogadorAtual;
            if (!espacosComerciais.TryGetValue(nomePropriedade, out var espaco))
            {
                Console.WriteLine($"Erro: Propriedade '{nomePropriedade}' não encontrada.");
            }
            else if (espaco.Dono != jogador)
            {
                Console.WriteLine($"Erro: Você não é o dono de [{espaco.Nome}].");
            }
            else if (espaco.NivelCasa == 0)
            {
                Console.WriteLine($"Erro: [{espaco.Nome}] não tem casas para vender.");
            }
            else
            {
                var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(espaco.Cor);
                int nivelMaximoNoGrupo = 0;
                foreach(string nomePropIrma in nomesPropsDoGrupo)
                {
                    int nivelIrma = espacosComerciais[nomePropIrma].NivelCasa;
                    if (nivelIrma > nivelMaximoNoGrupo) nivelMaximoNoGrupo = nivelIrma;
                }

                if (espaco.NivelCasa < nivelMaximoNoGrupo)
                {
                    Console.WriteLine($"Erro: Deve vender uniformemente. Venda primeiro de outra propriedade do grupo '{espaco.Cor}'.");
                }
                else
                {
                    int valorVenda = espaco.PrecoCasa / 2;
                    jogador.Dinheiro += valorVenda;
                    espaco.NivelCasa--;
                    Console.WriteLine($"Casa vendida de [{espaco.Nome}] por ${valorVenda}.");
                    Console.WriteLine($"Novo Nível: {espaco.NivelCasa}. Saldo atual: ${jogador.Dinheiro}.");
                }
            }
            
            Console.Write("\nPressione Enter para continuar...");
            Console.ReadLine();
        }

        private void MenuHipoteca()
        {
            if (!hipotecasAtivas)
            {
                Console.WriteLine("Erro: A funcionalidade 'Hipotecas' está desligada.");
                Console.Write("Pressione Enter para continuar...");
                Console.ReadLine();
                return;
            }

            var jogador = JogadorAtual;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"--- Menu de Gestão de Hipotecas ({jogador.Nome} | Saldo: ${jogador.Dinheiro}) ---");

                var minhasProps = ObterPropriedadesDoJogador(jogador).ToList();
                
                var elegiveisHipoteca = minhasProps.Where(p => !p.Hipotecado).ToList();
                Console.WriteLine("\n  Propriedades para HIPOTECAR (H [Nome]):");
                if (!elegiveisHipoteca.Any())
                {
                    Console.WriteLine("    (Nenhuma)");
                }
                foreach (var prop in elegiveisHipoteca)
                {
                    bool casasNoGrupo = false;
                    if (!string.IsNullOrEmpty(prop.Cor))
                    {
                        var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(prop.Cor);
                        casasNoGrupo = nomesPropsDoGrupo.Any(nomeIrma => espacosComerciais[nomeIrma].NivelCasa > 0);
                    }
                    
                    if (casasNoGrupo)
                    {
                        Console.WriteLine($"    - [{prop.Nome}] (Deve vender todas as casas deste grupo primeiro)");
                    }
                    else
                    {
                        Console.WriteLine($"    - [{prop.Nome}] | Recebe: ${prop.ValorHipoteca}");
                    }
                }

                var elegiveisResgate = minhasProps.Where(p => p.Hipotecado).ToList();
                Console.WriteLine("\n  Propriedades para RESGATAR (R [Nome]):");
                if (!elegiveisResgate.Any())
                {
                    Console.WriteLine("    (Nenhuma)");
                }
                foreach (var prop in elegiveisResgate)
                {
                    int valorResgate = (int)(prop.ValorHipoteca * 1.10); // Valor + 10% juros
                    Console.WriteLine($"    - [{prop.Nome}] | Custo: ${valorResgate}");
                }

                Console.WriteLine("\n  Use 'H Nome', 'R Nome' ou 'SAIR'.");
                Console.Write("  > ");
                string input = (Console.ReadLine() ?? "").Trim();
                
                if (input.Equals("SAIR", StringComparison.OrdinalIgnoreCase)) break;

                string[] partes = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (partes.Length != 2)
                {
                    Console.WriteLine("  Comando inválido. Use 'H NomeDaProp' ou 'R NomeDaProp'.");
                }
                else
                {
                    string comando = partes[0].ToUpper();
                    string nomeProp = partes[1];
                    if (comando == "H") HipotecarPropriedade(nomeProp);
                    else if (comando == "R") ResgatarHipoteca(nomeProp);
                    else Console.WriteLine("  Comando inválido. Use 'H' ou 'R'.");
                }
                
                if (!input.Equals("SAIR", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Write("\nPressione Enter para continuar...");
                    Console.ReadLine();
                }
            }
        }

        private void HipotecarPropriedade(string nomePropriedade)
        {
            var jogador = JogadorAtual;
            if (!espacosComerciais.TryGetValue(nomePropriedade, out var espaco))
            {
                Console.WriteLine($"Erro: Propriedade '{nomePropriedade}' não encontrada.");
                return;
            }
            if (espaco.Dono != jogador)
            {
                Console.WriteLine($"Erro: Você não é o dono de [{espaco.Nome}].");
                return;
            }
            if (espaco.Hipotecado)
            {
                Console.WriteLine($"Erro: [{espaco.Nome}] já está hipotecado.");
                return;
            }

            if (!string.IsNullOrEmpty(espaco.Cor))
            {
                var nomesPropsDoGrupo = EspacoComercial.ObterPropriedadesDoGrupo(espaco.Cor);
                if (nomesPropsDoGrupo.Any(nomeIrma => espacosComerciais[nomeIrma].NivelCasa > 0))
                {
                    Console.WriteLine($"Erro: Deve vender todas as casas do grupo '{espaco.Cor}' antes de hipotecar.");
                    return;
                }
            }
            
            jogador.Dinheiro += espaco.ValorHipoteca;
            espaco.Hipotecado = true;
            Console.WriteLine($"Propriedade [{espaco.Nome}] hipotecada. Você recebeu ${espaco.ValorHipoteca}.");
            Console.WriteLine($"Novo Saldo: ${jogador.Dinheiro}");
        }

        private void ResgatarHipoteca(string nomePropriedade)
        {
            var jogador = JogadorAtual;
            if (!espacosComerciais.TryGetValue(nomePropriedade, out var espaco))
            {
                Console.WriteLine($"Erro: Propriedade '{nomePropriedade}' não encontrada.");
                return;
            }
            if (espaco.Dono != jogador)
            {
                Console.WriteLine($"Erro: Você não é o dono de [{espaco.Nome}].");
                return;
            }
            if (!espaco.Hipotecado)
            {
                Console.WriteLine($"Erro: [{espaco.Nome}] não está hipotecado.");
                return;
            }

            int valorResgate = (int)(espaco.ValorHipoteca * 1.10);
            
            Console.WriteLine($"Resgatar [{espaco.Nome}] custará ${valorResgate}.");
            if (TentarPagar(jogador, valorResgate, "resgate de hipoteca"))
            {
                espaco.Hipotecado = false;
                Console.WriteLine($"Propriedade [{espaco.Nome}] resgatada!");
                Console.WriteLine($"Novo Saldo: ${jogador.Dinheiro}");
            }
        }


        // --- MÉTODOS DE OPÇÕES DE JOGO (EF) ---
        
        private void MenuFuncionalidadesExtras()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("--- Funcionalidades Extras (Opções de Jogo) ---");
                
                string estadoLeilao = leiloesAtivos ? "LIGADO" : "DESLIGADO";
                Console.WriteLine($"  1. Leilões         (Estado: {estadoLeilao})");
                
                string estadoVenda = vendaCasasAtiva ? "LIGADA" : "DESLIGADA";
                Console.WriteLine($"  2. Venda de Casas  (Estado: {estadoVenda})");
                
                string estadoHipoteca = hipotecasAtivas ? "LIGADA" : "DESLIGADA";
                Console.WriteLine($"  3. Hipotecas       (Estado: {estadoHipoteca})");

                Console.WriteLine("\n  Digite o número da opção para a alterar.");
                Console.WriteLine("  Digite 'SAIR' para voltar.");
                Console.Write("  > ");
                string input = (Console.ReadLine() ?? "").Trim();

                if (input.Equals("SAIR", StringComparison.OrdinalIgnoreCase))
                {
                    break; 
                }

                switch (input)
                {
                    case "1":
                        leiloesAtivos = !leiloesAtivos;
                        break;
                    case "2":
                        vendaCasasAtiva = !vendaCasasAtiva;
                        break;
                    case "3":
                        hipotecasAtivas = !hipotecasAtivas;
                        break;
                    default:
                        Console.WriteLine("  Opção inválida. Pressione Enter para tentar novamente...");
                        Console.ReadLine();
                        break;
                }
            }
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