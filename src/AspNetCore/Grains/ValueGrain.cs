using GrainInterfaces;
using Orleans;
using System.Threading.Tasks;

namespace Grains
{
    public class ValueGrain : Grain, IValueGrain
    {
        private string _value = "none";

        public Task<string> GetValue()
        {
            return Task.FromResult(_value);
        }

        public Task SetValue(string value)
        {
            _value = value;
            return Task.CompletedTask;
        }
    }
}