// Nome do Ficheiro: EspacoComercial.cs
using System.Collections.Generic;
using Resisto_dos_jogadores; 
using System;
using System.Linq; 

namespace MonopolyGameLogic
{
    public enum ResultadoCompra
    {
        Sucesso,
        JaTemDono,
        SemDinheiroSuficiente
    }

    public class EspacoComercial
    {
        // --- DICIONÁRIOS ESTÁTICOS (COM DADOS) ---
        private static readonly Dictionary<string, int> PrecosBase = new()
        {
            { "Brown1", 100 }, { "Brown2", 120 }, { "Teal1", 90 }, { "Teal2", 130 },
            { "Orange1", 120 }, { "Orange2", 120 }, { "Orange3", 140 },
            { "Black1", 110 }, { "Black2", 120 }, { "Black3", 130 },
            { "Red1", 130 }, { "Red2", 130 }, { "Red3", 160 },
            { "Green1", 120 }, { "Green2", 140 }, { "Green3", 160 },
            { "Blue1", 140 }, { "Blue2", 140 }, { "Blue3", 170 },
            { "Pink1", 160 }, { "Pink2", 180 }, { "White1", 160 }, { "White2", 180 }, { "White3", 190 },
            { "Yellow1", 140 }, { "Yellow2", 140 }, { "Yellow3", 170 },
            { "Violet1", 150 }, { "Violet2", 130 }, { "Train1", 150 }, { "Train2", 150 }, 
            { "Train3", 150 }, { "Train4", 150 }, { "Electric Company", 120 }, { "Water Works", 120 }
        };
 
        private static readonly Dictionary<string, string[]> GruposDeCor = new()
        {
            { "Brown", new[] { "Brown1", "Brown2" } },
            { "Teal", new[] { "Teal1", "Teal2" } },
            { "Orange", new[] { "Orange1", "Orange2", "Orange3" } },
            { "Black", new[] { "Black1", "Black2", "Black3" } },
            { "Red", new[] { "Red1", "Red2", "Red3" } },
            { "Green", new[] { "Green1", "Green2", "Green3" } },
            { "Blue", new[] { "Blue1", "Blue2", "Blue3" } },
            { "Pink", new[] { "Pink1", "Pink2" } },
            { "White", new[] { "White1", "White2", "White3" } },
            { "Yellow", new[] { "Yellow1", "Yellow2", "Yellow3" } },
            { "Violet", new[] { "Violet1", "Violet2" } }
        };

        
        // --- Propriedades da Classe ---
        public string Nome { get; }
        public int Preco { get; } 
        public SistemaJogo.Jogador Dono { get; set; }
        public int NivelCasa { get; set; }
        public int PrecoCasa { get; } 
        public string Cor { get; } 
        public bool Hipotecado { get; set; }
        public int ValorHipoteca { get; }

        
        // Construtor
        public EspacoComercial(string nome, int preco)
        {
            Nome = nome;
            Preco = preco; 
            Dono = null; 
            NivelCasa = 0; 
            PrecoCasa = (int)(Preco * 0.6); 
            Cor = ObterCorDaPropriedade(nome);
            Hipotecado = false;
            ValorHipoteca = Preco / 2; 
        }
        
        
        // --- Métodos Estáticos ---
        public static bool EspacoEComercial(string nome)
        {
            return PrecosBase.ContainsKey(nome);
        }

        public static IReadOnlyDictionary<string, int> ObterPrecosBase()
        {
            return PrecosBase;
        }

        public static string ObterCorDaPropriedade(string nomePropriedade)
        {
            foreach (var par in GruposDeCor)
            {
                if (par.Value.Contains(nomePropriedade))
                {
                    return par.Key; 
                }
            }
            return null; 
        }
        
        public static string[] ObterPropriedadesDoGrupo(string cor)
        {
            return GruposDeCor.ContainsKey(cor) ? GruposDeCor[cor] : null;
        }
        
        
        // --- Métodos de Lógica ---
        
        public ResultadoCompra TentarComprar(SistemaJogo.Jogador comprador)
        {
            if (this.Dono != null) { return ResultadoCompra.JaTemDono; }
            if (comprador.Dinheiro < this.Preco) { return ResultadoCompra.SemDinheiroSuficiente; }
            comprador.Dinheiro -= this.Preco;
            this.Dono = comprador;
            return ResultadoCompra.Sucesso;
        }
        
