public static class QubicDefinitions
{
    public const int SIGNATURE_LENGTH = 64;
    public const int PUBLIC_KEY_LENGTH = 32;
    public const int MAX_TRANSACTION_SIZE = 1024;
    public const int DIGEST_LENGTH = 32;
    public const int SPECTRUM_DEPTH = 24;
    public const int NUMBER_OF_TRANSACTIONS_PER_TICK = 1024;
    public const int MAX_NUMBER_OF_CONTRACTS = 1024;
    public const string EMPTY_ADDRESS = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
    public const string QX_ADDRESS = "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID";
    public const string ARBITRATOR = "AFZPUAIYVPNUYGJRQVLUKOPPVLHAZQTGLYAAUUNBXFTVTAMSBKQBLEIEPCVJ";
    public const int QX_TRANSFER_ASSET_FEE = 100; // 100 Qubic's
    public const int QX_ISSUE_ASSET_FEE = 1000000000; // 1b Qubic's
    public const int QX_ISSUE_ASSET_INPUT_TYPE = 1; // input type for a tx to issue an asset
    public const int QX_TRANSFER_ASSET_INPUT_TYPE = 2; // input type for a tx to transfer an asset
    public const int QX_ADD_ASK_ORDER = 5; // input type for a tx to create an ask order
    public const int QX_ADD_BID_ORDER = 6; // input type for a tx to create a bid order
    public const int QX_REMOVE_ASK_ORDER = 7; // input type for a tx to remove an ask order
    public const int QX_REMOVE_BID_ORDER = 8; // input type for a tx to remove a bid order

    /* QUTIL SC */
    public const string QUTIL_ADDRESS = "EAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAVWRF";
    public const int QUTIL_SENDMANY_INPUT_TYPE = 1; // input type for send many on Qutil
    public const int QUTIL_SENDMANY_FEE = 10; // fee in qubics for send many
}
