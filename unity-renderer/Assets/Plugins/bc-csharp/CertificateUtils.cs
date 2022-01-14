
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

public class CertificateUtils
{
    private static AsymmetricKeyParameter CreatePrivateKey(string subjectName = "CN=root")
    {
        const int keyStrength = 2048;

        // Generating Random Numbers
        var randomGenerator = new CryptoApiRandomGenerator();
        var random = new SecureRandom(randomGenerator);

        // The Certificate Generator
        var certificateGenerator = new X509V3CertificateGenerator();

        // Serial Number
        var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
        certificateGenerator.SetSerialNumber(serialNumber);

        // Issuer and Subject Name
        var subjectDn = new X509Name(subjectName);
        var issuerDn = subjectDn;
        certificateGenerator.SetIssuerDN(issuerDn);
        certificateGenerator.SetSubjectDN(subjectDn);

        // Valid For
        var notBefore = DateTime.UtcNow.Date;
        var notAfter = notBefore.AddYears(70);

        certificateGenerator.SetNotBefore(notBefore);
        certificateGenerator.SetNotAfter(notAfter);

        // Subject Public Key
        var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

        return subjectKeyPair.Private;
    }

    public static X509Certificate2 CreateSelfSignedCert(string subjectName = "CN=localhost", string issuerName = "CN=root")
    {
        const int keyStrength = 2048;
        var issuerPrivKey = CreatePrivateKey();

        // Generating Random Numbers
        var randomGenerator = new CryptoApiRandomGenerator();
        var random = new SecureRandom(randomGenerator);
        ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", issuerPrivKey, random);
        // The Certificate Generator
        var certificateGenerator = new X509V3CertificateGenerator();
        certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, new GeneralNames(new GeneralName[] { new GeneralName(GeneralName.DnsName, "localhost"), new GeneralName(GeneralName.DnsName, "127.0.0.1") }));
        certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, new ExtendedKeyUsage((new ArrayList() { new DerObjectIdentifier("1.3.6.1.5.5.7.3.1") })));

        // Serial Number
        var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
        certificateGenerator.SetSerialNumber(serialNumber);

        // Signature Algorithm
        //const string signatureAlgorithm = "SHA512WITHRSA";
        //certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);

        // Issuer and Subject Name
        var subjectDn = new X509Name(subjectName);
        var issuerDn = new X509Name(issuerName);
        certificateGenerator.SetIssuerDN(issuerDn);
        certificateGenerator.SetSubjectDN(subjectDn);

        // Valid For
        var notBefore = DateTime.UtcNow.Date;
        var notAfter = notBefore.AddYears(70);

        certificateGenerator.SetNotBefore(notBefore);
        certificateGenerator.SetNotAfter(notAfter);

        // Subject Public Key
        var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        var subjectKeyPair = keyPairGenerator.GenerateKeyPair();

        certificateGenerator.SetPublicKey(subjectKeyPair.Public);

        // self sign certificate
        var certificate = certificateGenerator.Generate(signatureFactory);

        // corresponding private key
        var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(subjectKeyPair.Private);


        // merge into X509Certificate2
        var x509 = new X509Certificate2(certificate.GetEncoded());

        var seq = (Asn1Sequence)Asn1Object.FromByteArray(info.ParsePrivateKey().GetDerEncoded());
        if (seq.Count != 9)
        {
            throw new PemException("malformed sequence in RSA private key");
        }

        var rsa = RsaPrivateKeyStructure.GetInstance(seq); //new RsaPrivateKeyStructure(seq);
        var rsaparams = new RsaPrivateCrtKeyParameters(
            rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent, rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2, rsa.Coefficient);

        x509.PrivateKey = DotNetUtilities.ToRSA(rsaparams);
        return x509;

    }
}