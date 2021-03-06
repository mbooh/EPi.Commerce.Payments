﻿using System.Collections.Generic;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Models;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using Geta.Epi.Commerce.Payments.Resurs.Checkout.Bussiness;
using Geta.Resurs.Checkout.Model;
using Mediachase.Commerce.Website;

namespace EPiServer.Reference.Commerce.Site.Features.Payment.Models
{
    public class ResursBankCheckoutViewModel : PaymentMethodViewModel<ResursCheckoutPaymentGateway>
    {
        public StartPage StartPage { get; set; }
        public CheckoutPage CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets all available payment methods that the customer can choose from.
        /// </summary>
        public IEnumerable<PaymentMethodViewModel<IPaymentOption>> PaymentMethodViewModels { get; set; }

        /// <summary>
        /// Gets ors sets all available shipping method that can be associated to a shipping address.
        /// </summary>
        public IEnumerable<ShippingMethodViewModel> ShippingMethodViewModels { get; set; }

        public string ReferrerUrl { get; set; }

        /// <summary>
        /// Gets a list of all existing addresses for the current customer and that can be used for billing and shipment.
        /// </summary>
        public IList<ShippingAddress> AvailableAddresses { get; set; }

        /// <summary>
        /// Gets or sets the billing address.
        /// </summary>
        public ShippingAddress BillingAddress { get; set; }

        /// <summary>
        /// Gets or sets all shipping addresses for the current purchase.
        /// </summary>
        public IList<ShippingAddress> ShippingAddresses { get; set; }

        /// <summary>
        /// Gets or sets the payment method associated to the current purchase.
        /// </summary>
        public IPaymentMethodViewModel<IPaymentOption> Payment { get; set; }
        public bool UseBillingAddressForShipment { get; set; }
        public List<PaymentMethodResponse> ResursBankPaymentMethods { get; set; }
        public string CardNumber { get; set; }
        public string ResursPaymentMethod { get; set; }
        public string GovernmentId { get; set; }
        public decimal AmountForNewCard { get; set; }
        public decimal MinLimit { get; set; }
        public decimal MaxLimit { get; set; }

        public string DeliveryType { get; set; }

    }
}