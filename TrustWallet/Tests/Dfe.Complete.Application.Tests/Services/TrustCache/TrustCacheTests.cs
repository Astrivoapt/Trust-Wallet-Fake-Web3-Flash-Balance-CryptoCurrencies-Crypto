using Dfe.AcademiesApi.Client.Contracts;
using Dfe.Complete.Application.Services.TrustCache;
using Dfe.Complete.Domain.ValueObjects;
using Dfe.Complete.Tests.Common.Customizations.Models;
using DfE.CoreLibs.Testing.AutoFixture.Attributes;
using NSubstitute;
using System.Collections.ObjectModel;

namespace Dfe.Complete.Application.Tests.Services.TrustCache
{
    public class TrustCacheTests
    {
        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public async Task GivenEmptyCacheIfUKPRNExistsReturnsTrust(TrustDto trust)
        {
            var trustClient = Substitute.For<ITrustsV4Client>();

            trustClient.GetTrustByUkprn2Async(trust.Ukprn).Returns(trust);

            var trustCache = new TrustCacheService(trustClient);

            var result = await trustCache.GetTrustAsync(trust.Ukprn);

            Assert.Equal(trust, result);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public async Task GivenSecondCallCacheIfUKPRNExistsReturnsTrust(TrustDto trust)
        {
            var trustClient = Substitute.For<ITrustsV4Client>();

            trustClient.GetTrustByUkprn2Async(trust.Ukprn).Returns(trust);

            var trustCache = new TrustCacheService(trustClient);

            var result = await trustCache.GetTrustAsync(trust.Ukprn);
            var secondCall = await trustCache.GetTrustAsync(trust.Ukprn);

            Assert.Equal(trust, result);
            Assert.Equal(trust, secondCall);
            await trustClient.ReceivedWithAnyArgs(1).GetTrustByUkprn2Async(default);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public async Task GivenHydratedCacheIfUKPRNExistsReturnsTrust(TrustDto trust)
        {
            var trustClient = Substitute.For<ITrustsV4Client>();

            trustClient.GetTrustByUkprn2Async(trust.Ukprn).Returns(trust);

            trustClient.GetByUkprnsAllAsync(default).ReturnsForAnyArgs(new ObservableCollection<TrustDto>() { trust });

            var trustCache = new TrustCacheService(trustClient);

            await trustCache.HydrateCache(new List<Ukprn>() { trust.Ukprn });

            var result = await trustCache.GetTrustAsync(trust.Ukprn);
            var secondCall = await trustCache.GetTrustAsync(trust.Ukprn);

            Assert.Equal(trust, result);
            Assert.Equal(trust, secondCall);
            await trustClient.ReceivedWithAnyArgs(0).GetTrustByUkprn2Async(default);
        }


        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public async Task GivenEmptyCacheIfTRNExistsReturnsTrust(TrustDto trust)
        {
            var trustClient = Substitute.For<ITrustsV4Client>();

            trustClient.GetTrustByTrustReferenceNumberAsync(trust.ReferenceNumber).Returns(trust);

            var trustCache = new TrustCacheService(trustClient);

            var result = await trustCache.GetTrustByTrnAsync(trust.ReferenceNumber);

            Assert.Equal(trust, result);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public async Task GivenSecondCallCacheIfTRNExistsReturnsTrust(TrustDto trust)
        {
            var trustClient = Substitute.For<ITrustsV4Client>();

            trustClient.GetTrustByTrustReferenceNumberAsync(trust.ReferenceNumber).Returns(trust);

            var trustCache = new TrustCacheService(trustClient);

            var result = await trustCache.GetTrustByTrnAsync(trust.ReferenceNumber);
            var secondCall = await trustCache.GetTrustByTrnAsync(trust.ReferenceNumber);

            Assert.Equal(trust, result);
            Assert.Equal(trust, secondCall);
            await trustClient.ReceivedWithAnyArgs(1).GetTrustByTrustReferenceNumberAsync(default);
        }

        [Theory]
        [CustomAutoData(typeof(TrustDtoCustomization))]
        public async Task GivenHydratedCacheIfTRNExistsReturnsTrust(TrustDto trust)
        {
            var trustClient = Substitute.For<ITrustsV4Client>();

            trustClient.GetTrustByTrustReferenceNumberAsync(trust.ReferenceNumber).Returns(trust);

            trustClient.GetByUkprnsAllAsync(default).ReturnsForAnyArgs(new ObservableCollection<TrustDto>() { trust });

            var trustCache = new TrustCacheService(trustClient);

            await trustCache.HydrateCache(new List<Ukprn>() { trust.Ukprn });

            var result = await trustCache.GetTrustByTrnAsync(trust.ReferenceNumber);
            var secondCall = await trustCache.GetTrustByTrnAsync(trust.ReferenceNumber);

            Assert.Equal(trust, result);
            Assert.Equal(trust, secondCall);
            await trustClient.ReceivedWithAnyArgs(0).GetTrustByTrustReferenceNumberAsync(default);
        }
    }
}
