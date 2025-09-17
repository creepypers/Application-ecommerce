using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace TPECOM.Services
{
    public class StripeService 
    {
        private readonly string _secretKey;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            _logger = logger;
            _secretKey = configuration["Stripe:SecretKey"];
            
            if (string.IsNullOrEmpty(_secretKey))
            {
                _logger.LogError("La clé secrète Stripe n'est pas configurée dans appsettings.json");
                throw new InvalidOperationException("La clé secrète Stripe n'est pas configurée");
            }
            
            _logger.LogInformation("Initialisation de Stripe avec la clé: {SecretKeyPrefix}...", _secretKey.Substring(0, 8));
            
            // Configurer la clé API globale de Stripe
            StripeConfiguration.ApiKey = _secretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency = "cad")
        {
            try
            {
                _logger.LogInformation("Création d'un PaymentIntent pour {Amount} {Currency}", amount, currency);
                
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Conversion en centimes
                    Currency = currency,
                    PaymentMethodTypes = new List<string> { "card" },
                    CaptureMethod = "automatic"
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);
                
                _logger.LogInformation("PaymentIntent créé avec succès: {PaymentIntentId}", intent.Id);
                
                return intent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création du PaymentIntent");
                throw;
            }
        }

        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.GetAsync(paymentIntentId);
        }

        public async Task<PaymentIntent> UpdatePaymentIntentAsync(string paymentIntentId, decimal amount)
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = (long)(amount * 100) 
            };

            var service = new PaymentIntentService();
            return await service.UpdateAsync(paymentIntentId, options);
        }

        public async Task<PaymentIntent> CancelPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.CancelAsync(paymentIntentId);
        }

        public async Task<PaymentIntent> ConfirmPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            return await service.ConfirmAsync(paymentIntentId);
        }
    }
} 