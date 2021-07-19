using CS.Csharp.CardanoCLI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS.Csharp.CardanoCLI
{
    public class Assets
    {
        private readonly CLI _cli;


        public Assets(CLI cli)
        {
            _cli = cli;
        }


        public string MintNativeTokens(PolicyParams pParams, MintParams mintParams, TransactionParams txParams)
        {
            Policies policies = new Policies(_cli);

            var policy = policies.Create(pParams);
            if (string.IsNullOrEmpty(policy.PolicyKeyHash)) { return "Error policy: " + policy; }

            Transactions transactions = new Transactions(pParams.SigningKeyFile, _cli);

            var ttl = _cli.QueryTip().Slot + 120;

            var prepare = transactions.PrepareTransaction(txParams, ttl, mintParams);
            if(_cli.HasError(prepare)) { return "Error prepare: " + prepare;  }
            
            var minFee = transactions.CalculateMinFee(txParams);
            if (_cli.HasError(minFee)) { return "Error minFee: " + prepare; }

            var build = transactions.BuildTransaction(txParams, Int64.Parse(minFee), ttl, mintParams);
            if (_cli.HasError(minFee)) { return "Error build: " + build; }

            var sign = transactions.SignTransaction(txParams);
            if (_cli.HasError(minFee)) { return "Error sign: " + sign; }

            var submit = transactions.SubmitTransaction(txParams);
            if (_cli.HasError(minFee)) { return "Error submit: " + submit; }

            return submit;
        }
    }
}
