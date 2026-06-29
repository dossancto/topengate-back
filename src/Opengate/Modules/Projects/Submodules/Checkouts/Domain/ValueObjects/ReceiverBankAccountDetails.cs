using Opengate.Modules.Projects.Submodules.Checkouts.Domain.Enums;

namespace Opengate.Modules.Projects.Submodules.Checkouts.Domain.ValueObjects;

public record ReceiverBankAccountDetails
{
    public string BankName { get; private init; } = string.Empty;

    public BankAccountType BankAccountType { get; private init; }

    public string Name { get; private init; } = string.Empty;

    public string Document { get; private init; } = string.Empty;

    public string DocumentType { get; private init; } = string.Empty;

    public string AccountNumber { get; private init; } = string.Empty;

    public string Agency { get; private init; } = string.Empty;

    public PixKeyType KeyType { get; private init; }

    public string PixKey { get; private init; }

    public ReceiverBankAccountDetails(
            string bankName,
            BankAccountType bankAccountType,
            string name,
            string document,
            string documentType,
            string accountNumber,
            string agency,
            PixKeyType keyType,
            string pixKey
            )
    {
        // TODO: Apply numbers only regex to sanitize the document
        var numbersOnlyDocument = document.Replace("-", "");

        BankName = bankName;
        BankAccountType = bankAccountType;
        Name = name;
        Document = numbersOnlyDocument;
        DocumentType = documentType.ToUpperInvariant();
        AccountNumber = accountNumber;
        Agency = agency;
        KeyType = keyType;
        PixKey = pixKey;
    }

    public ReceiverBankAccountDetails() : this(
        bankName: "",
        bankAccountType: BankAccountType.Personal,
        name: "",
        document: "",
        documentType: "",
        accountNumber: "",
        agency: "",
        keyType: PixKeyType.Email,
        pixKey: ""
    )
    { }
}