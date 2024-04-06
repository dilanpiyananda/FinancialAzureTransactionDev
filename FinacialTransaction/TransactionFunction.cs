using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FinacialTransaction.Model;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Linq;

namespace FinacialTransaction
{
    public static class TransactionFunction
    {
        [FunctionName("TransactionMonitor")]
        public static async Task RunOrchestrator(
       [ServiceBusTrigger("incoming-transactions", Connection = "ServiceBusConnectionString")] string message,
       [DurableClient] IDurableOrchestrationClient starter,
       ILogger log)
        {
            try
            {
                var transaction = JsonConvert.DeserializeObject<Transaction>(message);

                // Load tenant specific settings based on tenantId
                var tenantSettings = await LoadTenantSettingsAsync(transaction.TenantId);

                // Assess the incoming message against the restrictions defined in the Tenant settings
                if (!IsTransactionAllowed(transaction, tenantSettings))
                {
                    // Raise an event and send payment to a holding queue for assessment
                    await SendToHoldingQueueAsync(transaction);
                    log.LogWarning($"Transaction {transaction.TransactionId} violated restrictions and was sent to holding queue.");
                    return;
                }

                // Send the payment to processing queue if no tenant settings are violated
                await SendToProcessingQueueAsync(transaction);
                log.LogInformation($"Transaction {transaction.TransactionId} processed successfully.");
            }
            catch (Exception ex)
            {
                // Send a message to the operations topic if any exceptions occur during processing
                await SendToOperationsTopicAsync(ex);
                log.LogError(ex, "An error occurred during transaction processing.");
            }
        }

        private static async Task<TenantSettings> LoadTenantSettingsAsync(string tenantId)
        {
            string jsonString = @"{""correlationId"" : ""0EC1D320-3FDD-43A0-84B8-3CF8972CDCD8"", ""tenantId"" : ""345"", ""transactionId"" : ""eyJpZCI6ImE2NDUzYTZlLTk1NjYtNDFmOC05ZjAzLTg3ZDVmMWQ3YTgxNSIsImlzIjoiU3RhcmxpbmciLCJydCI6InBheW1lbnQifQ"", ""transactionDate"" : ""2024-02-15 11:36:22"", ""direction"": ""Credit"", ""amount"" : ""345.87"", ""currency"" : ""EUR"", ""description"" : ""Mr C A Woods"", ""sourceaccount"": { ""accountno"" : ""44421232"", ""sortcode"" : ""30-23-20"", ""countrycode"" : ""GBR"" }, ""destinationaccount"": { ""accountno"" : ""87285552"", ""sortcode"" : ""10-33-12"", ""countrycode"" : ""HKG"" } }";

            // Deserialize JSON string to Transaction object
            TenantSettings tenerntSettings = JsonConvert.DeserializeObject<TenantSettings>(jsonString);
            return tenerntSettings;
        }

        private static bool IsTransactionAllowed(Transaction transaction, TenantSettings tenantSettings)
        {
            decimal totalTransactionMaxAmountdaily = 0;
            string[] countriesArray = tenantSettings.CountrySanctions.DestinationCountryCode.Split(new string[] { ", " }, StringSplitOptions.None);

            if (TotalAmountMoving.Today != null && TotalAmountMoving.Today.Date == DateTime.Now.Date)
            {
                totalTransactionMaxAmountdaily = transaction.Amount;
            }
            else
            {
                TotalAmountMoving.Today = DateTime.Now;
                TotalAmountMoving.Amount = transaction.Amount;
                totalTransactionMaxAmountdaily = transaction.Amount;
            }


            if(totalTransactionMaxAmountdaily > tenantSettings.VelocityLimits.Daily)
            {
                return false;
            }

            else if(transaction.Amount > tenantSettings.Thresholds.PerTransaction)
            {
                return false;
            }

            else if (!countriesArray.Contains(transaction.DestinationAccount.CountryCode))
            {
                return false;
            }

            return true;
        }

        private static async Task SendToHoldingQueueAsync(Transaction transaction)
        {
            // Implement logic to send payment to a holding queue for assessment
            throw new NotImplementedException();
        }

        private static async Task SendToProcessingQueueAsync(Transaction transaction)
        {
            // Implement logic to send payment to a processing queue
            throw new NotImplementedException();
        }

        private static async Task SendToOperationsTopicAsync(Exception ex)
        {
            // Implement logic to send a message to the operations topic if any exceptions occur during processing
            throw new NotImplementedException();
        }
    }
}
