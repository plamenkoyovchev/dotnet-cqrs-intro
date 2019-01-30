using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using NodaMoney;
using SeparateModels.Domain;
using SeparateModels.Services;

namespace SeparateModels.Commands
{
    public class BuyAdditionalCoverHandler : IRequestHandler<BuyAdditionalCoverCommand, BuyAdditionalCoverResult>
    {
        private IDataStore dataStore;

        public BuyAdditionalCoverHandler(IDataStore dataStore)
        {
            this.dataStore = dataStore;
        }

        public Task<BuyAdditionalCoverResult> Handle(BuyAdditionalCoverCommand request, CancellationToken cancellationToken)
        {
            var policy = dataStore.Policies.WithNumber(request.PolicyNumber);
            var newCover = policy.Product.Covers.WithCode(request.NewCoverCode);
            policy.ExtendCoverage
            (
                request.EffectiveDateOfChange, 
                new CoverPrice(newCover,Money.Euro(request.NewCoverPrice),request.NewCoverPriceUnit)
            );
            var newPolicyVersion = policy.Versions.Last();
            dataStore.CommitChanges();
            
            return Task.FromResult(new BuyAdditionalCoverResult
            {
                PolicyNumber = policy.Number,
                VersionWithAdditionalCoversNumber = newPolicyVersion.VersionNumber
            });
        }
    }
}