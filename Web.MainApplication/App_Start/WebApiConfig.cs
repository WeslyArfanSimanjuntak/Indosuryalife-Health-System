using Repository.Application.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;

namespace Web.MainApplication
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            //builder.EntitySet<DeliveryRequest>("DeliveryRequestsOData");
            //builder.EntitySet<ARMADA>("ARMADAs");
            builder.EntitySet<AspNetGroups>("AspNetGroups");
            builder.EntitySet<AspNetGroupUser>("AspNetGroupUser");
            builder.EntitySet<AspNetRoleGroup>("AspNetRoleGroup");
            builder.EntitySet<AspNetRoles>("AspNetRoles");
            builder.EntitySet<AspNetUsers>("AspNetUsers");
            //builder.EntitySet<BATCH>("BATCH");
            //builder.EntitySet<BatchMix>("BatchMix");
            //builder.EntitySet<BatchProduct>("BatchProduct");
            //builder.EntitySet<BatchMixProduct>("BatchMixProduct");
            //builder.EntitySet<CLIENT>("CLIENT");
            //builder.EntitySet<CONTRACT>("CONTRACT");
            //builder.EntitySet<ContractProduct>("ContractProduct");
            //builder.EntitySet<DeliveryOrder>("DeliveryOrder");
            //builder.EntitySet<DeliveryOrderInvoice>("DeliveryOrderInvoice");
            //builder.EntitySet<DeliveryRequest>("DeliveryRequestsOData");
            //builder.EntitySet<DeliveryRequestProductDetailTransaction>("DeliveryRequestProductDetailTransaction");
            //builder.EntitySet<EMPLOYEE>("EMPLOYEE");
            //builder.EntitySet<FinanceBalance>("FinanceBalance");
            //builder.EntitySet<FinanceTransaction>("FinanceTransaction");
            //builder.EntitySet<FinanceTransactionNostro>("FinanceTransactionNostro");
            //builder.EntitySet<Menu>("Menu");
            //builder.EntitySet<NostroBank>("NostroBank");
            //builder.EntitySet<PRODUCT>("PRODUCT");
            //builder.EntitySet<ProductAdjustment>("ProductAdjustment");
            //builder.EntitySet<ProductConvertion>("ProductConvertion");
            //builder.EntitySet<ProductMixing>("ProductMixing");
            //builder.EntitySet<ProductSite>("ProductSite");
            //builder.EntitySet<RITASE>("RITASE");
            //builder.EntitySet<SITE>("SITE");
            //builder.EntitySet<SOURCE>("SOURCE");
            //builder.EntitySet<StockProduct>("StockProduct");
            //builder.EntitySet<TableSequence>("TableSequence");
            //builder.EntitySet<TransactionCode>("TransactionCode");
            //builder.EntitySet<TransactionProduct>("TransactionProductsOdata");
            //builder.EntitySet<UserClientMapping>("UserClientMapping");
            config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );


        }
    }
}
