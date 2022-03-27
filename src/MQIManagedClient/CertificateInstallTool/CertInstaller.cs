using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace CertificateInstallTool
{
    internal class CertInstaller
    {
        public static void SetupCert(string file, string password, string label)
        {
            Log.Information($"Cert check");
            if (string.IsNullOrEmpty(label) || string.IsNullOrEmpty(file)) return;

            var pathToPfx = Path.Combine(".", file);
            Log.Information($"Reading .p12 file '{pathToPfx}' ...");
            var certContent = File.ReadAllBytes(pathToPfx);
            Log.Information("Cert file is loaded");

            Log.Information($"Opening cert My store ...");
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
            {
                Log.Information($"Opened cert My store");
                var cert = new X509Certificate2(certContent, password);
                var clientAuthCerts = GetClientAuthCerts(store.Certificates, label);
                foreach (var installedCert in clientAuthCerts)
                {
                    if (installedCert.Thumbprint == cert.Thumbprint) continue;
                    store.Remove(installedCert);
                    Log.Information($"Uninstalled client certificate {installedCert.Thumbprint}");
                }

                if (!IsInstalled(store.Certificates, cert.Thumbprint))
                {
                    Log.Information($"Installing client auth cert to My store ...");
                    Log.Information("Adding cert to store collection ...");
                    store.Add(cert);
                    Log.Information($"Installed client certificate {cert.Thumbprint}");
                }
            }

            Log.Information($"Opening cert Root store ...");
            using (var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser, OpenFlags.ReadWrite))
            {
                Log.Information($"Opened cert Root store");
                var clientAuthCerts = GetClientAuthCerts(store.Certificates, label);
                foreach (var installedCert in clientAuthCerts)
                {
                    store.Remove(installedCert);
                    Log.Information($"Uninstalled client certificate {installedCert.Thumbprint}");
                }

                var collection2 = new X509Certificate2Collection();
                Log.Information("Load cert chain with keys ...");
                collection2.Import(certContent, password, X509KeyStorageFlags.PersistKeySet);
                Log.Information("Storing certs chain ...");
                foreach (var cert in collection2)
                {
                    if (cert.FriendlyName == label || IsInstalled(store.Certificates, cert.Thumbprint)) continue;
                    store.Add(cert);
                    Log.Information($"Installed chain certificate {cert.Thumbprint}");
                }
            }
            Log.Information($"Cert check passed");
        }

        private static X509Certificate2[] GetClientAuthCerts(X509Certificate2Collection certs, string certLabel) =>
            certs.Cast<X509Certificate2>()
                .Where(cert => string.Equals(cert.FriendlyName, certLabel, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();

        private static bool IsInstalled(X509Certificate2Collection certs, string thumbprint) => certs.Cast<X509Certificate2>().Any(cert => cert.Thumbprint == thumbprint);
    }
}
