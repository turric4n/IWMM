using System.Linq.Expressions;

namespace IWMM.Services.Abstractions
{
    public interface ISchemaRepository
    {
        void Save(dynamic schema, string path = "");
        dynamic Load(string path);
        string Serialize(dynamic schema);
    }
}
