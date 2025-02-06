using System;
using System.Linq;
using System.Threading.Tasks;

public class PublicKey : IQubicBuildPackage
{
    private byte[] bytes = new byte[QubicDefinitions.PUBLIC_KEY_LENGTH];
    private string identity;

    public PublicKey(string identity = null)
    {
        if (identity != null)
        {
            SetIdentityFromString(identity);
        }
    }

    public PublicKey(byte[] identity)
    {
        SetIdentity(identity);
    }

    public void SetIdentityFromString(string id)
    {
        identity = id;
        SetIdentity(KeyHelper.GetIdentityBytes(id));
    }

    public async Task SetIdentity(byte[] bytes)
    {
        this.bytes = bytes;
        identity = await new QubicHelper().GetIdentity(bytes);
    }

    public byte[] GetIdentity()
    {
        return bytes;
    }

    public string GetIdentityAsString()
    {
        return identity;
    }

    public int GetPackageSize()
    {
        return bytes.Length;
    }

    public byte[] GetPackageData()
    {
        return bytes;
    }

    public bool Equals(PublicKey compare)
    {
        return compare != null && bytes.Length == compare.bytes.Length && bytes.SequenceEqual(compare.bytes);
    }

    public async Task<bool> VerifyIdentity()
    {
        return await new QubicHelper().VerifyIdentity(identity);
    }
}


