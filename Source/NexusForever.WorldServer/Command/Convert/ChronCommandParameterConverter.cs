using System.Text;
using NexusForever.WorldServer.Command.Context;

namespace NexusForever.WorldServer.Command.Convert
{
    [Convert(typeof(ChronCommandParameterConverter))]
    public class ChronCommandParameterConverter : IParameterConvert
    {
        public string PlayerName { get; private set; }
        public string Message { get; private set; }
        public bool TextMode { get; private set; }

        public virtual object Convert(ICommandContext context, ParameterQueue queue)
        {
            if (queue.Front.ToLower().Equals("text") || queue.Front.ToLower().Equals("txt"))
            {
                TextMode = true;
                queue.Dequeue();
            }
            else
            {
                TextMode = false;
            }

            string parameter = queue.Dequeue();

            // concat parameters between quotes
            // "this will be passed as a single parameter"
            var sb = new StringBuilder();
            parameter = parameter[0..];

            while (true)
            {
                sb.Append(parameter);
                if (queue.Count == 0)
                    break;

                sb.Append(' ');
                parameter = queue.Dequeue();
            }

            string concatenatedCommand = sb.ToString();

            int playerTargetIndex = concatenatedCommand.IndexOf(':');
            if(playerTargetIndex <= 0)
            {
                PlayerName = null;
                Message = concatenatedCommand.Trim();
            }
            else
            {
                PlayerName = concatenatedCommand.Substring(0, playerTargetIndex).Trim();
                Message = concatenatedCommand.Substring(playerTargetIndex + 1).Trim();
            }

            return this;
        }
    }
}