        public void AterrarNoEspaco(SistemaJogo.Jogador jogador, SistemaJogo sistema)
        {
            if (this.Dono == null)
            {
                if (jogador.Dinheiro >= this.Preco)
                {
                    string resposta = ""; 
                    while (true) 
                    {
                        Console.WriteLine($"  Gostaria de comprar [{this.Nome}] por ${this.Preco}? (S/N)");
                        Console.Write("  > ");
                        resposta = (Console.ReadLine() ?? "").Trim().ToUpper();
                        if (resposta == "S" || resposta == "N") break; 
                        Console.WriteLine("  Resposta inválida. Por favor, digite 'S' (Sim) ou 'N' (Não).");
                    }
                    if (resposta == "S")
                    {
                        if (TentarComprar(jogador) == ResultadoCompra.Sucesso)
                        {
                            Console.WriteLine($"  Espaço comprado! O seu novo saldo é ${jogador.Dinheiro}.");
                        }
                    }
                    else {
                        Console.WriteLine($"  {jogador.Nome} decidiu não comprar o espaço.");
                        ExecutarLogicaDeRecusa(jogador, sistema);
                    }
                }
                else {
                    Console.WriteLine($"  {jogador.Nome} aterrou em [{this.Nome}] (${this.Preco}), mas não tem dinheiro suficiente.");
                    ExecutarLogicaDeRecusa(jogador, sistema);
                }
            }
            else 
            {
                if (this.Dono == jogador)
                {
                    Console.WriteLine($"  Você aterrou em [{this.Nome}], que é a sua propriedade.");
                }
                else
                {
                    if (this.Hipotecado)
                    {
                        Console.WriteLine($"  Você aterrou em [{this.Nome}], que pertence a {this.Dono.Nome}, mas está HIPOTECADO. Não paga renda.");
                    }
                    else
                    {
                        PagarRenda(jogador, this.Dono, sistema);
                    }
                }
            }
            Console.Write("  Pressione Enter para continuar...");
            Console.ReadLine();
        }
        
        private void PagarRenda(SistemaJogo.Jogador inquilino, SistemaJogo.Jogador proprietario, SistemaJogo sistema)
        {
            double rendaBase = this.Preco * 0.25;
            double rendaCasas = this.Preco * 0.75 * this.NivelCasa;
            int valorRenda = (int)(rendaBase + rendaCasas); 
            
            Console.WriteLine($"  Você aterrou em [{this.Nome}], que pertence a {proprietario.Nome}!");
            
            if (this.NivelCasa > 0)
            {
                Console.WriteLine($"  A propriedade tem Nível {this.NivelCasa}. A renda é de ${valorRenda}.");
            }
            else
            {
                Console.WriteLine($"  A renda base é de ${valorRenda}.");
            }

            if (sistema.TentarPagar(inquilino, valorRenda, $"renda a {proprietario.Nome}"))
            {
                proprietario.Dinheiro += valorRenda;
                Console.WriteLine($"  Você pagou ${valorRenda}. O seu saldo é ${inquilino.Dinheiro}.");
                Console.WriteLine($"  {proprietario.Nome} recebeu ${valorRenda}. O saldo dele é ${proprietario.Dinheiro}.");
            }
        }
        
        
        private void ExecutarLogicaDeRecusa(SistemaJogo.Jogador jogadorQueRecusou, SistemaJogo sistema)
        {
            var outrosJogadores = sistema.ObterOutrosJogadores(jogadorQueRecusou); 
            int totalJogadoresAtivos = sistema.ContagemJogadoresAtivos; 

            if (totalJogadoresAtivos == 2)
            {
                var outroJogador = outrosJogadores.FirstOrDefault(); 
                if (outroJogador != null)
                {
                    if (outroJogador.Dinheiro >= this.Preco) { OferecerPropriedade(outroJogador); }
                    else { Console.WriteLine($"  {outroJogador.Nome} não tem dinheiro (${this.Preco}) para comprar. A propriedade continua sem dono."); }
                }
            }
            else if (totalJogadoresAtivos > 2 && sistema.LeiloesAtivos)
            {
                var jogadoresElegiveis = outrosJogadores.Where(j => j.Dinheiro > 0).ToList();
                
                if (jogadoresElegiveis.Count > 0) { IniciarLeilao(jogadoresElegiveis, sistema); }
                else { Console.WriteLine("  Nenhum outro jogador tem dinheiro para o leilão. A propriedade continua sem dono."); }
            }
            else
            {
                if (totalJogadoresAtivos > 2)
                {
                    Console.WriteLine("  Leilões estão desligados. A propriedade continua sem dono.");
                }
            }
        }

