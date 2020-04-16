using Interface.Application;
using System;
using System.Web.Mvc;

namespace Web.MainApplication.Service.System
{
    public class IndosuryaModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(IAuditableObject))
            {
               
                return base.BindModel(controllerContext, bindingContext);

            }
            else
            {
                var data = Activator.CreateInstance(bindingContext.ModelType);
                return base.BindModel(controllerContext, bindingContext);
            }
        }
         
    }
}