using CS.Csharp.CardanoCLI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;


namespace CS.Csharp.CardanoCLI
{
    public class Transactions
    {
        private readonly string _incmd_newline = " ";
        private readonly string _signing_key;
        private readonly CLI _cli;

        public Transactions(string signning_key, CLI cli)
        {
            _signing_key = signning_key;
            _cli = cli;
        }

        public string PrepareTransaction(TransactionParams txParams, long ttl, MintParams mintParams = null)
        {
            return BuildTransaction(txParams, 170000, ttl, mintParams);
        }
       
        public string CalculateMinFee(TransactionParams txParams)
        {
            var cmd = @"transaction calculate-min-fee";
            cmd += _incmd_newline;

            cmd += "--tx-in-count 1";
            cmd += _incmd_newline;

            var outCount = txParams.SendAllTxInAda ? 1 : 2;
            cmd += $"--tx-out-count {outCount}";
            cmd += _incmd_newline;

            cmd += _cli._network;
            cmd += _incmd_newline;

            cmd += $"--tx-body-file {txParams.TxFileName}.raw";
            cmd += _incmd_newline;

            cmd += "--witness-count 0";
            cmd += _incmd_newline;

            cmd += "--protocol-params-file protocol.json";

            var output = _cli.RunCLICommand(cmd);

            return Regex.Replace(output, @"\s", "").Replace("Lovelace", "");
        }

        public string BuildTransaction(TransactionParams txParams, long minFee, long ttl, MintParams mintParams = null)
        {
            var cmd = @"transaction build-raw";
            cmd += _incmd_newline;

            //tx in
            cmd += $"--tx-in {txParams.TxInHash}#{txParams.TxInIx}";
            cmd += _incmd_newline;

            long lovelaceVal = txParams.SendAllTxInAda ? txParams.TxInLovelaceValue - minFee : txParams.LovelaceValue;

            if (mintParams == null)
            {
                //send to - tx out 
                cmd += $"--tx-out {txParams.SendToAddress}+{lovelaceVal}";

                foreach (NativeToken nativeToken in txParams.NativeTokensToSend)
                {
                    cmd += $"+\"{nativeToken.Amount} {nativeToken.TokenFullName}\"";
                }

                cmd += _incmd_newline;

                //return change - fee pays sender
                if (!txParams.SendAllTxInAda)
                {
                    cmd += $"--tx-out {txParams.SenderAddress}+{txParams.TxInLovelaceValue - txParams.LovelaceValue - minFee}";

                    foreach (NativeToken nativeToken in txParams.NativeTokensInUtxo)
                    {
                        var tokenSendingAmount = txParams.NativeTokensToSend.FirstOrDefault(x => x.TokenFullName == nativeToken.TokenFullName)?.Amount;
                        var amount = nativeToken.Amount - (tokenSendingAmount != null ? tokenSendingAmount : 0);
                        if (amount != 0)
                        {
                            cmd += $"+\"{nativeToken.Amount} {nativeToken.TokenFullName}\"";
                        }
                    }
                    cmd += _incmd_newline;
                }
            }
            else
            {
                var policies = new Policies(_cli);
                var policyId = policies.GeneratePolicyId(mintParams.PolicyName);

                cmd += $"--tx-out {txParams.SenderAddress}+{txParams.TxInLovelaceValue - minFee}";

                cmd += $"+\"{mintParams.TokenAmount} {policyId}.{mintParams.TokenName}\"";
                cmd += _incmd_newline;

                cmd += $"--mint=\"{mintParams.TokenAmount} {policyId}.{mintParams.TokenName}\"";
                cmd += _incmd_newline;

                cmd += $"--mint-script-file {mintParams.PolicyName}.script";
                cmd += _incmd_newline;
            }

            if (!String.IsNullOrEmpty(txParams.MetadataFileName))
            {
                cmd += $"--metadata-json-file {txParams.MetadataFileName}";
                cmd += _incmd_newline;
            }

            cmd += $"--ttl {ttl}";
            cmd += _incmd_newline;

            cmd += $"--fee {minFee}";
            cmd += _incmd_newline;

            cmd += $"--out-file {txParams.TxFileName}.raw";

            return _cli.RunCLICommand(cmd);
        }

        public string SignTransaction(TransactionParams txParams)
        {
            var cmd = @"transaction sign";
            cmd += _incmd_newline;

            cmd += $"--tx-body-file {txParams.TxFileName}.raw";
            cmd += _incmd_newline;

            cmd += $"--signing-key-file {_signing_key}";
            cmd += _incmd_newline;

            cmd += _cli._network;
            cmd += _incmd_newline;

            cmd += $"--out-file {txParams.TxFileName}.signed";

            return _cli.RunCLICommand(cmd);
        }

        public string SubmitTransaction(TransactionParams txParams)
        {
            var cmd = @"transaction submit";
            cmd += _incmd_newline;

            cmd += $"--tx-file {txParams.TxFileName}.signed";
            cmd += _incmd_newline;

            cmd += _cli._network;

            return _cli.RunCLICommand(cmd);

        }

        public string GetTxIdBeforeSubmit(TransactionParams txParams)
        {
            var cmd = $"transaction txid --tx-file {txParams.TxFileName}.signed";
            return _cli.RunCLICommand(cmd);
        }
    }
}