        private void OferecerPropriedade(SistemaJogo.Jogador compradorPotencial)
        {
            Console.WriteLine($"\n  A oferta passa para {compradorPotencial.Nome}.");
            string resposta = "";
            while (true)
            {
                Console.WriteLine($"  {compradorPotencial.Nome}, gostaria de comprar [{this.Nome}] por ${this.Preco}? (S/N)");
                Console.Write("  > ");
                resposta = (Console.ReadLine() ?? "").Trim().ToUpper();
                if (resposta == "S" || resposta == "N") break;
                Console.WriteLine("  Resposta inválida. Por favor, digite 'S' (Sim) ou 'N' (Não).");
            }
            if (resposta == "S")
            {
                if (TentarComprar(compradorPotencial) == ResultadoCompra.Sucesso)
                {
                    Console.WriteLine($"  Propriedade comprada por {compradorPotencial.Nome}! Novo saldo: ${compradorPotencial.Dinheiro}.");
                }
            }
            else { Console.WriteLine($"  {compradorPotencial.Nome} recusou. A propriedade continua sem dono."); }
        }

        private void IniciarLeilao(List<SistemaJogo.Jogador> licitantes, SistemaJogo sistema)
        {
            Console.WriteLine($"\n--- LEILÃO INICIADO PARA: {this.Nome} ---");
            Console.WriteLine("Licitantes: " + string.Join(", ", licitantes.Select(j => j.Nome)));
            Console.WriteLine("Digite um valor para licitar (maior que o atual) ou 'P' para passar/desistir.");
            Console.WriteLine("A licitação mínima é $1.");
            int licitacaoAtual = 0;
            SistemaJogo.Jogador maiorLicitante = null;
            var licitantesAtivos = new List<SistemaJogo.Jogador>(licitantes);
            int indiceAtual = 0;
            while (licitantesAtivos.Count > 1)
            {
                var jogadorAtual = licitantesAtivos[indiceAtual];
                Console.WriteLine($"\nLicitação atual: ${licitacaoAtual} (de {maiorLicitante?.Nome ?? "Ninguém"})");
                Console.WriteLine($"Turno de {jogadorAtual.Nome} (${jogadorAtual.Dinheiro}).");
                Console.Write("  > ");
                string input = Console.ReadLine().Trim();
                if (input.Equals("P", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"  {jogadorAtual.Nome} desistiu.");
                    licitantesAtivos.RemoveAt(indiceAtual);
                }
                else if (int.TryParse(input, out int novaLicitacao))
                {
                    if (novaLicitacao > licitacaoAtual && novaLicitacao <= jogadorAtual.Dinheiro && novaLicitacao > 0)
                    {
                        licitacaoAtual = novaLicitacao;
                        maiorLicitante = jogadorAtual;
                        Console.WriteLine($"  {jogadorAtual.Nome} licitou ${licitacaoAtual}!");
                        indiceAtual++; 
                    }
                    else if (novaLicitacao <= licitacaoAtual) { Console.WriteLine($"  Valor inválido.Deve ser MAIOR que ${licitacaoAtual}."); }
                    else if (novaLicitacao <= 0) { Console.WriteLine("  Valor inválido. Deve ser $1 ou mais."); }
                    else { Console.WriteLine($"  Valor inválido. Você só tem ${jogadorAtual.Dinheiro}."); }
                }
                else { Console.WriteLine("  Entrada inválida. Digite um número ou 'P'."); }
                if (indiceAtual >= licitantesAtivos.Count) { indiceAtual = 0; }
            }
            if (licitantesAtivos.Count == 1)
            {
                var vencedor = licitantesAtivos[0];
                if (maiorLicitante == null)
                {
                    if (vencedor.Dinheiro >= 1) 
                    {
                        licitacaoAtual = 1;
                        Console.WriteLine($"  Todos desistiram. {vencedor.Nome} é o único interessado.");
                        Console.WriteLine($"  {vencedor.Nome} pode comprar por $1.");
                        maiorLicitante = vencedor; 
                    }
                    else { Console.WriteLine($"  Todos desistiram. A propriedade continua sem dono."); return; }
                }
                vencedor = maiorLicitante; 
                Console.WriteLine($"--- LEILÃO ENCERRADO ---");
                Console.WriteLine($"  Vendido a {vencedor.Nome} por ${licitacaoAtual}!");
                
                if (sistema.TentarPagar(vencedor, licitacaoAtual, "leilão"))
                {
                    this.Dono = vencedor;
                    Console.WriteLine($"  O novo saldo de {vencedor.Nome} é ${vencedor.Dinheiro}.");
                }
            }
            else { Console.WriteLine($"--- LEILÃO ENCERRADO ---"); Console.WriteLine("  Todos desistiram (ou ninguém era elegível). A propriedade continua sem dono."); }
        }
    }
}