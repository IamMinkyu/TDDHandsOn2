using System.Reactive.Disposables;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Orders.Events;

namespace Orders.Messaging;

internal sealed class StorageQueueBus: IBus<PaymentApproved>, IAsyncObservable<PaymentApproved>
{
  private readonly QueueClient _client;

  public StorageQueueBus(QueueClient client) => this._client = client;
  
  public Task Send(PaymentApproved message)
    => _client.SendMessageAsync(BinaryData.FromObjectAsJson(message));


  public IDisposable Subscribe(Func<PaymentApproved?, Task> onNext)
  {
    bool listening = true;
    Run();
    return Disposable.Create(() => listening = false);
    
    async void Run()
    {
      await _client.CreateIfNotExistsAsync();
      while (listening)
      {
        QueueMessage[] messages = await _client.ReceiveMessagesAsync();
        foreach(QueueMessage message in messages)
        {
          if (listening)
          {
            if (message.Body is not null)
            {
              await onNext.Invoke(message.Body.ToObjectFromJson<PaymentApproved?>());
              await _client.DeleteMessageAsync(message.MessageId, message.PopReceipt);
            }
            
          }
        }
      }
    }
  }
}
