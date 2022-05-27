using NexusForever.WorldServer.Command.Context;
using System.Globalization;

namespace NexusForever.WorldServer.Command.Convert
{
    [Convert(typeof(float))]
    public class FloatParameterConverter : IParameterConvert
    {
        public object Convert(ICommandContext context, ParameterQueue queue)
        {
            return float.Parse(queue.Dequeue(), CultureInfo.InvariantCulture);
        }
    }
}
