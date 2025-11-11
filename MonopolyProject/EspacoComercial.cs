// Nome do Ficheiro: EspacoComercial.cs
using System.Collections.Generic;
using Resisto_dos_jogadores; 
using System; // <-- MUDANÇA 1: Adicionar 'using System' para Console.ReadLine()

namespace MonopolyGameLogic
{
    // ... (O enum ResultadoCompra fica igual) ...
    public enum ResultadoCompra
    {
        Sucesso,
        JaTemDono,
        SemDinheiroSuficiente
    }

    public class EspacoComercial
    {
        // ... (O Dicionário Estático PrecosBase fica igual) ...
        private static readonly Dictionary<string, int> PrecosBase = new()
        {
            // Browns
            { "Brown1", 100 }, { "Brown2", 120 },
            // ... (etc, todos os preços) ...
            { "Electric Company", 120 }, { "Water Works", 120 }
        };

        // ... (Propriedades, Construtor, e Métodos Estáticos ficam iguais) ...
        public string Nome { get; }
        public int Preco { get; } 
        public SistemaJogo.Jogador Dono { get; set; }

        public EspacoComercial(string nome, int preco)
        {
            Nome = nome;
            Preco = preco; 
            Dono = null; 
        }
        
        public static bool EspacoEComercial(string nome)
        {
            return PrecosBase.ContainsKey(nome);
        }

        public static IReadOnlyDictionary<string, int> ObterPrecosBase()
        {
            return PrecosBase;
        }

        // ... (O método TentarComprar fica igual, mas agora é usado "internamente") ...
        public ResultadoCompra TentarComprar(SistemaJogo.Jogador comprador)
        {
            if (this.Dono != null)
            {
                return ResultadoCompra.JaTemDono;
            }
            if (comprador.Dinheiro < this.Preco)
            {
                return ResultadoCompra.SemDinheiroSuficiente;
            }
            
            comprador.Dinheiro -= this.Preco;
            this.Dono = comprador;
            return ResultadoCompra.Sucesso;
        }


        // <-- MUDANÇA 2: NOVO MÉTODO PÚBLICO COM TODA A LÓGICA DE INTERAÇÃO -->
        public void AterrarNoEspaco(SistemaJogo.Jogador jogador)
        {
            // Lógica que estava ANTES em 'LancarDados', agora está AQUI.

            // 1. Verificar se tem dono
            if (this.Dono == null)
            {
                // 2. Verificar se tem dinheiro
                if (jogador.Dinheiro >= this.Preco)
                {
                    // 3. Perguntar ao jogador
                    Console.WriteLine($"  Gostaria de comprar [{this.Nome}] por ${this.Preco}? (S/N)");
                    Console.Write("  > ");
                    string resposta = Console.ReadLine().Trim().ToUpper();

                    if (resposta == "S")
                    {
                        // 4. Tentar comprar (chamando o outro método desta classe)
                        ResultadoCompra resultadoCompra = TentarComprar(jogador);
                        if (resultadoCompra == ResultadoCompra.Sucesso)
                        {
                            Console.WriteLine($"  Espaço comprado! O seu novo saldo é ${jogador.Dinheiro}.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("  Decidiu não comprar o espaço.");
                    }
                }
                else // Não tem dinheiro
                {
                    Console.WriteLine($"  Você aterrou em [{this.Nome}] (${this.Preco}), mas não tem dinheiro suficiente para comprar.");
                }
                
                Console.Write("  Pressione Enter para continuar...");
                Console.ReadLine();
            }
            else // Espaço já tem dono
            {
                Console.WriteLine($"  Você aterrou em [{this.Nome}], que pertence a {this.Dono.Nome}.");
                // (Aqui entraria a lógica de pagar renda no futuro)
                Console.Write("  Pressione Enter para continuar...");
                Console.ReadLine();
            }
        }
    }
}