using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace DynamoCrypto
{
    /// <summary>
    /// 该类包含用于在用户或本地证书存储中查找Dynamo证书的静态方法。
    /// 还有使用公钥/私钥对文件进行签名或验证的方法。
    /// 这些方法使用DSA算法进行验证。
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 查找证书,并返回私钥
        /// </summary>
        /// <param name="keyContainerName">The key container name.</param>
        /// <param name="certificate">An X509Certificate2 object containing a private key.</param>
        /// <returns>A byte array of the private key.</returns>
        public static byte[] GetPrivateKeyFromCertificate(X509Certificate2 certificate)
        {
            byte[] privateBlob;

            if (certificate.HasPrivateKey)
            {
                var dsa = certificate.PrivateKey as DSACryptoServiceProvider;

                if (dsa == null)
                {
                    Console.WriteLine("There was an error getting the private key from the certificate.");
                    return null;
                }

                privateBlob = dsa.ExportCspBlob(true);
                dsa.Dispose();
            }
            else
            {
                Console.WriteLine("The certificate does not contain a private key.");
                return null;
            }

            return privateBlob;
        }

        /// <summary>
        /// 在证书存储中查找证书并返回公钥
        /// </summary>
        /// <param name="keyContainerName">The key container name.</param>
        /// <param name="certificate">A X509Certificate2 object containing a public key.</param>
        /// <returns>A byte array of the the public key or null if the certificate does not contain a public key.</returns>
        public static byte[] GetPublicKeyFromCertificate(X509Certificate2 certificate)
        {
            var dsa = certificate.PublicKey.Key as DSACryptoServiceProvider;

            if (dsa == null)
            {
                Console.WriteLine("There was an error getting the public key from the certificate.");
                return null;
            }

            byte[] publicBlob = dsa.ExportCspBlob(false);
            dsa.Dispose();

            return publicBlob;
        }

        /// <summary>
        /// 使用私钥生成签名文件
        /// </summary>
        /// <param name="filePath">目标文件</param>
        /// <param name="signatureFilePath">将要生成的签名文件</param>
        /// <param name="privateBlob">私钥</param>
        public static void SignFile(string filePath, string signatureFilePath, byte[] privateBlob)
        {  
            try
            {
                if (privateBlob.Length == 0)
                {
                    throw new Exception("The specified private key is invalid.");
                }

                byte[] hash = null;

                using (Stream fileStream = File.Open(filePath, FileMode.Open))
                {
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    hash = sha1.ComputeHash(fileStream);
                }

                // Import the private key
                var dsa = new DSACryptoServiceProvider();
                dsa.ImportCspBlob(privateBlob);
                var rsaFormatter = new DSASignatureFormatter(dsa);
                rsaFormatter.SetHashAlgorithm("SHA1");

                // Create a signature based on the private key
                byte[] signature = rsaFormatter.CreateSignature(hash);

                // Write the signature file
                File.WriteAllBytes(signatureFilePath, signature);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// 使用签名文件和公钥进行文件验证
        /// </summary>
        /// <param name="filePath">目标文件</param>
        /// <param name="signatureFilePath">签名文件</param>
        /// <param name="publicBlob">公钥</param>
        /// <returns>验证是否通过</returns>
        public static bool VerifyFile(string filePath, string signatureFilePath, byte[] publicBlob)
        {
            if (publicBlob.Length == 0)
                return false;

            bool verified = false;
            byte[] hash = null;

            try
            {
                // Compute a hash of the installer
                using (Stream fileStream = File.Open(filePath, FileMode.Open))
                {
                    SHA1 sha1 = new SHA1CryptoServiceProvider();
                    hash = sha1.ComputeHash(fileStream);
                }

                // Import the public key
                var dsa = new DSACryptoServiceProvider();
                dsa.ImportCspBlob(publicBlob);

                var dsaDeformatter = new DSASignatureDeformatter(dsa);
                dsaDeformatter.SetHashAlgorithm("SHA1");

                // Read the signature file
                byte[] signature = File.ReadAllBytes(signatureFilePath);

                // Verify the signature against the hash of the installer
                verified = dsaDeformatter.VerifySignature(hash, signature);

                Console.WriteLine("File verified: {0}", verified);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);

                return false;
            }

            return verified;
        }

        /// <summary>
        /// 在密钥存储区中查找证书
        /// </summary>
        /// <param name="keyContainerName">The key container name.</param>
        /// <returns>An X509Certificate2 or null if no certificate can be found.</returns>
        public static X509Certificate2 FindCertificateForCurrentUser(string keyContainerName, StoreLocation location)
        {
            // Look for the Dynamo certificate in the certificate store. 
            // http://stackoverflow.com/questions/6304773/how-to-get-x509certificate-from-certificate-store-and-generate-xml-signature-dat
            var store = new X509Store(location);
            store.Open(OpenFlags.ReadOnly);
            var cers = store.Certificates.Find(X509FindType.FindBySubjectName, keyContainerName, false);

            X509Certificate2 cer = null;
            if (cers.Count == 0)
            {
                Console.WriteLine("The certificate could not be found in the certificate store.");
                return null;
            }

            cer = cers[0];
            return cer;
        }

        /// <summary>
        /// 在本地证书存储区中安装证书
        /// </summary>
        /// <param name="certPath">The installed certificate.</param>
        public static X509Certificate2 InstallCertificateForCurrentUser(string certPath)
        {
            var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            var cert = new X509Certificate2(certPath);
            store.Add(cert);
            return cert;
        }
    }
}
