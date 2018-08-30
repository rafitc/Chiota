﻿namespace Chiota.Messenger.Usecase.GetApprovedContacts
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  using Chiota.Messenger.Cache;
  using Chiota.Messenger.Comparison;
  using Chiota.Messenger.Entity;
  using Chiota.Messenger.Repository;

  using Newtonsoft.Json;

  using Tangle.Net.Entity;
  using Tangle.Net.Repository;

  /// <summary>
  ///   The get approved contacts interactor.
  /// </summary>
  public class
    GetApprovedContactsInteractor : IUsecaseInteractor<GetApprovedContactsRequest, GetApprovedContactsResponse>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="GetApprovedContactsInteractor"/> class.
    /// </summary>
    /// <param name="contactRepository">
    /// The contact repository.
    /// </param>
    /// <param name="transactionCache">
    /// The transaction cache.
    /// </param>
    /// <param name="iotaRepository">
    /// The iota repository.
    /// </param>
    public GetApprovedContactsInteractor(
      IContactRepository contactRepository,
      ITransactionCache transactionCache,
      IIotaRepository iotaRepository)
    {
      this.ContactRepository = contactRepository;
      this.TransactionCache = transactionCache;
      this.IotaRepository = iotaRepository;
    }

    /// <summary>
    /// Gets the contact repository.
    /// </summary>
    private IContactRepository ContactRepository { get; }

    /// <summary>
    /// Gets the iota repository.
    /// </summary>
    private IIotaRepository IotaRepository { get; }

    /// <summary>
    /// Gets the transaction cache.
    /// </summary>
    private ITransactionCache TransactionCache { get; }

    /// <inheritdoc />
    public async Task<GetApprovedContactsResponse> ExecuteAsync(GetApprovedContactsRequest request)
    {
      var contactRequests = await this.LoadContactRequests(request.ContactRequestAddress);
      var approvedContacts = await this.ContactRepository.LoadContactsAsync(request.PublicKeyAddress.Value);

      return new GetApprovedContactsResponse
               {
                 Contacts = approvedContacts.Union(
                   contactRequests,
                   new ChatAdressComparer()).ToList()
               };
    }

    /// <summary>
    /// The load contacts from tangle.
    /// </summary>
    /// <param name="address">
    /// The address.
    /// </param>
    /// <returns>
    /// The <see cref="Task"/>.
    /// </returns>
    private async Task<List<Contact>> LoadContactRequests(Address address)
    {
      var contacts = new List<Contact>();
      var cachedTransactionHashes = new List<Hash>();

      var cachedTransactions = await this.TransactionCache.LoadTransactionsByAddressAsync(address);
      foreach (var cachedTransaction in cachedTransactions)
      {
        cachedTransactionHashes.Add(cachedTransaction.TransactionHash);
        contacts.Add(JsonConvert.DeserializeObject<Contact>(cachedTransaction.TransactionTrytes.ToUtf8String()));
      }

      var transactions = await this.IotaRepository.FindTransactionsByAddressesAsync(new List<Address> { address });
      var newHashes = cachedTransactionHashes.Intersect(transactions.Hashes, new TryteComparer<Hash>()).ToList();

      foreach (var hash in newHashes)
      {
        var bundle = await this.IotaRepository.GetBundleAsync(hash);

        foreach (var message in bundle.GetMessages())
        {
          await this.TransactionCache.SaveTransactionAsync(
            new TransactionCacheItem
              {
                Address = address,
                TransactionHash = hash,
                TransactionTrytes = TryteString.FromUtf8String(message)
              });

          contacts.Add(JsonConvert.DeserializeObject<Contact>(message));
        }
      }

      return contacts;
    }
  }
}