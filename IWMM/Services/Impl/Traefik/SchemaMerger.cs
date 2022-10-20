using IWMM.Services.Abstractions;
using IWMM.Settings;
using Microsoft.Extensions.Options;
using System.Dynamic;

namespace IWMM.Services.Impl.Traefik
{
    public class SchemaMerger : ISchemaMerger
    {
        private dynamic GetPlainSchema()
        {
            //Only this by now

            var currentDynamic = (dynamic)new ExpandoObject();
            currentDynamic.http = new ExpandoObject() as IDictionary<string, object>;
            currentDynamic.http.middlewares = new ExpandoObject() as IDictionary<string, object>;
            currentDynamic.http.routers = new ExpandoObject() as IDictionary<string, object>;
            currentDynamic.http.services = new ExpandoObject() as IDictionary<string, object>;

            return currentDynamic;
        }

        private dynamic MergeKey(string key, dynamic currentSchema, dynamic additionalItem)
        {
            if (additionalItem.Key == key)
            {
                foreach (var currentSchemaItem in currentSchema.http)
                {
                    if (currentSchemaItem.Key == key)
                    {
                        foreach (var subadditionalItem in additionalItem.Value)
                        {
                            ((IDictionary<string, object>)currentSchemaItem.Value).Add(subadditionalItem.Key, subadditionalItem.Value);
                        }
                    }
                }
            }

            return currentSchema;
        }

        private dynamic Merge(dynamic currentSchema, dynamic additionalSchema)
        {
            var result = currentSchema;

            if (additionalSchema.http != null)
            {
                foreach (var additionalItem in additionalSchema.http)
                {
                    currentSchema = MergeKey("routers", currentSchema, additionalItem);
                    currentSchema = MergeKey("services", currentSchema, additionalItem);
                    currentSchema = MergeKey("middlewares", currentSchema, additionalItem);
                }
            }

            return currentSchema;
        }

        public dynamic Merge(List<dynamic> schemas)
        {
            var result = GetPlainSchema();

            foreach (dynamic schema in schemas)
            {
                result = Merge(result, schema);
            }

            return result;
        }
    }
}
