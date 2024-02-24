namespace Unichain.P2P.Packets; 

public enum EncryptionMode : byte {
    None,
    AES,
    RSA,
    ECDSA
}
