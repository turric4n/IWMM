namespace IWMM.Services.Abstractions
{
    public interface ISchemaMerger
    {
        public dynamic Merge(List<dynamic> schemas);
    }
}
