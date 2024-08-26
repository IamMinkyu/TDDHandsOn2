namespace Orders.Events;

public sealed record ExternalPaymentApproved(
  string tid,
  DateTime approved_at);
