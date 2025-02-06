using System;

public class Signature : IQubicBuildPackage
{
    private byte[] bytes = new byte[QubicDefinitions.SIGNATURE_LENGTH];

    public Signature(byte[] data = null)
    {
        if (data != null)
        {
            SetSignature(data);
        }
    }

    public void SetSignature(byte[] bytes)
    {
        this.bytes = bytes;
    }

    public byte[] GetPackageData()
    {
        return bytes;
    }

    public int GetPackageSize()
    {
        return bytes.Length;
    }
}



