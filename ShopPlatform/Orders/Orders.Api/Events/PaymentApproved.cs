namespace Orders.Events;

public sealed record PaymentApproved(
  string PaymentTransactionId,
  DateTime EventTimeUtc);
