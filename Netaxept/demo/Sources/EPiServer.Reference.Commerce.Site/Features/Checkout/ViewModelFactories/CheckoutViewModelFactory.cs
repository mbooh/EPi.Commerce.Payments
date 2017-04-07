using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Reference.Commerce.Site.Features.AddressBook.Services;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Cart.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Checkout.Pages;
using EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Payment.PaymentMethods;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModelFactories;
using EPiServer.Reference.Commerce.Site.Features.Payment.ViewModels;
using EPiServer.Reference.Commerce.Site.Features.Shared.Models;
using EPiServer.Reference.Commerce.Site.Features.Start.Pages;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EPiServer.Reference.Commerce.Site.Features.Checkout.ViewModelFactories
{
    [ServiceConfiguration(typeof(CheckoutViewModelFactory), Lifecycle = ServiceInstanceScope.Singleton)]
    public class CheckoutViewModelFactory
    {
        readonly LocalizationService _localizationService;
        readonly PaymentMethodViewModelFactory _paymentMethodViewModelFactory;
        readonly IAddressBookService _addressBookService;
        readonly IContentLoader _contentLoader;
        readonly UrlResolver _urlResolver;
        readonly ServiceAccessor<HttpContextBase> _httpContextAccessor;
        readonly ShipmentViewModelFactory _shipmentViewModelFactory;

        public CheckoutViewModelFactory(
            LocalizationService localizationService,
            PaymentMethodViewModelFactory paymentMethodViewModelFactory,
            IAddressBookService addressBookService,
            IContentLoader contentLoader,
            UrlResolver urlResolver,
            ServiceAccessor<HttpContextBase> httpContextAccessor,
            ShipmentViewModelFactory shipmentViewModelFactory)
        {
            _localizationService = localizationService;
            _paymentMethodViewModelFactory = paymentMethodViewModelFactory;
            _addressBookService = addressBookService;
            _contentLoader = contentLoader;
            _urlResolver = urlResolver;
            _httpContextAccessor = httpContextAccessor;
            _shipmentViewModelFactory = shipmentViewModelFactory;
        }

        public virtual CheckoutViewModel CreateCheckoutViewModel(ICart cart, CheckoutPage currentPage, IPaymentMethodViewModel<PaymentMethodBase> paymentViewModel = null)
        {
            if (cart == null)
            {
                return CreateEmptyCheckoutViewModel(currentPage);
            }

            var shipments = _shipmentViewModelFactory.CreateShipmentsViewModel(cart).ToList();

            var viewModel = new CheckoutViewModel
            {
                CurrentPage = currentPage,
                Shipments = shipments,
                BillingAddress = CreateBillingAddressModel(),
                UseBillingAddressForShipment = shipments.Count == 1 && _addressBookService.UseBillingAddressForShipment(),
                StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                AppliedCouponCodes = cart.GetFirstForm().CouponCodes.Distinct(),
                AvailableAddresses = new List<AddressModel>(),
                ReferrerUrl = GetReferrerUrl(),
                Payment = paymentViewModel,
            };

            UpdatePayment(viewModel);
                        
            var availableAddresses = GetAvailableAddresses();

            if (availableAddresses.Any())
            {
                viewModel.AvailableAddresses.Add(new AddressModel { Name = _localizationService.GetString("/Checkout/MultiShipment/SelectAddress") });
                
                foreach (var address in availableAddresses)
                {
                    viewModel.AvailableAddresses.Add(address);
                }
            }
            else
            {
                viewModel.AvailableAddresses.Add(new AddressModel { Name = _localizationService.GetString("/Checkout/MultiShipment/NoAddressFound") });
            }

            SetDefaultShipmentAddress(viewModel);

            _addressBookService.LoadAddress(viewModel.BillingAddress);
            PopulateCountryAndRegions(viewModel);

            return viewModel;
        }

        private void SetDefaultShipmentAddress(CheckoutViewModel viewModel)
        {
            if (viewModel.Shipments.Count == 1)
            {
                viewModel.Shipments[0].Address = viewModel.AvailableAddresses.SingleOrDefault(x => x.ShippingDefault) ?? viewModel.BillingAddress;
            }
        }

        private IList<AddressModel> GetAvailableAddresses()
        {
            var addresses = _addressBookService.List();
            foreach (var address in addresses.Where(x => string.IsNullOrEmpty(x.Name)))
            {
                address.Name = _localizationService.GetString("/Shared/Address/DefaultAddressName");
            }

            return addresses;
        }

        private CheckoutViewModel CreateEmptyCheckoutViewModel(CheckoutPage currentPage)
        {
            return new CheckoutViewModel
            {
                CurrentPage = currentPage,
                Shipments = new List<ShipmentViewModel>(),
                AppliedCouponCodes = new List<string>(),
                StartPage = _contentLoader.Get<StartPage>(ContentReference.StartPage),
                AvailableAddresses = new List<AddressModel>(),
                PaymentMethodViewModels = Enumerable.Empty<PaymentMethodViewModel<PaymentMethodBase>>(),
                UseBillingAddressForShipment = true
            };
        }

        private void PopulateCountryAndRegions(CheckoutViewModel viewModel)
        {
            foreach (var shipment in viewModel.Shipments)
            {
                _addressBookService.LoadCountriesAndRegionsForAddress(shipment.Address);
            }
        }

        private void UpdatePayment(CheckoutViewModel viewModel)
        {
            viewModel.PaymentMethodViewModels = _paymentMethodViewModelFactory.GetPaymentMethodViewModels();
            var selectedPaymentMethod = viewModel.Payment == null ? 
                viewModel.PaymentMethodViewModels.First() :
                viewModel.PaymentMethodViewModels.Single(p => p.SystemName == viewModel.Payment.SystemName);

            viewModel.Payment = PaymentMethodViewModelResolver.Resolve(selectedPaymentMethod.SystemName);
            viewModel.Payment.Description = selectedPaymentMethod.Description;
            viewModel.Payment.SystemName = selectedPaymentMethod.SystemName;
            viewModel.Payment.PaymentMethod.PaymentMethodId = selectedPaymentMethod.PaymentMethodId;
        }

        private AddressModel CreateBillingAddressModel()
        {
            var preferredBillingAddress = _addressBookService.GetPreferredBillingAddress();

            return new AddressModel
            {
                AddressId = preferredBillingAddress != null ? preferredBillingAddress.Name : null,
                Name = preferredBillingAddress != null ? preferredBillingAddress.Name : Guid.NewGuid().ToString(),
            };
        }

        private string GetReferrerUrl()
        {
            var httpContext = _httpContextAccessor();
            if (httpContext.Request.UrlReferrer != null &&
                httpContext.Request.UrlReferrer.Host.Equals(httpContext.Request.Url.Host, StringComparison.OrdinalIgnoreCase))
            {
                return httpContext.Request.UrlReferrer.ToString();
            }
            return _urlResolver.GetUrl(ContentReference.StartPage);
        }
    }
}