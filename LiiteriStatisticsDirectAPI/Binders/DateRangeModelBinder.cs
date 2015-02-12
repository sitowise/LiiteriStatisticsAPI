using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Diagnostics;

using System.Web.Http.ModelBinding;
using System.Web.Http.Controllers;

using LiiteriStatisticsCore.Util;

namespace LiiteriStatisticsDirectAPI.Binders
{
    public class DateRangeModelBinder : IModelBinder
    {
        public DateRangeModelBinder()
        {
            //Debug.WriteLine("In DateRangeModelBinder()");
        }

        public bool BindModel(
            HttpActionContext actionContext,
            ModelBindingContext bindingContext)
        {
            //Debug.WriteLine("In BindModel");

            string key = bindingContext.ModelName;
            var val = bindingContext.ValueProvider.GetValue(key);
            if (val == null) {
                //Debug.WriteLine("val == null");
                return false;
            }
            string s = val.AttemptedValue as string;
            if (s == null) {
                //Debug.WriteLine("s == null");
                return false;
            }

            string[] vals = val.AttemptedValue.Split(',');
            if (vals.Length != 2) {
                throw new ArgumentException("Incorrect format for DateRange");
            }
            bindingContext.Model = new DateRange(
                DateTime.Parse(vals[0]),
                DateTime.Parse(vals[1]));
            return true;
        }
    }
}
