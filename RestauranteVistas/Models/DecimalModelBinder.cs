using System.Globalization;
using System.Web.Mvc;

namespace RestauranteVistas.Models
{
    // HTML number controls submit invariant decimals, regardless of es-CR display culture.
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value == null) return bindingContext.ModelType == typeof(decimal?) ? null : (object)0m;
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
            decimal parsed;
            var input = value.AttemptedValue;
            if (string.IsNullOrWhiteSpace(input) && bindingContext.ModelType == typeof(decimal?)) return null;
            if (decimal.TryParse(input, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out parsed) ||
                decimal.TryParse(input, NumberStyles.Number, CultureInfo.GetCultureInfo("es-CR"), out parsed))
                return parsed;
            bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Ingrese un número decimal válido.");
            return bindingContext.ModelType == typeof(decimal?) ? null : (object)0m;
        }
    }
}
