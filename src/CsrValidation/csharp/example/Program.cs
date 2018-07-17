﻿using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using Microsoft.Intune;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // If scenario requires the use of a proxy with authentication then you need to supply your own HttpClient like the following.
            string proxyHost = "http://localhost";
            string proxyPort = "8888";
            string proxyUser = "proxyuser";
            string proxyPass = "proxypass";

            var proxy = new WebProxy()
            {
                Address = new Uri($"{proxyHost}:{proxyPort}"),
                UseDefaultCredentials = false,

                // *** These creds are given to the proxy server, not the web server ***
                Credentials = new NetworkCredential(
                    userName: proxyUser,
                    password: proxyPass)
            };

            // Uncomment the following line to use a proxy
            //System.Net.WebRequest.DefaultWebProxy = proxy;

            var validator = new IntuneScepValidator(
                providerNameAndVersion: "",   // A string that uniquely identifies your Certificate Authority and any version info for your app.
                intuneTenant: "",             // Tenant name i.e. contoso.onmicrosoft.com
                azureAppId: "",               // Application ID of Active Directory application in the tenants azure subscription.
                azureAppKey: "",              // Application secret to be used for authentication of tenant.
                trace: new TraceSource("log")
            );

            var transactionId = Guid.NewGuid(); // A GUID that will uniquley identify the entire transaction to allow for log correlation accross Validate and Notification calls.

            // The CSR should be in Base64 encoding format
            string csr = "";

            // This validates the request
            (validator.ValidateRequestAsync(transactionId.ToString(), csr)).Wait();
            Console.WriteLine("No exceptions means CSR was validated!");

            // This notification is to be sent on sucesfull issuance of certificate
            (validator.SendSuccessNotificationAsync(transactionId.ToString(), csr, "certThumbprint", "certSerial", "certExpirationDate", "certIssuingAuthority")).Wait();
            Console.WriteLine("No exceptions means success notification was recorded succesfully!");

            // This notification is to be sent on failure to issue certificate
            (validator.SendFailureNotificationAsync(transactionId.ToString(), csr, 0xFFFF, "Short description of error that occurred.")).Wait();
            Console.WriteLine("No exceptions means failure notification was recorded succesfully!");
        }
    }
}
