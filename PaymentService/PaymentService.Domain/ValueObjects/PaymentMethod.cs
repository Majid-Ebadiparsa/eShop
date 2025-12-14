using PaymentService.Domain.Aggregates;

namespace PaymentService.Domain.ValueObjects
{
	public sealed record PaymentMethod(PaymentMethodType Type, string MaskedPan = "", string WalletId = "")
	{
		public static PaymentMethod Card(string maskedPan) => new(PaymentMethodType.CARD, maskedPan);
		public static PaymentMethod Wallet(string walletId) => new(PaymentMethodType.WALLET, WalletId: walletId);
		public static PaymentMethod BankTransfer() => new(PaymentMethodType.BANK_TRANSFER);
	}
}
