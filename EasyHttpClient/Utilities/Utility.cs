﻿using EasyHttpClient.Attributes.Parameter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Reflection;
using System.Collections;

namespace EasyHttpClient.Utilities
{
    internal class Utility
    {
        public static IEnumerable<KeyValuePair<string, string>> ExtractUrlParameter(string name, object value, int deep)
        {
            if (deep < 0 || value == null)
                return Enumerable.Empty<KeyValuePair<string, string>>();


            Func<object, string> autoEncodeParameter = (val) =>
            {
                return Convert.ToString(val, CultureInfo.InvariantCulture);
                //return urlEncode?HttpUtility.UrlEncode(valStr):valStr;
            };

            var kps = new List<KeyValuePair<string, string>>();

            var paramType = value.GetType();
            if (paramType.IsEnum)
            {
                kps.Add(new KeyValuePair<string, string>(name, autoEncodeParameter((int)value)));
            }
            else if (paramType.IsBulitInType())
            {
                kps.Add(new KeyValuePair<string, string>(name, autoEncodeParameter(value)));
            }
            else if (paramType.IsEnumerableType())
            {
                var vals = ((IEnumerable)value).Cast<object>();
                foreach (var val in vals)
                    kps.Add(new KeyValuePair<string, string>(name, autoEncodeParameter(val)));
                //kps.Add(new KeyValuePair<string, string>(name, autoEncodeParameter((string.Join(",", value)))));
            }
            else
            {
                foreach (var p in paramType.GetProperties())
                {
                    if (p.IsDefined(typeof(HttpIgnoreAttribute)))
                    {
                        continue;
                    }
                    var pName = p.Name;
                    var pNameAttr = p.GetCustomAttribute<HttpAliasAttribute>();
                    if (pNameAttr != null && !string.IsNullOrWhiteSpace(pNameAttr.Name))
                    {
                        pName = pNameAttr.Name;
                    }
                    var propertyValue = p.GetValue(value);
                    if (propertyValue != null)
                    {
                        kps.AddRange(ExtractUrlParameter(pName, propertyValue, deep-1));
                    }
                }
            }
            return kps;
        }

        public static Uri CombinePaths(Uri baseUri, params string[] paths)
        {
            return CombinePaths(baseUri, paths as IEnumerable<string>);
        }

        public static Uri CombinePaths(Uri baseUri, IEnumerable<string> paths)
        {
            if (paths != null && paths.Any())
            {
                return CombinePaths(new Uri(baseUri, paths.FirstOrDefault()), paths.Skip(1));
            }
            else
            {
                return baseUri;
            }
        }
    }
}
