using CS.Csharp.CardanoCLI;
using System;

namespace ExampleApp
{
    class Program
    {
        private static readonly string _network = "--testnet-magic 1097911063"; //--mainnet
        private static readonly string _cardano_cli_location = $"/home/azureuser/cardano-node-1.27.0/cardano-cli"; //.exe for windows
        private static readonly string _working_directory = "/home/azureuser/cardano-node-1.27.0";
        static void Main(string[] args)
        {
            var examples = new Examples(_network, _cardano_cli_location, _working_directory);
            examples.TestMintTokens();
        }
    }
}
