using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
//https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/custom-model-binding?view=aspnetcore-3.1
//https://docs.microsoft.com/en-us/aspnet/core/mvc/advanced/custom-model-binding?view=aspnetcore-3.1
namespace WebApp.Models
{
    public class ObjectIdBinder : IModelBinder
    {        
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            string modelName = bindingContext.ModelName;

            ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            string value = valueProviderResult.FirstValue;
            
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            if (ObjectId.TryParse(value, out ObjectId id))
            {
                bindingContext.Result = ModelBindingResult.Success(id);
            }
            
            return Task.CompletedTask;
        }

    }

    public class ObjectIdBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(ObjectId))
            {
                return new BinderTypeModelBinder(typeof(ObjectIdBinder));
            }

            return null;
        }
    }

}
