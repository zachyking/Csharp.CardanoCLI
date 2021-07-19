using CS.Csharp.CardanoCLI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Csharp.CardanoCLI
{
    public class Examples
    {
        public readonly string _network; //= "--testnet-magic 1097911063"; //--mainnet
        public readonly string _cardano_cli_location; //= $"/home/azureuser/cardano-node-1.27.0/cardano-cli"; //.exe for windows
        public readonly string _working_directory; //= "/home/azureuser/cardano-node-1.27.0";
        private readonly CLI cli;

        public Examples(string network, string cardano_cli_path, string working_dir)
        {
            _network = network;
            _working_directory = working_dir;
            _cardano_cli_location = cardano_cli_path;
            cli = new CLI(_network, _cardano_cli_location, _working_directory);
        }


        public void TestMintTokens()
        {
            var mintParams = new MintParams
            {
                PolicyName = "testpolicy",
                TokenAmount = 1000,
                TokenName = "CSTEST"
            };

            var policyParams = new PolicyParams
            {
                PolicyName = "testpolicy",
                TimeLimited = true,
                ValidForMinutes = 20,
                SigningKeyFile = "signing-key",
                VerificationKeyFile = "verification-key"
            };

            var txParams = new TransactionParams()
            {
                TxFileName = $"testpolicy",
                LovelaceValue = 5000000,
                SendAllTxInAda = false,
                SenderAddress = "addr_test1vrw3r08naaq8wrtemegjk7p3e9zp7a2ceul9rd84pd3nckcynl6xq",
                SendToAddress = "addr_test1vrw3r08naaq8wrtemegjk7p3e9zp7a2ceul9rd84pd3nckcynl6xq",
                TxInLovelaceValue = 989650078,
                TxInHash = "37626db011baf6c4900bd8fb1a010fea3003b19067f88c46c391c77e4c4f5948",
                TxInIx = 1
            };

            Assets assets = new Assets(cli);

            Console.Write(assets.MintNativeTokens(policyParams, mintParams, txParams));

        }

        public void TestCreatePolicy()
        {
            Policies policies = new Policies(cli);

            var policyParams = new PolicyParams
            {
                PolicyName = "testpolicy1",
                TimeLimited = true,
                ValidForMinutes = 20
            };

            var policy = policies.Create(policyParams);

            if (string.IsNullOrEmpty(policy.PolicyKeyHash))
            {
                Console.WriteLine("error");
            }
            else
            {
                Console.WriteLine("success");
            }
        }

        public void TestTransactionWithTokens()
        {
            var txParams = new TransactionParams()
            {
                TxFileName = $"tx-tokens",
                LovelaceValue = 2000000,
                SendAllTxInAda = false,
                SenderAddress = "addr_test1vrw3r08naaq8wrtemegjk7p3e9zp7a2ceul9rd84pd3nckcynl6xq",
                SendToAddress = "addr_test1vpl22c6vml7p7n5vv4n2mjf6sfw9kcse5c7jjk3uxc9dllcvvvj8q",
                TxInLovelaceValue = 989477405,
                TxInHash = "e1c85be256a393cc341ba0353e257d921544892e8a6529a38e5204e1bab4a73e",
                TxInIx = 0,
                NativeTokensInUtxo = new List<NativeToken>(){
                    new NativeToken{
                        Amount = 1000,
                        TokenFullName = "1986a6fba600525df58ad520bedaba94a8e7a297ea929a23cf230376.CSTEST"
                    }
                },
                NativeTokensToSend = new List<NativeToken>(){
                    new NativeToken{
                        Amount = 1000,
                        TokenFullName = "1986a6fba600525df58ad520bedaba94a8e7a297ea929a23cf230376.CSTEST"
                    }
                },
                SigningKeyFile = "signing-key"
            };
            Transaction(txParams);
        }

        public void TestTransaction()
        {
            var txParams = new TransactionParams()
            {
                TxFileName = $"tx2",
                LovelaceValue = 5000000,
                SendAllTxInAda = false,
                SenderAddress = "addr_test1vrw3r08naaq8wrtemegjk7p3e9zp7a2ceul9rd84pd3nckcynl6xq",
                SendToAddress = "addr_test1vpl22c6vml7p7n5vv4n2mjf6sfw9kcse5c7jjk3uxc9dllcvvvj8q",
                TxInLovelaceValue = 989477405,
                TxInHash = "7b8b5e3141b1239bf69e7513e599babc02a204602952abcac2fea226563712ab",
                TxInIx = 1
            };
            Transaction(txParams);
        }


        public void Transaction(TransactionParams txParams)
        {
            var ttl = cli.QueryTip().Slot + 100;

            var transactions = new Transactions(txParams.SigningKeyFile, cli);

            var f = transactions.PrepareTransaction(txParams, ttl);
            Console.WriteLine(f);
            if (!f.StartsWith("CS.Error"))
            {
                var protocolParams = cli.SetProtocolParamaters();
                if (!cli.HasError(protocolParams))
                {
                    var minFee = transactions.CalculateMinFee(txParams);
                    if (!cli.HasError(minFee))
                    {
                        Console.WriteLine(minFee);
                        var buildTx = transactions.BuildTransaction(txParams, (long)Convert.ToInt64(minFee.Replace(" Lovelace", "")), ttl);
                        if (!cli.HasError(buildTx))
                        {
                            var signTx = transactions.SignTransaction(txParams);
                            if (!cli.HasError(signTx))
                            {
                                var submit = transactions.SubmitTransaction(txParams);
                                Console.WriteLine(submit);
                                if (!cli.HasError(submit))
                                {
                                    Console.WriteLine("Success!");
                                }
                            }
                            else
                            {
                                Console.WriteLine("SIGN ERROR: " + signTx);
                            }
                        }
                        else
                        {
                            Console.WriteLine("BUILD ERROR: " + buildTx);
                        }
                    }
                    else
                    {
                        Console.WriteLine("FEE CALC ERROR: " + minFee);
                    }
                }
                else
                {
                    Console.WriteLine("PROTOCOL PARAMS ERROR: " + protocolParams);
                }
            }
            else
            {
                Console.WriteLine("PREPARE ERROR: " + f);
            }
        }

    }
}
